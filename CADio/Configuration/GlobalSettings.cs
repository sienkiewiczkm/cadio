using CADio.ViewModels;

namespace CADio.Configuration
{
    public static class GlobalSettings
    {
        public static bool FreezeDrawing = false;
        public static QualitySettingsViewModel QualitySettingsViewModel { get; set; }
    }
}