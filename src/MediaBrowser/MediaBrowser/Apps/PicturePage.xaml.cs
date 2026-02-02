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
namespace MediaBrowser.Apps;

public sealed partial class PicturePage : Page
{
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
        
    }

    private async void previousButton_Click(object sender, RoutedEventArgs e)
    {
        
    }

    private async void nextButton_Click(object sender, RoutedEventArgs e)
    {
        
    }

    private void infoBar_CloseButtonClick(Microsoft.UI.Xaml.Controls.InfoBar sender, object args)
    {
        infoBar.Visibility = Visibility.Collapsed;
    }
    #endregion
}
