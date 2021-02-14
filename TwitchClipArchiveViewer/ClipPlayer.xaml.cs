using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TwitchClipArchiveViewer
{
    /// <summary>
    /// ClipPlayer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ClipPlayer : Window
    {
        public TwitchClip twitchClip;
        public ClipPlayerViewModel ViewModel;

        #region Fields
        private DateTime LastMouseMoveTime;
        private bool IsControllerHideCompleted;
        private Point LastMousePosition;
        private DispatcherTimer MouseMoveTimer;
        #endregion

        #region Properties
        private Storyboard HideControllerAnimation => FindResource("HideControlOpacity") as Storyboard;
        private Storyboard ShowControllerAnimation => FindResource("ShowControlOpacity") as Storyboard;
        #endregion

        public ClipPlayer(TwitchClip twitchClip)
        {
            this.twitchClip = twitchClip;
            InitializeComponent();
            ViewModel = new ClipPlayerViewModel(this, twitchClip);
            this.DataContext = ViewModel;

            InitializeMainWindow();

            ViewModel.Commands.OpenCommand.Execute(twitchClip.download_url);
        }

        /// <summary>
        /// Handles the Loaded event of the MainWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Remove the event handler reference
            Loaded -= OnWindowLoaded;

            ViewModel.OnApplicationLoaded();

            //// Compute and Apply Sizing Properties
            //if (Content is UIElement contentElement &&
            //    VisualTreeHelper.GetParent(contentElement) is ContentPresenter presenter)
            //{
            //    presenter.MinWidth = MinWidth;
            //    presenter.MinHeight = MinHeight;

            //    SizeToContent = SizeToContent.WidthAndHeight;
            //    MinWidth = ActualWidth;
            //    MinHeight = ActualHeight;
            //    SizeToContent = SizeToContent.Manual;
            //}

            // Place on secondary screen by default if there is one
            {
                var screenOffsetX = 0d;
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;

                if ((int)SystemParameters.VirtualScreenWidth != (int)SystemParameters.FullPrimaryScreenWidth &&
                    (int)SystemParameters.VirtualScreenLeft == 0 && (int)SystemParameters.VirtualScreenTop == 0)
                {
                    screenOffsetX = SystemParameters.PrimaryScreenWidth;
                    screenWidth = SystemParameters.VirtualScreenWidth - SystemParameters.PrimaryScreenWidth;
                }

                Left = screenOffsetX + ((screenWidth - ActualWidth) / 2d);
                Top = (screenHeight - ActualHeight) / 2d;
            }

            //// Open a file if it is specified in the arguments
            //var args = Environment.GetCommandLineArgs();
            //if (args.Length > 1)
            //{
            //    ViewModel.Commands.OpenCommand.Execute(args[1].Trim());
            //}
        }

        /// <summary>
        /// Initializes the main window.
        /// </summary>
        private void InitializeMainWindow()
        {
            Loaded += OnWindowLoaded;


            #region Mouse Move Detection for Hiding the Controller Panel

            LastMouseMoveTime = DateTime.UtcNow;

            Loaded += (s, e) =>
            {
                Storyboard.SetTarget(HideControllerAnimation, ControllerPanel);
                Storyboard.SetTarget(ShowControllerAnimation, ControllerPanel);

                HideControllerAnimation.Completed += (es, ee) =>
                {
                    ControllerPanel.Visibility = Visibility.Hidden;
                    IsControllerHideCompleted = true;
                };

                ShowControllerAnimation.Completed += (es, ee) =>
                {
                    IsControllerHideCompleted = false;
                };
            };

            MouseMove += (s, e) =>
            {
                var currentPosition = e.GetPosition(clipPlayer);
                if (Math.Abs(currentPosition.X - LastMousePosition.X) > double.Epsilon ||
                    Math.Abs(currentPosition.Y - LastMousePosition.Y) > double.Epsilon)
                    LastMouseMoveTime = DateTime.UtcNow;

                LastMousePosition = currentPosition;
            };

            MouseLeave += (s, e) =>
            {
                LastMouseMoveTime = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(10));
            };

            MouseMoveTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(150),
                IsEnabled = true
            };

            MouseMoveTimer.Tick += (s, e) =>
            {
                var elapsedSinceMouseMove = DateTime.UtcNow.Subtract(LastMouseMoveTime);
                if (elapsedSinceMouseMove.TotalMilliseconds >= 3000 && Media.IsOpen && ControllerPanel.IsMouseOver == false
                    //&& ControllerPanel.SoundMenuPopup.IsOpen == false
                    )
                {
                    if (IsControllerHideCompleted) return;
                    Cursor = Cursors.None;
                    HideControllerAnimation?.Begin();
                    IsControllerHideCompleted = false;
                }
                else
                {
                    Cursor = Cursors.Arrow;
                    ControllerPanel.Visibility = Visibility.Visible;
                    ShowControllerAnimation?.Begin();
                }
            };

            MouseMoveTimer.Start();

            #endregion
        }
    }
}
