using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace MediaBrowser.Dialogs
{
    public sealed partial class RebootDialog : ContentDialog
    {
        public RebootDialog()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            MainPage mainPage = new MainPage();

            // Attempt restart, with arguments.
            AppRestartFailureReason result =
                await CoreApplication.RequestRestartAsync("-fastInit -level 1 -foo");

            // Restart request denied, send a toast to tell the user to restart manually.
            if (result == AppRestartFailureReason.NotInForeground
                || result == AppRestartFailureReason.Other)
            {
                
                TimeSpan period = TimeSpan.FromSeconds(60);

                ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer((source) =>
                {

                }, period);
            }
        }
    }
}
