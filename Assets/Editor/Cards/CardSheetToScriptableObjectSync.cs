using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Assets.Cards.Base;
using Assets.Cards.Base.Damage;
using Assets.CustomTypes;
using Assets.Effects.Base;
using Assets.ElementalSystem;
using Assets.Targeting;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.Cards
{
    public static class CardSheetToScriptableObjectSync
    {
        private const string MENU_PATH = "Tools/Cards/Sync Cards From Sheet";
        private const string CARDS_SHEET_PATH = "Assets/Cards/CardsLibrary/CardsSheet.csv";
        private const string CARDS_LIBRARY_PATH = "Assets/Cards/CardsLibrary";
        private const string DAMAGE_ASSETS_PATH = "Assets/Cards/Base/Damage";
        private const string STATUS_EFFECTS_ROOT_PATH = "Assets/Effects/StatusEffects";
        private const string INSTANT_EFFECTS_ROOT_PATH = "Assets/Effects/InstantEffects";

        private const string TITLE_PROPERTY = "<Title>k__BackingField";
        private const string DESCRIPTION_PROPERTY = "<Description>k__BackingField";
        private const string START_ENERGY_COST_PROPERTY = "<StartEnergyCost>k__BackingField";
        private const string ELEMENT_PROPERTY = "<Element>k__BackingField";
        private const string ELEMENTAL_VISUAL_BASE_PROPERTY = "<ElementalVisualBase>k__BackingField";
        private const string FRONT_GRAPHIC_PROPERTY = "<FrontGraphic>k__BackingField";
        private const string ATTACK_STEPS_PROPERTY = "<AttackSteps>k__BackingField";
        private const string DAMAGE_VALUE_PROPERTY = "<DamageValue>k__BackingField";
        private const string ATTACK_COUNT_PROPERTY = "<AttackCount>k__BackingField";
        private const string START_POSITION_PROPERTY = "<StartPosition>k__BackingField";
        private const string TARGET_MODE_PROPERTY = "<TargetMode>k__BackingField";
        private const string STEP_DAMAGE_PROPERTY = "<Damage>k__BackingField";
        private const string STEP_EFFECT_PROPERTY = "<Effect>k__BackingField";
        private const string STEP_EFFECT_CHANCE_PROPERTY = "<EffectChance>k__BackingField";
        private const BindingFlags INSTANCE_NON_PUBLIC_FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;

        [MenuItem(MENU_PATH)]
        private static void SyncCardsFromSheet()
        {
            CardSheetImportReport report = new CardSheetImportReport();

            try
            {
                if (!File.Exists(CARDS_SHEET_PATH))
                {
                    Debug.LogError($"Cards sheet not found at '{CARDS_SHEET_PATH}'.");
                    return;
                }

                string csvContent = File.ReadAllText(CARDS_SHEET_PATH, Encoding.UTF8);
                List<Dictionary<string, string>> rows = ParseCsv(csvContent);
                Dictionary<DamageAssetKey, CardDamageSO> damageCache = BuildDamageCache(report);

                for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                {
                    ProcessRow(rows[rowIndex], rowIndex + 2, damageCache, report);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                LogSummary(report);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Card sheet sync failed with an unexpected error: {exception}");
            }
        }

        [MenuItem(MENU_PATH, true)]
        private static bool CanSyncCardsFromSheet()
        {
            return File.Exists(CARDS_SHEET_PATH);
        }

        private static void ProcessRow(
            IReadOnlyDictionary<string, string> row,
            int csvRowNumber,
            IDictionary<DamageAssetKey, CardDamageSO> damageCache,
            CardSheetImportReport report)
        {
            string title = GetTrimmedValue(row, "Title");
            string rowLabel = string.IsNullOrWhiteSpace(title)
                ? $"CSV row {csvRowNumber}"
                : $"CSV row {csvRowNumber} ('{title}')";

            try
            {
                CardSheetRow parsedRow = ParseRow(row, csvRowNumber);
                WarnOnUnsupportedDescription(parsedRow, report);

                string sanitizedTitle = SanitizeName(parsedRow.Title, "Card");
                string cardFolderPath = $"{CARDS_LIBRARY_PATH}/{parsedRow.Element}";
                EnsureFolderExists(cardFolderPath);

                CardConfigBaseSO cardAsset = LoadOrCreateCardAsset(cardFolderPath, sanitizedTitle, report);
                List<CardAttackStepDefinition> resolvedSteps = ResolveAttackSteps(parsedRow, damageCache, report);

                UpdateCardAsset(cardAsset, parsedRow, resolvedSteps);
                report.ImportedRows++;
            }
            catch (CardSheetImportException exception)
            {
                report.Errors.Add($"{rowLabel}: {exception.Message}");
            }
            catch (Exception exception)
            {
                report.Errors.Add($"{rowLabel}: unexpected error {exception.Message}");
            }
        }

        private static CardSheetRow ParseRow(IReadOnlyDictionary<string, string> row, int csvRowNumber)
        {
            string elementText = GetRequiredValue(row, "Element", csvRowNumber);
            string title = GetRequiredValue(row, "Title", csvRowNumber);
            string attackText = GetRequiredValue(row, "Attack", csvRowNumber);
            string energyText = GetRequiredValue(row, "Start Energy Cost", csvRowNumber);
            string description = GetTrimmedValue(row, "Description");
            string frontGraphicPath = GetRequiredValue(row, "Front Graphic", csvRowNumber);
            string visualBasePath = GetRequiredValue(row, "Visual Base", csvRowNumber);

            if (!Enum.TryParse(elementText, true, out Elements element))
            {
                throw new CardSheetImportException($"invalid element '{elementText}'.");
            }

            if (!byte.TryParse(energyText, out byte startEnergyCost) || startEnergyCost > 9)
            {
                throw new CardSheetImportException($"invalid Start Energy Cost '{energyText}'. Expected 0..9.");
            }

            Sprite frontGraphic = ResolveAssetFromSheetPath<Sprite>(frontGraphicPath);
            if (frontGraphic == null)
            {
                throw new CardSheetImportException($"Front Graphic asset not found for '{frontGraphicPath}'.");
            }

            CardElementalVisualBaseSO visualBase = ResolveAssetFromSheetPath<CardElementalVisualBaseSO>(visualBasePath);
            if (visualBase == null)
            {
                throw new CardSheetImportException($"Visual Base asset not found for '{visualBasePath}'.");
            }

            List<CardAttackStepDefinition> attackDefinitions = ParseAttackDefinitions(attackText, csvRowNumber);
            if (attackDefinitions.Count == 0)
            {
                throw new CardSheetImportException("Attack field does not contain any valid steps.");
            }

            return new CardSheetRow(
                csvRowNumber,
                element,
                title,
                description,
                startEnergyCost,
                frontGraphic,
                visualBase,
                attackDefinitions);
        }

        private static List<CardAttackStepDefinition> ResolveAttackSteps(
            CardSheetRow row,
            IDictionary<DamageAssetKey, CardDamageSO> damageCache,
            CardSheetImportReport report)
        {
            List<CardAttackStepDefinition> resolvedSteps = new List<CardAttackStepDefinition>(row.AttackSteps.Count);

            for (int stepIndex = 0; stepIndex < row.AttackSteps.Count; stepIndex++)
            {
                CardAttackStepDefinition step = row.AttackSteps[stepIndex];
                CardDamageSO damageAsset = GetOrCreateDamageAsset(step, damageCache, report);
                EffectSO effectAsset = ResolveEffect(step, row.CsvRowNumber);

                if (!step.HasEffect && step.RawChancePercent.HasValue)
                {
                    report.Warnings.Add($"CSV row {row.CsvRowNumber} ('{row.Title}') step {stepIndex + 1}: chance token ignored because effect is None.");
                }

                resolvedSteps.Add(step.WithResolvedAssets(damageAsset, effectAsset));
            }

            return resolvedSteps;
        }

        private static EffectSO ResolveEffect(CardAttackStepDefinition step, int csvRowNumber)
        {
            if (!step.HasEffect)
            {
                return null;
            }

            string sanitizedEffectName = SanitizeName(step.EffectName, step.EffectName);

            string statusEffectPath = $"{STATUS_EFFECTS_ROOT_PATH}/{sanitizedEffectName}/{sanitizedEffectName}.asset";
            EffectSO effectAsset = AssetDatabase.LoadAssetAtPath<EffectSO>(statusEffectPath);
            if (effectAsset != null)
            {
                return effectAsset;
            }

            string instantEffectPath = $"{INSTANT_EFFECTS_ROOT_PATH}/{sanitizedEffectName}/{sanitizedEffectName}.asset";
            effectAsset = AssetDatabase.LoadAssetAtPath<EffectSO>(instantEffectPath);
            if (effectAsset != null)
            {
                return effectAsset;
            }

            throw new CardSheetImportException(
                $"effect asset not found for '{sanitizedEffectName}' in row {csvRowNumber}. " +
                $"Searched '{statusEffectPath}' and '{instantEffectPath}'.");
        }

        private static CardConfigBaseSO LoadOrCreateCardAsset(string cardFolderPath, string sanitizedTitle, CardSheetImportReport report)
        {
            string assetPath = $"{cardFolderPath}/{sanitizedTitle}Config.asset";
            CardConfigBaseSO cardAsset = AssetDatabase.LoadAssetAtPath<CardConfigBaseSO>(assetPath);
            if (cardAsset != null)
            {
                report.UpdatedCards++;
                return cardAsset;
            }

            cardAsset = ScriptableObject.CreateInstance<CardConfigBaseSO>();
            AssetDatabase.CreateAsset(cardAsset, assetPath);
            report.CreatedCards++;
            return cardAsset;
        }

        private static void UpdateCardAsset(CardConfigBaseSO cardAsset, CardSheetRow row, IReadOnlyList<CardAttackStepDefinition> attackSteps)
        {
            ApplyCardAssetValues(cardAsset, row, attackSteps);
            EditorUtility.SetDirty(cardAsset);
        }

        private static void ApplyCardAssetValues(CardConfigBaseSO cardAsset, CardSheetRow row, IReadOnlyList<CardAttackStepDefinition> attackSteps)
        {
            SetBackingField(cardAsset, TITLE_PROPERTY, row.Title);
            SetBackingField(cardAsset, DESCRIPTION_PROPERTY, row.Description);
            SetBackingField(cardAsset, START_ENERGY_COST_PROPERTY, row.StartEnergyCost);
            SetBackingField(cardAsset, ELEMENT_PROPERTY, row.Element);
            SetBackingField(cardAsset, ELEMENTAL_VISUAL_BASE_PROPERTY, row.VisualBase);
            SetBackingField(cardAsset, FRONT_GRAPHIC_PROPERTY, row.FrontGraphic);
            SetBackingField(cardAsset, ATTACK_STEPS_PROPERTY, BuildCardAttackSteps(attackSteps));

            SerializedObject serializedObject = new SerializedObject(cardAsset);
            serializedObject.FindProperty(TITLE_PROPERTY).stringValue = row.Title;
            serializedObject.FindProperty(DESCRIPTION_PROPERTY).stringValue = row.Description;
            serializedObject.FindProperty(START_ENERGY_COST_PROPERTY).intValue = row.StartEnergyCost;
            serializedObject.FindProperty(ELEMENT_PROPERTY).enumValueIndex = (int)row.Element;
            serializedObject.FindProperty(ELEMENTAL_VISUAL_BASE_PROPERTY).objectReferenceValue = row.VisualBase;
            serializedObject.FindProperty(FRONT_GRAPHIC_PROPERTY).objectReferenceValue = row.FrontGraphic;

            SerializedProperty attackStepsProperty = serializedObject.FindProperty(ATTACK_STEPS_PROPERTY);
            attackStepsProperty.arraySize = attackSteps.Count;

            for (int index = 0; index < attackSteps.Count; index++)
            {
                CardAttackStepDefinition step = attackSteps[index];
                SerializedProperty stepProperty = attackStepsProperty.GetArrayElementAtIndex(index);
                stepProperty.FindPropertyRelative(STEP_DAMAGE_PROPERTY).objectReferenceValue = step.DamageAsset;
                stepProperty.FindPropertyRelative(STEP_EFFECT_PROPERTY).objectReferenceValue = step.EffectAsset;
                stepProperty.FindPropertyRelative(STEP_EFFECT_CHANCE_PROPERTY).floatValue = step.EffectChance;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static List<CardAttackStep> BuildCardAttackSteps(IReadOnlyList<CardAttackStepDefinition> attackSteps)
        {
            List<CardAttackStep> cardAttackSteps = new List<CardAttackStep>(attackSteps.Count);

            for (int index = 0; index < attackSteps.Count; index++)
            {
                CardAttackStepDefinition step = attackSteps[index];
                cardAttackSteps.Add(new CardAttackStep(step.DamageAsset, step.EffectAsset, step.EffectChance));
            }

            return cardAttackSteps;
        }

        private static CardDamageSO GetOrCreateDamageAsset(
            CardAttackStepDefinition step,
            IDictionary<DamageAssetKey, CardDamageSO> damageCache,
            CardSheetImportReport report)
        {
            DamageAssetKey key = new DamageAssetKey(step.DamageValue, step.AttackCount, step.StartPosition, step.TargetMode);
            if (damageCache.TryGetValue(key, out CardDamageSO cachedDamage))
            {
                EnsureDamageAssetState(cachedDamage, step);
                report.ReusedDamageAssets++;
                return cachedDamage;
            }

            EnsureFolderExists(DAMAGE_ASSETS_PATH);
            string explicitAssetName = BuildDamageAssetName(step);
            string explicitAssetPath = $"{DAMAGE_ASSETS_PATH}/{explicitAssetName}.asset";
            string legacyAssetPath = $"{DAMAGE_ASSETS_PATH}/{BuildLegacyDamageAssetName(step)}.asset";

            CardDamageSO damageAsset = AssetDatabase.LoadAssetAtPath<CardDamageSO>(explicitAssetPath);
            if (damageAsset == null)
            {
                damageAsset = AssetDatabase.LoadAssetAtPath<CardDamageSO>(legacyAssetPath);
            }

            if (damageAsset != null)
            {
                EnsureDamageAssetState(damageAsset, step);
                string currentAssetPath = AssetDatabase.GetAssetPath(damageAsset);
                if (!string.Equals(currentAssetPath, explicitAssetPath, StringComparison.OrdinalIgnoreCase))
                {
                    string targetPath = explicitAssetPath;
                    if (AssetDatabase.LoadAssetAtPath<CardDamageSO>(targetPath) != null)
                    {
                        targetPath = AssetDatabase.GenerateUniqueAssetPath(targetPath);
                        report.Warnings.Add(
                            $"Damage asset name collision detected for '{explicitAssetName}.asset'. Moved repaired asset to '{Path.GetFileName(targetPath)}' instead.");
                    }

                    string moveError = AssetDatabase.MoveAsset(currentAssetPath, targetPath);
                    if (!string.IsNullOrEmpty(moveError))
                    {
                        report.Warnings.Add($"Could not move damage asset '{currentAssetPath}' to '{targetPath}': {moveError}");
                    }
                }

                damageCache[key] = damageAsset;
                report.ReusedDamageAssets++;
                return damageAsset;
            }

            string createPath = explicitAssetPath;
            if (AssetDatabase.LoadAssetAtPath<CardDamageSO>(createPath) != null)
            {
                createPath = AssetDatabase.GenerateUniqueAssetPath(createPath);
                report.Warnings.Add($"Damage asset name collision detected for '{explicitAssetName}.asset'. Created '{Path.GetFileName(createPath)}' instead.");
            }

            damageAsset = ScriptableObject.CreateInstance<CardDamageSO>();
            damageAsset.name = Path.GetFileNameWithoutExtension(createPath);
            ApplyDamageAssetValues(damageAsset, step);
            AssetDatabase.CreateAsset(damageAsset, createPath);
            EnsureDamageAssetState(damageAsset, step);

            damageCache[key] = damageAsset;
            report.CreatedDamageAssets++;
            return damageAsset;
        }

        private static void EnsureDamageAssetState(CardDamageSO damageAsset, CardAttackStepDefinition step)
        {
            string desiredName = BuildDamageAssetName(step);
            if (!string.Equals(damageAsset.name, desiredName, StringComparison.Ordinal))
            {
                damageAsset.name = desiredName;
            }

            ApplyDamageAssetValues(damageAsset, step);
            EditorUtility.SetDirty(damageAsset);
        }

        private static void ApplyDamageAssetValues(CardDamageSO damageAsset, CardAttackStepDefinition step)
        {
            SetBackingField(damageAsset, DAMAGE_VALUE_PROPERTY, step.DamageValue);
            SetBackingField(damageAsset, ATTACK_COUNT_PROPERTY, step.AttackCount);
            SetBackingField(damageAsset, START_POSITION_PROPERTY, step.StartPosition);
            SetBackingField(damageAsset, TARGET_MODE_PROPERTY, step.TargetMode);

            SerializedObject serializedObject = new SerializedObject(damageAsset);
            serializedObject.FindProperty(DAMAGE_VALUE_PROPERTY).intValue = step.DamageValue;
            serializedObject.FindProperty(ATTACK_COUNT_PROPERTY).intValue = step.AttackCount;
            serializedObject.FindProperty(START_POSITION_PROPERTY).enumValueIndex = (int)step.StartPosition;
            serializedObject.FindProperty(TARGET_MODE_PROPERTY).enumValueIndex = (int)step.TargetMode;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetBackingField<TTarget, TValue>(TTarget target, string fieldName, TValue value) where TTarget : class
        {
            FieldInfo fieldInfo = typeof(TTarget).GetField(fieldName, INSTANCE_NON_PUBLIC_FLAGS);
            if (fieldInfo == null)
            {
                throw new CardSheetImportException($"Could not resolve backing field '{fieldName}' on {typeof(TTarget).Name}.");
            }

            fieldInfo.SetValue(target, value);
        }

        private static Dictionary<DamageAssetKey, CardDamageSO> BuildDamageCache(CardSheetImportReport report)
        {
            Dictionary<DamageAssetKey, CardDamageSO> damageCache = new Dictionary<DamageAssetKey, CardDamageSO>();

            foreach (string guid in AssetDatabase.FindAssets("t:CardDamageSO", new[] { DAMAGE_ASSETS_PATH }))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                CardDamageSO damageAsset = AssetDatabase.LoadAssetAtPath<CardDamageSO>(assetPath);
                if (damageAsset == null)
                {
                    continue;
                }

                DamageAssetKey key = new DamageAssetKey(
                    damageAsset.DamageValue,
                    damageAsset.AttackCount,
                    damageAsset.StartPosition,
                    damageAsset.TargetMode);

                if (!damageCache.ContainsKey(key))
                {
                    damageCache.Add(key, damageAsset);
                    continue;
                }

                report.Warnings.Add($"Duplicate CardDamageSO detected for values '{key}'. Using '{damageCache[key].name}' and ignoring '{damageAsset.name}'.");
            }

            return damageCache;
        }

        private static void WarnOnUnsupportedDescription(CardSheetRow row, CardSheetImportReport report)
        {
            if (string.IsNullOrWhiteSpace(row.Description))
            {
                return;
            }

            string normalized = row.Description.ToLowerInvariant();
            string[] unsupportedMarkers =
            {
                "50%",
                "chance",
                "szans",
                "jezeli",
                "if ",
                "nastepnej turze",
                "next turn"
            };

            if (unsupportedMarkers.Any(marker => normalized.Contains(marker)))
            {
                report.Warnings.Add(
                    $"CSV row {row.CsvRowNumber} ('{row.Title}'): description may describe unsupported mechanics not encoded in Attack. Imported encoded values only.");
            }
        }

        private static List<CardAttackStepDefinition> ParseAttackDefinitions(string attackText, int csvRowNumber)
        {
            List<CardAttackStepDefinition> attackDefinitions = new List<CardAttackStepDefinition>();
            string[] lines = attackText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] tokens = line.Split('_');
                if (tokens.Length != 4)
                {
                    throw new CardSheetImportException(
                        $"invalid attack step '{line}' in row {csvRowNumber}. Expected 4 underscore-separated tokens.");
                }

                string[] damageTokens = tokens[0].Split('x');
                if (damageTokens.Length != 2)
                {
                    throw new CardSheetImportException($"invalid damage token '{tokens[0]}' in row {csvRowNumber}.");
                }

                if (!int.TryParse(damageTokens[0], out int damageValue) || damageValue < 0)
                {
                    throw new CardSheetImportException($"invalid damage value '{damageTokens[0]}' in row {csvRowNumber}.");
                }

                if (!int.TryParse(damageTokens[1], out int attackCount) || attackCount < 0)
                {
                    throw new CardSheetImportException($"invalid attack count '{damageTokens[1]}' in row {csvRowNumber}.");
                }

                if (!Enum.TryParse(tokens[1], true, out StartPosition startPosition))
                {
                    throw new CardSheetImportException($"invalid StartPosition '{tokens[1]}' in row {csvRowNumber}.");
                }

                if (!Enum.TryParse(tokens[2], true, out TargetingMode targetMode))
                {
                    throw new CardSheetImportException($"invalid TargetMode '{tokens[2]}' in row {csvRowNumber}.");
                }

                string effectToken = tokens[3].Trim();
                string effectName = effectToken;
                int? rawChancePercent = null;
                float effectChance = 1f;

                int chanceSeparatorIndex = effectToken.IndexOf('%');
                if (chanceSeparatorIndex >= 0)
                {
                    effectName = effectToken.Substring(0, chanceSeparatorIndex).Trim();
                    string chanceText = effectToken.Substring(chanceSeparatorIndex + 1).Trim();

                    if (string.IsNullOrWhiteSpace(effectName))
                    {
                        throw new CardSheetImportException($"missing effect name before chance token in row {csvRowNumber}.");
                    }

                    if (!int.TryParse(chanceText, out int chancePercent) || chancePercent < 0 || chancePercent > 100)
                    {
                        throw new CardSheetImportException($"invalid chance percent '{chanceText}' in row {csvRowNumber}. Expected 0..100.");
                    }

                    rawChancePercent = chancePercent;
                    effectChance = chancePercent / 100f;
                }

                attackDefinitions.Add(new CardAttackStepDefinition(
                    damageValue,
                    attackCount,
                    startPosition,
                    targetMode,
                    effectName,
                    effectChance,
                    rawChancePercent,
                    null,
                    null));
            }

            return attackDefinitions;
        }

        private static List<Dictionary<string, string>> ParseCsv(string content)
        {
            List<List<string>> parsedRows = new List<List<string>>();
            List<string> currentRow = new List<string>();
            StringBuilder currentCell = new StringBuilder();
            bool isInsideQuotes = false;

            for (int index = 0; index < content.Length; index++)
            {
                char character = content[index];

                if (character == '"')
                {
                    bool isEscapedQuote = isInsideQuotes && index + 1 < content.Length && content[index + 1] == '"';
                    if (isEscapedQuote)
                    {
                        currentCell.Append('"');
                        index++;
                        continue;
                    }

                    isInsideQuotes = !isInsideQuotes;
                    continue;
                }

                if (character == ',' && !isInsideQuotes)
                {
                    currentRow.Add(currentCell.ToString());
                    currentCell.Clear();
                    continue;
                }

                if ((character == '\n' || character == '\r') && !isInsideQuotes)
                {
                    if (character == '\r' && index + 1 < content.Length && content[index + 1] == '\n')
                    {
                        index++;
                    }

                    currentRow.Add(currentCell.ToString());
                    currentCell.Clear();

                    if (currentRow.Any(cell => !string.IsNullOrWhiteSpace(cell)))
                    {
                        parsedRows.Add(currentRow);
                    }

                    currentRow = new List<string>();
                    continue;
                }

                currentCell.Append(character);
            }

            if (currentCell.Length > 0 || currentRow.Count > 0)
            {
                currentRow.Add(currentCell.ToString());
                if (currentRow.Any(cell => !string.IsNullOrWhiteSpace(cell)))
                {
                    parsedRows.Add(currentRow);
                }
            }

            if (parsedRows.Count == 0)
            {
                return new List<Dictionary<string, string>>();
            }

            List<string> headers = parsedRows[0]
                .Select(header => header.Trim().TrimStart('\uFEFF'))
                .ToList();

            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
            for (int rowIndex = 1; rowIndex < parsedRows.Count; rowIndex++)
            {
                List<string> values = parsedRows[rowIndex];
                Dictionary<string, string> row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                for (int columnIndex = 0; columnIndex < headers.Count; columnIndex++)
                {
                    string header = headers[columnIndex];
                    string value = columnIndex < values.Count ? values[columnIndex] : string.Empty;
                    row[header] = value;
                }

                rows.Add(row);
            }

            return rows;
        }

        private static T ResolveAssetFromSheetPath<T>(string sheetPath) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(sheetPath))
            {
                return null;
            }

            string normalizedPath = sheetPath.Trim().Replace('\\', '/');
            T directAsset = AssetDatabase.LoadAssetAtPath<T>(normalizedPath);
            if (directAsset != null)
            {
                return directAsset;
            }

            if (typeof(T) == typeof(Sprite))
            {
                T spriteDirectAsset = LoadSpriteAsset(normalizedPath) as T;
                if (spriteDirectAsset != null)
                {
                    return spriteDirectAsset;
                }
            }

            string directory = Path.GetDirectoryName(normalizedPath)?.Replace('\\', '/');
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(normalizedPath);
            if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(fileNameWithoutExtension))
            {
                return null;
            }

            foreach (string guid in AssetDatabase.FindAssets(fileNameWithoutExtension, new[] { directory }))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid).Replace('\\', '/');
                string assetPathWithoutExtension = Path.Combine(
                    Path.GetDirectoryName(assetPath) ?? string.Empty,
                    Path.GetFileNameWithoutExtension(assetPath)).Replace('\\', '/');

                if (!string.Equals(assetPathWithoutExtension, normalizedPath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    return asset;
                }

                if (typeof(T) == typeof(Sprite))
                {
                    T spriteAsset = LoadSpriteAsset(assetPath) as T;
                    if (spriteAsset != null)
                    {
                        return spriteAsset;
                    }
                }
            }

            return null;
        }

        private static Sprite LoadSpriteAsset(string assetPath)
        {
            Sprite directSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (directSprite != null)
            {
                return directSprite;
            }

            return AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().FirstOrDefault();
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] segments = folderPath.Split('/');
            string currentPath = segments[0];

            for (int index = 1; index < segments.Length; index++)
            {
                string nextPath = $"{currentPath}/{segments[index]}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, segments[index]);
                }

                currentPath = nextPath;
            }
        }

        private static string GetRequiredValue(IReadOnlyDictionary<string, string> row, string key, int csvRowNumber)
        {
            string value = GetTrimmedValue(row, key);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new CardSheetImportException($"required column '{key}' is empty in row {csvRowNumber}.");
            }

            return value;
        }

        private static string GetTrimmedValue(IReadOnlyDictionary<string, string> row, string key)
        {
            return row.TryGetValue(key, out string value) ? value?.Trim() ?? string.Empty : string.Empty;
        }

        private static string SanitizeName(string rawValue, string fallback)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return fallback;
            }

            StringBuilder builder = new StringBuilder();
            bool capitalizeNext = true;

            foreach (char character in rawValue.Trim())
            {
                if (!char.IsLetterOrDigit(character))
                {
                    capitalizeNext = true;
                    continue;
                }

                builder.Append(capitalizeNext ? char.ToUpperInvariant(character) : character);
                capitalizeNext = false;
            }

            return builder.Length == 0 ? fallback : builder.ToString();
        }

        private static string BuildDamageAssetName(CardAttackStepDefinition step)
        {
            return $"{step.DamageValue}x{step.AttackCount}_{step.StartPosition}_{step.TargetMode}";
        }

        private static string BuildLegacyDamageAssetName(CardAttackStepDefinition step)
        {
            return $"{step.DamageValue}x{step.AttackCount}{ToLegacyStartPositionCode(step.StartPosition)}{ToLegacyTargetModeCode(step.TargetMode)}";
        }

        private static char ToLegacyStartPositionCode(StartPosition startPosition)
        {
            return startPosition switch
            {
                StartPosition.Start => 's',
                StartPosition.Center => 'c',
                StartPosition.End => 'e',
                _ => 's'
            };
        }

        private static char ToLegacyTargetModeCode(TargetingMode targetMode)
        {
            return targetMode switch
            {
                TargetingMode.Same => 's',
                TargetingMode.All => 'o',
                TargetingMode.Random => 'r',
                _ => 's'
            };
        }

        private static void LogSummary(CardSheetImportReport report)
        {
            foreach (string warning in report.Warnings)
            {
                Debug.LogWarning(warning);
            }

            foreach (string error in report.Errors)
            {
                Debug.LogError(error);
            }

            Debug.Log(
                "Card sheet sync finished. " +
                $"Imported rows: {report.ImportedRows}, " +
                $"created cards: {report.CreatedCards}, " +
                $"updated cards: {report.UpdatedCards}, " +
                $"reused damage assets: {report.ReusedDamageAssets}, " +
                $"created damage assets: {report.CreatedDamageAssets}, " +
                $"warnings: {report.Warnings.Count}, " +
                $"errors: {report.Errors.Count}.");
        }

        private sealed class CardSheetImportException : Exception
        {
            public CardSheetImportException(string message) : base(message)
            {
            }
        }

        private sealed class CardSheetImportReport
        {
            public int ImportedRows { get; set; }
            public int CreatedCards { get; set; }
            public int UpdatedCards { get; set; }
            public int ReusedDamageAssets { get; set; }
            public int CreatedDamageAssets { get; set; }
            public List<string> Warnings { get; } = new List<string>();
            public List<string> Errors { get; } = new List<string>();
        }

        private sealed class CardSheetRow
        {
            public int CsvRowNumber { get; }
            public Elements Element { get; }
            public string Title { get; }
            public string Description { get; }
            public byte StartEnergyCost { get; }
            public Sprite FrontGraphic { get; }
            public CardElementalVisualBaseSO VisualBase { get; }
            public List<CardAttackStepDefinition> AttackSteps { get; }

            public CardSheetRow(
                int csvRowNumber,
                Elements element,
                string title,
                string description,
                byte startEnergyCost,
                Sprite frontGraphic,
                CardElementalVisualBaseSO visualBase,
                List<CardAttackStepDefinition> attackSteps)
            {
                CsvRowNumber = csvRowNumber;
                Element = element;
                Title = title;
                Description = description;
                StartEnergyCost = startEnergyCost;
                FrontGraphic = frontGraphic;
                VisualBase = visualBase;
                AttackSteps = attackSteps;
            }
        }

        private sealed class CardAttackStepDefinition
        {
            public int DamageValue { get; }
            public int AttackCount { get; }
            public StartPosition StartPosition { get; }
            public TargetingMode TargetMode { get; }
            public string EffectName { get; }
            public float EffectChance { get; }
            public int? RawChancePercent { get; }
            public CardDamageSO DamageAsset { get; }
            public EffectSO EffectAsset { get; }
            public bool HasEffect => !string.Equals(EffectName, EffectType.None.ToString(), StringComparison.OrdinalIgnoreCase);

            public CardAttackStepDefinition(
                int damageValue,
                int attackCount,
                StartPosition startPosition,
                TargetingMode targetMode,
                string effectName,
                float effectChance,
                int? rawChancePercent,
                CardDamageSO damageAsset,
                EffectSO effectAsset)
            {
                DamageValue = damageValue;
                AttackCount = attackCount;
                StartPosition = startPosition;
                TargetMode = targetMode;
                EffectName = effectName;
                EffectChance = effectChance;
                RawChancePercent = rawChancePercent;
                DamageAsset = damageAsset;
                EffectAsset = effectAsset;
            }

            public CardAttackStepDefinition WithResolvedAssets(CardDamageSO damageAsset, EffectSO effectAsset)
            {
                return new CardAttackStepDefinition(
                    DamageValue,
                    AttackCount,
                    StartPosition,
                    TargetMode,
                    EffectName,
                    EffectChance,
                    RawChancePercent,
                    damageAsset,
                    effectAsset);
            }
        }

        private readonly struct DamageAssetKey : IEquatable<DamageAssetKey>
        {
            private readonly int _damageValue;
            private readonly int _attackCount;
            private readonly StartPosition _startPosition;
            private readonly TargetingMode _targetMode;

            public DamageAssetKey(int damageValue, int attackCount, StartPosition startPosition, TargetingMode targetMode)
            {
                _damageValue = damageValue;
                _attackCount = attackCount;
                _startPosition = startPosition;
                _targetMode = targetMode;
            }

            public bool Equals(DamageAssetKey other)
            {
                return _damageValue == other._damageValue
                    && _attackCount == other._attackCount
                    && _startPosition == other._startPosition
                    && _targetMode == other._targetMode;
            }

            public override bool Equals(object obj)
            {
                return obj is DamageAssetKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = _damageValue;
                    hashCode = (hashCode * 397) ^ _attackCount;
                    hashCode = (hashCode * 397) ^ (int)_startPosition;
                    hashCode = (hashCode * 397) ^ (int)_targetMode;
                    return hashCode;
                }
            }

            public override string ToString()
            {
                return $"{_damageValue}x{_attackCount}-{_startPosition}-{_targetMode}";
            }
        }
    }
}
