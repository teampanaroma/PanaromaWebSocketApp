using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Panaroma.Communication.Application
{
    public partial class NotificationWindow : Window, IComponentConnector
    {
        private static readonly List<NotificationWindow> _notificationWindows = new List<NotificationWindow>();
        private static readonly MediaPlayer mediaPlayer = new MediaPlayer();

        public int ViewTimeOut { get; set; } = Convert.ToInt16(ConfigurationManager.AppSettings["BildirimSuresi"]);

        public string Header { get; set; }

        public string Description { get; set; }

        public string Time { get; }

        public NotificationType NotificationType { get; set; }

        public NotificationWindow()
        {
            InitializeComponent();
        }

        public NotificationWindow(NotificationType notificationType, string header, string description, string time)
        {
            InitializeComponent();
            ShowInTaskbar = false;
            Topmost = true;
            closeButton.Visibility = Visibility.Hidden;
            NotificationType = notificationType;
            Header = header;
            Description = description;
            Time = time;
            if(!_notificationWindows.Any())
            {
                Left = SystemParameters.WorkArea.Width - Width;
                Top = SystemParameters.WorkArea.Height - Height - 3.0;
                doubleAnimation.From = new double?(Left);
                doubleAnimation.To = new double?(Left + 15.0);
            }
            else
            {
                NotificationWindow notificationWindow = _notificationWindows.LastOrDefault();
                if(notificationWindow != null)
                {
                    if(notificationWindow.RestoreBounds.Top < Height)
                    {
                        notificationWindow.Close();
                        _notificationWindows.Remove(notificationWindow);
                        notificationWindow = _notificationWindows.LastOrDefault();
                    }

                    if(notificationWindow != null)
                    {
                        Left = notificationWindow.Left;
                        Top = notificationWindow.RestoreBounds.Top - Height - 5.0;
                        doubleAnimation.From = new double?(Left);
                        doubleAnimation.To = new double?(Left + 15.0);
                    }
                }
            }

            Loaded += new RoutedEventHandler(NotificationWindow_Loaded);
            _notificationWindows.Add(this);
        }

        protected override void OnActivated(System.EventArgs e)
        {
            Task.Delay(ViewTimeOut).ContinueWith((Action<Task>)(b =>
               Dispatcher.BeginInvoke((Action)(() => closeButton_Click(this, new RoutedEventArgs())),
                   Array.Empty<object>())));
        }

        public NotificationWindow Build()
        {
            prepareNotificationWindow(NotificationType);
            return this;
        }

        private void NotificationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            playSound();
        }

        private void playSound()
        {
            mediaPlayer.Open(new Uri("/shake.mp3", UriKind.Relative));
            mediaPlayer.Play();
        }

        private void prepareNotificationWindow(NotificationType notificationType)
        {
            switch(notificationType)
            {
                case NotificationType.Information:
                    NotificationImage.Source = Helpers.ResourceHelper.GetBitmapImage("information");
                    HeaderTextBlock.Text = Header;
                    TimeTextBlock.Text = Time;
                    DescriptionTextBlock.Text = Description;
                    MainDockPanel.Background = Brushes.CadetBlue;
                    break;

                case NotificationType.Success:
                    NotificationImage.Source = Helpers.ResourceHelper.GetBitmapImage("success");
                    HeaderTextBlock.Text = Header;
                    TimeTextBlock.Text = Time;
                    DescriptionTextBlock.Text = Description;
                    MainDockPanel.Background = Brushes.DarkSeaGreen;
                    break;

                case NotificationType.Warning:
                    NotificationImage.Source = Helpers.ResourceHelper.GetBitmapImage("warning");
                    HeaderTextBlock.Text = Header;
                    TimeTextBlock.Text = Time;
                    DescriptionTextBlock.Text = Description;
                    MainDockPanel.Background = Brushes.DarkSalmon;
                    break;

                case NotificationType.Error:
                    NotificationImage.Source = Helpers.ResourceHelper.GetBitmapImage("error");
                    HeaderTextBlock.Text = Header;
                    TimeTextBlock.Text = Time;
                    DescriptionTextBlock.Text = Description;
                    MainDockPanel.Background = Brushes.Brown;
                    break;

                case NotificationType.Default:
                    HeaderTextBlock.Text = Header;
                    TimeTextBlock.Text = Time;
                    DescriptionTextBlock.Text = Description;
                    MainDockPanel.Background = Brushes.White;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("notificationType", notificationType, null);
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation doubleAnimation1 = new DoubleAnimation();
            doubleAnimation1.Duration = new Duration(TimeSpan.FromMilliseconds(1000.0));
            doubleAnimation1.From = new double?(1.0);
            doubleAnimation1.To = new double?(0.0);
            doubleAnimation1.FillBehavior = FillBehavior.Stop;
            DoubleAnimation doubleAnimation2 = doubleAnimation1;
            doubleAnimation2.Completed += new EventHandler(CloseDoubleAnimation_Completed);
            BeginAnimation(OpacityProperty, doubleAnimation2);
        }

        private void CloseDoubleAnimation_Completed(object sender, System.EventArgs e)
        {
            _notificationWindows.Remove(this);
            Close();
        }

        private void MainDockPanel_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                Background.Opacity = 0.5;
                MainDockPanel.Opacity = 0.5;
                DragMove();
            }

            Background.Opacity = 1.0;
            MainDockPanel.Opacity = 1.0;
        }

        private void MainDockPanel_OnMouseMove(object sender, MouseEventArgs e)
        {
            closeButton.Visibility = Visibility.Visible;
        }

        private void MainDockPanel_OnMouseLeave(object sender, MouseEventArgs e)
        {
            closeButton.Visibility = Visibility.Hidden;
        }
    }
}