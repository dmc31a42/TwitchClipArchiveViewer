using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using Unosquare.FFME;
using Unosquare.FFME.Common;
using Unosquare.FFME.Windows.Sample.Foundation;

namespace TwitchClipArchiveViewer
{
    public class ClipPlayerViewModel : ViewModelBase
    {
        private MediaElement m_MediaElement;
        private double m_PlaybackProgress;
        private TaskbarItemProgressState m_PlaybackProgressState;

        public ClipPlayerViewModel(ClipPlayer clipPlayer, TwitchClip twitchClip)
        {
            m_MediaElement = clipPlayer.Media;
            Commands = new AppCommands(ViewModel: this);
            Controller = new ControllerViewModel(this);
            this.TwitchClip = twitchClip;
        }

        public TwitchClip TwitchClip
        {
            get;
        }

        /// <summary>
        /// Gets the media element hosted by the main window.
        /// </summary>
        public MediaElement MediaElement
        {
            get
            {
                return m_MediaElement;
            }
        }

        /// <summary>
        /// Gets the controller.
        /// </summary>
        public ControllerViewModel Controller { get; }


        /// <summary>
        /// Gets or sets the playback progress.
        /// </summary>
        public double PlaybackProgress
        {
            get
            {
                return m_PlaybackProgress;
            }
            set
            {
                m_PlaybackProgress = value;
                NotifyPropertyChanged(nameof(PlaybackProgress));
            }
        }

        /// <summary>
        /// Gets or sets the state of the playback progress.
        /// </summary>
        public TaskbarItemProgressState PlaybackProgressState
        {
            get
            {
                return m_PlaybackProgressState;
            }
            set
            {
                m_PlaybackProgressState = value;
                NotifyPropertyChanged(nameof(PlaybackProgressState));
            }
        }

        /// <summary>
        /// Provides access to application-wide commands.
        /// </summary>
        public AppCommands Commands { get; }

        /// <summary>
        /// Called when application has finished loading.
        /// </summary>
        internal void OnApplicationLoaded()
        {
            Controller.OnApplicationLoaded();

            var m = MediaElement;
            //m.WhenChanged(UpdateWindowTitle,
            //    nameof(m.IsOpen),
            //    nameof(m.IsOpening),
            //    nameof(m.MediaState),
            //    nameof(m.Source));

            //m.MediaOpened += (s, e) =>
            //{
            //    // Reset the Zoom
            //    Controller.MediaElementZoom = 1d;

            //    // Update the Controls
            //    Playlist.IsInOpenMode = false;
            //    IsPlaylistPanelOpen = false;
            //    Playlist.OpenMediaSource = e.Info.MediaSource;
            //};

            //IsPlaylistPanelOpen = true;
            //IsApplicationLoaded = true;
        }

    }
}
