namespace Assets.Constants
{
    public static class AsepriteUiAnimationConstants
    {
        public const string EXPORT_MENU_PATH = "Tools/Aseprite/Export UI Animation Assets From Selected Aseprite";
        public const string EXPORTED_ASSETS_SUFFIX = "_UIAnimation";
        public const string CLIP_NAME_SUFFIX = "_Clip";
        public const string CONTROLLER_NAME_SUFFIX = "_AC";
        public const string AUTO_EXPORT_PREFS_KEY_PREFIX = "Assets.Editor.AsepriteUiAnimationBindingPostprocessor.AutoExported.";
        public const string AUTO_EXPORT_GUARD_KEY = "Assets.Editor.AsepriteUiAnimationBindingPostprocessor.AutoExportInProgress";
        public const string BASE_LAYER_NAME = "Base Layer";
        public const string ASSETS_ROOT_PATH = "Assets";
        public static readonly string[] ASEPRITE_EXTENSIONS = { ".ase", ".aseprite" };
    }
}
