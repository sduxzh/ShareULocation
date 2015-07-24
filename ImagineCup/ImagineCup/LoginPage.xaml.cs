using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ImagineCup
{
    public partial class LoginPage : PhoneApplicationPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void usernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/blankbackground.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            usernameTextBox.Background = brush;
        }

        private void passwordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/blankbackground.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            passwordBox.Background = brush;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/registerButton.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            RegisterButton.Background = brush;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/loginButton.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            LoginButton.Background = brush;
        }

        private void usernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }



        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/AfterLoginPage.xaml", UriKind.RelativeOrAbsolute));
        }



    }
}