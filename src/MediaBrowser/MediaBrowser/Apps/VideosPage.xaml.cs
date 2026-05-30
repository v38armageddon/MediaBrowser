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
	private List<StorageFile> files = new List<StorageFile>();
	private int currentFileIndex = 0;
	private DispatcherTimer dispatcherTimer;
    private DispatcherTimer dispatchTimerMouse;
	private TimeSpan durationMF;

	public VideosPage()
	{
		this.InitializeComponent();
		// This is for the videoSlider for obtaining the current position of the video
		dispatcherTimer = new DispatcherTimer();
		dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
		videoSlider.TickFrequency = 1.00;
		dispatcherTimer.Tick += DispatcherTimer_Tick_EventHandler;
        dispatchTimerMouse = new DispatcherTimer();
        dispatchTimerMouse.Interval = new TimeSpan(0, 0, 3);
        dispatchTimerMouse.Tick += DispatchTimerMouse_Tick_EventHandler;
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

		// If the user doesn't select any file, then return
		if (selectedFiles.Count == 0) return;

		// Update the files collection with selected files
		files = selectedFiles.ToList();
		currentFileIndex = 0;
		// HACK: Do force the source due to Uno not implmentation
		var storageFile = files[currentFileIndex];
		var tempFolder = ApplicationData.Current.TemporaryFolder;
		var tempFile = await storageFile.CopyAsync(tempFolder, storageFile.Name, NameCollisionOption.ReplaceExisting);
		var uri = new Uri(tempFile.Path);
		var source = MediaSource.CreateFromUri(uri);

		await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
		{
			videoSlider.Value = 0; // Reset the Slider everytime if there is a change
			mediaPlayerElement.Source = source;

			// Play the video
			mediaPlayerElement.MediaPlayer.Play();
			playButton.Visibility = Visibility.Collapsed;
			pauseButton.Visibility = Visibility.Visible;
			mediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDurationChanged += PlaybackSession_NaturalDurationChanged_EventHandler;
			dispatcherTimer.Start();
		});
	}


    private void mediaPlayerElement_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        dispatchTimerMouse.Stop();
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
		dispatcherTimer.Stop();
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
			if (currentFileIndex == 0)
			{
				currentFileIndex = files.Count - 1;

			}
			else
			{
				currentFileIndex--;
			}
			var storageFile = files[currentFileIndex];
			var tempFolder = ApplicationData.Current.TemporaryFolder;
			var tempFile = await storageFile.CopyAsync(tempFolder, storageFile.Name, NameCollisionOption.ReplaceExisting);
			var uri = new Uri(tempFile.Path);
			var source = MediaSource.CreateFromUri(uri);
			mediaPlayerElement.Source = source;
			mediaPlayerElement.MediaPlayer.Play();
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
				dispatcherTimer.Start();
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
				dispatcherTimer.Stop();
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
			if (currentFileIndex == 0)
			{
				currentFileIndex = files.Count - 1;

			}
			else
			{
				currentFileIndex++;
			}
			var storageFile = files[currentFileIndex];
			var tempFolder = ApplicationData.Current.TemporaryFolder;
			var tempFile = await storageFile.CopyAsync(tempFolder, storageFile.Name, NameCollisionOption.ReplaceExisting);
			var uri = new Uri(tempFile.Path);
			var source = MediaSource.CreateFromUri(uri);
			mediaPlayerElement.Source = source;
			mediaPlayerElement.MediaPlayer.Play();
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
		double volume = volumeSlider.Value / 100.0; // Scale the value to be between 0 and 1
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

		// If the slider is at the end, put at the beginning or stop completely
		if (sliderValue >= maxValue)
		{
			dispatcherTimer.Stop();
			// Check if there are multiple files, if there are, then play the next video, if not, then stop the video and reset the slider
			if (files.Count > 1)
			{
				currentFileIndex = (currentFileIndex + 1) % files.Count;
				var storageFile = files[currentFileIndex];
				var tempFolder = ApplicationData.Current.TemporaryFolder;
				var tempFile = await storageFile.CopyAsync(tempFolder, storageFile.Name, NameCollisionOption.ReplaceExisting);
				var uri = new Uri(tempFile.Path);
				var source = MediaSource.CreateFromUri(uri);
				mediaPlayerElement.Source = source;
				videoSlider.Value = 0;
			}
			else
			{
				mediaPlayerElement.Source = null;
				videoSlider.Value = 0;
			}
		}
		mediaPlayerElement.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(sliderValue);
	}
	#endregion

	// void, async, Task, bool n stuff
	private void DispatcherTimer_Tick_EventHandler(object sender, object e)
	{
		videoSlider.Value += 1;
	}

	private async void PlaybackSession_NaturalDurationChanged_EventHandler(MediaPlaybackSession sender, object args)
	{
		await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
		{
			durationMF = sender.NaturalDuration;
			videoSlider.Maximum = durationMF.TotalSeconds;
		});
	}

    private void DispatchTimerMouse_Tick_EventHandler(object? sender, object e)
    {
        // Don't do anything if there is no source
        if (mediaPlayerElement.Source == null) return;

        // Hide the controls if the mouse is not moved for 3 seconds
        if (dispatchTimerMouse.Interval.TotalSeconds == 3)
        {
            commandBar1.Visibility = Visibility.Collapsed;
            commandBar2.Visibility = Visibility.Collapsed;
            bottomCommandBar.Visibility = Visibility.Collapsed;
        }
    }
}
