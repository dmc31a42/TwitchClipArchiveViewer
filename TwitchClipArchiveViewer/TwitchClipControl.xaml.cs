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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TwitchClipArchiveViewer
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TwitchClipControl : UserControl
    {
        private bool IsPlay = false;
        public TwitchClipControl()
        {
            InitializeComponent();
        }

        private void MediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            (e.Source as MediaElement)?.Play();
            (e.Source as MediaElement)?.Pause();
        }

        private void MediaElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is MediaElement mediaElement)
            {
                if(IsPlay)
                {
                    mediaElement.Stop();
                    IsPlay = false;
                } else
                {
                    mediaElement.Play();
                    IsPlay = true;
                }
            }
        }

        private void MediaElement_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(sender is MediaElement mediaElement)
            {
                if(mediaElement.IsVisible)
                {
                    mediaElement.Play();
                    mediaElement.Pause();
                } else
                {
                    mediaElement.Stop();
                }
            }
        }
    }
}
