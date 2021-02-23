using VisualMute.Properties;

namespace VisualMute.Workers
{
    public static class SettingsHelper
    {
        public static void UpdateApplicationSetting(string propertyName, object value)
        {
            Settings.Default[propertyName] = value;
            Settings.Default.Save();
            Settings.Default.Reload();
        }

        // private static T LoadInternalSetting<T>(string propertyName)
        // {
        //     return (T) Settings.Default[propertyName];
        // }
    }
}