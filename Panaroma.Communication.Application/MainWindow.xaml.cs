using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace Panaroma.Communication.Application
{
    public partial class MainWindow : Window, IComponentConnector
    {
        private WebSocketServer _panaromaWebSocketServer;
        private bool _isSuccessMouseDown;
        private bool _isErrorMouseDown;
        private bool _isWarningMouseDown;
        private bool _pleaseClose;
        private readonly DataGrid _dataGridSuccess;
        private readonly DataGrid _dataGridError;
        private readonly DataGrid _dataGridWarning;
        private int count = 0;
        private string okccmd = "#okccmd#";
        private string okcres = "#okcres#";

        public MainWindow()
        {
            #region ProcessTypeWebSocket

            if(ConfigurationManager.AppSettings["ProcessType"] == "1")
            {
                Title = "                                                   MX-915 İletişim Ekranı" + " - " + "WebSocket";
                InitializeComponent();
                MainWindowProperty();
                VisibilityDefaultStatus();
                System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                _dataGridWarning = GetDefaultDataGrid(DataGridType.Warning);
                _dataGridSuccess = GetDefaultDataGrid(DataGridType.Success);
                _dataGridError = GetDefaultDataGrid(DataGridType.Error);
                Loaded += new RoutedEventHandler(MainWindow_Loaded);
            }

            #endregion ProcessTypeWebSocket

            #region ProcessTypeClipBoard

            else if(ConfigurationManager.AppSettings["ProcessType"] == "2")
            {
                Title = "                                                   MX-915 İletişim Ekranı" + " - " + "ClipBoard";
                InitializeComponent();
                MainWindowProperty();
                VisibilityDefaultStatus();
                System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                _dataGridWarning = GetDefaultDataGrid(DataGridType.Warning);
                _dataGridSuccess = GetDefaultDataGrid(DataGridType.Success);
                _dataGridError = GetDefaultDataGrid(DataGridType.Error);
            }

            #endregion ProcessTypeClipBoard

            #region NetworkComminucation
            else if(ConfigurationManager.AppSettings["ProcessType"]=="3")
            {
                Title = "                                                   MX-915 İletişim Ekranı" + " - " + "ClipBoard";
                InitializeComponent();
                MainWindowProperty();
                VisibilityDefaultStatus();
                System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                _dataGridWarning = GetDefaultDataGrid(DataGridType.Warning);
                _dataGridSuccess = GetDefaultDataGrid(DataGridType.Success);
                _dataGridError = GetDefaultDataGrid(DataGridType.Error);
            }
            #endregion

            #region ProcessTypeNone

            else
            {
                Title = "                                                   MX-915 İletişim Ekranı" + " - " + "None";
                InitializeComponent();
                MainWindowProperty();
                VisibilityDefaultStatus();
                System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                _dataGridWarning = GetDefaultDataGrid(DataGridType.Warning);
                _dataGridSuccess = GetDefaultDataGrid(DataGridType.Success);
                _dataGridError = GetDefaultDataGrid(DataGridType.Error);
                ConfigurationManager.AppSettings["BildirimSuresi"] = "5000";
                Dispatcher.BeginInvoke(
                    new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                            "Program şu anda hiç bir şekilde etkileşimde değil  !!!", Helpers.DateTimeHelper.GetDateTime()))
                        .Build().Show()), Array.Empty<object>());
            }

            #endregion ProcessTypeNone
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var windowClipboardManager = new ClipboardManager(this);
            windowClipboardManager.ClipboardChanged += ClipboardChanged;
        }

        private void ClipboardChanged(object sender, EventArgs e)
        {
            if(Clipboard.ContainsText())
            {
                try
                {
                    string t = Win32ClipboardAPI.GetText().Trim();
                    bool request = t.StartsWith("#okccmd#");
                    bool response = t.StartsWith("#okcres#");
                    if(!response == true)
                    {
                        if(request == true)
                        {
                            if(String.IsNullOrWhiteSpace(t))
                                return;
                            if(t == oldValue)
                            {
                                    return;
                            }
                            oldValue = t;
                            if(t == okccmd)
                            {
                                //Dispatcher.BeginInvoke(
                                // new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                                //"Yalnızca tag gönderilemez !!!", Helpers.DateTimeHelper.GetDateTime()))
                                //.Build().Show()), Array.Empty<object>());
                                return;
                            }

                            if(t.IndexOf(okccmd) == -1)
                            {
                                //Dispatcher.BeginInvoke(
                                //new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                                //"Tag Bulunamadı !!!", Helpers.DateTimeHelper.GetDateTime()))
                                //.Build().Show()), Array.Empty<object>());
                                return;
                            }

                            t = t.Substring(okccmd.Length);

                            try
                            {
                                try
                                {
                                    (new ProcessWorker(JsonConvert.DeserializeObject<TcpCommand>(t))).DoWork();
                                    string str =
                                        PublicCommunication.ConvertFromInternalCommunication(InternalCommunication
                                            .GetInternalCommunication());
                                    Win32ClipboardAPI.SetText(okcres + str);
                                    if(InternalCommunication.GetInternalCommunication().NotificationWindowses.Any())
                                    {
                                        AddLogToGrid(str);
                                    }
                                }
                                catch(Exception exception)
                                {
                                    _catch(exception);
                                }
                            }
                            finally
                            {
                                _finally();
                            }
                        }
                        else
                        {
                            //Dispatcher.BeginInvoke(
                            //new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                            //"Gönderilen format doğru başlamadı. Komut tag #okccmd# ile başlamalı Kontrol ediniz. !!!", Helpers.DateTimeHelper.GetDateTime()))
                            //.Build().Show()), Array.Empty<object>());
                        }
                    }
                    else
                    {
                        //Dispatcher.BeginInvoke(
                        //new Action(() => (new NotificationWindow(NotificationType.Information, "Bilgi ",
                        // "Mesaj Cevabı Yazıldı...", Helpers.DateTimeHelper.GetDateTime()))
                        //.Build().Show()), Array.Empty<object>());
                    }
                }
                catch(Exception ex)
                {
                    Dispatcher.BeginInvoke(
                    new Action(() => (new NotificationWindow(NotificationType.Error, "Hata ",
                    "ClipBoard Açılamadı Yeniden Başlatılıyor....", Helpers.DateTimeHelper.GetDateTime()))
                    .Build().Show()), Array.Empty<object>());
                    App.AllowMultipleApplication(true);
                }
            }
        }

        private void MainWindowProperty()
        {
            ShowInTaskbar = true;
            lblVersionInfo.Content = "Version: " + getRunningVersion().Major + "." + getRunningVersion().MajorRevision +
                                     "." + getRunningVersion().Build + "." + " Release_180928-1709";
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ToolTip = "Bu Program WebSocket teknolojisi veya ClipBoard ile haberleşme yapar. Yalnızca Json Formatı ile habeleşme kurar.";
        }

        private void CheckLogData()
        {
            Helpers.FileHelper.RemoveOldLogFiles(
                Convert.ToInt16(ConfigurationManager.AppSettings["RemoveLogsOldDayLimit"]));
            Helpers.FileHelper.RemoveOldLogFilesDll(
                Convert.ToInt16(ConfigurationManager.AppSettings["RemoveLogsOldDayLimit"]));
        }

        private void getListboxData()
        {
            listBox.Items.Add("WEBSOCKETPORT: " + ConfigurationManager.AppSettings["WebSocketPort"]);
            listBox.Items.Add("OKCCONNECTIONTYPE: " + ConfigurationManager.AppSettings["OKCConnectionType"] +
                              " (1-COM  / 2-ETHERNET)");
            listBox.Items.Add("OKCIPADRESS: " + ConfigurationManager.AppSettings["OKCIpAddress"] + ":41200");
            listBox.Items.Add("OKCCOMPORT: " + ConfigurationManager.AppSettings["OKCCOMPort"]);
            listBox.Items.Add("OKCLOG: " + ConfigurationManager.AppSettings["OKCLog"]);
            listBox.Items.Add("PASSWORD: " + ConfigurationManager.AppSettings["Password"]);
            listBox.Items.Add("PASSWORDHAK: " + ConfigurationManager.AppSettings["PasswordHak"]);
            listBox.Items.Add("REMOVELOGSOLDDAY: " + ConfigurationManager.AppSettings["RemoveLogsOldDayLimit"]);
            listBox.Items.Add("UPDATEFILEADRESS: " + ConfigurationManager.AppSettings["UpdateFileAddress"]);
            listBox.Items.Add("APPSETTINGSLOGLEVEL: " + ConfigurationManager.AppSettings["AppSettingsLogLevel"]);
            listBox.ToolTip = "Programın ayarlarını görüntüler.";
        }

        private void VisibilityDefaultStatus()
        {
            _passworBox.Visibility = Visibility.Hidden;
            _label.Visibility = Visibility.Hidden;
            _textBox.Visibility = Visibility.Hidden;
            _comChange.Visibility = Visibility.Hidden;
            _connectionType.Visibility = Visibility.Hidden;
            listBox.Visibility = Visibility.Hidden;
            _labelConfig.Visibility = Visibility.Hidden;
            _ipChange.Visibility = Visibility.Hidden;
            GridMenu.Visibility = Visibility.Visible;
            listBox.SelectionMode = SelectionMode.Extended;
            _logChange.Visibility = Visibility.Hidden;
        }

        private void MainWindow_Loaded_Task(Task b)
        {
            bool flag;
            Guid guid = Guid.NewGuid();
            Helpers.RegistryHelper.IfNotExistsCashIdThenAdd(guid.ToString(), out flag);
            if(!flag)
            {
                return;
            }

            _pleaseClose = true;
            Dispatcher.Invoke(new Action(Close));
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckUpdates();
                Task task = Task.Factory.StartNew(new Action(Helpers.FolderHelper.IfNotExistsCreatePanaromaSideFolder));
                await task.ContinueWith((Task b) => { }).ContinueWith((Task b) =>
                {
                    bool flag;
                    Helpers.RegistryHelper.IfNotExistsCashIdThenAdd(Guid.NewGuid().ToString(), out flag);
                    if(!flag)
                    {
                        return;
                    }

                    _pleaseClose = true;
                    Dispatcher.Invoke(new Action(Close));
                });
                _panaromaWebSocketServer = new WebSocketServer();
                _panaromaWebSocketServer.OnMessageChanged +=
                    new WebSocketServer.MessageChanged(Panaroma_OnMessageChanged);
            }
            catch(Exception exception)
            {
                _catch(exception);
            }
            finally
            {
                _finally();
            }
        }

        private void Main_Closing(object sender, CancelEventArgs e)
        {
            VisibilityDefaultStatus();
            //Logger.Info("Program kapatılmaya Çalışıldı");
            if(!_pleaseClose)
            {
                Dispatcher.BeginInvoke(
                    new Action(() => (new NotificationWindow(NotificationType.Information, "Uyarı",
                        "Programı kapatmak için; Ana sayfa üzerindeki Uygulama menüsünü kullanın.",
                        Helpers.DateTimeHelper.GetDateTime())).Build().Show()), Array.Empty<object>());
            }

            e.Cancel = !_pleaseClose;
        }

        private void TetxtBlockSuccess_OnMouseEnter(object sender, MouseEventArgs e)
        {
            StackPanelSuccessLine.Background = Brushes.DarkSeaGreen;
        }

        private void TetxtBlockSuccess_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if(!_isSuccessMouseDown)
            {
                StackPanelSuccessLine.Background = Brushes.Transparent;
            }
        }

        private void TextBlockError_OnMouseEnter(object sender, MouseEventArgs e)
        {
            StackPanelErrorLine.Background = Brushes.Brown;
        }

        private void TextBlockError_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if(!_isErrorMouseDown)
            {
                StackPanelErrorLine.Background = Brushes.Transparent;
            }
        }

        private void TextBlockWarning_OnMouseEnter(object sender, MouseEventArgs e)
        {
            StackPanelWarningLine.Background = Brushes.DarkSalmon;
        }

        private void TextBlockWarning_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if(!_isWarningMouseDown)
            {
                StackPanelWarningLine.Background = Brushes.Transparent;
            }
        }

        private string oldValue = null;

        private void Panaroma_OnMessageChanged(WebSocketEventArgs e)
        {
            if(string.IsNullOrEmpty(e.Message))
            {
                return;
            }

            try
            {
                try
                {
                    (new ProcessWorker(JsonConvert.DeserializeObject<TcpCommand>(e.Message))).DoWork();
                    string str =
                        PublicCommunication.ConvertFromInternalCommunication(InternalCommunication
                            .GetInternalCommunication());
                    _panaromaWebSocketServer.SendMessage(str);
                    if(InternalCommunication.GetInternalCommunication().NotificationWindowses.Any())
                    {
                        AddLogToGrid(e.Message);
                    }
                }
                catch(Exception exception)
                {
                    _catch(exception);
                }
            }
            finally
            {
                _finally();
            }
        }

        private static async void GetCashId()
        {
            string str = await Task.Factory.StartNew(new Func<string>(Helpers.RegistryHelper.GetCashId));
            new NotificationWindow(NotificationType.Success, "Kasa Numarası",
                string.Format(
                    "Kasa Numarası : {0}.\nKasa numaranız kopyalanmıştır. Herhangi bir alana yapıştırabilirsiniz.",
                    str), Helpers.DateTimeHelper.GetDateTime()).Build().Show();
            Clipboard.SetText(str);
        }

        private DataGrid GetDefaultDataGrid(DataGridType dataGridType)
        {
            DataGrid dataGrid = new DataGrid()
            {
                Margin = new Thickness(0, 2, 0, 0),
                AutoGenerateColumns = false,
                IsReadOnly = true
            };
            dataGrid.Columns.Add(new DataGridTextColumn()
            {
                Header = "Başlık",
                Binding = new Binding("Header")
            });
            dataGrid.Columns.Add(new DataGridTextColumn()
            {
                Header = "Açıklama",
                Binding = new Binding("Description")
            });
            dataGrid.Columns.Add(new DataGridTextColumn()
            {
                Header = "Saat",
                Binding = new Binding("Time")
            });
            DataGrid darkSeaGreen = dataGrid;
            switch(dataGridType)
            {
                case DataGridType.Success:
                    {
                        darkSeaGreen.AlternatingRowBackground = Brushes.DarkSeaGreen;
                        break;
                    }
                case DataGridType.Error:
                    {
                        darkSeaGreen.AlternatingRowBackground = Brushes.Tomato;
                        break;
                    }
                case DataGridType.Warning:
                    {
                        darkSeaGreen.AlternatingRowBackground = Brushes.Salmon;
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException("dataGridType", dataGridType, null);
                    }
            }

            return darkSeaGreen;
        }

        private void _catch(Exception exception)
        {
            try
            {
                try
                {
                    AddLogToGrid(null);
                    Dispatcher.Invoke(() => (new NotificationWindow(NotificationType.Error, "Beklenmedik hata",
                        string.Format("İşlem yürütülürken beklenmedik bir hata oluştu. Hata :{0}", exception.Message),
                        Helpers.DateTimeHelper.GetDateTime())).Build().Show());
                }
                catch
                {
                }
            }
            finally
            {
                SetInternalCommunicationDefault();
            }
        }

        private void AddLogToGrid(string message = null)
        {
            foreach(NotificationWindows notificationWindowse in InternalCommunication.GetInternalCommunication()
                .NotificationWindowses)
            {
                NotificationWindows notificationWindow = notificationWindowse;
                notificationWindow.Description = string.Concat(notificationWindow.Description, "   ", message);
                Dispatcher.Invoke(() =>
                {
                    switch(notificationWindowse.NotificationType)
                    {
                        case NotificationType.Information:
                        case NotificationType.Default:
                            {
                                if(InternalCommunication.GetInternalCommunication().ShowDesktop)
                                {
                                    (new NotificationWindow(notificationWindowse.NotificationType,
                                        notificationWindowse.Header, notificationWindowse.Description,
                                        notificationWindowse.Time)).Build().Show();
                                }

                                return;
                            }
                        case NotificationType.Success:
                            {
                                _dataGridSuccess.Items.Add(new
                                {
                                    Header = notificationWindowse.Header,
                                    Description = notificationWindowse.Description,
                                    Time = notificationWindowse.Time
                                });
                                if(InternalCommunication.GetInternalCommunication().ShowDesktop)
                                {
                                    (new NotificationWindow(notificationWindowse.NotificationType,
                                        notificationWindowse.Header, notificationWindowse.Description,
                                        notificationWindowse.Time)).Build().Show();
                                }

                                return;
                            }
                        case NotificationType.Warning:
                            {
                                _dataGridWarning.Items.Add(new
                                {
                                    Header = notificationWindowse.Header,
                                    Description = notificationWindowse.Description,
                                    Time = notificationWindowse.Time
                                });
                                if(InternalCommunication.GetInternalCommunication().ShowDesktop)
                                {
                                    (new NotificationWindow(notificationWindowse.NotificationType,
                                        notificationWindowse.Header, notificationWindowse.Description,
                                        notificationWindowse.Time)).Build().Show();
                                }

                                return;
                            }
                        case NotificationType.Error:
                            {
                                _dataGridError.Items.Add(new
                                {
                                    Header = notificationWindowse.Header,
                                    Description = notificationWindowse.Description,
                                    Time = notificationWindowse.Time
                                });
                                if(InternalCommunication.GetInternalCommunication().ShowDesktop)
                                {
                                    (new NotificationWindow(notificationWindowse.NotificationType,
                                        notificationWindowse.Header, notificationWindowse.Description,
                                        notificationWindowse.Time)).Build().Show();
                                }

                                return;
                            }
                    }

                    throw new ArgumentOutOfRangeException();
                });
            }
        }

        private static void _finally()
        {
            try
            {
                SetInternalCommunicationDefault();
            }
            catch
            {
            }
        }

        private static void SetInternalCommunicationDefault()
        {
            InternalCommunication.GetInternalCommunication().Exceptions.Clear();
            InternalCommunication.GetInternalCommunication().NotificationWindowses.Clear();
            InternalCommunication.GetInternalCommunication().HasError = false;
            InternalCommunication.GetInternalCommunication().IsSuccess = false;
            InternalCommunication.GetInternalCommunication().Results = null;
        }

        private static void CheckUpdates()
        {
            if(Environment.GetCommandLineArgs().Length != 1)
            {
                return;
            }

            if(!File.Exists(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName,
                    "ApplicationUpdater.exe")) || ConfigurationManager.AppSettings["UpdateFileAddress"] == null)
            {
                return;
            }

            StartUpdate();
        }

        private static void StartUpdate()
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                Arguments = string.Format("{0} {1}.exe {2} {3}",
                    new object[]
                    {
                        Assembly.GetExecutingAssembly().Location.Replace(" ", "$"),
                        Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location).Replace(" ", "$"),
                        ConfigurationManager.AppSettings["UpdateFileAddress"], Process.GetCurrentProcess().Id
                    }),
                CreateNoWindow = false,
                FileName = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName,
                    "ApplicationUpdater.exe"),
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            Process.Start(processStartInfo);
        }

        private void TetxtBlockSuccess_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isSuccessMouseDown = true;
            _isErrorMouseDown = false;
            _isWarningMouseDown = false;
            StackPanelSuccessLine.Background = Brushes.DarkSeaGreen;
            StackPanelErrorLine.Background = Brushes.Transparent;
            StackPanelWarningLine.Background = Brushes.Transparent;
            if(GridDataGridviewContent.Children.Count > 0)
            {
                GridDataGridviewContent.Children.Clear();
            }

            GridDataGridviewContent.Children.Add(_dataGridSuccess);
        }

        private void TextBlockError_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isErrorMouseDown = true;
            _isSuccessMouseDown = false;
            _isWarningMouseDown = false;
            StackPanelErrorLine.Background = Brushes.Brown;
            StackPanelSuccessLine.Background = Brushes.Transparent;
            StackPanelWarningLine.Background = Brushes.Transparent;
            if(GridDataGridviewContent.Children.Count > 0)
            {
                GridDataGridviewContent.Children.Clear();
            }

            GridDataGridviewContent.Children.Add(_dataGridError);
        }

        private void TextBlockWarning_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isWarningMouseDown = true;
            _isSuccessMouseDown = false;
            _isErrorMouseDown = false;
            StackPanelWarningLine.Background = Brushes.DarkSalmon;
            StackPanelErrorLine.Background = Brushes.Transparent;
            StackPanelSuccessLine.Background = Brushes.Transparent;
            if(GridDataGridviewContent.Children.Count > 0)
            {
                GridDataGridviewContent.Children.Clear();
            }

            GridDataGridviewContent.Children.Add(_dataGridWarning);
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            GetCashId();
        }

        private void Restart_OnClick(object sender, RoutedEventArgs e)
        {
            App.AllowMultipleApplication(true);
        }

        private void About_OnClick(object sender, RoutedEventArgs e)
        {
            VisibilityDefaultStatus();
            MessageBox.Show(
                "Bu Program Panaroma Bilişim için geliştirilmiştir.\nTüm Hakları Saklıdır." + "\n http://www.pbt.com.tr/"
                                                                                            + "\n" + "Version: " +
                                                                                            getRunningVersion().Major +
                                                                                            "." + getRunningVersion()
                                                                                                .MajorRevision + "." +
                                                                                            getRunningVersion().Build +
                                                                                            "." +
                                                                                            " Release_180815-56",
                "Hakkında", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK,
                MessageBoxOptions.RightAlign);
        }

        private Version getRunningVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            VisibilityDefaultStatus();
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
                new Action(delegate () { _passworBox.Focus(); }));
            _label.Content = "Şifre:";
            _passworBox.ToolTip = "Vazgeçmek için ESC tuşuna basın.";
            _label.ToolTip = "Vazgeçmek için ESC tuşuna basın.";
            _passworBox.Visibility = Visibility.Visible;
            GridMenu.Visibility = Visibility.Hidden;
            _label.Visibility = Visibility.Visible;
            GridDataGridviewContent.Visibility = Visibility.Hidden;
        }

        private void PasswordBoxOnKeyDownHandler(object sender, KeyEventArgs e)
        {
            string pwd = ConfigurationManager.AppSettings["Password"];
            if(e.Key == Key.Enter)
            {
                if(pwd == _passworBox.Password)
                {
                    if(MessageBox.Show("Yazar kasa ile bağlantınız kesilecek, onaylıyor musunuz?",
                            "Uygulama kapanıyor", MessageBoxButton.OKCancel, MessageBoxImage.Question) !=
                        MessageBoxResult.OK)
                    {
                        _passworBox.Visibility = Visibility.Hidden;
                        GridMenu.Visibility = Visibility.Visible;
                        _label.Visibility = Visibility.Hidden;
                        Dispatcher.BeginInvoke(
                            new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                                "Çıkış yapılamadı.", Helpers.DateTimeHelper.GetDateTime())).Build().Show()),
                            Array.Empty<object>());

                        return;
                    }

                    CheckLogData();
                    _pleaseClose = true;
                    Close();
                    return;
                }
                else
                {
                    if(count != Convert.ToInt16(ConfigurationManager.AppSettings["PasswordHak"]))
                    {
                        count++;
                        Dispatcher.BeginInvoke(
                            new Action(() => (new NotificationWindow(NotificationType.Error,
                                "Hata !!! Çıkış Yapılamadı ", "Şifre Hatalı Girildi " + count + ".hak",
                                Helpers.DateTimeHelper.GetDateTime())).Build().Show()), Array.Empty<object>());
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(
                            new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı",
                                    "Şifre 3 Kere Hatalı girildi. Çıkış Yapılamaz !",
                                    Helpers.DateTimeHelper.GetDateTime()))
                                .Build().Show()), Array.Empty<object>());
                        return;
                    }
                }
            }
            else if(e.Key == Key.Escape)
            {
                VisibilityDefaultStatus();
                return;
            }
        }

        private void Bildirim_OnClick(object sender, RoutedEventArgs e)
        {
            VisibilityDefaultStatus();
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,
                new Action(delegate () { _textBox.Focus(); }));
            _textBox.ToolTip = "Sol alttaki bildirim ekranı süresini belirler.";
            _label.Content = "Bildirim Süresi";
            _textBox.Text = ConfigurationManager.AppSettings["BildirimSuresi"];
            _label.Visibility = Visibility.Visible;
            _textBox.Visibility = Visibility.Visible;
            GridMenu.Visibility = Visibility.Hidden;
            GridDataGridviewContent.Visibility = Visibility.Hidden;
        }

        private void NotificationOnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if(_textBox.Text != ConfigurationManager.AppSettings["BildirimSuresi"])
                {
                    if(MessageBox.Show(_textBox.Text + ": Bildirim süresi değiştirilsin mi?",
                            "Bildirim Süresi Değişikliği",
                            MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                    {
                        Dispatcher.BeginInvoke(
                            new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                                    "Bildirim Süresi Değiştirilmedi.", Helpers.DateTimeHelper.GetDateTime())).Build()
                                .Show()), Array.Empty<object>());

                        return;
                    }

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["BildirimSuresi"].Value = _textBox.Text;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
                    Dispatcher.BeginInvoke(
                        new Action(() => (new NotificationWindow(NotificationType.Information, "Bilgi ",
                                "Bildirim Süresi Değiştirildi :" + _textBox.Text, Helpers.DateTimeHelper.GetDateTime()))
                            .Build().Show()), Array.Empty<object>());
                }
                else
                {
                    Dispatcher.BeginInvoke(
                        new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                            "Değişiklik yapılmadı.",
                            Helpers.DateTimeHelper.GetDateTime())).Build().Show()), Array.Empty<object>());
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,
                        new Action(delegate () { _textBox.Focus(); }));
                    return;
                }
            }
            else if(e.Key == Key.Escape)
            {
                VisibilityDefaultStatus();
                return;
            }
        }

        private void ComChange_OnClick(object sender, RoutedEventArgs e)
        {
            VisibilityDefaultStatus();
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,
                new Action(delegate () { _comChange.Focus(); }));
            _comChange.ToolTip = "COM Port bilgisini değiştirir.";
            _label.Content = "Com Port Giriniz";
            _comChange.Text = ConfigurationManager.AppSettings["OKCComPort"];
            _label.Visibility = Visibility.Visible;
            _comChange.Visibility = Visibility.Visible;
            GridMenu.Visibility = Visibility.Hidden;
            GridDataGridviewContent.Visibility = Visibility.Hidden;
        }

        private void ComChangeOnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if(_comChange.Text != ConfigurationManager.AppSettings["OKCCOMPort"])
                {
                    if(MessageBox.Show(_comChange.Text + " olarak değiştirilecek onaylıyormusunuz ?",
                            "Com bilgisi değişikliği", MessageBoxButton.OKCancel, MessageBoxImage.Question) !=
                        MessageBoxResult.OK)
                    {
                        Dispatcher.BeginInvoke(
                            new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                                "Com Bilgisi Değiştirilmedi.", Helpers.DateTimeHelper.GetDateTime())).Build().Show()),
                            Array.Empty<object>());

                        return;
                    }

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["OKCCOMPort"].Value = _comChange.Text;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
                    Dispatcher.BeginInvoke(
                        new Action(() => (new NotificationWindow(NotificationType.Information, "Bilgi ",
                                "COM Bilgisi Değiştirildi :" + _comChange.Text, Helpers.DateTimeHelper.GetDateTime()))
                            .Build().Show()), Array.Empty<object>());
                    Dispatcher.BeginInvoke(
                        new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                            "Değişikliklerin etkili olabilmesi için uygulamayı yeniden başlatın",
                            Helpers.DateTimeHelper.GetDateTime())).Build().Show()), Array.Empty<object>());
                }
                else
                {
                    Dispatcher.BeginInvoke(
                        new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                            "Değişiklik yapılmadı.",
                            Helpers.DateTimeHelper.GetDateTime())).Build().Show()), Array.Empty<object>());
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,
                        new Action(delegate () { _comChange.Focus(); }));
                    return;
                }
            }
            else if(e.Key == Key.Escape)
            {
                VisibilityDefaultStatus();
                return;
            }
        }

        private void ConnectionType_OnClick(object sender, RoutedEventArgs e)
        {
            VisibilityDefaultStatus();
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,
                new Action(delegate () { _connectionType.Focus(); }));
            _connectionType.ToolTip = "Bağlantı bilgisini değiştirir. 1-COM // 2-Ethernet";
            _label.Content = "Bağlantı Tipini Giriniz = 1:COM// 2:Ethernet";
            _connectionType.Text = ConfigurationManager.AppSettings["OKCConnectionType"];
            _label.Visibility = Visibility.Visible;
            _connectionType.Visibility = Visibility.Visible;
            GridMenu.Visibility = Visibility.Hidden;
            GridDataGridviewContent.Visibility = Visibility.Hidden;
        }

        private void ConnectionTypeOnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if(_connectionType.Text != ConfigurationManager.AppSettings["OKCConnectionType"])
                {
                    if(MessageBox.Show(_connectionType.Text + " olarak değiştirilecek onaylıyormusunuz ?",
                            "ConnectionType=1-COM : 2-Ethernet ", MessageBoxButton.OKCancel,
                            MessageBoxImage.Question) != MessageBoxResult.OK)
                    {
                        Dispatcher.BeginInvoke(
                            new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                                    "Bağlantı Bilgisi Değiştirilmedi.", Helpers.DateTimeHelper.GetDateTime())).Build()
                                .Show()), Array.Empty<object>());

                        return;
                    }

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["OKCConnectionType"].Value = _connectionType.Text;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
                    Dispatcher.BeginInvoke(
                        new Action(() => (new NotificationWindow(NotificationType.Information, "Bilgi ",
                            "Bağlantı Bilgisi Değiştirildi :" + _connectionType.Text,
                            Helpers.DateTimeHelper.GetDateTime())).Build().Show()), Array.Empty<object>());
                }
                else
                {
                    Dispatcher.BeginInvoke(
                        new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                            "Değişiklik yapılmadı.",
                            Helpers.DateTimeHelper.GetDateTime())).Build().Show()), Array.Empty<object>());
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,
                        new Action(delegate () { _connectionType.Focus(); }));
                    return;
                }
            }
            else if(e.Key == Key.Escape)
            {
                VisibilityDefaultStatus();
                return;
            }
        }

        private void AppConfig_OnClick(object sender, RoutedEventArgs e)
        {
            VisibilityDefaultStatus();
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,
                new Action(delegate () { listBox.Focus(); }));
            listBox.Visibility = Visibility.Visible;
            GridMenu.Visibility = Visibility.Hidden;
            _labelConfig.Visibility = Visibility.Visible;
            GridDataGridviewContent.Visibility = Visibility.Hidden;
            getListboxData();
        }

        private void AppConfigOnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                VisibilityDefaultStatus();
                return;
            }
        }

        private void Ip_OnClick(object sender, RoutedEventArgs e)
        {
            VisibilityDefaultStatus();
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,
                new Action(delegate () { _ipChange.Focus(); }));
            _ipChange.ToolTip = "Ip Bilgisini Değiştirir.";
            _label.Content = "Ip Bilgisini Giriniz";
            _ipChange.Text = ConfigurationManager.AppSettings["OKCIpAddress"];
            _label.Visibility = Visibility.Visible;
            _ipChange.Visibility = Visibility.Visible;
            GridMenu.Visibility = Visibility.Hidden;
            GridDataGridviewContent.Visibility = Visibility.Hidden;
        }

        private void IpChangeOnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if(_ipChange.Text != ConfigurationManager.AppSettings["OKCIpAddress"])
                {
                    if(MessageBox.Show(_ipChange.Text + " olarak değiştirilecek onaylıyormusunuz ?",
                            "IP bilgisi değişikliği", MessageBoxButton.OKCancel, MessageBoxImage.Question) !=
                        MessageBoxResult.OK)
                    {
                        Dispatcher.BeginInvoke(
                            new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                                "IP Bilgisi Değiştirilmedi.", Helpers.DateTimeHelper.GetDateTime())).Build().Show()),
                            Array.Empty<object>());

                        return;
                    }

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["OKCIpAddress"].Value = _ipChange.Text;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
                    Dispatcher.BeginInvoke(
                        new Action(() => (new NotificationWindow(NotificationType.Information, "Bilgi ",
                                "IP Bilgisi Değiştirildi :" + _ipChange.Text, Helpers.DateTimeHelper.GetDateTime()))
                            .Build().Show()), Array.Empty<object>());
                    Dispatcher.BeginInvoke(
                        new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                            "Değişikliklerin etkili olabilmesi için uygulamayı yeniden başlatın",
                            Helpers.DateTimeHelper.GetDateTime())).Build().Show()), Array.Empty<object>());
                }
                else
                {
                    Dispatcher.BeginInvoke(
                        new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                            "Değişiklik yapılmadı.",
                            Helpers.DateTimeHelper.GetDateTime())).Build().Show()), Array.Empty<object>());
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,
                        new Action(delegate () { _ipChange.Focus(); }));
                    return;
                }
            }
            else if(e.Key == Key.Escape)
            {
                VisibilityDefaultStatus();
                return;
            }
        }

        private void RemoveOldLog_OnClick(object sender, RoutedEventArgs e)
        {
            VisibilityDefaultStatus();
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,
                new Action(delegate () { _logChange.Focus(); }));
            _logChange.ToolTip = "Max. tutulacak log süresini değiştirir.";
            _label.Content = "Yeni Log Süre Bilgisini Giriniz";
            _logChange.Text = ConfigurationManager.AppSettings["RemoveLogsOldDayLimit"];
            _label.Visibility = Visibility.Visible;
            _logChange.Visibility = Visibility.Visible;
            GridMenu.Visibility = Visibility.Hidden;
            GridDataGridviewContent.Visibility = Visibility.Hidden;
        }

        private void LogChangeOnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if(_logChange.Text != ConfigurationManager.AppSettings["RemoveLogsOldDayLimit"])
                {
                    if(MessageBox.Show(_logChange.Text + " olarak değiştirilecek onaylıyormusunuz ?",
                            "Log süresi bilgisi değişikliği", MessageBoxButton.OKCancel, MessageBoxImage.Question) !=
                        MessageBoxResult.OK)
                    {
                        Dispatcher.BeginInvoke(
                            new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                                    "Log Süresi Bilgisi Değiştirilmedi.", Helpers.DateTimeHelper.GetDateTime())).Build()
                                .Show()),
                            Array.Empty<object>());

                        return;
                    }

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["RemoveLogsOldDayLimit"].Value = _logChange.Text;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
                    Dispatcher.BeginInvoke(
                        new Action(() => (new NotificationWindow(NotificationType.Information, "Bilgi ",
                                "IP Bilgisi Değiştirildi :" + _logChange.Text, Helpers.DateTimeHelper.GetDateTime()))
                            .Build().Show()), Array.Empty<object>());
                    Dispatcher.BeginInvoke(
                        new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                            "Değişikliklerin etkili olabilmesi için uygulamayı yeniden başlatın",
                            Helpers.DateTimeHelper.GetDateTime())).Build().Show()), Array.Empty<object>());
                }
                else
                {
                    Dispatcher.BeginInvoke(
                        new Action(() => (new NotificationWindow(NotificationType.Warning, "Uyarı ",
                            "Değişiklik yapılmadı.",
                            Helpers.DateTimeHelper.GetDateTime())).Build().Show()), Array.Empty<object>());
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,
                        new Action(delegate () { _logChange.Focus(); }));
                    return;
                }
            }
            else if(e.Key == Key.Escape)
            {
                VisibilityDefaultStatus();
                return;
            }
        }
    }
}