using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Ddns
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);

            var deferral = args.TaskInstance.GetDeferral();

            try
            {
                if (args.TaskInstance.Task.Name == BackgroundTaskNames.NetworkStateChanged)
                {
                    var localIps = IPHelper.GetPublicIps();
                    StringBuilder toastText = new StringBuilder();

                    string ipv4 = localIps.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();
                    if (ipv4 != null)
                    {
                        var (ok, message) = await HeNetClient.UpdateIpAsync(Settings.HostName, Settings.Key, ipv4);
                        if (ok)
                            toastText.Append(string.Format("IPv4 DNS record is set to {0}.\n", ipv4));
                        else
                            toastText.Append(string.Format("IPv4 DNS update error. Message: {0}", message));
                    }

                    string ipv6 = localIps.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)?.ToString();
                    if (ipv6 != null)
                    {
                        var (ok, message) = await HeNetClient.UpdateIpAsync(Settings.HostName, Settings.Key, ipv6);
                        if (ok)
                            toastText.Append(string.Format("IPv6 DNS record is set to {0}.\n", ipv6));
                        else
                            toastText.Append(string.Format("IPv6 DNS update error. Message: {0}", message));
                    }

                    if (toastText.ToString().EndsWith('\n'))
                        toastText.Remove(toastText.Length - 1, 1);
                    if (toastText.Length == 0)
                        toastText.Append("No routable IP found. DNS is not updated.");

                    var toastContent = new ToastContent()
                    {
                        Visual = new ToastVisual()
                        {
                            BindingGeneric = new ToastBindingGeneric()
                            {
                                Children =
                            {
                                new AdaptiveText(){Text = "Network changed" },
                                new AdaptiveText(){Text = toastText.ToString() }
                            }
                            }
                        }
                    };

                    var toastNotification = new ToastNotification(toastContent.GetXml())
                    {
                        Group = NotificationGroups.NetworkChanged
                    };
                    ToastNotificationManager.History.RemoveGroup(NotificationGroups.NetworkChanged);
                    ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
                }
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
