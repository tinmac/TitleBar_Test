using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;

using Microsoft.UI;           // Needed for WindowId.
using Microsoft.UI.Windowing; // Needed for AppWindow.
using WinRT.Interop;          // Needed for XAML/HWND interop.

using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.DependencyInjection;
using WinRT;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Controls;
using TitleBar_Test.Helper;
using TitleBar_Test.DesktopWap.Helper;

namespace TitleBar_Test
{
    // Used for Theme Stuff
    class WindowsSystemDispatcherQueueHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        struct DispatcherQueueOptions
        {
            internal int dwSize;
            internal int threadType;
            internal int apartmentType;
        }

        [DllImport("CoreMessaging.dll")]
        private static unsafe extern int CreateDispatcherQueueController(DispatcherQueueOptions options, IntPtr* instance);

        IntPtr m_dispatcherQueueController = IntPtr.Zero;
        public void EnsureWindowsSystemDispatcherQueueController()
        {
            if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
            {
                // one already exists, so we'll just use it.
                return;
            }

            if (m_dispatcherQueueController == IntPtr.Zero)
            {
                DispatcherQueueOptions options;
                options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
                options.threadType = 2;    // DQTYPE_THREAD_CURRENT
                options.apartmentType = 2; // DQTAT_COM_STA

                unsafe
                {
                    IntPtr dispatcherQueueController;
                    CreateDispatcherQueueController(options, &dispatcherQueueController);
                    m_dispatcherQueueController = dispatcherQueueController;
                }
            }
        }
    }

    /// <summary>
    /// Title Bar Works a treat!!
    /// 
    /// Code taken from MS learn : https://learn.microsoft.com/en-us/windows/apps/develop/title-bar?tabs=wasdk
    /// 
    /// </summary>


    public sealed partial class MainWindow : Window
    {
        private AppWindow m_AppWindow;
        private OverlappedPresenter _presenter;

        public Main_VM ViewModel { get; }


        // THEME STUFF
        WindowsSystemDispatcherQueueHelper m_wsdqHelper;
        BackdropType m_currentBackdrop;
        MicaController m_micaController;
        DesktopAcrylicController m_acrylicController;
        SystemBackdropConfiguration m_configurationSource;


        public MainWindow()
        {
            this.InitializeComponent();

            ViewModel = Ioc.Default.GetRequiredService<Main_VM>();


            #region THEME STUFF

            //  ((FrameworkElement)this.Content).RequestedTheme = TitleBar_Test.Helper.ThemeHelper.RootTheme;
            ExtendsContentIntoTitleBar = true;
            // SetTitleBar(titleBar);
            m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

            SetBackdrop(BackdropType.Mica);

            #endregion



            #region TITLE BAR STUFF

            GetAppWindowForCurrentWindow();

            // Check to see if customization is supported.
            // The method returns true on Windows 10 since Windows App SDK 1.2, and on all versions of
            // Windows App SDK on Windows 11.
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                // this is for System titlebar NOT the custom (has no effect)
                //m_AppWindow.TitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;

                var titleBar = m_AppWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;

                AppTitleBar.Loaded += AppTitleBar_Loaded;
                AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
            }
            else
            {
                // In the case that title bar customization is not supported, hide the custom title bar
                // element.
                AppTitleBar.Visibility = Visibility.Collapsed;

                // Show alternative UI for any functionality in
                // the title bar, such as search.
            }


            //SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop()
            //{
            //    Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base
            //};


            #endregion

            // SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();

        }

        #region THEME STUFF

        public enum BackdropType
        {
            Mica,
            MicaAlt,
            DesktopAcrylicBase,
            DesktopAcrylicThin,
            DefaultColor,
        }


        public void SetBackdrop(BackdropType type)
        {
            // Reset to default color. If the requested type is supported, we'll update to that.
            // Note: This sample completely removes any previous controller to reset to the default
            //       state. This is done so this sample can show what is expected to be the most
            //       common pattern of an app simply choosing one controller type which it sets at
            //       startup.

            //       If an app wants to toggle between Mica and Acrylic it could simply
            //       call RemoveSystemBackdropTarget() on the old controller and then setup the new
            //       controller, reusing any existing m_configurationSource and Activated/Closed
            //       event handlers.

            m_currentBackdrop = BackdropType.DefaultColor;

            ViewModel.SelectedTheme = "None (default theme color)";

            ViewModel.ChangeStatus = "";

            if (m_micaController != null)
            {
                m_micaController.Dispose();
                m_micaController = null;
            }

            if (m_acrylicController != null)
            {
                m_acrylicController.Dispose();
                m_acrylicController = null;
            }
            this.Activated -= Window_Activated;
            this.Closed -= Window_Closed;

            ((FrameworkElement)this.Content).ActualThemeChanged -= Window_ThemeChanged;

            m_configurationSource = null;

            if (type == BackdropType.Mica)
            {
                if (TrySetMicaBackdrop(false))
                {
                    ViewModel.SelectedTheme = "Custom Mica";
                    m_currentBackdrop = type;
                }
                else
                {
                    // Mica isn't supported. Try Acrylic.
                    type = BackdropType.DesktopAcrylicBase;
                    ViewModel.ChangeStatus += "  Mica isn't supported. Trying Acrylic.";
                }
            }

            if (type == BackdropType.MicaAlt)
            {
                if (TrySetMicaBackdrop(true))
                {
                    ViewModel.SelectedTheme = "Custom MicaAlt";
                    m_currentBackdrop = type;
                }
                else
                {
                    // MicaAlt isn't supported. Try Acrylic.
                    type = BackdropType.DesktopAcrylicBase;
                    ViewModel.ChangeStatus += "  MicaAlt isn't supported. Trying Acrylic.";
                }
            }

            if (type == BackdropType.DesktopAcrylicBase)
            {
                if (TrySetAcrylicBackdrop(false))
                {
                    ViewModel.SelectedTheme = "Custom Acrylic (Base)";
                    m_currentBackdrop = type;
                }
                else
                {
                    // Acrylic isn't supported, so take the next option, which is DefaultColor, which is already set.
                    ViewModel.ChangeStatus += "  Acrylic Base isn't supported. Switching to default color.";
                }
            }
            if (type == BackdropType.DesktopAcrylicThin)
            {
                if (TrySetAcrylicBackdrop(true))
                {
                    ViewModel.SelectedTheme = "Custom Acrylic (Thin)";
                    m_currentBackdrop = type;
                }
                else
                {
                    // Acrylic isn't supported, so take the next option, which is DefaultColor, which is already set.
                    ViewModel.ChangeStatus += "  Acrylic Thin isn't supported. Switching to default color.";
                }
            }

            // announce visual change to automation
            // UIHelper.AnnounceActionForAccessibility(btnChangeBackdrop, $"Background changed to {tbCurrentBackdrop.Text}", "BackgroundChangedNotificationActivityId");
        }

        // Setting Mica or Acrylic (the two methods below) is the only way to set Dark/Light theme too,
        // because of the Activated & Closed events that are subscribed.
        // To set Light/Dark use the method employed my WinUIGallery app, ie use ThemeHelper & TitleBarHelper
        // Which involves a ConboBox which calls into these two helpers
        bool TrySetMicaBackdrop(bool useMicaAlt)
        {
            if (MicaController.IsSupported())
            {
                // Hooking up the policy object.
                // https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.systembackdrops.systembackdropconfiguration?view=windows-app-sdk-1.4#remarks
                m_configurationSource = new SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;
                ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

                // Initial configuration state.
                m_configurationSource.IsInputActive = true;// the system backdrop should consider the current window as having input focus
                SetConfigurationSourceTheme();

                m_micaController = new MicaController();

                m_micaController.Kind = useMicaAlt ? MicaKind.BaseAlt : MicaKind.Base;

                // Enable the system backdrop.
                // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
                m_micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
                return true; // Succeeded.
            }

            return false; // Mica is not supported on this system.
        }

        bool TrySetAcrylicBackdrop(bool useAcrylicThin)
        {
            if (DesktopAcrylicController.IsSupported())
            {
                // Hooking up the policy object.
                m_configurationSource = new SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;
                ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

                // Initial configuration state.
                m_configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();

                m_acrylicController = new DesktopAcrylicController();

                m_acrylicController.Kind = useAcrylicThin ? DesktopAcrylicKind.Thin : DesktopAcrylicKind.Base;

                // Enable the system backdrop.
                // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
                m_acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
                return true; // Succeeded.
            }

            return false; // Acrylic is not supported on this system
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }



        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
            // use this closed window.
            if (m_micaController != null)
            {
                m_micaController.Dispose();
                m_micaController = null;
            }
            if (m_acrylicController != null)
            {
                m_acrylicController.Dispose();
                m_acrylicController = null;
            }
            this.Activated -= Window_Activated;
            m_configurationSource = null;
        }


        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (m_configurationSource != null)
            {
                // SetConfigurationSourceTheme();
            }
        }

        private void SetConfigurationSourceTheme()
        {
            switch (((FrameworkElement)this.Content).ActualTheme)
            {
                case ElementTheme.Dark: m_configurationSource.Theme = SystemBackdropTheme.Dark; break;
                case ElementTheme.Light: m_configurationSource.Theme = SystemBackdropTheme.Light; break;
                case ElementTheme.Default: m_configurationSource.Theme = SystemBackdropTheme.Default; break;
            }
        }


        // Ligt/Dark CBO
        private void themeMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTheme = ((ComboBoxItem)themeMode.SelectedItem)?.Tag?.ToString();
            //var window = WindowHelper.GetWindowForElement(this);
            var window = this;
            string color;
            if (selectedTheme != null)
            {
                // Set the theme
                var the_enum = App.GetEnum<ElementTheme>(selectedTheme);
                ThemeHelper.RootTheme = App.GetEnum<ElementTheme>(selectedTheme);
               
                // Set Colours
                if (selectedTheme == "Dark")
                {
                    TitleBarHelper.SetCaptionButtonColors(window, Colors.White);
                    color = selectedTheme;
                }
                else if (selectedTheme == "Light")
                {
                    TitleBarHelper.SetCaptionButtonColors(window, Colors.Black);
                    color = selectedTheme;
                }
                else
                {
                    color = TitleBarHelper.ApplySystemThemeToCaptionButtons(window) == Colors.White ? "Dark" : "Light";
                }
            }
        }


        // CHANGE BackDrop  event handlers


        private void btn_open_mica_Click(object sender, RoutedEventArgs e)
        {
            SetBackdrop(BackdropType.Mica);
        }

        private void btn_open_mica_alt_Click(object sender, RoutedEventArgs e)
        {
            SetBackdrop(BackdropType.MicaAlt);
        }


        private void btn_open_acrylic_Click(object sender, RoutedEventArgs e)
        {
            SetBackdrop(BackdropType.DesktopAcrylicBase);
        }

        private void btn_open_acrylic_thin_Click(object sender, RoutedEventArgs e)
        {
            SetBackdrop(BackdropType.DesktopAcrylicThin);
        }


        private void btn_open_plain_Click(object sender, RoutedEventArgs e)
        {
            SetBackdrop(BackdropType.DefaultColor);
        }


        #endregion


        #region TITLE BAR STUFF

        private void GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            m_AppWindow = AppWindow.GetFromWindowId(wndId);
            _presenter = m_AppWindow.Presenter as OverlappedPresenter;
        }

        private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            // Check to see if customization is supported.
            // The method returns true on Windows 10 since Windows App SDK 1.2, and on all versions of
            // Windows App SDK on Windows 11.
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                SetDragRegionForCustomTitleBar(m_AppWindow);
            }
        }

        private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Check to see if customization is supported.
            // The method returns true on Windows 10 since Windows App SDK 1.2, and on all versions of
            // Windows App SDK on Windows 11.
            if (AppWindowTitleBar.IsCustomizationSupported()
                && m_AppWindow.TitleBar.ExtendsContentIntoTitleBar)
            {
                // Update drag region if the size of the title bar changes.
                SetDragRegionForCustomTitleBar(m_AppWindow);
            }
        }

        [DllImport("Shcore.dll", SetLastError = true)]
        internal static extern int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY);

        internal enum Monitor_DPI_Type : int
        {
            MDT_Effective_DPI = 0,
            MDT_Angular_DPI = 1,
            MDT_Raw_DPI = 2,
            MDT_Default = MDT_Effective_DPI
        }

        private double GetScaleAdjustment()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            DisplayArea displayArea = DisplayArea.GetFromWindowId(wndId, DisplayAreaFallback.Primary);
            IntPtr hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

            // Get DPI.
            int result = GetDpiForMonitor(hMonitor, Monitor_DPI_Type.MDT_Default, out uint dpiX, out uint _);
            if (result != 0)
            {
                throw new Exception("Could not get DPI for monitor.");
            }

            uint scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
            return scaleFactorPercent / 100.0;
        }

        private void SetDragRegionForCustomTitleBar(AppWindow appWindow)
        {
            // Check to see if customization is supported.
            // The method returns true on Windows 10 since Windows App SDK 1.2, and on all versions of
            // Windows App SDK on Windows 11.
            if (AppWindowTitleBar.IsCustomizationSupported()
                && appWindow.TitleBar.ExtendsContentIntoTitleBar)
            {
                double scaleAdjustment = GetScaleAdjustment();

                RightPaddingColumn.Width = new GridLength(appWindow.TitleBar.RightInset / scaleAdjustment);
                LeftPaddingColumn.Width = new GridLength(appWindow.TitleBar.LeftInset / scaleAdjustment);

                List<Windows.Graphics.RectInt32> dragRectsList = new();

                Windows.Graphics.RectInt32 dragRectL;
                dragRectL.X = (int)((LeftPaddingColumn.ActualWidth) * scaleAdjustment);
                dragRectL.Y = 0;
                dragRectL.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
                dragRectL.Width = (int)((IconColumn.ActualWidth
                                        + TitleColumn.ActualWidth
                                        + LeftDragColumn.ActualWidth) * scaleAdjustment);
                dragRectsList.Add(dragRectL);

                Windows.Graphics.RectInt32 dragRectR;
                dragRectR.X = (int)((LeftPaddingColumn.ActualWidth
                                    + IconColumn.ActualWidth
                                    + TitleTextBlock.ActualWidth
                                    + LeftDragColumn.ActualWidth
                                    + SearchColumn.ActualWidth) * scaleAdjustment);
                dragRectR.Y = 0;
                dragRectR.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
                dragRectR.Width = (int)(RightDragColumn.ActualWidth * scaleAdjustment);
                dragRectsList.Add(dragRectR);

                Windows.Graphics.RectInt32[] dragRects = dragRectsList.ToArray();

                appWindow.TitleBar.SetDragRectangles(dragRects);
            }
        }
        #endregion

    }
}


