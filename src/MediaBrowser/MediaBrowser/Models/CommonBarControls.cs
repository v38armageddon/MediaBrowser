using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI.ViewManagement;

namespace MediaBrowser.Models;

public class CommonBarControls
{
    public static void ExitApplication()
    {
        App.Current.Exit();
    }

    public static void ToggleFullScreen()
    {
        var app = App.Current as App;
        if (app?.MainWindow is not null)
        {
            var myWindow = app.MainWindow.AppWindow;
            if (myWindow.Presenter.Kind != AppWindowPresenterKind.FullScreen)
            {
                myWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
            }
            else
            {
                myWindow.SetPresenter(AppWindowPresenterKind.Default);
            }
        }
    }

    public static void ReturnPreviousPage()
    {
        Frame rootFrame = Window.Current.Content as Frame;
        rootFrame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
    }

    public static void NavigateHomePage()
    {
        Frame rootFrame = Window.Current.Content as Frame;
        rootFrame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
    }
}
