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
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Pickers;
using MediaBrowser.Shared.Services;

namespace MediaBrowser.Apps;

public sealed partial class MusicPage : Page
{
    private readonly MediaPlayerService _mediaService = new();
    private DispatcherTimer _dispatcherTimer;
    private DispatcherTimer _dispatchTimerMouse;

    public MusicPage()
    {
        InitializeComponent();
        InitializeTimers();
        SubscribeToMediaServiceEvents();
    }

    private void InitializeTimers()
    {
        _dispatcherTimer = new DispatcherTimer();
        _dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
        _dispatcherTimer.Tick += DispatcherTimer_Tick_EventHandler;

        _dispatchTimerMouse = new DispatcherTimer();
        _dispatchTimerMouse.Interval = TimeSpan.FromSeconds(3);
        _dispatchTimerMouse.Tick += DispatchTimerMouse_Tick_EventHandler;
        _dispatchTimerMouse.Start();
    }

    private void SubscribeToMediaServiceEvents()
    {
        _mediaService.MediaChanged += MediaService_MediaChanged;
        _mediaService.PlaybackStateChanged += MediaService_PlaybackStateChanged;
        _mediaService.DurationChanged += MediaService_DurationChanged;
    }

    #region Top
    private void buttonWindow_Click(object sender, RoutedEventArgs e)
    {
        CommonBarControls.ToggleFullScreen();
    }

    private void buttonClose_Click(object sender, RoutedEventArgs e)
    {
        CommonBarControls.ExitApplication();
    }

    private void buttonReturn_Click(object sender, RoutedEventArgs e)
    {
        CommonBarControls.ReturnPreviousPage();
    }

    private void buttonHome_Click(object sender, RoutedEventArgs e)
    {
        CommonBarControls.NavigateHomePage();
    }
    #endregion

    #region Center
    private async void openFileButton_ClickAsync(object sender, RoutedEventArgs e)
    {
        FileOpenPicker p = new FileOpenPicker();
        var fileTypes = new List<string>()
        {
            ".mp3",
            ".wav",
            ".flac",
            ".aac",
            ".m4a",
            ".ogg",
            ".wma"
        };
        foreach (var fileType in fileTypes)
        {
            p.FileTypeFilter.Add(fileType);
        }
        var selectedFiles = await p.PickMultipleFilesAsync();
        if (selectedFiles.Count == 0) return;

        // Load files into MediaPlayerService
        _mediaService.LoadFiles(selectedFiles.ToList());
        await LoadAndPlayMediaAsync();
    }

    private async Task LoadAndPlayMediaAsync()
    {
        var currentFile = _mediaService.CurrentFile;
        if (currentFile == null) return;

        // HACK: Copy to temp folder due to Uno not handling file paths correctly
        var tempFolder = ApplicationData.Current.TemporaryFolder;
        var tempFile = await currentFile.CopyAsync(tempFolder, currentFile.Name, NameCollisionOption.ReplaceExisting);
        var uri = new Uri(tempFile.Path);
        var source = MediaSource.CreateFromUri(uri);

        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        {
            musicSlider.Value = 0;
            mediaPlayerElement.Source = source;
            mediaPlayerElement.MediaPlayer.Play();
            playButton.Visibility = Visibility.Collapsed;
            pauseButton.Visibility = Visibility.Visible;
            mediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDurationChanged += PlaybackSession_NaturalDurationChanged_EventHandler;
            _dispatcherTimer.Start();
            _mediaService.SetPlayingState(true);
        });
    }
    #endregion

    #region Bottom
    private void stopButton_Click(object sender, RoutedEventArgs e)
    {
        _dispatcherTimer.Stop();
        musicSlider.Value = 0;
        mediaPlayerElement.AutoPlay = false;
        mediaPlayerElement.Source = null;
        _mediaService.Clear();
        _mediaService.SetPlayingState(false);
    }

    private async void previousButton_Click(object sender, RoutedEventArgs e)
    {
        if (mediaPlayerElement.Source == null)
        {
            infoBar.Visibility = Visibility.Visible;
        }
        else
        {
            _mediaService.PlayPrevious();
            await LoadAndPlayMediaAsync();
        }
    }

    private void playButton_Click(object sender, RoutedEventArgs e)
    {
        if (mediaPlayerElement != null)
        {
            if (mediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration == TimeSpan.Zero)
            {
                infoBar.Visibility = Visibility.Visible;
            }
            else
            {
                _dispatcherTimer.Start();
                mediaPlayerElement.MediaPlayer.Play();
                playButton.Visibility = Visibility.Collapsed;
                pauseButton.Visibility = Visibility.Visible;
                _mediaService.SetPlayingState(true);
            }
        }
        else
        {
            infoBar.Visibility = Visibility.Visible;
        }
    }

    private void pauseButton_Click(object sender, RoutedEventArgs e)
    {
        if (mediaPlayerElement != null)
        {
            if (mediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration == TimeSpan.Zero)
            {
                infoBar.Visibility = Visibility.Visible;
            }
            else
            {
                _dispatcherTimer.Stop();
                mediaPlayerElement.MediaPlayer.Pause();
                playButton.Visibility = Visibility.Visible;
                pauseButton.Visibility = Visibility.Collapsed;
                _mediaService.SetPlayingState(false);
            }
        }
        else
        {
            infoBar.Visibility = Visibility.Visible;
        }
    }

    private async void nextButton_Click(object sender, RoutedEventArgs e)
    {
        if (mediaPlayerElement.Source == null)
        {
            infoBar.Visibility = Visibility.Visible;
        }
        else
        {
            _mediaService.PlayNext();
            await LoadAndPlayMediaAsync();
        }
    }

    private void volumeButton_Click(object sender, RoutedEventArgs e)
    {
        if (volumeBar.Visibility == Visibility.Collapsed)
        {
            volumeBar.Visibility = Visibility.Visible;
        }
        else
        {
            volumeBar.Visibility = Visibility.Collapsed;
        }
    }

    private void volumeSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        double volume = volumeSlider.Value / 100.0;
        _mediaService.Volume = volume;
        mediaPlayerElement.MediaPlayer.Volume = volume;
    }

    private void infoBar_CloseButtonClick(Microsoft.UI.Xaml.Controls.InfoBar sender, object args)
    {
        infoBar.Visibility = Visibility.Collapsed;
    }

    private async void musicSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        int sliderValue = (int)Math.Round(e.NewValue);
        int maxValue = (int)Math.Round(musicSlider.Maximum);

        // If the slider is at the end, auto-play next or stop
        if (sliderValue >= maxValue)
        {
            _dispatcherTimer.Stop();
            if (_mediaService.Files.Count > 1)
            {
                _mediaService.PlayNext();
                await LoadAndPlayMediaAsync();
                musicSlider.Value = 0;
            }
            else
            {
                mediaPlayerElement.Source = null;
                musicSlider.Value = 0;
                _mediaService.Clear();
            }
        }
        mediaPlayerElement.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(sliderValue);
    }
    #endregion

    // Event Handlers
    private void DispatcherTimer_Tick_EventHandler(object sender, object e)
    {
        musicSlider.Value += 1;
    }

    private async void PlaybackSession_NaturalDurationChanged_EventHandler(MediaPlaybackSession sender, object args)
    {
        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        {
            var duration = sender.NaturalDuration;
            _mediaService.SetDuration(duration);
            musicSlider.Maximum = duration.TotalSeconds;
        });
    }

    private void DispatchTimerMouse_Tick_EventHandler(object? sender, object e)
    {
        if (mediaPlayerElement.Source == null) return;

        // Note: MusicPage does not have command bars to hide
        // This handler is kept for consistency with other media pages
    }

    private void MediaService_MediaChanged(object? sender, MediaChangedEventArgs e)
    {
        // Media changed event from service (handled by LoadAndPlayMediaAsync)
    }

    private void MediaService_PlaybackStateChanged(object? sender, PlaybackStateChangedEventArgs e)
    {
        // Playback state changed (UI already updated in button handlers)
    }

    private void MediaService_DurationChanged(object? sender, DurationChangedEventArgs e)
    {
        // Duration changed (handled in PlaybackSession_NaturalDurationChanged_EventHandler)
    }
}

