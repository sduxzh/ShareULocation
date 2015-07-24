using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Com.AMap.Api.Maps;
using Com.AMap.Api.Maps.Model;
using Com.AMap.Api.Services;
using Com.AMap.Api.Services.Results;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace ImagineCup
{
    public partial class SearchPage : PhoneApplicationPage
    {
        double _transToMainPageLat;//传到主界面标识maker
        double _transToMainPageLng;//传到主界面标识maker
        public SearchPage()
        {
            InitializeComponent();
            if (SearchTextBox == null) return;
            SearchTextBox.TextChanged += Search_Changed;//搜索框内容变化时调用

        }

        /// <summary>
        /// 搜索框获取焦点时将“搜索”删掉
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "搜索...")
                SearchTextBox.Text = "";
        }

        /// <summary>
        /// 搜索框失去焦点时，根据内容判定是否将“搜索”呈现
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "")
                SearchTextBox.Text = "搜索...";
        }

        private void Search_Changed(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox.Text.Equals(""))
            {
                return;
            }
            if (SearchTextBox.Text != "搜索...")
                GetAmapInputTips(SearchTextBox.Text);
        }

        /// <summary>
        /// 根据提示获搜索信息
        /// </summary>
        /// <param name="words"></param>
        public async void GetAmapInputTips(string words)
        {
            
            var tipResults = await AMapInputTips.InputTips(words);

            if (tipResults.Erro == null && tipResults.TipList != null)
            {
                if (tipResults.TipList.Count == 0) //TipList数组中存放 AMapTip 对象(Adcode区域编码\District区域名称\Name提示信息)
                {
                    //MessageBox.Show("无查询结果");
                    return;
                }

                //绑定列表数据
                ResultOfRearchListBox.ItemsSource = tipResults.TipList.ToList();
            }
            else
            {
                if (tipResults.Erro != null) MessageBox.Show(tipResults.Erro.Message);
            }

        }
        /// <summary>
        /// ListBox条目单击事件，获取到搜索条目名称，将条目值传入主界面时
        /// 进而进行地图相关操作（获取地理位置，标记maker等）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ResultOfRearchListBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (ResultOfRearchListBox.SelectedItem != null)
            {
                AMapTip aMapTip = (AMapTip)ResultOfRearchListBox.SelectedItem;
                await AddressToGeoCode(aMapTip.Name);
            }
            //Debug.WriteLine(aMapTip.District); 
            //Debug.WriteLine(aMapTip.Name);         
        }

        /// <summary>
        /// 根据搜索的条目名称获取该条目的经纬度，在主界面标识maker
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private async Task AddressToGeoCode(string address)
        {
            AMapGeoCodeResult cr = await AMapGeoCodeSearch.AddressToGeoCode(address);
            Dispatcher.BeginInvoke(() =>
            {
                IEnumerable<AMapGeoCode> geocode = cr.GeoCodeList;
                AMapGeoCode aMapGeoCode = null;
                foreach (var gcs in geocode)
                {
                    aMapGeoCode = gcs;
                }
                if (aMapGeoCode != null)
                {
                    App app = Application.Current as App;
                    if (app != null) app.IsFromSearchPage = true;
                    _transToMainPageLat = aMapGeoCode.Location.Lat;
                    _transToMainPageLng = aMapGeoCode.Location.Lon;
                    Debug.WriteLine("检索到的数据为：{0}，{1}", _transToMainPageLat, _transToMainPageLng);
                    string myUrl = string.Format("/MainPage/{0}/{1}/{2}", _transToMainPageLat, _transToMainPageLng, address);
                    NavigationService.Navigate(new Uri(myUrl, UriKind.Relative));
                }
            });
        }

        /// <summary>
        /// 每次进入搜索页面,自动清空ResultOfRearchListBox内容
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ResultOfRearchListBox.ItemsSource = null;
        }

        /// <summary>
        /// 修改搜索界面后退键,使其导航到MainPage
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if ((Application.Current as App).IsFromRoutePage)
            {
                e.Cancel = false;
                this.NavigationService.Navigate(new Uri("/RoutePage.xaml", UriKind.Relative));
            }
            else
            {
                e.Cancel = false;
                this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
            

        }

    }
}