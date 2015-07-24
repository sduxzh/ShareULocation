using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Diagnostics;


namespace ImagineCup
{
    public partial class RoutePage : PhoneApplicationPage
    {

        public RoutePage()
        {
            InitializeComponent();
            
            App app = Application.Current as App;
            if (PhoneApplicationService.Current.State.ContainsKey("start"))
            {
                myLocationTextBox.Text = (string)PhoneApplicationService.Current.State["start"];
                
            }
            if (PhoneApplicationService.Current.State.ContainsKey("end"))
            {
                toLocationTextBox.Text = (string)PhoneApplicationService.Current.State["end"];
                
            }
   
        }

        

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            var uri1 = "/Images/add.png";
            Uri imgUri1 = new Uri(uri1, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource1 = new BitmapImage(imgUri1);
            ImageBrush brush1 = new ImageBrush();
            brush1.ImageSource = imgSource1;

            var uri2 = "/Images/close.png";
            Uri imgUri2 = new Uri(uri2, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource2 = new BitmapImage(imgUri2);
            ImageBrush brush2 = new ImageBrush();
            brush2.ImageSource = imgSource2;



            if (toLocationTextBox.Margin == new Thickness(0, 60, 50, 0))
            {
                toLocationTextBox.Margin = new Thickness(0, 120, 50, 0);
                passbyLocation.Visibility = Visibility.Visible;
                addButton.Background = brush2;
            }
            else if (toLocationTextBox.Margin == new Thickness(0, 120, 50, 0))
            {
                toLocationTextBox.Margin = new Thickness(0, 60, 50, 0);
                passbyLocation.Visibility = Visibility.Collapsed;
                addButton.Background = brush1;
            }
            else if(toLocationTextBox.Margin==new Thickness(0,0,50,0)){
                if (myLocationTextBox.Margin == new Thickness(0, 60, 50, 0))
                {
                    myLocationTextBox.Margin = new Thickness(0, 120, 50, 0);
                    passbyLocation.Visibility = Visibility.Visible;
                    addButton.Background = brush2;
                }
                else if (myLocationTextBox.Margin == new Thickness(0, 120, 50, 0)) {
                    myLocationTextBox.Margin = new Thickness(0, 60, 50, 0);
                    passbyLocation.Visibility = Visibility.Collapsed;
                    addButton.Background = brush1;
                }
            }
            
        }

        private void exchangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (toLocationTextBox.Margin == new Thickness(0,60,50,0))
            {
                toLocationTextBox.Margin = new Thickness(0,0,50,0);
                myLocationTextBox.Margin = new Thickness(0,60,50,0);
            }
            else if (toLocationTextBox.Margin == new Thickness(0,0,50,0))
            {
                if (myLocationTextBox.Margin == new Thickness(0, 60, 50, 0))
                {
                    toLocationTextBox.Margin = new Thickness(0, 60, 50, 0);
                    myLocationTextBox.Margin = new Thickness(0, 0, 50, 0);
                }
                else if (myLocationTextBox.Margin == new Thickness(0, 120, 50, 0)) {
                    toLocationTextBox.Margin = new Thickness(0,120,50,0);
                    myLocationTextBox.Margin = new Thickness(0, 0, 50, 0);
                }
          
            }
            else if (toLocationTextBox.Margin == new Thickness(0,120,50,0)) {
                toLocationTextBox.Margin = new Thickness(0,0,50,0);
                myLocationTextBox.Margin = new Thickness(0,120,50,0);
            }

            var uri = "/Images/exchangeButton.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            exchangeButton.Background = brush;
        }

        private void myLocationTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var uri= "/Images/locationtextbox.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource= new BitmapImage(imgUri);
            ImageBrush brush= new ImageBrush();
            brush.ImageSource = imgSource;
            myLocationTextBox.Background = brush;

            App app = Application.Current as App;
            NavigationService.Navigate(new Uri("/SearchPage.xaml", UriKind.RelativeOrAbsolute));
            
        }

        private void toLocationTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/locationtextbox.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            toLocationTextBox.Background = brush;

            App app = Application.Current as App;

            NavigationService.Navigate(new Uri("/SearchPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void passbyLocation_GotFocus(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/locationtextbox.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            passbyLocation.Background = brush;
        }

        private void goHomeTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/locationtextbox.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            goHomeTextBox.Background = brush;
        }

        private void goBusinessTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/blankbackground.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            goBusinessTextBox.Background = brush;
        }

        private void removeHistoryTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/blankbackground.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            removeHistoryTextBox.Background = brush;
        }

        private void goBackButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/gobackButton.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            goBackButton.Background = brush;
            goBackButton.Background.Opacity = 0.5;

        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/searchButton.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            searchButton.Background = brush;
        }

        /// <summary>
        /// 修改搜索界面后退键,使其导航到MainPage
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            e.Cancel = false;
            this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            (Application.Current as App).IsFromRoutePage = true;
        }

    }
    
}