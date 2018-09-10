﻿//https://www.codeproject.com/Articles/856020/Draw-Behind-Desktop-Icons-in-Windows
using DZY.WinAPI;
using DZY.WinAPI.Desktop.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LiveWallpaperEngine.NativeWallpapers
{
    public class HandlerWallpaper
    {
        static IDesktopWallpaper _desktopWallpaperAPI = DesktopWallpaperFactory.Create();
        static IntPtr _workerw;

        static bool _showed;
        static bool _initlized;
        static IntPtr _handler;

        public static void Close(bool restoreParent = false)
        {
            if (!_showed)
                return;

            _showed = false;

            if (!_initlized)
                Initlize();
          
            if (restoreParent)
            {
                var desktop = USER32Wrapper.GetDesktopWindow();
                USER32Wrapper.SetParent(_handler, desktop);
            }
            _desktopWallpaperAPI.Enable(true);
        }

        public static void Show(IntPtr handler)
        {
            _handler = handler;
            if (_handler == IntPtr.Zero || _showed)
                return;
            _showed = true;

            if (!_initlized)
            {
                bool isOk = Initlize();
                if (!isOk)
                    return;
            }
            USER32Wrapper.SetParent(_handler, _workerw);
            _desktopWallpaperAPI.Enable(false);
        }

        private static bool Initlize()
        {
            IntPtr progman = USER32Wrapper.FindWindow("Progman", null);
            IntPtr result = IntPtr.Zero;
            USER32Wrapper.SendMessageTimeout(progman,
                                   0x052C,
                                   new IntPtr(0),
                                   IntPtr.Zero,
                                   SendMessageTimeoutFlags.SMTO_NORMAL,
                                   1000,
                                   out result);
            _workerw = IntPtr.Zero;
            var result1 = USER32Wrapper.EnumWindows(new EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = USER32Wrapper.FindWindowEx(tophandle,
                                            IntPtr.Zero,
                                            "SHELLDLL_DefView",
                                            IntPtr.Zero);

                if (p != IntPtr.Zero)
                {
                    _workerw = USER32Wrapper.FindWindowEx(IntPtr.Zero,
                                             tophandle,
                                             "WorkerW",
                                             IntPtr.Zero);
                }

                return true;
            }), IntPtr.Zero);
            _initlized = result1;
            return result1;
        }
    }

}
