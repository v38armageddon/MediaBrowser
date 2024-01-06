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
using Windows.UI.Core;
using Windows.Media.Playback;

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
            // This is for the musicSlider for obtaining the current position of the video
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            musicSlider.TickFrequency = 1.00;
            dispatcherTimer.Tick += DispatcherTimerTick_EventHandler;
        }

        // Top
        private void buttonReturn_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
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
            var source = MediaSource.CreateFromStorageFile(files[currentFileIndex]);
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                musicSlider.Value = 0; // Reset the Slider everytime if there is a change
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

        private void infoBar_CloseButtonClick(Microsoft.UI.Xaml.Controls.InfoBar sender, object args)
        {
            infoBar.Visibility = Visibility.Collapsed;
        }

        private void musicSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            int sliderValue = Convert.ToInt32(Math.Round(e.NewValue).ToString());
            // If the slider is at the end, stop the video
            if (sliderValue >= musicSlider.Maximum)
            {
                dispatcherTimer.Stop();
                mediaPlayerElement.Source = null;
                musicSlider.Value = 0;
                mediaPlayerElement.MediaPlayer.PlaybackSession.Position = new TimeSpan(0, 0, sliderValue);
            }
            mediaPlayerElement.MediaPlayer.PlaybackSession.Position = new TimeSpan(0, 0, sliderValue);
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
