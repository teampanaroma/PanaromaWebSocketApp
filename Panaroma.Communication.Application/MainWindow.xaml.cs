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

namespace Panaroma.Communication.Application
{
    public partial class MainWindow : Window, IComponentConnector
    {
        private System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();
        private System.Windows.Forms.ContextMenu notifyIconContexMenu = new System.Windows.Forms.ContextMenu();
        private System.Windows.Forms.MenuItem menuItem = new System.Windows.Forms.MenuItem();
        private System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer timer2 = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer timer3 = new System.Windows.Forms.Timer();
        private System.Windows.Forms.WebBrowser webBrowser = new System.Windows.Forms.WebBrowser();

        private WebSocketServer _panaromaWebSocketServer;
        private bool _isSuccessMouseDown;
        private bool _isErrorMouseDown;
        private bool _isWarningMouseDown;
        private bool _pleaseClose;
        private readonly DataGrid _dataGridSuccess;
        private readonly DataGrid _dataGridError;
        private readonly DataGrid _dataGridWarning;
        private int count = 0;
        private bool isLoad = false;
        private string okccmd = "#okccmd#";
        private string okcres = "#okcres#";
        private string msgNotReady = "Henüz hazır değil";
        private string msgConOpen = "Bağlantı açık";
        private string t=null;

        public MainWindow()
        {
            InitializeComponent();
            MainWindowProperty();
            VisibilityDefaultStatus();
            System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            _dataGridWarning = GetDefaultDataGrid(DataGridType.Warning);
            _dataGridSuccess = GetDefaultDataGrid(DataGridType.Success);
            _dataGridError = GetDefaultDataGrid(DataGridType.Error);
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        private void ClipBoardSettings()
        {
            timerDefaultValues();
            setIE();
            webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            webBrowser.Location = new System.Drawing.Point(0, 0);
            webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            webBrowser.Size = new System.Drawing.Size(800, 450);
            webBrowser.TabIndex = 0;
            webBrowser.Visible = true;
            webBrowser.DocumentCompleted += WebBrowser1_DocumentCompleted;
            string curDir = Directory.GetCurrentDirectory();
            webBrowser.Url = new Uri(String.Format("file:///{0}/OKC.html", curDir));
        }

        private void MainWindowProperty()
        {
            ShowInTaskbar = true;
            lblVersionInfo.Content = Clipboard.GetText();
            lblVersionInfo.Content = "Version: " + getRunningVersion().Major + "." + getRunningVersion().MajorRevision +
                                     "." + getRunningVersion().Build + "." + " Release_180815-56";
            Title = "                                                   MX-915 İletişim Ekranı";
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ToolTip = "Bu Program WebSocket teknolojisi kullanır. Yalnızca Json Formatı ile habeleşme kurar.";
        }

        private void NotifyIconSetings()
        {
            Closed += new EventHandler(Window_Closed);
            Deactivated += new EventHandler(Window_Deactivated);
            //notifyIcon.Icon = new System.Drawing.Icon("bird.ico");
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(notifyIcon_Click);
            notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
            notifyIconContexMenu.MenuItems.Add("Uygulamayı A&ç", new EventHandler(Open));
            notifyIconContexMenu.MenuItems.Add("Uygulamayı K&apat", new EventHandler(Close));
            notifyIconContexMenu.MenuItems.Add("Uygulama H&akkında", new EventHandler(About));
            notifyIcon.ContextMenu = notifyIconContexMenu;
            notifyIcon.Text = "Panaroma Bilişim Haberleşme Uygulaması";
        }

        private void timerDefaultValues()
        {
            //timer1 yönetir
            timer1.Enabled = true;
            timer1.Interval = 100;
            timer1.Tick += new EventHandler(timer1_Tick);

            //timer2 yönetir
            timer2.Enabled = false;
            timer2.Interval = 80;
            timer2.Tick += new EventHandler(timer2_Tick);

            //şimdilik test timer
            //timer3.Enabled = true;
            //timer3.Interval = 100;
            //timer3.Tick += new EventHandler(timer3_Tick);
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            t = Clipboard.GetText();
            lblVersionInfo.Content = t;
        }

        private void notifyIcon_DoubleClick(object Sender, System.EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
            Activate();
        }

        private void Window_Deactivated(object sender, System.EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            notifyIcon.Dispose();
        }

        private void Open(object sender, System.EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
            Activate();
            Show();
        }

        private void About(object sender, System.EventArgs e)
        {
            MessageBox.Show("www.panaroma.com.tr", "www.mrcyazilim.com");
        }

        private void notifyIcon_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                notifyIcon.ShowBalloonTip(1000, "Uygulama Bildirimi !!!", "Uygulama Buradan Çalıştırılamaz...", System.Windows.Forms.ToolTipIcon.Warning);
            }
        }

        private void Close(object sender, System.EventArgs e)
        {
            notifyIcon.Dispose();
            if (MessageBox.Show("Yazar kasa ile bağlantınız kesilecek, onaylıyor musunuz?", "Uygulama kapanıyor", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                return;
            _pleaseClose = true;
            Close();
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
            WebBrowser1.Visibility=Visibility.Hidden;
        }

        private void MainWindow_Loaded_Task(Task b)
        {
            bool flag;
            Guid guid = Guid.NewGuid();
            Helpers.RegistryHelper.IfNotExistsCashIdThenAdd(guid.ToString(), out flag);
            if (!flag)
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
                    if (!flag)
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
            catch (Exception exception)
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
            if (!_pleaseClose)
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
            if (!_isSuccessMouseDown)
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
            if (!_isErrorMouseDown)
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
            if (!_isWarningMouseDown)
            {
                StackPanelWarningLine.Background = Brushes.Transparent;
            }
        }

        private void Panaroma_OnMessageChanged(WebSocketEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message))
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
                    if (InternalCommunication.GetInternalCommunication().NotificationWindowses.Any())
                    {
                        AddLogToGrid(e.Message);
                    }
                }
                catch (Exception exception)
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
            switch (dataGridType)
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
            foreach (NotificationWindows notificationWindowse in InternalCommunication.GetInternalCommunication()
                .NotificationWindowses)
            {
                NotificationWindows notificationWindow = notificationWindowse;
                notificationWindow.Description = string.Concat(notificationWindow.Description, "   ", message);
                Dispatcher.Invoke(() =>
                {
                    switch (notificationWindowse.NotificationType)
                    {
                        case NotificationType.Information:
                        case NotificationType.Default:
                            {
                                if (InternalCommunication.GetInternalCommunication().ShowDesktop)
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
                                if (InternalCommunication.GetInternalCommunication().ShowDesktop)
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
                                if (InternalCommunication.GetInternalCommunication().ShowDesktop)
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
                                if (InternalCommunication.GetInternalCommunication().ShowDesktop)
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
            if (Environment.GetCommandLineArgs().Length != 1)
            {
                return;
            }

            if (!File.Exists(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName,
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
            if (GridDataGridviewContent.Children.Count > 0)
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
            if (GridDataGridviewContent.Children.Count > 0)
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
            if (GridDataGridviewContent.Children.Count > 0)
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
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,
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
            if (e.Key == Key.Enter)
            {
                if (pwd == _passworBox.Password)
                {
                    if (MessageBox.Show("Yazar kasa ile bağlantınız kesilecek, onaylıyor musunuz?",
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
                    if (count != Convert.ToInt16(ConfigurationManager.AppSettings["PasswordHak"]))
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
            else if (e.Key == Key.Escape)
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
            if (e.Key == Key.Enter)
            {
                if (_textBox.Text != ConfigurationManager.AppSettings["BildirimSuresi"])
                {
                    if (MessageBox.Show(_textBox.Text + ": Bildirim süresi değiştirilsin mi?",
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
            else if (e.Key == Key.Escape)
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
            if (e.Key == Key.Enter)
            {
                if (_comChange.Text != ConfigurationManager.AppSettings["OKCCOMPort"])
                {
                    if (MessageBox.Show(_comChange.Text + " olarak değiştirilecek onaylıyormusunuz ?",
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
            else if (e.Key == Key.Escape)
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
            if (e.Key == Key.Enter)
            {
                if (_connectionType.Text != ConfigurationManager.AppSettings["OKCConnectionType"])
                {
                    if (MessageBox.Show(_connectionType.Text + " olarak değiştirilecek onaylıyormusunuz ?",
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
            else if (e.Key == Key.Escape)
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
            if (e.Key == Key.Escape)
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
            if (e.Key == Key.Enter)
            {
                if (_ipChange.Text != ConfigurationManager.AppSettings["OKCIpAddress"])
                {
                    if (MessageBox.Show(_ipChange.Text + " olarak değiştirilecek onaylıyormusunuz ?",
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
            else if (e.Key == Key.Escape)
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
            if (e.Key == Key.Enter)
            {
                if (_logChange.Text != ConfigurationManager.AppSettings["RemoveLogsOldDayLimit"])
                {
                    if (MessageBox.Show(_logChange.Text + " olarak değiştirilecek onaylıyormusunuz ?",
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
            else if (e.Key == Key.Escape)
            {
                VisibilityDefaultStatus();
                return;
            }
        }

        private void WebBrowser1_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            isLoad = true;
        }

        private string getStatus()
        {
            if (!isLoad)
                return msgNotReady;
            try
            {
                
                return webBrowser.Document.GetElementById("status").InnerText;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private bool isReady()
        {
            return getStatus() == msgConOpen;
        }

        private void sendCmd(string cmd)
        {
            if (!isReady())
                setClipboard(okcres + getStatus());
            try
            {
                System.Windows.Forms.HtmlElement el = webBrowser.Document.GetElementById("cmd");
                el.SetAttribute("value", cmd);
                el.RaiseEvent("onChange");
                webBrowser.Document.GetElementById("response").Focus();
                timer2.Enabled = true;
            }
            catch (Exception ex)
            {
                setClipboard(okcres + ex.Message);
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            System.Windows.Forms.HtmlElement el = webBrowser.Document.GetElementById("response");
            string val = el.GetAttribute("value");
            if (string.IsNullOrWhiteSpace(val))
                return;
            timer2.Enabled = false;
            el.SetAttribute("value", "");
            setClipboard(val);
        }

        private void setClipboard(string val)
        {
            while (true)
            {
                try
                {
                    Clipboard.SetText(okcres + val);
                    break;
                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(20);
                }
            }
        }

        private string oldValue = null;

        private void timer1_Tick(object sender, EventArgs e)
        {
            t = Clipboard.GetText();
            if (String.IsNullOrWhiteSpace(t))
                return;

            if (t == oldValue)
                return;
            oldValue = t;

            if (t.IndexOf(okccmd) == -1)
                return;

            t = t.Substring(okccmd.Length);

            if (t.ToLower() == "checkstatus")
                setClipboard(okcres + getStatus());
            else
                sendCmd(t);
        }

        private bool setIE()
        {
            int BrowserVer, RegVal;
            BrowserVer = new System.Windows.Forms.WebBrowser().Version.Major;
            if (BrowserVer >= 11)
                RegVal = 111000;
            else if (BrowserVer == 10)
                RegVal = 10001;
            else if (BrowserVer == 9)
                RegVal = 9999;
            else if (BrowserVer == 8)
                RegVal = 8888;
            else
                RegVal = 7000;
            using (Microsoft.Win32.RegistryKey Key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree))
                if (Key.GetValue(Process.GetCurrentProcess().ProcessName + ".exe") == null)
                    Key.SetValue(Process.GetCurrentProcess().ProcessName + ".exe", RegVal, Microsoft.Win32.RegistryValueKind.DWord);
            return true;
        }

        private void ClipBoard_Click(object sender, RoutedEventArgs e)
        {
            ClipBoardSettings();
        }
    }
}