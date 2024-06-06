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
using Newtonsoft.Json.Linq;
using Windows.Media.Playback;
using Windows.UI.Core;

namespace MediaBrowser.Apps
{
    public sealed partial class MusicPage : Page
    {
        private List<StorageFile> files = new List<StorageFile>();
        private int currentFileIndex = 0;
        private DispatcherTimer dispatcherTimer;
        private TimeSpan durationMF;

        public MusicPage()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
            {
                buttonWindow.Visibility = Visibility.Collapsed;
                buttonClose.Visibility = Visibility.Collapsed;
                volumeButton.Visibility = Visibility.Collapsed;
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("MusicPage"))
            {
                this.DataContext = ApplicationData.Current.LocalSettings.Values["MusicPage"];
            }
            loadSettings();

            // This is for the musicSlider for obtaining the current position of the video
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            musicSlider.TickFrequency = 1.00;
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
            ApplicationData.Current.LocalSettings.Values["MusicPage"] = this.DataContext;
            ApplicationData.Current.LocalSettings.Values["MusicPageCurrentFileIndex"] = currentFileIndex;
            if (files != null && files.Count > 0)
            {
                JArray filesArray = new JArray();
                foreach (var file in files)
                {
                    filesArray.Add(file.Path);
                }
                ApplicationData.Current.LocalSettings.Values["MusicPageFiles"] = filesArray.ToString();
            }
            if (mediaPlayerElement.Source != null)
            {
                ApplicationData.Current.LocalSettings.Values["MusicPageCurrentFile"] = files[currentFileIndex].Path;
            }
        }

        private void buttonHome_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayerElement.Source = null;
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
        }

        // Center
        private async void openFileButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            FileOpenPicker p = new FileOpenPicker();
            p.FileTypeFilter.Add(".mp3");
            p.FileTypeFilter.Add(".ogg");
            p.FileTypeFilter.Add(".wav");
            p.FileTypeFilter.Add(".flac");
            var selectedFiles = await p.PickMultipleFilesAsync();

            // If the user doesn't select any file, then return
            if (selectedFiles.Count == 0) return;

            // Add the files to the list
            files = selectedFiles.ToList();

            // Preload the music into memory
            var stream = await files[currentFileIndex].OpenAsync(FileAccessMode.Read);
            var source = MediaSource.CreateFromStream(stream, files[currentFileIndex].ContentType);

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                musicSlider.Value = 0; // Reset the Slider everytime if there is a change
                mediaPlayerElement.Source = source;

                // Play the music
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
            musicSlider.Value = 0;
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

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayerElement.Source == null)
            {
                infoBar.Visibility = Visibility.Visible;
            }
            else
            {
                dispatcherTimer.Start();
                mediaPlayerElement.MediaPlayer.Play();
                playButton.Visibility = Visibility.Collapsed;
                pauseButton.Visibility = Visibility.Visible;
            }
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayerElement.Source == null)
            {
                infoBar.Visibility = Visibility.Visible;
            }
            else
            {
                dispatcherTimer.Stop();
                mediaPlayerElement.MediaPlayer.Pause();
                playButton.Visibility = Visibility.Visible;
                pauseButton.Visibility = Visibility.Collapsed;
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
            double volume = volumeSlider.Value / 100.0; // Scale the value to be between 0 and 1
            mediaPlayerElement.MediaPlayer.Volume = volume;
        }

        private void infoBar_CloseButtonClick(Microsoft.UI.Xaml.Controls.InfoBar sender, object args)
        {
            infoBar.Visibility = Visibility.Collapsed;
        }

        private void musicSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            int sliderValue = (int)Math.Round(e.NewValue);
            int maxValue = (int)Math.Round(musicSlider.Maximum);

            // If the slider is at the end, stop the video
            if (sliderValue >= maxValue)
            {
                dispatcherTimer.Stop();
                mediaPlayerElement.Source = null;
                musicSlider.Value = 0;
                // In any case, the video update it's position
                //mediaPlayerElement.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(sliderValue);
            }
            mediaPlayerElement.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(sliderValue);
        }

        private void musicSlider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            dispatcherTimer.Stop();
        }

        private void musicSlider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            dispatcherTimer.Start();
        }

        // void, async, Task, bool n stuff
        private void loadSettings()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("MusicPageCurrentFileIndex"))
            {
                currentFileIndex = (int)ApplicationData.Current.LocalSettings.Values["MusicPageCurrentFileIndex"];
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("MusicPageFiles"))
            {
                JArray filesArray = JArray.Parse((string)ApplicationData.Current.LocalSettings.Values["MusicPageFiles"]);
                foreach (var file in filesArray)
                {
                    files.Add(StorageFile.GetFileFromPathAsync((string)file).GetAwaiter().GetResult());
                }
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("MusicPageCurrentFile"))
            {
                mediaPlayerElement.Source = MediaSource.CreateFromStorageFile(StorageFile.GetFileFromPathAsync((string)ApplicationData.Current.LocalSettings.Values["MusicPageCurrentFile"]).GetAwaiter().GetResult());
            }
        }

        private void DispatcherTimerTick_EventHandler(object sender, object e)
        {
            musicSlider.Value += 1;
        }

        private async void NaturalDurationChanged_EventHandler(MediaPlaybackSession sender, object args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                durationMF = sender.NaturalDuration;
                musicSlider.Maximum = durationMF.TotalSeconds;
            });
        }
    }
}
