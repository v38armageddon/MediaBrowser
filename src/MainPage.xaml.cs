using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace FM_Center
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        // Top
        private void buttonReduce_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void buttonWindow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
