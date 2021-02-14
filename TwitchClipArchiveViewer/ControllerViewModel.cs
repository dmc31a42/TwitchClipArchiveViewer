using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Unosquare.FFME.Common;
using Unosquare.FFME.Windows.Sample.Foundation;

namespace TwitchClipArchiveViewer
{
    public class ControllerViewModel: ViewModelBase
    {
        private bool m_IsAudioControlEnabled = true;

        private Visibility m_AudioControlVisibility = Visibility.Visible;
        private Visibility m_PauseButtonVisibility = Visibility.Visible;
        private Visibility m_PlayButtonVisibility = Visibility.Visible;
        private Visibility m_StopButtonVisibility = Visibility.Visible;

        public ControllerViewModel(ClipPlayerViewModel clipPlayerViewModel)
        {
            Root = clipPlayerViewModel;
        }
        /// <summary>
        /// Gets the root VM this object belongs to.
        /// </summary>
        public ClipPlayerViewModel Root { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is audio control enabled.
        /// </summary>
        public bool IsAudioControlEnabled
        {
            get => m_IsAudioControlEnabled;
            set => SetProperty(ref m_IsAudioControlEnabled, value);
        }

        /// <summary>
        /// Gets or sets the audio control visibility.
        /// </summary>
        public Visibility AudioControlVisibility
        {
            get => m_AudioControlVisibility;
            set => SetProperty(ref m_AudioControlVisibility, value);
        }

        // <summary>
        /// Gets or sets the pause button visibility.
        /// </summary>
        public Visibility PauseButtonVisibility
        {
            get => m_PauseButtonVisibility;
            set => SetProperty(ref m_PauseButtonVisibility, value);
        }

        /// <summary>
        /// Gets or sets the play button visibility.
        /// </summary>
        public Visibility PlayButtonVisibility
        {
            get => m_PlayButtonVisibility;
            set => SetProperty(ref m_PlayButtonVisibility, value);
        }

        /// <summary>
        /// Gets or sets the stop button visibility.
        /// </summary>
        public Visibility StopButtonVisibility
        {
            get => m_StopButtonVisibility;
            set => SetProperty(ref m_StopButtonVisibility, value);
        }

        internal void OnApplicationLoaded()
        {
            var m = Root.MediaElement;

            m.WhenChanged(() =>
            {
                AudioControlVisibility = m.HasAudio ? Visibility.Visible : Visibility.Hidden;
                IsAudioControlEnabled = m.HasAudio;
            },
           nameof(m.HasAudio));

            m.WhenChanged(() => PauseButtonVisibility = m.CanPause && m.IsPlaying ? Visibility.Visible : Visibility.Collapsed,
               nameof(m.CanPause),
               nameof(m.IsPlaying));

            m.WhenChanged(() =>
            {
                PlayButtonVisibility =
                    m.IsOpen && m.IsPlaying == false && m.HasMediaEnded == false && m.IsSeeking == false && m.IsChanging == false ?
                    Visibility.Visible : Visibility.Collapsed;
            },
            nameof(m.IsOpen),
            nameof(m.IsPlaying),
            nameof(m.HasMediaEnded),
            nameof(m.IsSeeking),
            nameof(m.IsChanging));

            m.WhenChanged(() =>
            {
                StopButtonVisibility =
                    m.IsOpen && m.IsChanging == false && m.IsSeeking == false && (m.HasMediaEnded || (m.IsSeekable && m.MediaState != MediaPlaybackState.Stop)) ?
                    Visibility.Visible : Visibility.Hidden;
            },
            nameof(m.IsOpen),
            nameof(m.HasMediaEnded),
            nameof(m.IsSeekable),
            nameof(m.MediaState),
            nameof(m.IsChanging),
            nameof(m.IsSeeking));

        }
    }
}
