using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using CommunityToolkit.Mvvm.DependencyInjection;


// new
using System.Threading.Tasks;
using Windows.Storage;

using System.Globalization;
using CommunityToolkit.Mvvm.Messaging;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TitleBar_Test.Helper;
using TitleBar_Test.Data;
using TitleBar_Test.DesktopWap.DataModel;
using TitleBar_Test.Common;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TitleBar_Test
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        /// 

        public static IServiceProvider ServiceProvider { get; private set; }
        private static MainWindow startupWindow;
        //Window m_window;

        // Get the initial window created for this app
        // On UWP, this is simply Window.Current
        // On Desktop, multiple Windows may be created, and the StartupWindow may have already
        // been closed.
        public static MainWindow StartupWindow
        {
            get
            {
                return startupWindow;
            }
        }


        public App()
        {
            this.InitializeComponent();

            ServiceProvider = ConfigureServices();
            Ioc.Default.ConfigureServices(ServiceProvider);

        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            //m_window = new Window();
            //m_window.Activate();

            startupWindow = WindowHelper.CreateWindow();
            startupWindow.ExtendsContentIntoTitleBar = true;

            //startupWindow = new Window();
            //StartupWindow.Activate();

            //startupWindow = WindowHelper.CreateWindow();
            //startupWindow.ExtendsContentIntoTitleBar = true;

            EnsureWindow();
        }

       // private Window m_window;

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<Main_VM>();

            var svcs = services.BuildServiceProvider();

            return svcs;
        }



        // From WinUIGAllery
        public static TEnum GetEnum<TEnum>(string text) where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum)
            {
                throw new InvalidOperationException("Generic parameter 'TEnum' must be an enum.");
            }
            return (TEnum)Enum.Parse(typeof(TEnum), text);
        }

        private async void EnsureWindow(IActivatedEventArgs args = null)
        {
            // No matter what our destination is, we're going to need control data loaded - let's knock that out now.
            // We'll never need to do this again.
            await ControlInfoDataSource.Instance.GetGroupsAsync();
            await IconsDataSource.Instance.LoadIcons();

            Frame rootFrame = GetRootFrame();

            ThemeHelper.Initialize();

            //(StartupWindow.Content as FrameworkElement).RequestedTheme = RootTheme;// Set Theme
            //StartupWindow = SetBackdrop(App.m_window) as Window;// Set Backdrop (IsMica, IsAcrylic)

            WindowHelper.ActiveWindows.Add(StartupWindow);
            StartupWindow.Activate();

            //Type targetPageType = typeof(NewControlsPage);
            //string targetPageArguments = string.Empty;

            //if (args != null)
            //{
            //    if (args.Kind == ActivationKind.Launch)
            //    {
            //        if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            //        {
            //            try
            //            {
            //                await SuspensionManager.RestoreAsync();
            //            }
            //            catch (SuspensionManagerException)
            //            {
            //                //Something went wrong restoring state.
            //                //Assume there is no state and continue
            //            }
            //        }

            //        targetPageArguments = ((Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)args).Arguments;
            //    }
            //    else if (args.Kind == ActivationKind.Protocol)
            //    {
            //        Match match;

            //        string targetId = string.Empty;

            //        switch (((ProtocolActivatedEventArgs)args).Uri?.AbsoluteUri)
            //        {
            //            case string s when IsMatching(s, "(/*)category/(.*)"):
            //                targetId = match.Groups[2]?.ToString();
            //                if (targetId == "AllControls")
            //                {
            //                    targetPageType = typeof(AllControlsPage);
            //                }
            //                else if (targetId == "NewControls")
            //                {
            //                    targetPageType = typeof(NewControlsPage);
            //                }
            //                else if (ControlInfoDataSource.Instance.Groups.Any(g => g.UniqueId == targetId))
            //                {
            //                    targetPageType = typeof(SectionPage);
            //                }
            //                break;

            //            case string s when IsMatching(s, "(/*)item/(.*)"):
            //                targetId = match.Groups[2]?.ToString();
            //                if (ControlInfoDataSource.Instance.Groups.Any(g => g.Items.Any(i => i.UniqueId == targetId)))
            //                {
            //                    targetPageType = typeof(ItemPage);
            //                }
            //                break;
            //        }

            //        targetPageArguments = targetId;

            //        bool IsMatching(string parent, string expression)
            //        {
            //            match = Regex.Match(parent, expression);
            //            return match.Success;
            //        }
            //    }
            //}

            //NavigationRootPage rootPage = StartupWindow.Content as NavigationRootPage;
            //rootPage.Navigate(targetPageType, targetPageArguments);

            //if (targetPageType == typeof(NewControlsPage))
            //{
            //    ((Microsoft.UI.Xaml.Controls.NavigationViewItem)((NavigationRootPage)App.StartupWindow.Content).NavigationView.MenuItems[0]).IsSelected = true;
            //}
            //else if (targetPageType == typeof(ItemPage))
            //{
            //    NavigationRootPage.GetForElement(this).EnsureNavigationSelection(targetPageArguments);
            //}

            // Ensure the current window is active

        }

        public Frame GetRootFrame()
        {
            //Frame rootFrame ;
            Frame rootFrame = null;

            //NavigationRootPage rootPage = StartupWindow.Content as NavigationRootPage;
            //if (rootPage == null)
            //{
            //    rootPage = new NavigationRootPage();
            //    rootFrame = (Frame)rootPage.FindName("rootFrame");
            //    if (rootFrame == null)
            //    {
            //        throw new Exception("Root frame not found");
            //    }
            //    SuspensionManager.RegisterFrame(rootFrame, "AppFrame");
            //    rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
            //    rootFrame.NavigationFailed += OnNavigationFailed;

            //    StartupWindow.Content = rootPage;
            //}
            //else
            //{
            //    rootFrame = (Frame)rootPage.FindName("rootFrame");
            //}

            return rootFrame;
        }

    }
}
