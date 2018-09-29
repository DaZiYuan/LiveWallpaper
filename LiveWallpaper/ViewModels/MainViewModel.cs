﻿using Caliburn.Micro;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using LiveWallpaperEngine;
using MultiLanguageManager;
using LiveWallpaper.Managers;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Dynamic;
using Windows.Storage;

namespace LiveWallpaper.ViewModels
{
    public class MainViewModel : ScreenWindow
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private CreateWallpaperViewModel _createVM;

        public MainViewModel()
        {
            Wallpapers = new ObservableCollection<Wallpaper>(AppService.Wallpapers);
        }

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
        }

        #region  public methods

        public void CreateWallpaper()
        {
            if (_createVM != null)
            {
                _createVM.ActiveUI();
                return;
            }

            var windowManager = IoC.Get<IWindowManager>();
            _createVM = IoC.Get<CreateWallpaperViewModel>();
            _createVM.Deactivated += _createVM_Deactivated;
            dynamic windowSettings = new ExpandoObject();
            windowSettings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            windowSettings.Owner = GetView();
            windowManager.ShowWindow(_createVM, null, windowSettings);
        }

        public void EditWallpaper(Wallpaper s)
        {
            CreateWallpaper();
            _createVM.SetPaper(s);
        }

        private void _createVM_Deactivated(object sender, DeactivationEventArgs e)
        {
            _createVM.Deactivated -= _createVM_Deactivated;
            if (_createVM.Result)
            {
                RefreshLocalWallpaper();
            }
            _createVM = null;
        }

        public override object GetView(object context = null)
        {
            return base.GetView(context);
        }

        public void RefreshLocalWallpaper()
        {
            AppService.RefreshLocalWallpapers();
            Wallpapers = new ObservableCollection<Wallpaper>(AppService.Wallpapers);
        }

        public void ExploreWallpaper(Wallpaper s)
        {
            try
            {
                string path = s.AbsolutePath;
#if UWP
                //https://stackoverflow.com/questions/48849076/uwp-app-does-not-copy-file-to-appdata-folder
                var uwpAppData = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "Roaming\\LiveWallpaper");

                var wpfAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                wpfAppData = Path.Combine(wpfAppData, "LiveWallpaper");
                path = path.Replace(wpfAppData, uwpAppData);
#endif
                Process.Start("Explorer.exe", $" /select, {path}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                MessageBox.Show(ex.Message);
            }
        }

        public async void DeleteWallpaper(Wallpaper w)
        {
            try
            {
                await Task.Run(() =>
                {
                    WallpaperManager.Delete(w);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            RefreshLocalWallpaper();
        }

        public async void ApplyWallpaper(Wallpaper w)
        {
            WallpaperManager.Show(w);
            AppService.AppData.Wallpaper = w.AbsolutePath;
            await AppService.ApplyAppDataAsync();
        }

        public void Setting()
        {
            IoC.Get<ContextMenuViewModel>().Config(GetView());
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
        }

        #endregion

        #region properties

        #region Wallpapers

        /// <summary>
        /// The <see cref="Wallpapers" /> property's name.
        /// </summary>
        public const string WallpapersPropertyName = "Wallpapers";

        private ObservableCollection<Wallpaper> _Wallpapers;

        /// <summary>
        /// Wallpapers
        /// </summary>
        public ObservableCollection<Wallpaper> Wallpapers
        {
            get { return _Wallpapers; }

            set
            {
                if (_Wallpapers == value) return;

                _Wallpapers = value;
                NotifyOfPropertyChange(WallpapersPropertyName);
            }
        }

        #endregion

        #endregion
    }
}
