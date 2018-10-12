﻿using Caliburn.Micro;
using DZY.DotNetUtil.Helpers;
using JsonConfiger;
using JsonConfiger.Models;
using LiveWallpaper.Store.Helpers;
using LiveWallpaper.Store.Models.Settngs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LiveWallpaper.Store.ViewModels
{
    public class SettingViewModel : Screen
    {
        JCrService _jcrService = new JCrService();

        public SettingViewModel()
        {
            LoadConfig();
        }

        #region properties


        #region JsonConfierViewModel

        /// <summary>
        /// The <see cref="JsonConfierViewModel" /> property's name.
        /// </summary>
        public const string JsonConfierViewModelPropertyName = "JsonConfierViewModel";

        private JsonConfierViewModel _JsonConfierViewModel;

        /// <summary>
        /// JsonConfierViewModel
        /// </summary>
        public JsonConfierViewModel JsonConfierViewModel
        {
            get { return _JsonConfierViewModel; }

            set
            {
                if (_JsonConfierViewModel == value) return;

                _JsonConfierViewModel = value;
                NotifyOfPropertyChange(JsonConfierViewModelPropertyName);
            }
        }

        #endregion

        #endregion

        private async void LoadConfig()
        {
            var json = await ApplicationData.Current.LocalSettings.ReadAsync<string>("config");
            if (string.IsNullOrEmpty(json))
            {
                SettingObject setting = SettingObject.GetDefaultSetting();
                json = await JsonHelper.JsonSerializeAsync(setting);
            }
            var config = await JsonHelper.JsonDeserializeAsync<dynamic>(json);
            //UWP
            string descPath = Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Res\\setting.desc.json");
            var descConfig = await JsonHelper.JsonDeserializeFromFileAsync<dynamic>(descPath);
            JsonConfierViewModel = _jcrService.GetVM(config, descConfig);
        }

        public void Save()
        {
            var data = _jcrService.GetData(JsonConfierViewModel.Nodes);
        }
    }
}
