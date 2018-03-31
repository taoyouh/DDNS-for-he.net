using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Ddns
{
    static class Settings
    {
        private const string HostNameSetting = "hostname";

        public static string HostName
        {
            get
            {
                var settings = ApplicationData.Current.LocalSettings.Values;
                if (settings.ContainsKey(HostNameSetting))
                    return (string)settings[HostNameSetting];
                else
                    return null;
            }
            set
            {
                var settings = ApplicationData.Current.LocalSettings.Values;
                settings[HostNameSetting] = value;
            }
        }

        private const string KeySetting = "key";

        public static string Key
        {
            get
            {
                var settings = ApplicationData.Current.LocalSettings.Values;
                if (settings.ContainsKey(KeySetting))
                    return (string)settings[KeySetting];
                else
                    return null;
            }
            set
            {
                var settings = ApplicationData.Current.LocalSettings.Values;
                settings[KeySetting] = value;
            }
        }

        private const string BackgroundUpdateEnabledSetting = "BackgroundUpdate";

        public static bool BackgroundUpdateEnabled
        {
            get
            {
                var settings = ApplicationData.Current.LocalSettings.Values;
                if (settings.ContainsKey(BackgroundUpdateEnabledSetting))
                    return (bool)settings[BackgroundUpdateEnabledSetting];
                else
                    return false;
            }
            set
            {
                var settings = ApplicationData.Current.LocalSettings.Values;
                settings[BackgroundUpdateEnabledSetting] = value;
            }
        }
    }
}
