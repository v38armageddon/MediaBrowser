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
using Windows.UI.Core;

namespace MediaBrowser.Apps
{
    public sealed partial class VideosPage : Page
    {
        private List<StorageFile> files = new List<StorageFile>();
        private int currentFileIndex = 0;
        private DispatcherTimer dispatcherTimer;
        private TimeSpan durationMF;

        public VideosPage()
        {
            this.InitializeComponent();
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("VideosPage"))
            {
                this.DataContext = ApplicationData.Current.LocalSettings.Values["VideosPage"];
            }
            // This is for the videoSlider for obtaining the current position of the video
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            videoSlider.TickFrequency = 1.00;
            dispatcherTimer.Tick += DispatcherTimerTick_EventHandler;
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
            Application.Current.Exit();
        }

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
            // Maybe try to add more extensions in the future
            p.FileTypeFilter.Add(".mp4");
            p.FileTypeFilter.Add(".mkv");
            p.FileTypeFilter.Add(".wmv");
            p.FileTypeFilter.Add(".mov");
            p.FileTypeFilter.Add(".avi");
            var selectedFiles = await p.PickMultipleFilesAsync();

            // If the user doesn't select any file, then return
            if (selectedFiles.Count == 0) return;

            // Add the files to the list
            files = selectedFiles.ToList();
            var source = MediaSource.CreateFromStorageFile(files[currentFileIndex]);
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                mediaPlayerElement.Source = source;

                // Play the video
                mediaPlayerElement.AutoPlay = true;
                playButton.Visibility = Visibility.Collapsed;
                pauseButton.Visibility = Visibility.Visible;
                mediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDurationChanged += NaturalDurationChanged_EventHandler;
                dispatcherTimer.Start();
            });
        }

        // Bottom
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            videoSlider.Value = 0;
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
                videoSlider.Value = 0;
                if (currentFileIndex == 0)
                    currentFileIndex = files.Count - 1;
                else
                    currentFileIndex--;
                var source = MediaSource.CreateFromStorageFile(files[currentFileIndex]);
                mediaPlayerElement.Source = source;
                mediaPlayerElement.MediaPlayer.Play();
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayerElement.Source != null)
            {
                if (mediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration == TimeSpan.Zero)
                {
                    infoBar.Visibility = Visibility.Visible;
                }
                dispatcherTimer.Start();
                mediaPlayerElement.MediaPlayer.Play();
                playButton.Visibility = Visibility.Collapsed;
                pauseButton.Visibility = Visibility.Visible;
            }
            else
            {
                infoBar.Visibility = Visibility.Visible;
            }
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayerElement.Source != null)
            {
                if (mediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration == TimeSpan.Zero)
                {
                    infoBar.Visibility = Visibility.Visible;
                }
                dispatcherTimer.Stop();
                mediaPlayerElement.MediaPlayer.Pause();
                playButton.Visibility = Visibility.Visible;
                pauseButton.Visibility = Visibility.Collapsed;
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
                videoSlider.Value = 0;
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
            double volume = volumeSlider.Value / 100.0; // Scale the value to be between 0 and 1
            mediaPlayerElement.MediaPlayer.Volume = volume;
        }

        private void infoBar_CloseButtonClick(Microsoft.UI.Xaml.Controls.InfoBar sender, object args)
        {
            infoBar.Visibility = Visibility.Collapsed;
        }

        private void videoSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (mediaPlayerElement.Source == null) return;
            int sliderValue = Convert.ToInt32(e.NewValue.ToString());
            mediaPlayerElement.MediaPlayer.PlaybackSession.Position = new TimeSpan(0, 0, sliderValue);
        }

        private void videoSlider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            dispatcherTimer.Stop();
        }

        private void videoSlider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            dispatcherTimer.Start();
        }

        // void, async, Task, bool n stuff
        private void DispatcherTimerTick_EventHandler(object sender, object e)
        {
            videoSlider.Value += 1;
        }

        private async void NaturalDurationChanged_EventHandler(MediaPlaybackSession sender, object args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                durationMF = sender.NaturalDuration;
                videoSlider.Maximum = durationMF.TotalSeconds;
            });
        }
    }
}
