using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Unosquare.FFME;
using Unosquare.FFME.Windows.Sample.Foundation;

namespace TwitchClipArchiveViewer
{
    public class AppCommands
    {
        #region Private State
        private ClipPlayerViewModel ViewModel;
        private DelegateCommand m_OpenCommand;
        private DelegateCommand m_PauseCommand;
        private DelegateCommand m_PlayCommand;
        private DelegateCommand m_StopCommand;
        private DelegateCommand m_CloseCommand;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AppCommands"/> class.
        /// </summary>
        public AppCommands(ClipPlayerViewModel ViewModel)
        {
            this.ViewModel = ViewModel;
        }

        #endregion

        /// <summary>
        /// Gets the open command.
        /// </summary>
        /// <value>
        /// The open command.
        /// </value>
        public DelegateCommand OpenCommand => m_OpenCommand ??
            (m_OpenCommand = new DelegateCommand(async a =>
            {
                try
                {
                    var uriString = a as string;
                    if (string.IsNullOrWhiteSpace(uriString))
                        return;

                    var m = ViewModel.MediaElement;
                    var target = new Uri(uriString);
                    if (target.ToString().StartsWith(FileInputStream.Scheme, StringComparison.OrdinalIgnoreCase))
                        await m.Open(new FileInputStream(target.LocalPath));
                    else
                        await m.Open(target);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        Application.Current.MainWindow,
                        $"Media Failed: {ex.GetType()}\r\n{ex.Message}",
                        $"{nameof(MediaElement)} Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);
                }
            }));

        /// <summary>
        /// Gets the close command.
        /// </summary>
        /// <value>
        /// The close command.
        /// </value>
        public DelegateCommand CloseCommand => m_CloseCommand ??
            (m_CloseCommand = new DelegateCommand(async o =>
            {
                await ViewModel.MediaElement.Close();

                // or, you can totally dispose manually:
                // ViewModel.MediaElement.Dispose();
            }));

        /// <summary>
        /// Gets the pause command.
        /// </summary>
        /// <value>
        /// The pause command.
        /// </value>
        public DelegateCommand PauseCommand => m_PauseCommand ??
            (m_PauseCommand = new DelegateCommand(async o =>
            {
                await ViewModel.MediaElement.Pause();
            }));

        /// <summary>
        /// Gets the play command.
        /// </summary>
        /// <value>
        /// The play command.
        /// </value>
        public DelegateCommand PlayCommand => m_PlayCommand ??
            (m_PlayCommand = new DelegateCommand(async o =>
            {
                // await Current.MediaElement.Seek(TimeSpan.Zero)
                await ViewModel.MediaElement.Play();
            }));

        /// <summary>
        /// Gets the stop command.
        /// </summary>
        /// <value>
        /// The stop command.
        /// </value>
        public DelegateCommand StopCommand => m_StopCommand ??
            (m_StopCommand = new DelegateCommand(async o =>
            {
                await ViewModel.MediaElement.Stop();
            }));

        /// <summary>
        /// Gets the toggle fullscreen command.
        /// </summary>
        /// <value>
        /// The toggle fullscreen command.
        /// </value>
    }
}
