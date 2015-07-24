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
    public partial class AfterLoginPage : PhoneApplicationPage
    {
        public AfterLoginPage()
        {
            InitializeComponent();
        }

        private void guessyourlikeButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/gridborder.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            guessyourlikeButton.Background = brush;
        }

        private void dtknow1_Click(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/gridborder.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            dtknow1.Background = brush;
            
        }

        private void dtknow2_Click(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/gridborder.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            dtknow2.Background = brush;
            
        }

     

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/MainPage.xaml",UriKind.RelativeOrAbsolute));
        } 






 


    }
}