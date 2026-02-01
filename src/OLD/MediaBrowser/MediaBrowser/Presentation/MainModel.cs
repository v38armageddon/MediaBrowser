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
namespace MediaBrowser.Presentation;

public partial record MainModel
{
    private readonly INavigator _navigator;

    public MainModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator)
    {
        _navigator = navigator;
        Title = "Main";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";
    }

    public string? Title { get; }

    public IState<bool> IsFullScreen => State<bool>.Value(this, () => false);
    
    public IState<int> SelectedPivotIndex => State<int>.Value(this, () => 1);

    public async ValueTask ToggleFullScreen()
    {
        var currentState = await IsFullScreen;
        await IsFullScreen.Update(_ => !currentState);
    }

    public async ValueTask CloseApp()
    {
        // Close application
        Application.Current.Exit();
    }

    public async ValueTask GoBack()
    {
        await _navigator.NavigateBackAsync(this);
    }

    public async ValueTask GoHome()
    {
        await _navigator.NavigateViewModelAsync<MainModel>(this);
    }

    public async ValueTask Exit()
    {
        Application.Current.Exit();
    }

    public async ValueTask ShowAbout()
    {
        // TODO: Navigate to About page
        // await _navigator.NavigateViewModelAsync<AboutModel>(this);
    }

    public async ValueTask OpenMyPictures()
    {
        // TODO: Navigate to Pictures page
        // await _navigator.NavigateViewModelAsync<PicturesModel>(this);
    }

    public async ValueTask OpenCamera()
    {
        // TODO: Navigate to Camera page
        // await _navigator.NavigateViewModelAsync<CameraModel>(this);
    }

    public async ValueTask OpenMyMusic()
    {
        // TODO: Navigate to Music page
        // await _navigator.NavigateViewModelAsync<MusicModel>(this);
    }

    public async ValueTask OpenMyVideos()
    {
        // TODO: Navigate to Videos page
        // await _navigator.NavigateViewModelAsync<VideosModel>(this);
    }

    public async ValueTask OpenBingMap()
    {
        // TODO: Navigate to BingMap page
        // await _navigator.NavigateViewModelAsync<BingMapModel>(this);
    }
}
