﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LiveWallpaperEngineLib.Controls
{
    /// <summary>
    /// Interaction logic for RenderWindow.xaml
    /// </summary>
    public partial class RenderWindow : Window
    {
        public RenderWindow()
        {
            InitializeComponent();
            Loaded += RenderWindow_Loaded;
        }

        internal void Mute(bool mute)
        {
            if (Wallpaper != null)
                Wallpaper.Muted = mute;
        }

        private void RenderWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //处理alt+tab可以看见本程序
            //https://stackoverflow.com/questions/357076/best-way-to-hide-a-window-from-the-alt-tab-program-switcher
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

            Loaded -= RenderWindow_Loaded;

            //double width = Screen.AllScreens[0].Bounds.Width;
            //double height = Screen.AllScreens[0].Bounds.Height;

            ////Top = -4;
            ////Left = 0;
            //WindowState = WindowState.Maximized;

            //Width = width;
            //Height = height;

            //WindowStartupLocation = WindowStartupLocation.Manual;
            //Top = 0;
            //Left = 0;
        }

        #region Wallpaper

        public Wallpaper Wallpaper
        {
            get { return (Wallpaper)GetValue(WallpaperProperty); }
            set { SetValue(WallpaperProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Wallpaper.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WallpaperProperty =
            DependencyProperty.Register("Wallpaper", typeof(Wallpaper), typeof(RenderWindow), new PropertyMetadata(null));

        #endregion

        internal void Pause()
        {
            Dispatcher.Invoke(() =>
            {
                IsEnabled = false;
            });
            //以后可能会用其他方案实现
        }

        internal void Resume()
        {
            Dispatcher.Invoke(() =>
            {
                IsEnabled = true;
            });
        }

        #region Window styles

        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);


        #endregion

    }
}
