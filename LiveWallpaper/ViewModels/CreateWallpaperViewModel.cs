﻿using Caliburn.Micro;
using System.Diagnostics;
using MultiLanguageManager;
using LiveWallpaperEngine;
using LiveWallpaperEngine.Controls;
using LiveWallpaperEngine.NativeWallpapers;
using System.Windows.Interop;
using LiveWallpaper.Services;
using System.Threading.Tasks;
using System.Windows;
using System.Text;
using System.IO;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LiveWallpaper.ViewModels
{
    public class CreateWallpaperViewModel : ScreenWindow
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        //默认是false，修改后内存保存
        private static bool _preview;

        public CreateWallpaperViewModel()
        {
            DisplayName = LanService.Get("common_create").Result;
            PreviewWallpaper = _preview;
        }

        #region properties

        public bool Result { get; set; }

        #region CurrentWallpaper

        /// <summary>
        /// The <see cref="CurrentWallpaper" /> property's name.
        /// </summary>
        public const string CurrentWallpaperPropertyName = "CurrentWallpaper";

        private Wallpaper _CurrentWallpaper;

        /// <summary>
        /// CurrentWallpaper
        /// </summary>
        public Wallpaper CurrentWallpaper
        {
            get { return _CurrentWallpaper; }

            set
            {
                if (_CurrentWallpaper == value) return;

                _CurrentWallpaper = value;
                if (_preview)
                    Preview();

                NotifyOfPropertyChange(CurrentWallpaperPropertyName);
            }
        }

        #endregion

        #region PreviewWallpaper

        /// <summary>
        /// The <see cref="PreviewWallpaper" /> property's name.
        /// </summary>
        public const string PreviewWallpaperPropertyName = "PreviewWallpaper";

        private bool _PreviewWallpaper;

        /// <summary>
        /// PreviewWallpaper
        /// </summary>
        public bool PreviewWallpaper
        {
            get { return _PreviewWallpaper; }

            set
            {
                if (_PreviewWallpaper == value) return;

                _PreviewWallpaper = value;
                NotifyOfPropertyChange(PreviewWallpaperPropertyName);
            }
        }

        #endregion

        #region FilePath

        /// <summary>
        /// The <see cref="FilePath" /> property's name.
        /// </summary>
        public const string FilePathPropertyName = "FilePath";

        private string _FilePath;

        /// <summary>
        /// FilePath
        /// </summary>
        public string FilePath
        {
            get { return _FilePath; }

            set
            {
                if (_FilePath == value) return;

                _FilePath = value;
                NotifyOfPropertyChange(FilePathPropertyName);
            }
        }

        #endregion

        #region CanSave

        /// <summary>
        /// The <see cref="CanSave" /> property's name.
        /// </summary>
        public const string CanSavePropertyName = "CanSave";

        private bool _CanSave = true;

        /// <summary>
        /// CanSave
        /// </summary>
        public bool CanSave
        {
            get { return _CanSave; }

            set
            {
                if (_CanSave == value) return;

                _CanSave = value;
                NotifyOfPropertyChange(CanSavePropertyName);
            }
        }

        #endregion

        #endregion

        #region public methods 

        public async void SelectFile()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(await LanService.Get("wallpaperEditor_fileDialogType"));
            foreach (var item in WallpaperManager.SupportedExtensions)
            {
                sb.Append($"*{item};");
            }
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = sb.ToString()
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                FilePath = openFileDialog.FileName;
            }
        }

        internal void SetPaper(Wallpaper w)
        {
            DisplayName = LanService.Get("common_edit").Result;
            CurrentWallpaper = w;
        }

        public async void Preview()
        {
            _preview = true;

            //防止显示黑屏
            if (CurrentWallpaper != null)
                await Task.Run(() =>
            {
                WallpaperManager.Preivew(CurrentWallpaper);
            });
        }

        public async void StopPreview()
        {
            _preview = false;
            await Task.Run(new System.Action(WallpaperManager.StopPreview));
        }

        public void Cancel()
        {
            StopPreview();
            Result = false;
            TryClose();
        }

        public async void Save()
        {
            if (CurrentWallpaper == null)
            {
                MessageBox.Show(await LanService.Get("wallpaperEditor_warning_invalidWallpaper"));
                return;
            }
            if (string.IsNullOrEmpty(CurrentWallpaper.ProjectInfo.Title))
            {
                MessageBox.Show(await LanService.Get("wallpaperEditor_warning_titleEmpty"));
                return;
            }

            CanSave = false;

            string destDir = Path.Combine(AppService.LocalWallpaperDir, Guid.NewGuid().ToString());
            try
            {
                await Task.Run(() =>
                {
                    WallpaperManager.CreateLocalPack(CurrentWallpaper, destDir);
                    WallpaperManager.Show(CurrentWallpaper);
                });
            }
            catch (Exception ex)
            {
                CanSave = true;
                logger.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            Result = true;
            TryClose();
        }

        public void GeneratePreview(object parameter)
        {
            WallpaperRender render = parameter as WallpaperRender;
            if (render == null || CurrentWallpaper == null)
                return;

            try
            {
                using (FileStream stream = File.Open(CurrentWallpaper.AbsolutePreviewPath, FileMode.Create))
                {
                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)render.ActualWidth,
                        (int)render.ActualHeight, 96, 96, PixelFormats.Pbgra32);

                    bmp.Render(render);

                    //截图保持正方形
                    var size = (int)(render.ActualWidth > render.ActualHeight ? render.ActualHeight : render.ActualWidth);
                    var x = (int)(render.ActualWidth / 2 - size / 2);
                    var y = (int)(render.ActualHeight / 2 - size / 2);
                    CroppedBitmap crop = new CroppedBitmap(bmp, new Int32Rect(x, y, size, size));

                    PngBitmapEncoder coder = new PngBitmapEncoder
                    {
                        Interlace = PngInterlaceOption.Off
                    };
                    coder.Frames.Add(BitmapFrame.Create(crop));
                    coder.Save(stream);
                }

                CurrentWallpaper.NotifyOfPropertyChange(Wallpaper.AbsolutePreviewPathPropertyName);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessageBox.Show(ex.Message);
            }
        }

        //public void RestartExploer()
        //{
        //    foreach (Process exe in Process.GetProcesses())
        //    {
        //        if (exe.ProcessName.StartsWith("explorer"))
        //        {
        //            exe.Kill();
        //            break;
        //        }
        //    }

        //    var p = new Process();
        //    string explorer = "explorer.exe";
        //    p.StartInfo.FileName = explorer;
        //    p.Start();
        //    p.Kill();
        //}

        #endregion

        #region private methods

        #endregion
    }
}
