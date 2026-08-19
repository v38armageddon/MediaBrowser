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

namespace MediaBrowser.Apps;

public sealed partial class VideosPage : Page
{
	public VideosPage()
	{
		this.InitializeComponent();
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
		ApplicationData.Current.LocalSettings.Values["CurrentPage"] = this.DataContext;
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
			".mp4",
			".mkv",
			".avi",
			".mov",
			".wmv"
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
			videoSlider.Value = 0;
			mediaPlayerElement.Source = source;
			mediaPlayerElement.MediaPlayer.Play();
			playButton.Visibility = Visibility.Collapsed;
			pauseButton.Visibility = Visibility.Visible;
			mediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDurationChanged += PlaybackSession_NaturalDurationChanged_EventHandler;
			_dispatcherTimer.Start();
			_mediaService.SetPlayingState(true);
		});
	}


    private void mediaPlayerElement_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _dispatchTimerMouse.Stop();
        if (commandBar2.Visibility == Visibility.Collapsed || commandBar1.Visibility == Visibility.Collapsed || bottomCommandBar.Visibility == Visibility.Collapsed)
        {
            commandBar1.Visibility = Visibility.Visible;
            commandBar2.Visibility = Visibility.Visible;
            bottomCommandBar.Visibility = Visibility.Visible;
        }
    }
    #endregion
    #region Bottom
	private void stopButton_Click(object sender, RoutedEventArgs e)
	{
		videoSlider.Value = 0;
		mediaPlayerElement.AutoPlay = false;
		mediaPlayerElement.Source = null;
	}

	private async void previousButton_Click(object sender, RoutedEventArgs e)
	{
		if (mediaPlayerElement.Source == null)
		{
			infoBar.Visibility = Visibility.Visible;
		}
		else
		{
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
				mediaPlayerElement.MediaPlayer.Play();
				playButton.Visibility = Visibility.Collapsed;
				pauseButton.Visibility = Visibility.Visible;
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
				mediaPlayerElement.MediaPlayer.Pause();
				playButton.Visibility = Visibility.Visible;
				pauseButton.Visibility = Visibility.Collapsed;
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
		mediaPlayerElement.MediaPlayer.Volume = volume;
	}

	private void infoBar_CloseButtonClick(Microsoft.UI.Xaml.Controls.InfoBar sender, object args)
	{
		infoBar.Visibility = Visibility.Collapsed;
	}

	private async void videoSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
	{
		int sliderValue = (int)Math.Round(e.NewValue);
		int maxValue = (int)Math.Round(videoSlider.Maximum);

		// If the slider is at the end, auto-play next or stop
		if (sliderValue >= maxValue)
		{
			_dispatcherTimer.Stop();
			if (_mediaService.Files.Count > 1)
			{
				_mediaService.PlayNext();
				await LoadAndPlayMediaAsync();
				videoSlider.Value = 0;
			}
			else
			{
				mediaPlayerElement.Source = null;
				videoSlider.Value = 0;
				_mediaService.Clear();
			}
		}
		mediaPlayerElement.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(sliderValue);
	}
	#endregion
}
