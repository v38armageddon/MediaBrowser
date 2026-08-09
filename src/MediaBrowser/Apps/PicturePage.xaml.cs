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
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;

namespace MediaBrowser.Apps;

public sealed partial class PicturePage : Page
{
    private List<StorageFile> files = new List<StorageFile>();
    private int currentFileIndex = 0;

    public PicturePage()
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
    }

    private void buttonHome_Click(object sender, RoutedEventArgs e)
    {
        CommonBarControls.NavigateHomePage();
    }
    #endregion
    #region Bottom
    private async void openFileButton_Click(object sender, RoutedEventArgs e)
    {
        if (files.Count > 0)
        {
            files.Clear();
            currentFileIndex = 0;
        }
        FileOpenPicker p = new FileOpenPicker();
        var fileTypes = new List<string>()
        {
            ".jpg",
            ".png",
            ".bmp",
            ".ico",
            ".webp"
        };
        foreach (var fileType in fileTypes)
        {
            p.FileTypeFilter.Add(fileType);
        }
        var selectedFiles = await p.PickMultipleFilesAsync();
        if (selectedFiles.Count == 0) return;
        // Update the files collection with selected files
        files = selectedFiles.ToList();
        var image = new BitmapImage();
        image.SetSource(await files[currentFileIndex].OpenAsync(FileAccessMode.Read));
        Image.Source = image;
    }

    private async void previousButton_Click(object sender, RoutedEventArgs e)
    {
        if (Image.Source == null) infoBar.Visibility = Visibility.Visible;
        if (currentFileIndex == 0)
        {
            currentFileIndex = files.Count - 1;
        }
        else
        {
            currentFileIndex--;
        }
        var image = new BitmapImage();
        image.SetSource(await files[currentFileIndex].OpenAsync(FileAccessMode.Read));
        Image.Source = image;
    }

    private async void nextButton_Click(object sender, RoutedEventArgs e)
    {
        if (Image.Source == null) infoBar.Visibility = Visibility.Visible;
        if (currentFileIndex == files.Count - 1)
        {
            currentFileIndex = 0;
        }
        else
        {
            currentFileIndex++;
        }
        var image = new BitmapImage();
        image.SetSource(await files[currentFileIndex].OpenAsync(FileAccessMode.Read));
        Image.Source = image;
    }

    private void infoBar_CloseButtonClick(Microsoft.UI.Xaml.Controls.InfoBar sender, object args)
    {
        infoBar.Visibility = Visibility.Collapsed;
    }
    #endregion
}
