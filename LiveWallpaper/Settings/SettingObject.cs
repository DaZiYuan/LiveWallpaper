﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveWallpaper.Settings
{
    public class GeneralSettting
    {
        public bool StartWithWindows { get; set; }

        public bool MinimizeUI { get; set; }

        public string CurrentLan { get; set; }

        public bool RecordWindowSize { get; set; }

        public float Width { get; set; }

        public float Height { get; set; }

        public static GeneralSettting GetDefaultGeneralSettting()
        {
            return new GeneralSettting()
            {
                StartWithWindows = true,
                MinimizeUI = true,
                RecordWindowSize = true,
                Width = 436,
                Height = 337,
                CurrentLan = "zh"
            };
        }
    }

    public enum ActionWhenMaximized
    {
        Play,
        Stop,
        Pause
    }

    public class WallpaperSetting
    {
        public ActionWhenMaximized ActionWhenMaximized { get; set; }
        public string VideoAspect { get; set; }

        public static WallpaperSetting GetDefaultWallpaperSetting()
        {
            return new WallpaperSetting()
            {
                ActionWhenMaximized = ActionWhenMaximized.Pause
            };
        }

    }

    public class ServerSetting
    {
        public string ServerUrl { get; set; }

        public static ServerSetting GetDefaultServerSetting()
        {
            return new ServerSetting()
            {
                ServerUrl = "http://localhost:8080"
            };
        }
    }

    public class SettingObject
    {
        public GeneralSettting General { get; set; }

        public WallpaperSetting Wallpaper { get; set; }

        //public ServerSetting Server { get; set; }
    }
}
