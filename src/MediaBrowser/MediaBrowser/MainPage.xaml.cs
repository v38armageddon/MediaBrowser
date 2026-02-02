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
using MediaBrowser.Apps;
using MediaBrowser.Models;
using Microsoft.UI.Xaml.Media.Animation;

namespace MediaBrowser;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
#if DESKTOP
        if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
        {
            buttonWindow.Visibility = Visibility.Collapsed;
            buttonClose.Visibility = Visibility.Collapsed;
        }
#else
        buttonWindow.Visibility = Visibility.Collapsed;
        buttonClose.Visibility = Visibility.Collapsed;
#endif
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.Loaded -= OnLoaded;
        mainPivot.SelectedIndex = 1;
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
    #endregion
    #region Center
    // Tasks
    private async void buttonExit_Click(object sender, RoutedEventArgs e)
    {
        Dialogs.ExitDialog exitDialog = new Dialogs.ExitDialog();
        exitDialog.XamlRoot = this.XamlRoot;
        await exitDialog.ShowAsync();
    }

    private async void buttonAbout_Click(object sender, RoutedEventArgs e)
    {
        Dialogs.AboutDialog aboutDialog = new Dialogs.AboutDialog();
        aboutDialog.XamlRoot = this.XamlRoot;
        await aboutDialog.ShowAsync();
    }

    // Picture
    private void myPicturesButton_Click(object sender, RoutedEventArgs e)
    {
        Frame rootFrame = Window.Current.Content as Frame;
        rootFrame.Navigate(typeof(PicturePage), null, new DrillInNavigationTransitionInfo());
    }

    private void cameraButton_Click(object sender, RoutedEventArgs e)
    {
        Frame rootFrame = Window.Current.Content as Frame;
        rootFrame.Navigate(typeof(CameraPage), null, new DrillInNavigationTransitionInfo());
    }

    // Music
    private void myMusicButton_Click(object sender, RoutedEventArgs e)
    {
        Frame rootFrame = Window.Current.Content as Frame;
        rootFrame.Navigate(typeof(MusicPage), null, new DrillInNavigationTransitionInfo());
    }

    // Videos
    private void myVideosButton_Click(object sender, RoutedEventArgs e)
    {
        Frame rootFrame = Window.Current.Content as Frame;
        rootFrame.Navigate(typeof(VideosPage), null, new DrillInNavigationTransitionInfo());
    }

    // Extras
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Frame rootFrame = Window.Current.Content as Frame;
        rootFrame.Navigate(typeof(BingMapPage), null, new DrillInNavigationTransitionInfo());
    }
    #endregion
}
