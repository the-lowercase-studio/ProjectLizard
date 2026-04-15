using Assets.Constants;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Editor
{
    public class AsepriteUiAnimationBindingPostprocessor : AssetPostprocessor
    {

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths,
            bool didDomainReload)
        {
            if (SessionState.GetBool(AsepriteUiAnimationConstants.AUTO_EXPORT_GUARD_KEY, false))
            {
                return;
            }

            HashSet<string> asepritePaths = CollectImportedAsepritePaths(importedAssets);
            if (asepritePaths.Count == 0)
            {
                return;
            }

            SessionState.SetBool(AsepriteUiAnimationConstants.AUTO_EXPORT_GUARD_KEY, true);
            try
            {
                foreach (string asepritePath in asepritePaths)
                {
                    if (!ShouldAutoExport(asepritePath))
                    {
                        continue;
                    }

                    if (ExportAndConvertAsepriteAsset(asepritePath))
                    {
                        MarkAutoExported(asepritePath);
                    }
                }
            }
            finally
            {
                SessionState.SetBool(AsepriteUiAnimationConstants.AUTO_EXPORT_GUARD_KEY, false);
            }
        }

        [MenuItem(AsepriteUiAnimationConstants.EXPORT_MENU_PATH)]
        private static void ExportSelectedAsepriteAssets()
        {
            HashSet<string> asepritePaths = CollectSelectedAsepritePaths();
            int exported = ExportAndConvertAsepriteAssets(asepritePaths);
            Debug.Log($"Aseprite UI export finished. Processed {exported} Aseprite asset(s).");
        }

        [MenuItem(AsepriteUiAnimationConstants.EXPORT_MENU_PATH, true)]
        private static bool CanExportSelectedAsepriteAssets()
        {
            return Selection.objects != null && Selection.objects.Length > 0;
        }

        private static HashSet<string> CollectImportedAsepritePaths(string[] importedAssets)
        {
            HashSet<string> asepritePaths = new HashSet<string>();

            foreach (string path in importedAssets)
            {
                if (IsAsepriteSource(path))
                {
                    asepritePaths.Add(path);
                }
            }

            return asepritePaths;
        }

        private static HashSet<string> CollectSelectedAsepritePaths()
        {
            HashSet<string> asepritePaths = new HashSet<string>();

            foreach (Object selectedObject in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(selectedObject);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (IsAsepriteSource(path))
                {
                    asepritePaths.Add(path);
                    continue;
                }

                if (HasAsepriteSibling(path, out string asepriteSiblingPath))
                {
                    asepritePaths.Add(asepriteSiblingPath);
                }
            }

            return asepritePaths;
        }

        private static int ExportAndConvertAsepriteAssets(HashSet<string> asepritePaths)
        {
            int processedCount = 0;

            foreach (string asepritePath in asepritePaths)
            {
                if (ExportAndConvertAsepriteAsset(asepritePath))
                {
                    processedCount++;
                }
            }

            if (processedCount > 0)
            {
                AssetDatabase.SaveAssets();
            }

            return processedCount;
        }

        private static bool ShouldAutoExport(string asepritePath)
        {
            string guid = AssetDatabase.AssetPathToGUID(asepritePath);
            if (string.IsNullOrEmpty(guid))
            {
                return false;
            }

            string key = AsepriteUiAnimationConstants.AUTO_EXPORT_PREFS_KEY_PREFIX + guid;
            return !EditorPrefs.GetBool(key, false);
        }

        private static void MarkAutoExported(string asepritePath)
        {
            string guid = AssetDatabase.AssetPathToGUID(asepritePath);
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            string key = AsepriteUiAnimationConstants.AUTO_EXPORT_PREFS_KEY_PREFIX + guid;
            EditorPrefs.SetBool(key, true);
        }

        private static bool IsAsepriteSource(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();

            foreach (string asepriteExtension in AsepriteUiAnimationConstants.ASEPRITE_EXTENSIONS)
            {
                if (extension == asepriteExtension)
                {
                    return true;
                }
            }

            return false;
        }

        private static string EnsureSuffix(string value, string suffix)
        {
            if (string.IsNullOrEmpty(value))
            {
                return suffix;
            }

            if (value.EndsWith(suffix))
            {
                return value;
            }

            return value + suffix;
        }

        private static bool ExportAndConvertAsepriteAsset(string asepritePath)
        {
            AnimatorController sourceController = GetControllerAtPath(asepritePath);
            if (sourceController == null)
            {
                return false;
            }

            AnimationClip[] sourceClips = sourceController.animationClips;
            if (sourceClips == null || sourceClips.Length == 0)
            {
                return false;
            }

            string outputFolder = EnsureOutputFolder(asepritePath);
            Dictionary<int, AnimationClip> exportedClipByInstanceId = new Dictionary<int, AnimationClip>();
            Dictionary<string, AnimationClip> exportedClipByGlobalObjectId = new Dictionary<string, AnimationClip>();
            Dictionary<string, AnimationClip> exportedClipByName = new Dictionary<string, AnimationClip>();

            foreach (AnimationClip sourceClip in sourceClips)
            {
                if (sourceClip == null)
                {
                    continue;
                }

                string clipAssetName = EnsureSuffix(sourceClip.name, AsepriteUiAnimationConstants.CLIP_NAME_SUFFIX);
                string clipPath = $"{outputFolder}/{clipAssetName}.anim";
                AnimationClip exportedClip = UpsertAnimationClip(sourceClip, clipPath);
                if (exportedClip == null)
                {
                    continue;
                }

                ConvertSpriteRendererBindingsToImageBindings(exportedClip);
                exportedClipByInstanceId[sourceClip.GetInstanceID()] = exportedClip;
                exportedClipByName[sourceClip.name] = exportedClip;

                GlobalObjectId globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(sourceClip);
                string globalObjectIdKey = globalObjectId.ToString();
                if (!string.IsNullOrEmpty(globalObjectIdKey))
                {
                    exportedClipByGlobalObjectId[globalObjectIdKey] = exportedClip;
                }
            }

            if (exportedClipByInstanceId.Count == 0)
            {
                return false;
            }

            string baseName = Path.GetFileNameWithoutExtension(asepritePath);
            string controllerAssetName = EnsureSuffix(baseName, AsepriteUiAnimationConstants.CONTROLLER_NAME_SUFFIX);
            string controllerPath = $"{outputFolder}/{controllerAssetName}.controller";
            AnimatorController exportedController = UpsertAnimatorController(sourceController, controllerPath);
            if (exportedController == null)
            {
                return false;
            }

            RebuildControllerWithExportedClips(exportedController, sourceController, sourceClips, exportedClipByName);
            EditorUtility.SetDirty(exportedController);
            return true;
        }

        private static AnimatorController GetControllerAtPath(string asepritePath)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(asepritePath);
            foreach (Object asset in assets)
            {
                if (asset is AnimatorController controller)
                {
                    return controller;
                }
            }

            return null;
        }

        private static AnimationClip UpsertAnimationClip(AnimationClip sourceClip, string clipPath)
        {
            AnimationClip destinationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (destinationClip == null)
            {
                destinationClip = new AnimationClip();
                AssetDatabase.CreateAsset(destinationClip, clipPath);
            }

            EditorUtility.CopySerialized(sourceClip, destinationClip);
            destinationClip.name = Path.GetFileNameWithoutExtension(clipPath);
            EditorUtility.SetDirty(destinationClip);
            return destinationClip;
        }

        private static AnimatorController UpsertAnimatorController(AnimatorController sourceController, string controllerPath)
        {
            AnimatorController destinationController = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (destinationController == null)
            {
                AnimatorController controllerCopy = Object.Instantiate(sourceController);
                AssetDatabase.CreateAsset(controllerCopy, controllerPath);
                destinationController = controllerCopy;
            }
            else
            {
                EditorUtility.CopySerialized(sourceController, destinationController);
            }

            destinationController.name = Path.GetFileNameWithoutExtension(controllerPath);
            EditorUtility.SetDirty(destinationController);
            return destinationController;
        }

        private static void RebuildControllerWithExportedClips(
            AnimatorController exportedController,
            AnimatorController sourceController,
            AnimationClip[] sourceClips,
            Dictionary<string, AnimationClip> exportedClipByName)
        {
            List<AnimationClip> orderedExportedClips = new List<AnimationClip>();

            foreach (AnimationClip sourceClip in sourceClips)
            {
                if (sourceClip == null)
                {
                    continue;
                }

                if (!exportedClipByName.TryGetValue(sourceClip.name, out AnimationClip exportedClip))
                {
                    continue;
                }

                if (!orderedExportedClips.Contains(exportedClip))
                {
                    orderedExportedClips.Add(exportedClip);
                }
            }

            if (orderedExportedClips.Count == 0)
            {
                return;
            }

            string defaultClipName = GetDefaultClipName(sourceController);
            AnimatorStateMachine stateMachine = GetOrCreateLocalBaseStateMachine(exportedController);
            ClearStateMachine(stateMachine);

            AnimatorState defaultState = null;
            foreach (AnimationClip exportedClip in orderedExportedClips)
            {
                AnimatorState state = stateMachine.AddState(exportedClip.name);
                state.motion = exportedClip;

                if (defaultState == null)
                {
                    defaultState = state;
                }

                if (!string.IsNullOrEmpty(defaultClipName) && exportedClip.name == defaultClipName)
                {
                    defaultState = state;
                }
            }

            if (defaultState != null)
            {
                stateMachine.defaultState = defaultState;
            }

            EditorUtility.SetDirty(stateMachine);
        }

        private static string GetDefaultClipName(AnimatorController sourceController)
        {
            AnimatorControllerLayer[] layers = sourceController.layers;
            if (layers == null || layers.Length == 0)
            {
                return string.Empty;
            }

            AnimatorState defaultState = layers[0].stateMachine?.defaultState;
            if (defaultState == null)
            {
                return string.Empty;
            }

            if (defaultState.motion is AnimationClip defaultClip)
            {
                return defaultClip.name;
            }

            return string.Empty;
        }

        private static AnimatorStateMachine GetOrCreateLocalBaseStateMachine(AnimatorController controller)
        {
            AnimatorControllerLayer[] layers = controller.layers;
            AnimatorStateMachine stateMachine = null;
            string controllerPath = AssetDatabase.GetAssetPath(controller);

            if (layers != null && layers.Length > 0)
            {
                stateMachine = layers[0].stateMachine;
                if (stateMachine != null && AssetDatabase.GetAssetPath(stateMachine) == controllerPath)
                {
                    AnimatorControllerLayer layer = layers[0];
                    layer.name = AsepriteUiAnimationConstants.BASE_LAYER_NAME;
                    layer.defaultWeight = 1f;
                    layer.stateMachine = stateMachine;
                    controller.layers = new[] { layer };
                    return stateMachine;
                }
            }

            stateMachine = new AnimatorStateMachine { name = AsepriteUiAnimationConstants.BASE_LAYER_NAME };
            AssetDatabase.AddObjectToAsset(stateMachine, controller);

            AnimatorControllerLayer newLayer = new AnimatorControllerLayer
            {
                name = AsepriteUiAnimationConstants.BASE_LAYER_NAME,
                stateMachine = stateMachine,
                defaultWeight = 1f,
                blendingMode = AnimatorLayerBlendingMode.Override,
                iKPass = false,
                syncedLayerIndex = -1,
                syncedLayerAffectsTiming = false
            };

            controller.layers = new[] { newLayer };
            return stateMachine;
        }

        private static void ClearStateMachine(AnimatorStateMachine stateMachine)
        {
            ChildAnimatorStateMachine[] childMachines = stateMachine.stateMachines;
            for (int i = childMachines.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveStateMachine(childMachines[i].stateMachine);
            }

            ChildAnimatorState[] states = stateMachine.states;
            for (int i = states.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveState(states[i].state);
            }

            AnimatorStateTransition[] anyStateTransitions = stateMachine.anyStateTransitions;
            for (int i = anyStateTransitions.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveAnyStateTransition(anyStateTransitions[i]);
            }

            AnimatorTransition[] entryTransitions = stateMachine.entryTransitions;
            for (int i = entryTransitions.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveEntryTransition(entryTransitions[i]);
            }

            stateMachine.defaultState = null;
        }

        private static string EnsureOutputFolder(string asepritePath)
        {
            string directoryPath = Path.GetDirectoryName(asepritePath)?.Replace('\\', '/') ?? AsepriteUiAnimationConstants.ASSETS_ROOT_PATH;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(asepritePath);
            string folderPath = $"{directoryPath}/{fileNameWithoutExtension}{AsepriteUiAnimationConstants.EXPORTED_ASSETS_SUFFIX}";
            EnsureFolderExists(folderPath);
            return folderPath;
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] segments = folderPath.Split('/');
            if (segments.Length == 0)
            {
                return;
            }

            string currentPath = segments[0];
            for (int i = 1; i < segments.Length; i++)
            {
                string nextPath = $"{currentPath}/{segments[i]}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, segments[i]);
                }

                currentPath = nextPath;
            }
        }

        private static bool HasAsepriteSibling(string path, out string asepritePath)
        {
            string directoryPath = Path.GetDirectoryName(path);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);

            foreach (string asepriteExtension in AsepriteUiAnimationConstants.ASEPRITE_EXTENSIONS)
            {
                string candidatePath = Path.Combine(directoryPath ?? string.Empty, fileNameWithoutExtension + asepriteExtension).Replace('\\', '/');
                if (AssetDatabase.LoadAssetAtPath<Object>(candidatePath) != null)
                {
                    asepritePath = candidatePath;
                    return true;
                }
            }

            asepritePath = string.Empty;
            return false;
        }

        private static bool ConvertSpriteRendererBindingsToImageBindings(AnimationClip clip)
        {
            bool changed = false;
            EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);

            foreach (EditorCurveBinding binding in bindings)
            {
                if (binding.type != typeof(SpriteRenderer))
                {
                    continue;
                }

                ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                EditorCurveBinding imageBinding = binding;
                imageBinding.type = typeof(Image);

                AnimationUtility.SetObjectReferenceCurve(clip, imageBinding, keyframes);
                AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(clip);
            }

            return changed;
        }
    }
}
