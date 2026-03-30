using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Editor
{
    public class AsepriteUiAnimationBindingPostprocessor : AssetPostprocessor
    {
        private const string MENU_PATH = "Tools/Aseprite/Convert Selected Clips to UI.Image Bindings";
        private static readonly string[] ASEPRITE_EXTENSIONS = { ".ase", ".aseprite" };

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths,
            bool didDomainReload)
        {
            HashSet<AnimationClip> clipsToConvert = CollectCandidateClips(importedAssets);
            ConvertAndSave(clipsToConvert);
        }

        [MenuItem(MENU_PATH)]
        private static void ConvertSelectedClips()
        {
            HashSet<AnimationClip> clipsToConvert = new HashSet<AnimationClip>();

            foreach (Object selectedObject in Selection.objects)
            {
                if (selectedObject is AnimationClip clip)
                {
                    clipsToConvert.Add(clip);
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(selectedObject);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                AddClipsAtPath(path, clipsToConvert);
            }

            int converted = ConvertAndSave(clipsToConvert);
            Debug.Log($"Aseprite UI binding conversion finished. Converted {converted} clip(s).");
        }

        [MenuItem(MENU_PATH, true)]
        private static bool CanConvertSelectedClips()
        {
            return Selection.objects != null && Selection.objects.Length > 0;
        }

        private static HashSet<AnimationClip> CollectCandidateClips(string[] importedAssets)
        {
            HashSet<AnimationClip> clips = new HashSet<AnimationClip>();

            foreach (string path in importedAssets)
            {
                if (IsAsepriteSource(path))
                {
                    AddClipsAtPath(path, clips);
                    continue;
                }

                if (Path.GetExtension(path).ToLowerInvariant() == ".anim" && HasAsepriteSibling(path))
                {
                    AddClipsAtPath(path, clips);
                    continue;
                }

                if (Path.GetExtension(path).ToLowerInvariant() == ".prefab" && HasAsepriteSibling(path))
                {
                    AddClipDependencies(path, clips);
                }
            }

            return clips;
        }

        private static void AddClipsAtPath(string path, HashSet<AnimationClip> clips)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (Object asset in assets)
            {
                if (asset is AnimationClip clip)
                {
                    clips.Add(clip);
                }
            }
        }

        private static void AddClipDependencies(string path, HashSet<AnimationClip> clips)
        {
            string[] dependencyPaths = AssetDatabase.GetDependencies(path, false);

            foreach (string dependencyPath in dependencyPaths)
            {
                if (Path.GetExtension(dependencyPath).ToLowerInvariant() != ".anim")
                {
                    continue;
                }

                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(dependencyPath);
                if (clip != null)
                {
                    clips.Add(clip);
                }
            }
        }

        private static bool IsAsepriteSource(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();

            foreach (string asepriteExtension in ASEPRITE_EXTENSIONS)
            {
                if (extension == asepriteExtension)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAsepriteSibling(string path)
        {
            string directoryPath = Path.GetDirectoryName(path);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);

            foreach (string asepriteExtension in ASEPRITE_EXTENSIONS)
            {
                string asepritePath = Path.Combine(directoryPath ?? string.Empty, fileNameWithoutExtension + asepriteExtension).Replace('\\', '/');
                if (AssetDatabase.LoadAssetAtPath<Object>(asepritePath) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static int ConvertAndSave(HashSet<AnimationClip> clips)
        {
            int convertedCount = 0;

            foreach (AnimationClip clip in clips)
            {
                if (clip == null)
                {
                    continue;
                }

                if (ConvertSpriteRendererBindingsToImageBindings(clip))
                {
                    convertedCount++;
                }
            }

            if (convertedCount > 0)
            {
                AssetDatabase.SaveAssets();
            }

            return convertedCount;
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
