using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Cards.Base;
using Assets.ElementalSystem;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.Cards
{
    public static class UnsyncedCardsDeletion
    {
        private const string MENU_PATH = "Tools/Cards/Delete Unsynced Cards";
        private const string CARDS_SHEET_PATH = "Assets/Cards/CardsLibrary/CardsSheet.csv";
        private const string CARDS_LIBRARY_PATH = "Assets/Cards/CardsLibrary";

        [MenuItem(MENU_PATH)]
        private static void DeleteUnsyncedCards()
        {
            CardSheetDeletionReport report = new CardSheetDeletionReport();

            try
            {
                if (!File.Exists(CARDS_SHEET_PATH))
                {
                    Debug.LogError($"Cards sheet not found at '{CARDS_SHEET_PATH}'.");
                    return;
                }

                string csvContent = File.ReadAllText(CARDS_SHEET_PATH, Encoding.UTF8);
                List<Dictionary<string, string>> rows = ParseCsv(csvContent);
                HashSet<string> expectedCardAssetPaths = BuildExpectedCardAssetPaths(rows, report);

                DeleteStaleCardAssets(expectedCardAssetPaths, report);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                LogSummary(report);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Delete unsynced cards failed with an unexpected error: {exception}");
            }
        }

        [MenuItem(MENU_PATH, true)]
        private static bool CanDeleteUnsyncedCards()
        {
            return File.Exists(CARDS_SHEET_PATH);
        }

        private static HashSet<string> BuildExpectedCardAssetPaths(
            IReadOnlyList<Dictionary<string, string>> rows,
            CardSheetDeletionReport report)
        {
            HashSet<string> expectedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                IReadOnlyDictionary<string, string> row = rows[rowIndex];
                string elementText = GetTrimmedValue(row, "Element");
                string title = GetTrimmedValue(row, "Title");

                if (string.IsNullOrWhiteSpace(elementText) || string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                if (!Enum.TryParse(elementText, true, out Elements element))
                {
                    report.Warnings.Add($"CSV row {rowIndex + 2}: could not derive expected card path because element '{elementText}' is invalid.");
                    continue;
                }

                string sanitizedTitle = SanitizeName(title, "Card");
                expectedPaths.Add($"{CARDS_LIBRARY_PATH}/{element}/{sanitizedTitle}Config.asset");
            }

            return expectedPaths;
        }

        private static void DeleteStaleCardAssets(HashSet<string> expectedCardAssetPaths, CardSheetDeletionReport report)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:CardConfigBaseSO", new[] { CARDS_LIBRARY_PATH }))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid).Replace('\\', '/');
                if (expectedCardAssetPaths.Contains(assetPath))
                {
                    continue;
                }

                if (AssetDatabase.DeleteAsset(assetPath))
                {
                    report.DeletedCards++;
                    continue;
                }

                report.Warnings.Add($"Could not delete unsynced card asset '{assetPath}'.");
            }
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

        private static void LogSummary(CardSheetDeletionReport report)
        {
            foreach (string warning in report.Warnings)
            {
                Debug.LogWarning(warning);
            }

            Debug.Log(
                "Delete unsynced cards finished. " +
                $"deleted cards: {report.DeletedCards}, " +
                $"warnings: {report.Warnings.Count}.");
        }

        private sealed class CardSheetDeletionReport
        {
            public int DeletedCards { get; set; }
            public List<string> Warnings { get; } = new List<string>();
        }
    }
}
