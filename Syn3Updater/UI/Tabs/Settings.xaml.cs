﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows;
using Cyanlabs.Syn3Updater.Model;
using Cyanlabs.Updater.Common;
using ModernWpf.Controls;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    /// <summary>
    ///     Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        private Api.Ivsus2 _syncVersions;

        public Settings()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this)) (DataContext as SettingsViewModel)?.Init();
        }

        private void SyncVersionsAutoSuggestBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (SyncVersionsAutoSuggestBox.ItemsSource != null) return;
            try
            {
                HttpRequestMessage httpRequestMessage = new()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Api.SyncVersions),
                    Headers = { 
                        { nameof(HttpRequestHeader.Authorization), $"Bearer {ApiSecret.Token}" },
                    },
                };

                HttpResponseMessage response = AppMan.Client.SendAsync(httpRequestMessage).GetAwaiter().GetResult();
                _syncVersions = JsonHelpers.Deserialize<Api.Ivsus2>(response.Content.ReadAsStreamAsync().Result);
            }
            catch (Exception)
            {
                //Do nothing
            }

            SyncVersionsAutoSuggestBox.ItemsSource = _syncVersions.Ivsu.OrderByDescending(u => u.Version).GroupBy(u => u.Version).Select(u => u.First()).ToList();;
        }

        private void Settings_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue && !(bool) e.OldValue) (DataContext as SettingsViewModel)?.ReloadSettings();
        }
        
        #region Code to Move to ViewModel at later date

        //TODO - Move to viewModel
        private void My20Toggle_OnToggled(object sender, RoutedEventArgs e)
        {
            (DataContext as SettingsViewModel)?.UpdateMy20Toggle(My20Toggle.IsChecked);
        }

        private void AdvancedModeToggle_OnToggled(object sender, RoutedEventArgs e)
        {
            (DataContext as SettingsViewModel)?.UpdateAdvancedModeToggle(AdvancedToggle.IsOn);
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                SyncVersionsAutoSuggestBox.ItemsSource =
                    _syncVersions.Ivsu.OrderByDescending(u => u.Version).Where(u => u.Version.StartsWith(SyncVersionsAutoSuggestBox.Text)).GroupBy(u => u.Version).Select(u => u.First()).ToList();
        }

        #endregion
    }
}