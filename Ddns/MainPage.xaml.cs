using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Ddns
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                hostNameBox.Text = Settings.HostName ?? string.Empty;
                keyBox.Password = Settings.Key ?? string.Empty;
                autoUpdateCheckBox.IsChecked = Settings.BackgroundUpdateEnabled;

                if (Settings.HostName != null)
                    await RefreshStatusAsync();

            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                (sender as Button).IsEnabled = false;

                Settings.HostName = hostNameBox.Text;
                Settings.Key = keyBox.Password;
                Settings.BackgroundUpdateEnabled = autoUpdateCheckBox.IsChecked == true;

                await SetBackgroundTask(Settings.BackgroundUpdateEnabled);
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
            finally
            {
                (sender as Button).IsEnabled = true;
            }
        }

        private async void RefreshStatusButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                (sender as Button).IsEnabled = false;

                if (Settings.HostName == null)
                {
                    ShowDialog("You need to set host name first.");
                    return;
                }

                await RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
            finally
            {
                (sender as Button).IsEnabled = true;
            }
        }

        private async void ManualUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                manualUpdateButton.IsEnabled = false;
                manualUpdateResultBlock.Text = string.Empty;

                if (Settings.HostName == null || Settings.Key == null)
                {
                    ShowDialog("You need to set host name and key first.");
                    return;
                }

                var localIps = IPHelper.GetPublicIps();

                string ipv4 = localIps.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();
                if (ipv4 != null)
                {
                    var (ok, message) = await HeNetClient.UpdateIpAsync(Settings.HostName, Settings.Key, ipv4);
                    if (ok)
                        manualUpdateResultBlock.Text += string.Format("IPv4 DNS record is set to {0}.\n", ipv4);
                    else
                        manualUpdateResultBlock.Text += string.Format("IPv4 DNS update error. Message: {0}", message);
                }

                string ipv6 = localIps.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)?.ToString();
                if (ipv6 != null)
                {
                    var (ok, message) = await HeNetClient.UpdateIpAsync(Settings.HostName, Settings.Key, ipv6);
                    if (ok)
                        manualUpdateResultBlock.Text += string.Format("IPv6 DNS record is set to {0}.\n", ipv6);
                    else
                        manualUpdateResultBlock.Text += string.Format("IPv6 DNS update error. Message: {0}", message);
                }

                await RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
            finally
            {
                manualUpdateButton.IsEnabled = true;
            }
        }

        private void ShowException(Exception ex)
        {
            ContentDialog dialog = new ContentDialog
            {
                Content = ex.Message,
                CloseButtonText = "Dismiss"
            };
            var r = dialog.ShowAsync();
        }

        private void ShowDialog(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Content = message,
                CloseButtonText = "Dismiss"
            };
            var r = dialog.ShowAsync();
        }

        private async Task RefreshStatusAsync()
        {
            hostBlock.Text = Settings.HostName;

            var localIps = IPHelper.GetPublicIps();
            localIpBlock.Text = string.Join('\n', localIps.Select(x => x.ToString()));

            try
            {
                var remoteDnsResults = await Dns.GetHostAddressesAsync(Settings.HostName);
                dnsResultBlock.Text = string.Join('\n', remoteDnsResults.Select(x => x.ToString()));
            }
            catch (Exception ex)
            {
                dnsResultBlock.Text = ex.Message;
            }
        }

        private async Task SetBackgroundTask(bool enable)
        {
            BackgroundExecutionManager.RemoveAccess();
            if (enable)
            {
                var backgroundAccess = await BackgroundExecutionManager.RequestAccessAsync();
                if (backgroundAccess == BackgroundAccessStatus.DeniedByUser)
                {
                    ShowDialog("You have disabled this app to run in background. Once you enabled background running, auto updating will start working.");
                }

                foreach(var item in BackgroundTaskRegistration.AllTasks
                    .Where(x => x.Value.Name == BackgroundTaskNames.NetworkStateChanged)
                    .Skip(1))
                {
                    item.Value.Unregister(false);
                }

                if (!BackgroundTaskRegistration.AllTasks.Any(x => x.Value.Name == BackgroundTaskNames.NetworkStateChanged))
                {
                    SystemTrigger networkTrigger = new SystemTrigger(SystemTriggerType.NetworkStateChange, false);

                    var builder = new BackgroundTaskBuilder
                    {
                        Name = BackgroundTaskNames.NetworkStateChanged
                    };
                    builder.SetTrigger(networkTrigger);
                    BackgroundTaskRegistration task = builder.Register();
                }
            }
            else
            {
                foreach (var item in BackgroundTaskRegistration.AllTasks.Where(x => x.Value.Name == BackgroundTaskNames.NetworkStateChanged))
                {
                    item.Value.Unregister(false);
                }
            }
        }
    }
}
