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

namespace MediaBrowser.Apps
{
    public sealed partial class PicturePage : Page
    {
        private List<StorageFile> files = new List<StorageFile>();
        private int currentFileIndex = 0;

        public PicturePage()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
            {
                buttonWindow.Visibility = Visibility.Collapsed;
                buttonClose.Visibility = Visibility.Collapsed;
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
            Windows.UI.Xaml.Application.Current.Exit();
        }

        private void buttonReturn_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
        }

        private void buttonHome_Click(object sender, RoutedEventArgs e)
        {
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
            if (currentFileIndex == 0)
                currentFileIndex = files.Count - 1;
            else
                currentFileIndex--;
            var image = new BitmapImage();
            image.SetSource(await files[currentFileIndex].OpenAsync(FileAccessMode.Read));
            Image.Source = image;
        }

        private async void nextButton_Click(object sender, RoutedEventArgs e)
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
}
