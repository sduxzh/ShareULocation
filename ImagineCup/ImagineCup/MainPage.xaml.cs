using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using ClientClassLibrary;
using Com.AMap.Api.Maps;
using Com.AMap.Api.Maps.Model;
using Com.AMap.Api.Services;
using Com.AMap.Api.Services.Results;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using AMapPolygon = Com.AMap.Api.Services.Results.AMapPolygon;
using Extensions = Com.AMap.Api.Services.Extensions;
using ClientClassLibrary;
using Point = System.Windows.Point;

namespace ImagineCup
{
    public partial class MainPage : PhoneApplicationPage
    {
        public delegate void ConnectSrv(SocketClient socket);
        private  SocketClient _client;
        private AMap _amap; //地图
        private AMapCircle _circle; //定位点周围圆圈 
        protected LatLng _latlng; //实时位置经纬度
        protected LatLng _cameraLatlng;//当前视野中心经纬度
        protected LatLng NavigateEndLngLng;//记录导航结束点
        protected LatLng TabLatLng;//用户标识位置经纬度
        private LatLng _researchLatLng;//搜索界面返回的经纬度
        private LatLng _avoidLatLng;//规避点经纬度
        private LatLng _daBeiJing;//屏蔽大北京的经纬度
        private AMapGeolocator _mylocation; //获取用户位置
        private AMapMarker _locationMarker; //标注点（实时定位）
        private AMapMarker _researchMarker;//搜索结果
        private AMapMarker _tabMarker;//标注点（用户手动标识）
        private string _address;//存储搜索结果地址
        private List<AMapPolyline> _aMapPolyline;//导航路线
        private List<AMapMarker>_navigationMarker;//导航标识
        private AMapReGeoCodeResult amapHoldResult;//用户长按标识AMapReGeoCodeResult
        
        private bool isExit;//双击返回键退出变量
        private DispatcherTimer dt;

        //手动实现地图缩放控件变量
        /*****************************************
         ****************************************/
        private float _mapZoom; //地图比例尺
        private float _bearing;//旋转角度
        private float _tile;//水平、竖直角度
        /*****************************************
        ****************************************/
        private bool _zoomIsNew;//初次进入地图时的地图缩放比例(地图初始化时为true)
        private bool _cameraIsNew;//防止查看地图其他部分时视角被拉回所在实时位置(地图初始化时为true)
        private bool _isNew;//屏蔽大北京
        // 构造函数
        public MainPage()
        {
            
            InitializeComponent();
            /******变量初始化*****/
            _zoomIsNew = true;
            _cameraIsNew = true;
            _isNew = true; 
            isExit = false;
            _client = (Application.Current as App).SocketClient;
            /******变量初始化*****/

            

            _amap = (Application.Current as App).TempMap;
            if (_amap == null)
            {
                _amap=new AMap();//新建地图
            }
            if (_amap != null)
            {
                ContentPanel.Children.Add(_amap); //加载地图
                //恢复地图相关参数
                #region
                _latlng = (Application.Current as App)._latlng;
                _bearing = (Application.Current as App)._bearing;
                _tile = (Application.Current as App)._tile;
                _circle = (Application.Current as App)._circle;
                _locationMarker = (Application.Current as App)._locationMarker;
                _cameraLatlng = (Application.Current as App)._CameraLatLng;
                _mapZoom = (Application.Current as App)._mapZoom;
                _cameraIsNew = (Application.Current as App)._cameraIsNew;
                _isNew = (Application.Current as App)._isNew;
                #endregion

                DoubleTabToExit(2);//按两次退出键退出程序

                //异步连接服务器，增加程序友好性
                Dispatcher.BeginInvoke(() =>
                {
                    Debug.WriteLine("连接服务器");
                    ConnectServer(_client);
                }
                    );

            }

            var uiset = _amap.GetUiSettings();
            uiset.CompassControlEnabled = false; //将原有指南针设置为不显示
            uiset.ZoomControlsEnabled = false; //将原有缩放控件设置为不显示
            MakerTextBlock.Visibility = Visibility.Collapsed;//设置标记信息文本框不可见
            MakertextblockbgGrid.Visibility = Visibility.Collapsed;//设置标记信息文本框背景不可见
            BuildLocalizedApplicationBar();// 用于本地化 ApplicationBar 的示例代码
            //ApplicationBar.IsVisible = false;

            _amap.CameraChangeListener += amap_CameraChangeListener; //视角移动函数（指南针使用）
            _amap.Tap += amap_Tap;//添加地图单击事件函数
            _amap.Hold += amap_hold;//添加地图长按事件函数   
            Loaded += Mylocation_Loaded;
            Unloaded += Mylocation_Unloaded;
            

        }

        /// <summary>
        /// 异步连接服务器
        /// </summary>
        /// <param name="socketClient"></param>
        private async void ConnectServer(SocketClient socketClient)
        {
            if (!_client.ConnectState())
                new Task(() => { socketClient.Connect_Server(); }).RunSynchronously();
 
            if (_client.ConnectState())
                Debug.WriteLine("服务器连接成功");

            else
            {
                Debug.WriteLine("服务器连接失败");
            }
        }


        /// <summary>
        /// 初始化主界面下方ApplicationBar
        /// </summary>
        private void BuildLocalizedApplicationBar()
        {
            // 将页面的 ApplicationBar 设置为 ApplicationBar 的新实例。
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 80;

            //路线button
            ApplicationBarIconButton routeButton = new ApplicationBarIconButton(new Uri("/Images/luxian.png", UriKind.Relative));
            routeButton.Text = "路线";
            routeButton.Click += new EventHandler(routeButton_Click);
            ApplicationBar.Buttons.Add(routeButton);
            //导航button
            ApplicationBarIconButton navigateButton = new ApplicationBarIconButton(new Uri("/Images/daohang000.png", UriKind.Relative));
            navigateButton.Text = "导航";
            ApplicationBar.Buttons.Add(navigateButton);


            //使用 AppResources 中的本地化字符串创建新菜单项。
            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem("公交");
            ApplicationBar.MenuItems.Add(appBarMenuItem);
            ApplicationBarMenuItem appBarMenuItem1 = new ApplicationBarMenuItem("离线资源");
            ApplicationBar.MenuItems.Add(appBarMenuItem1);
        }

        
        private void Mylocation_Loaded(object sender, RoutedEventArgs e)
        {
            //设置定位为高精度定位,更新间隔为0.1s,
            _mylocation = new AMapGeolocator(PositionAccuracy.High, 1, 100);
            _mylocation.Start();
            if (_isNew)
            {
                //定位用户位置，屏蔽大北京
                IAsyncOperation<Geoposition> getCurrentPosition = _mylocation.GetGeopositionAsync(new TimeSpan(1), new TimeSpan(3000));
                getCurrentPosition.Completed += GetCurrentPosition;
                _isNew = false;
            }
            
            //触发位置改变事件
            _mylocation.PositionChanged += mylocation_PositionChanged;
            

        }

        /// <summary>
        /// 获取用户位置后，触发本函数
        /// </summary>
        /// <param name="asyncInfo"></param>
        /// <param name="asyncStatus"></param>
        private void GetCurrentPosition(IAsyncOperation<Geoposition> asyncInfo, AsyncStatus asyncStatus)
        {
            Debug.WriteLine("获取到位置信息");
            Geoposition currentGeoposition =asyncInfo.GetResults();
            Geocoordinate coordinate= currentGeoposition.Coordinate;
            //获取的数据有偏差
            
            LatLng currentLatLng = new LatLng(coordinate.Latitude + 0.000423, coordinate.Longitude + 0.006090);
            _daBeiJing = currentLatLng;
            Debug.WriteLine(currentLatLng);
            
                if(_amap!=null)
                {
                    _amap.Dispatcher.BeginInvoke(() =>
                    {
                        _amap.AnimateCamera(CameraUpdateFactory.NewCameraPosition(currentLatLng, 17, _bearing, _tile), 2);//移动视角(动画效果)
                        _cameraIsNew = false;
                        if (_locationMarker == null)
                        {
                            //添加圆
                            _circle = _amap.AddCircle(new AMapCircleOptions
                            {
                                Center = currentLatLng, //圆点位置
                                Radius = (float)20, //半径
                                FillColor = Color.FromArgb(80, 100, 150, 255),//圆的填充颜色
                                StrokeWidth = 1, //边框粗细
                                StrokeColor = Color.FromArgb(80, 100, 150, 255) //圆的边框颜色
                            });


                            //添加点标注，用于标注地图上的点
                            _locationMarker = _amap.AddMarker(new AMapMarkerOptions
                            {
                                Position = currentLatLng, //图标的位置
                                //待修改，更换IconUri的图标//
                                IconUri = new Uri("Images/myLocationIcon.png", UriKind.RelativeOrAbsolute), //图标的URL
                                Anchor = new Point(0.5, 0.5) //图标中心点
                            });
                        }
                        else
                        {
                            //点标注和圆的位置在当前经纬度
                            _locationMarker.Position = currentLatLng;
                            _circle.Center = currentLatLng;
                            _circle.Radius = (float)20; //圆半径
                        }
                    });
                    
                    
                }
            
            


        }

        
        /// <summary>
        ///     用户位置改变后marker绘制函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void mylocation_PositionChanged(AMapGeolocator sender, AMapPositionChangedEventArgs args)
        {
            _latlng = args.LngLat;//系统定位经纬度


            _amap.Dispatcher.BeginInvoke(() =>
            {
                //GeoSearch(args.LngLat);

                if (_locationMarker == null)
                {
                    //添加圆
                    _circle = _amap.AddCircle(new AMapCircleOptions
                    {
                        Center = args.LngLat, //圆点位置
                        Radius = (float)20, //半径
                        FillColor = Color.FromArgb(80, 100, 150, 255),//圆的填充颜色
                        StrokeWidth = 1, //边框粗细
                        StrokeColor = Color.FromArgb(80, 100, 150, 255) //圆的边框颜色
                    });


                    //添加点标注，用于标注地图上的点
                    _locationMarker = _amap.AddMarker(new AMapMarkerOptions
                    {
                        Position = args.LngLat, //图标的位置
                        //待修改，更换IconUri的图标//
                        IconUri = new Uri("Images/myLocationIcon.png", UriKind.RelativeOrAbsolute), //图标的URL
                        Anchor = new Point(0.5, 0.5) //图标中心点
                    });
                }
                else
                {
                    //点标注和圆的位置在当前经纬度
                    _locationMarker.Position = args.LngLat;
                    _circle.Center = args.LngLat;
                    _circle.Radius = (float)20; //圆半径
                }

                //设置当前地图的经纬度和缩放级别
                if (_cameraIsNew)
                {
                    if (_mapZoom == 0)
                    _mapZoom = 17;
                    _amap.AnimateCamera(CameraUpdateFactory.NewCameraPosition(_latlng, _mapZoom, _bearing, _tile), 2);//移动视角(动画效果)
                    _cameraIsNew = false;
                    
                }

                Debug.WriteLine("定位精度：" + args.Accuracy + "米");
                Debug.WriteLine("定位经纬度：" + args.LngLat);

            });

            AMapReGeoCodeResult currentLocationResult;//用户实时位置AMapReGeoCodeResult
            currentLocationResult = await AMapReGeoCodeSearch.GeoCodeToAddress(_latlng.longitude, _latlng.latitude, 50, "", Extensions.All);
            Dispatcher.BeginInvoke(() =>
            {
                if (currentLocationResult.Erro == null && currentLocationResult.ReGeoCode != null)
                {
                    AMapReGeoCode regeocode = currentLocationResult.ReGeoCode;
                    MakerTextBlock.Text = regeocode.Formatted_address;
                    Debug.WriteLine("道路信息");
                    foreach (var temp in regeocode.Roadslist)
                    {
                        Debug.WriteLine(temp.Id);
                        
                        _client.SendLocation(temp.Id,temp.Name, args.LngLat.latitude.ToString(CultureInfo.InvariantCulture), args.LngLat.longitude.ToString(CultureInfo.InvariantCulture));
                        break;
                    }

                }
            }
                );
        }

        private void Mylocation_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_mylocation == null) return;
            _mylocation.PositionChanged -= mylocation_PositionChanged;
            _mylocation.Stop();
        }


        /// <summary>
        ///     地图视窗参数捕捉函数（指南针）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void amap_CameraChangeListener(object sender, AMapEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                ((RotateTransform)Compass.RenderTransform).Angle = -(e.CameraPosition.bearing); //获取旋转坐标，实现指南针功能
                //获取重建缩放功能相关变量
                if (_zoomIsNew)
                {
                    _mapZoom = 17;
                    _zoomIsNew = false;
                }
                else
                {
                    _mapZoom = e.CameraPosition.zoom; //缩放级别   
                }
                //当前视野中心的相关参数
                _cameraLatlng = e.CameraPosition.target; //经纬度
                _tile = e.CameraPosition.tilt; //地图倾角
                _bearing = e.CameraPosition.bearing; //旋转角度  
            });
        }

        /// <summary>
        /// 视野扩大
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/add000.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            Add.Background = brush;
            _mapZoom += 1;
            var cameraupdate = CameraUpdateFactory.NewCameraPosition(_cameraLatlng, _mapZoom, _bearing, _tile);
            _amap.MoveCamera(cameraupdate);
        }

        /// <summary>
        /// 视野缩小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Minus_Click(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/minus.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            Minus.Background = brush;
            _mapZoom -= 1;
            var cameraupdate = CameraUpdateFactory.NewCameraPosition(_cameraLatlng, _mapZoom, _bearing, _tile);
            _amap.MoveCamera(cameraupdate);
        }

        /// <summary>
        /// button_click_重定位到当前位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Relocate_Click(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/daohang.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            RelocateButton.Background = brush;
            _amap.AnimateCamera(CameraUpdateFactory.NewCameraPosition(_latlng, _mapZoom, _bearing, _tile), 2);//移动视角(动画效果)
        }

        /// <summary>
        /// 地图单击函数
        /// 清空用户手动标识maker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void amap_Tap(object sender, GestureEventArgs e)
        {
            if (_researchMarker != null)
            {
                if (_tabMarker != null)
                    _tabMarker.Destroy();
                MakerTextBlock.Text = _address;
            }
            else
            {
                if (_tabMarker != null)
                    _tabMarker.Destroy();
                DisShowDownStateSettings();
            }
        }

        /// <summary>
        /// 标记后显示该位置的相关信息（地理位置名称等信息）
        ///地图maker标记函数（长按）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void amap_hold(object sender, GestureEventArgs e)
        {
            //获取用户标识点所在位置经纬度
            TabLatLng = _amap.GetProjection().FromScreenLocation((e.GetPosition(_amap)));
            //删除用户先前标识的_tabMaker
            if (_tabMarker != null)
               _tabMarker.Destroy();
            //创建新的_tabMaker
             _tabMarker = _amap.AddMarker(new AMapMarkerOptions()
                {
                    Position = TabLatLng,//maker添加位置
                    IconUri = new Uri("Images/RED.png", UriKind.RelativeOrAbsolute)//maker图标
                });

            NavigateEndLngLng = TabLatLng;
            //纠正定位偏差，将搜索半径设置为50米
            amapHoldResult = await AMapReGeoCodeSearch.GeoCodeToAddress(TabLatLng.longitude, TabLatLng.latitude,50, "", Extensions.All);
            //amapHoldResult = await AMapReGeoCodeSearch.GeoCodeToAddress(TabLatLng.longitude, TabLatLng.latitude);
            Dispatcher.BeginInvoke(() =>
            {
                if (amapHoldResult.Erro == null && amapHoldResult.ReGeoCode != null)
                {
                    AMapReGeoCode regeocode = amapHoldResult.ReGeoCode;
                    ShowDownStateSettings();
                    MakerTextBlock.Text = regeocode.Formatted_address;
                    
                    
                }
            }
                );
          
            
        }


        /// <summary>
        /// 主界面图标(定位、视野放大、缩小)上移函数
        /// </summary>
        private void ShowDownStateSettings()
        {
            MakerTextBlock.Visibility = Visibility.Visible;//设置标记信息文本框可见
            Debug.WriteLine(MakerTextBlock.Visibility);
            MakertextblockbgGrid.Visibility = Visibility.Visible;//设置标记信息文本框背景可见
            //设置地图上的重定位、放大、缩小按钮的位置
           Add.SetValue(MarginProperty, new Thickness(0, 0, 15, 135));
           Minus.SetValue(MarginProperty, new Thickness(0, 0, 15, 90));
           RelocateButton.SetValue(MarginProperty, new Thickness(10, 0, 0, 105));
        }

        /// <summary>
        /// 主界面图标(定位、视野放大、缩小)上移函数
        /// </summary>
        private void DisShowDownStateSettings()
        {
            MakerTextBlock.Visibility = Visibility.Collapsed;//设置标记信息文本框不可见
            MakertextblockbgGrid.Visibility = Visibility.Collapsed;//设置标记信息文本框背景不可见
            Add.SetValue(MarginProperty, new Thickness(0, 0, 15, 70));
            Minus.SetValue(MarginProperty, new Thickness(0, 0, 15, 25));
            RelocateButton.SetValue(MarginProperty, new Thickness(10, 0, 0, 60));
        }

        /// <summary>
        /// 点击重定位，刷新图标，营造动态效果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RelocateButton_GotFocus(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/daohang.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            RelocateButton.Background = brush;
        }

        private void RelocateButton_LostFocus(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/location002.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            RelocateButton.Background = brush;
        }

        /// <summary>
        /// 地图退出函数
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            //识别是否从搜索界面进入
            if (_researchMarker == null)
            {
                if (!isExit)
                {
                    isExit = true;
                    dt.Start();
                    e.Cancel = true; 
                }
                else
                {
                    dt.Stop();
                    //清空导航历史记录
                    while (NavigationService.CanGoBack)
                    {
                        NavigationService.RemoveBackEntry();
                    }
                    //删除地图
                    _amap.Destory();
                    _client.Disconnect();//关闭与服务器连接

                }
            }
            
            base.OnBackKeyPress(e);   
        }

        /// <summary>
        /// 双击退出键退出地图
        /// </summary>
        /// <param name="timeSpan"></param>
        private void DoubleTabToExit(int timeSpan)
        {
            dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(timeSpan);
            dt.Tick += (s, e) =>
            {
                if (isExit)
                {
                    isExit = false;
                    dt.Stop();
                }
            };
        }

        /// <summary>
        /// 行车路线规划
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private async void GetNavigationDriving(LatLng start, LatLng end)   //路线规划
        {
            
            AMapRouteResults rts;
            if (_avoidLatLng==null) //不做路径规避的路线
            {
                //距离最短导航方式
                rts = await AMapNavigationSearch.DrivingNavigation(start.longitude, start.latitude, end.longitude, end.latitude, 2, null, null, null, null, null, Extensions.All);

            }
            else  //做路径规避的路线
            {
                Debug.WriteLine("Avoidlatlng经纬度" + _avoidLatLng.longitude + " " + _avoidLatLng.latitude);
                List<AMapLocation> locations = computeAvoidLocations(_avoidLatLng);//获取规避区域
                AMapPolygon polygon = new AMapPolygon();
                polygon._Type = AMapPolygon.Types.Type.POLYGON;
                polygon.Points = locations;
                List<AMapPolygon> polygons = new List<AMapPolygon>();
                polygons.Add(polygon);
                rts = await AMapNavigationSearch.DrivingNavigation(start.longitude, start.latitude, end.longitude, end.latitude, 2, null, null, null, polygons, null, Extensions.All);
            
            }

            if (rts.Erro == null)
            {
                if (rts.Count == 0)
                {
                    MessageBox.Show("无查询结果");
                    return;
                }

                AMapRoute route = rts.Route;
                List<AMapPath> paths = route.Paths.ToList();
                foreach (AMapPath item in paths)
                {
                    
                    Debug.WriteLine("起点终点距离:" + item.Distance);
                    Debug.WriteLine("预计耗时:" + item.Duration / 60 + "分钟");
                    Debug.WriteLine("导航策略:" + item.Strategy); 

                    
                    List<AMapStep> steps = item.Steps.ToList();
                    List<RoadState> roadStates=new List<RoadState>();
                    string avoid_roadname = null;
                    //查询每一条道路
                    foreach (var temp in steps)
                    {
                        if(temp.Road=="")
                            continue;
                        RoadState tempState=new RoadState();
                        tempState.RoadName = temp.Road;
                        tempState.State = true;
                        tempState.State = _client.AcquireRoadState(temp.Road);
                        roadStates.Add(tempState);
                    }
                    //寻找第一条规避的路径
                    foreach (var road in roadStates)
                    {
                        if (road.State == false)
                        {
                            avoid_roadname = road.RoadName;
                            break;
                        }

                    }

                    GetNavigationDriving_Avoid(start, end, avoid_roadname);
                }
            }
            else
            {
                MessageBox.Show(rts.Erro.Message);
            }
            
        }

        /// <summary>
        /// 规避某条道路导航
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="roadname"></param>
        private async void GetNavigationDriving_Avoid(LatLng start, LatLng end,string roadname)
        {
            //roadname = "舜华东路";
            AMapRouteResults rts;
            //距离最短导航方式
            rts = await AMapNavigationSearch.DrivingNavigation(start.longitude, start.latitude, end.longitude, end.latitude, 2, null, null, null, null,roadname, Extensions.All);
            if (rts.Erro == null)
            {
                if (rts.Count == 0)
                {
                    MessageBox.Show("无查询结果");
                    return;
                }

                AMapRoute route = rts.Route;
                List<AMapPath> paths = route.Paths.ToList();
                foreach (AMapPath item in paths)
                {

                    Debug.WriteLine("起点终点距离:" + item.Distance);
                    Debug.WriteLine("预计耗时:" + item.Duration / 60 + "分钟");
                    Debug.WriteLine("导航策略:" + item.Strategy);


                    List<AMapStep> steps = item.Steps.ToList();
                    DrawNavigationRoute(steps);//绘制驾车路线
                }
            }
            else
            {
                MessageBox.Show(rts.Erro.Message);
            }
        }

        /// <summary>
        /// 绘制导航路线
        /// </summary>
        /// <param name="steps"></param>
        private void DrawNavigationRoute(List<AMapStep> steps)
        {
            List<AMapPolyline> _aMapPolyline = new List<AMapPolyline>();//导航路线
            List<AMapMarker> _navigationMarker = new List<AMapMarker>();//导航标识

            //删除上一次导航标识的路线
            if (this._aMapPolyline != null)
            {
                var temp = this._aMapPolyline.Count;
                for (int i = 0; i < temp; i++)
                {
                    this._aMapPolyline[i].Destroy();
                    this._navigationMarker[i].Destroy();

                }
            }

            foreach (AMapStep st in steps)
            {
                Debug.WriteLine(st.Instruction);

                //Debug.WriteLine(st.Road);
                //Debug.WriteLine(st.Assistant_action);


                var tempMarker = _amap.AddMarker(new AMapMarkerOptions()
                {
                    Position = latLagsFromString(st.Polyline).FirstOrDefault(),
                    Title = "",
                    Snippet = "Snippet",
                    IconUri = new Uri("Images/car.png", UriKind.Relative),
                });
                _navigationMarker.Add(tempMarker);

                if (_avoidLatLng == null)
                {
                    var tempLine = _amap.AddPolyline(new AMapPolylineOptions()
                    {
                        Points = latLagsFromString(st.Polyline),
                        Color = Color.FromArgb(255, 0, 0, 255),
                        Width = 4,
                    });
                    _aMapPolyline.Add(tempLine);
                } //count=0时 画蓝线

                else
                {
                    var tempLine = _amap.AddPolyline(new AMapPolylineOptions()
                    {
                        Points = latLagsFromString(st.Polyline),
                        Color = Color.FromArgb(255, 255, 0, 0),
                        Width = 4,
                    });
                    _aMapPolyline.Add(tempLine);
                }
            }
            this._aMapPolyline = _aMapPolyline;
            this._navigationMarker = _navigationMarker;
        }
        private List<LatLng> latLagsFromString(string polyline)
        {
            List<LatLng> latlng = new List<LatLng>();

            string[] arrystring = polyline.Split(new char[] { ';' });
            foreach (String str in arrystring)
            {
                String[] lnglatds = str.Split(new char[] { ',' });
                latlng.Add(new LatLng(Double.Parse(lnglatds[1]), Double.Parse(lnglatds[0])));
            }
            return latlng;

        }

        
        /// <summary>
        /// 根据规避点计算规避区域
        /// </summary>
        /// <returns></returns>
        private List<AMapLocation> computeAvoidLocations(LatLng avoidLatLng)
        {
            double distance = 0.001;//规避区域参数
            List<AMapLocation> avoidLocations = new List<AMapLocation>();
            avoidLocations.Add(new AMapLocation(avoidLatLng.longitude - distance, avoidLatLng.latitude - distance));
            avoidLocations.Add(new AMapLocation(avoidLatLng.longitude - distance, avoidLatLng.latitude + distance));
            avoidLocations.Add(new AMapLocation(avoidLatLng.longitude + distance, avoidLatLng.latitude + distance));
            avoidLocations.Add(new AMapLocation(avoidLatLng.longitude + distance, avoidLatLng.latitude - distance));

            return avoidLocations;

        }

       /// <summary>
       /// 路径规划展示
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
       {
           this.Dispatcher.BeginInvoke(() =>
           {
               if (_latlng != null && NavigateEndLngLng != null)
               {
                   GetNavigationDriving(_latlng, NavigateEndLngLng);
               }
               else if (_latlng != null && _researchLatLng != null)
               {
                   GetNavigationDriving(_latlng, _researchLatLng);
               }

           });
           
       }

        /// <summary>
        /// 导航至登陆界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginorRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = "/Images/loginicon.png.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            LoginorRegisterButton.Background = brush;

            this.NavigationService.Navigate(new Uri("/AfterLoginPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// 从主界面导航至路线搜索界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void routeButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/RoutePage.xaml", UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// 从MainPage导航到SearchPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveToSearchPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SearchPage.xaml", UriKind.RelativeOrAbsolute));
            var uri = "/Images/search.png";
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSource = new BitmapImage(imgUri);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imgSource;
            Search.Background = brush;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (_amap != null)
            {
                if (_researchMarker != null)
                    _researchMarker.Destroy();
                (Application.Current as App).TempMap = _amap;
                if (_latlng != null)
                {
                    (Application.Current as App)._latlng = _latlng;

                }
                else
                {
                    (Application.Current as App)._latlng = _daBeiJing;
                }
                
                (Application.Current as App)._bearing = _bearing;
                (Application.Current as App)._tile = _tile;
                (Application.Current as App)._locationMarker = _locationMarker;
                (Application.Current as App)._circle = _circle;
                (Application.Current as App)._CameraLatLng = _cameraLatlng;
                (Application.Current as App)._mapZoom = _mapZoom;
                (Application.Current as App)._cameraIsNew = _cameraIsNew;
                (Application.Current as App)._isNew = _isNew;
            }
            ContentPanel.Children.Remove(_amap);

        }

        /// <summary>
        /// 供SearchPage返回时调用
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            base.OnNavigatedTo(e); 
            var app = Application.Current as App;
            if (app != null && app.IsFromSearchPage)
            {
                double lat = double.Parse(NavigationContext.QueryString["lat"]);
                double lon = double.Parse(NavigationContext.QueryString["lon"]);
                _address = NavigationContext.QueryString["address"];
                _researchLatLng = new LatLng(lat, lon);//新建经纬度，供标记搜索maker使用
                _cameraIsNew = false;//防止实时定位视角拉动

                //绘制搜索结果maker
                _researchMarker = _amap.AddMarker(new AMapMarkerOptions()
                {
                    Position = _researchLatLng,//maker添加位置
                    IconUri = new Uri("Images/RED.png", UriKind.RelativeOrAbsolute)//maker图标
                });
                (Application.Current as App).IsFromSearchPage = false;
                _amap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(_researchLatLng, 16));//移动视角

                ShowDownStateSettings();
                MakerTextBlock.Text = _address;
                //GetNavigationDriving(Location, _researchLatLng); 

            }
        }

        /// <summary>
        /// 指南针控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Compass_Tap(object sender, GestureEventArgs e)
        {
            this._bearing = 0;
            this._tile = 0;
            var cameraupdate = CameraUpdateFactory.NewCameraPosition(_cameraLatlng, _mapZoom, _bearing, _tile);
            _amap.MoveCamera(cameraupdate);


        }

    }

}