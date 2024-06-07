/*
 * MediaBrowser, A Modern version of Windows Media Center
 * Copyright (C) 2022 - 2024 - v38armageddon
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace MediaBrowser.Apps
{
    public sealed partial class PicturePage : Page
    {
        private List<StorageFile> files = new List<StorageFile>();
        private int currentFileIndex = 0;

        public PicturePage()
        {
            this.InitializeComponent();
            loadSettings();
        }

        private void loadSettings()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("PicturePage"))
            {
                this.DataContext = ApplicationData.Current.LocalSettings.Values["PicturePage"];
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("PicturePageCurrentFileIndex"))
            {
                currentFileIndex = (int)ApplicationData.Current.LocalSettings.Values["PicturePageCurrentFileIndex"];
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("PicturePageFiles"))
            {
                JArray filesArray = JArray.Parse((string)ApplicationData.Current.LocalSettings.Values["PicturePageFiles"]);
                foreach (var file in filesArray)
                {
                    files.Add(StorageFile.GetFileFromPathAsync((string)file).GetAwaiter().GetResult());
                }
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("ImageSource"))
            {
                string imageSourceString = ApplicationData.Current.LocalSettings.Values["ImageSource"].ToString();
                Uri imageSourceUri;
                if (Uri.TryCreate(imageSourceString, UriKind.Absolute, out imageSourceUri))
                {
                    Image.Source = new BitmapImage(imageSourceUri);
                }
            }

        }

        // Top
        private void buttonWindow_Click(object sender, RoutedEventArgs e)
        {
            var currentSize = ApplicationView.GetForCurrentView();
            if (!currentSize.IsFullScreenMode)
            {
                currentSize.TryEnterFullScreenMode();
                symbolButtonWindow.Symbol = Symbol.BackToWindow;
            }
            else
            {
                currentSize.ExitFullScreenMode();
                symbolButtonWindow.Symbol = Symbol.FullScreen;
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values.Clear();
            Windows.UI.Xaml.Application.Current.Exit();
        }

        private void buttonReturn_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
            ApplicationData.Current.LocalSettings.Values["PicturePage"] = this.DataContext;
            ApplicationData.Current.LocalSettings.Values["PicturePageCurrentFileIndex"] = currentFileIndex;
            //ApplicationData.Current.LocalSettings.Values["ImageSource"] = Image.Source;
            if (files != null && files.Count > 0)
            {
                JArray filesArray = new JArray();
                foreach (var file in files)
                {
                    filesArray.Add(file.Path);
                }
                ApplicationData.Current.LocalSettings.Values["PicturePageFiles"] = filesArray.ToString();
            }
            //if (Image.Source != null)
            //{
            //    BitmapImage bitmapImage = Image.Source as BitmapImage;
            //    if (bitmapImage != null)
            //    {
            //        ApplicationData.Current.LocalSettings.Values["ImageSource"] = bitmapImage.UriSource.AbsoluteUri;
            //    }
            //}
        }

        private void buttonHome_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = null;
            ApplicationData.Current.LocalSettings.Values.Clear();
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
        }

        // Bottom
        private async void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker p = new FileOpenPicker();
            p.FileTypeFilter.Add(".jpg");
            p.FileTypeFilter.Add(".png");
            p.FileTypeFilter.Add(".bmp");
            p.FileTypeFilter.Add(".ico");
            var selectedFiles = await p.PickMultipleFilesAsync();
            if (selectedFiles.Count == 0) return;
            files = selectedFiles.ToList();
            var image = new BitmapImage();
            image.SetSource(await files[currentFileIndex].OpenAsync(FileAccessMode.Read));
            Image.Source = image;
        }

        private async void previousButton_Click(object sender, RoutedEventArgs e)
        {
            if (Image.Source == null)
            {
                infoBar.Visibility = Visibility.Visible;
            }
            else
            {
                if (currentFileIndex == 0)
                    currentFileIndex = files.Count - 1;
                else
                    currentFileIndex--;
                var image = new BitmapImage();
                image.SetSource(await files[currentFileIndex].OpenAsync(FileAccessMode.Read));
                Image.Source = image;
            }
        }

        private async void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (Image.Source == null)
            {
                infoBar.Visibility = Visibility.Visible;
            }
            else
            {
                if (currentFileIndex == files.Count - 1)
                    currentFileIndex = 0;
                else
                    currentFileIndex++;
                var image = new BitmapImage();
                image.SetSource(await files[currentFileIndex].OpenAsync(FileAccessMode.Read));
                Image.Source = image;
            }
        }

        private void infoBar_CloseButtonClick(Microsoft.UI.Xaml.Controls.InfoBar sender, object args)
        {
            infoBar.Visibility = Visibility.Collapsed;
        }
    }
}
