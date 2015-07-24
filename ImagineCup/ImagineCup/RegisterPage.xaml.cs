using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ClientClassLibrary;
using Microsoft.Phone.Controls;

namespace ImagineCup
{
    public partial class RegisterPage : PhoneApplicationPage
    {
        private string _username;//用户姓名
        private string _password;//用户密码
        private string _mail;//用户邮箱
        private SocketClient _client;//用于与服务器通信

        public RegisterPage()
        {
            InitializeComponent();
            _client=(Application.Current as App).SocketClient;
            if(!_client.ConnectState())
                _client.Connect_Server();//连接服务器

        }


        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            //检测用户输入信息是否完整
            if (userNameTextBox.Text == "" || passwordagainBox.Password == "" || passwordagainBox.Password == "" ||
                emailTextBox.Text == "")
                MessageBox.Show("请完善上述信息");
            else
            {
                //对比两次输入的密码是否一致
                if (this.passwordBox.Password.Equals(this.passwordagainBox.Password))
                {
                    _username = this.userNameTextBox.Text;
                    _password = this.passwordBox.Password;
                    _mail = this.emailTextBox.Text;
                    User newUser = new User();
                    newUser.Username = _username;
                    newUser.Password = _password;
                    newUser.Mail = _mail;

                    bool bSuccess = _client.SendRegister(newUser.Username, newUser.Password, newUser.Mail);
                    if (bSuccess)
                        //Debug.WriteLine("注册成功");
                        MessageBox.Show("注册成功");
                    else
                    {
                        //Debug.WriteLine("用户名重复");
                        MessageBox.Show("用户名重复");
                    }                   
                    
                }
                else
                {
                    MessageBox.Show("两次输入的密码不一致，请重新输入！");
                    this.passwordBox.Password = "";
                    this.passwordagainBox.Password = "";
                }
            }

            var uri = "/Images/registerButton.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            registerButton.Background = brush;
        }

        private void userNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {

            var uri = "/Images/blankbackground.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            userNameTextBox.Background = brush;
        }

        private void emailTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/blankbackground.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            emailTextBox.Background = brush;

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

        private void passwordagainBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/blankbackground.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            passwordagainBox.Background = brush;
        }

 
        /// <summary>
        /// 用户离开注册界面时，关闭连接
        /// </summary>
        /// <param name="navigationEventArgs"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs navigationEventArgs)
        {
            
        }

    }
}