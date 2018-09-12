﻿using DZY.DotNetUtil.Helpers;
using LiveWallpaperEngine.NativeWallpapers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LiveWallpaperEngine
{
    public static partial class WallpaperManager
    {
        public static string[] VideoExtensions { get; } = new string[] {
            ".mp4",
            };
        public static List<string> SupportedExtensions { get; } = new List<string>();
        static WallpaperManager()
        {
            SupportedExtensions.AddRange(VideoExtensions);
        }

        public static void Initlize()
        {
            //恢复桌面，以防上次崩溃x显示黑屏
            HandlerWallpaper.DesktopWallpaperAPI.Enable(true);
        }

        /// <summary>
        /// 解析壁纸
        /// </summary>
        /// <param name="filePath">壁纸路径</param>
        /// <remarks>如果目录下没有project.json，则会生成默认对象</remarks>
        /// <returns></returns>
        public static Wallpaper ResolveFromFile(string filePath)
        {
            Wallpaper result = new Wallpaper()
            {
                AbsolutePath = filePath
            };

            string dir = Path.GetDirectoryName(filePath);
            result.ProjectInfo = GetProjectInfo(dir);
            if (result.ProjectInfo == null)
                result.ProjectInfo = new ProjectInfo(filePath);

            return result;
        }

        public static ProjectInfo GetProjectInfo(string dirPath)
        {
            string file = Path.Combine(dirPath, "project.json");
            var info = JsonHelper.JsonDeserializeFromFile<ProjectInfo>(file);
            return info;
        }

        public static IEnumerable<Wallpaper> GetWallpapers(string dir)
        {
            //test E:\SteamLibrary\steamapps\workshop\content\431960
            foreach (var item in Directory.EnumerateFiles(dir, "project.json", SearchOption.AllDirectories))
            {
                var info = JsonHelper.JsonDeserializeFromFileAsync<ProjectInfo>(item).Result;
                var result = new Wallpaper(info, item);
                yield return result;
            }
        }

        public static void CreateLocalPack(Wallpaper wallpaper, string destDir)
        {
            var currentDir = Path.GetDirectoryName(wallpaper.AbsolutePath);
            string projectInfoPath = Path.Combine(currentDir, "project.json");
            if (File.Exists(projectInfoPath))
            {
                //有详细信息，全拷。兼容wallpaper engine
                CopyFolder(new DirectoryInfo(currentDir), new DirectoryInfo(destDir));
            }
            else
            {
                CopyFileToDir(wallpaper.AbsolutePreviewPath, destDir);
                CopyFileToDir(wallpaper.AbsolutePath, destDir);
            }

            string jsonPath = Path.Combine(destDir, "project.json");
            JsonHelper.JsonSerialize(wallpaper.ProjectInfo, jsonPath);
        }

        public static void Delete(Wallpaper wallpaper)
        {
            if (RenderWindow != null && RenderWindow.Wallpaper != null
                && RenderWindow.Wallpaper.AbsolutePath == wallpaper.AbsolutePath)
                Close();
            string dir = Path.GetDirectoryName(wallpaper.AbsolutePath);
            Directory.Delete(dir, true);
        }

        public static void CopyFileToDir(string path, string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(path))
                return;

            FileInfo file = new FileInfo(path);
            string target = Path.Combine(dir, file.Name);
            file.CopyTo(target, true);
        }

        public static void CopyFolder(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyFolder(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}