using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Media.Core;

namespace MediaBrowser.Apps
{
    public sealed partial class VideosPage : Page
    {
        private List<StorageFile> files = new List<StorageFile>();
        private int currentFileIndex = 0;
        private List<string> extensionFile = new List<string>();

        public VideosPage()
        {
            this.InitializeComponent();
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("VideosPage"))
            {
                this.DataContext = ApplicationData.Current.LocalSettings.Values["VideosPage"];
            }
        }

        // Top
        private void buttonReturn_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
            ApplicationData.Current.LocalSettings.Values["CurrentPage"] = this.DataContext;
        }

        private void buttonHome_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
            mediaPlayerElement.Source = null;
        }

        // Center
        private async void openFileButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            FileOpenPicker p = new FileOpenPicker();
            p.FileTypeFilter.Add(".mp4");
            p.FileTypeFilter.Add(".mkv");
            p.FileTypeFilter.Add(".wmv");
            p.FileTypeFilter.Add(".mov");
            p.FileTypeFilter.Add(".avi");
            var selectedFiles = await p.PickMultipleFilesAsync();
            if (selectedFiles.Count == 0) return;
            files = selectedFiles.ToList();
            var source = MediaSource.CreateFromStorageFile(files[currentFileIndex]);
            mediaPlayerElement.Source = source;
            mediaPlayerElement.AutoPlay = true;
            playButton.Visibility = Visibility.Collapsed;
            pauseButton.Visibility = Visibility.Visible;
        }

        // Bottom
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayerElement.AutoPlay = false;
            mediaPlayerElement.Source = null;
        }

        private void previousButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayerElement.Source == null)
            {
                infoBar.Visibility = Visibility.Visible;
            }
            else
            {
                if (currentFileIndex == 0)
                    currentFileIndex = files.Count - 1;
                else
                    currentFileIndex--;
                var source = MediaSource.CreateFromStorageFile(files[currentFileIndex]);
                mediaPlayerElement.Source = source;
                mediaPlayerElement.MediaPlayer.Play();
            }
        }

        [Obsolete]
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayerElement.Source != null)
            {
                if (mediaPlayerElement.MediaPlayer.NaturalDuration == TimeSpan.Zero)
                {
                    infoBar.Visibility = Visibility.Visible;
                }
                mediaPlayerElement.MediaPlayer.Play();
                playButton.Visibility = Visibility.Collapsed;
                pauseButton.Visibility = Visibility.Visible;
            }
            else
            {
                infoBar.Visibility = Visibility.Visible;
            }
        }

        [Obsolete]
        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayerElement.Source != null)
            {
                if (mediaPlayerElement.MediaPlayer.NaturalDuration == TimeSpan.Zero)
                {
                    infoBar.Visibility = Visibility.Visible;
                }
                mediaPlayerElement.MediaPlayer.Pause();
                playButton.Visibility = Visibility.Collapsed;
                pauseButton.Visibility = Visibility.Visible;
            }
            else
            {
                infoBar.Visibility = Visibility.Visible;
            }
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayerElement.Source == null)
            {
                infoBar.Visibility = Visibility.Visible;
            }
            else
            {
                if (currentFileIndex == files.Count - 1)
                    currentFileIndex = 0;
                else
                    currentFileIndex++;
                var source = MediaSource.CreateFromStorageFile(files[currentFileIndex]);
                mediaPlayerElement.Source = source;
                mediaPlayerElement.MediaPlayer.Play();
            }
        }

        private void volumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (volumeBar.Visibility == Visibility.Collapsed) volumeBar.Visibility = Visibility.Visible;
            else if (volumeBar.Visibility == Visibility.Visible) volumeBar.Visibility = Visibility.Collapsed;
        }

        private void volumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            int value = (int)e.NewValue;
            int NewVolume = ((ushort.MaxValue / 100) * value);
            uint NewVolumeAllChannels = (((uint)NewVolume & 0x0000ffff) | ((uint)NewVolume << 16));
            //waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels);
        }

        private void infoBar_CloseButtonClick(Microsoft.UI.Xaml.Controls.InfoBar sender, object args)
        {
            infoBar.Visibility = Visibility.Collapsed;
        }
    }
}
