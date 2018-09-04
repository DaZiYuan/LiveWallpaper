﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveWallpaperEngine.NativeWallpapers
{
    public class HandlerWallpaper
    {
        static IntPtr _workerw;
        static string _defaultBG;
        static bool _initlized;

        public static Task Clean()
        {
            return Task.Run(() =>
            {
                if (!_initlized)
                    Initlize();
                if (!string.IsNullOrEmpty(_defaultBG))
                {
                    ImgWallpaper.SetBG(_defaultBG);
                    _defaultBG = null;
                }

                //var resul = W32.RedrawWindow(workerw, IntPtr.Zero, IntPtr.Zero, RedrawWindowFlags.Invalidate);
                //var temp = W32.GetParent(workerw);
                //W32.SendMessage(temp, 0x000F, 0, IntPtr.Zero);
                //W32.SendMessage(workerw, 0x000F, 0, IntPtr.Zero);
                //W32.SendMessage(workerw, W32.WM_CHANGEUISTATE, 2, IntPtr.Zero);
                //W32.SendMessage(workerw, W32.WM_UPDATEUISTATE, 2, IntPtr.Zero);
            });
        }

        public static async Task Show(IntPtr handler)
        {
            if (handler == IntPtr.Zero)
                return;

            if (!_initlized)
            {
                bool isOk = await Task.Run(() => Initlize());
                if (!isOk)
                    return;
            }
            _defaultBG = await ImgWallpaper.GetCurrentBG();

            W32.SetParent(handler, _workerw);
        }

        private static bool Initlize()
        {
            IntPtr progman = W32.FindWindow("Progman", null);
            IntPtr result = IntPtr.Zero;
            W32.SendMessageTimeout(progman,
                                   0x052C,
                                   new IntPtr(0),
                                   IntPtr.Zero,
                                   W32.SendMessageTimeoutFlags.SMTO_NORMAL,
                                   1000,
                                   out result);
            _workerw = IntPtr.Zero;
            var result1 = W32.EnumWindows(new W32.EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = W32.FindWindowEx(tophandle,
                                            IntPtr.Zero,
                                            "SHELLDLL_DefView",
                                            IntPtr.Zero);

                if (p != IntPtr.Zero)
                {
                    // Gets the WorkerW Window after the current one.
                    _workerw = W32.FindWindowEx(IntPtr.Zero,
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
