using Sdl.MultiSelectComboBox.API;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TwitchClipArchiveViewer
{
    public class TwitchClip: INotifyPropertyChanged
    {
        public string id { get; set; }
        public string url { get; set; }
        public string embed_url { get; set; }
        public string broadcaster_id { get; set; }
        public string broadcaster_name { get; set; }
        public string creator_id { get; set; }
        public string creator_name { get; set; }
        public string video_id { get; set; }
        public string game_id { get; set; }
        public string language { get; set; }
        public string title { get; set; }
        public int view_count { get; set; }
        public DateTime created_at { get; set; }
        private string _thumbnail_url;
        public string thumbnail_url { 
            get { return _thumbnail_url; } 
            set {
                _thumbnail_url = value;
                OnPropertyChanged(nameof(thumbnail_url));
            } }
        public TwitchGame game { get; set; }
        private string _download_url;
        public string download_url
        {
            get
            {
                if (_download_url == null)
                {
                    int index = this.thumbnail_url.IndexOf("-preview");
                    if (index != -1)
                    {
                        _download_url = this.thumbnail_url.Substring(0, index) + ".mp4";
                    }
                    else
                    {
                        _download_url = "";
                    }
                }
                return this._download_url;
            }
            set
            {
                this._download_url = value;
            }
        }
        private string _filename;
        public string filename
        {
            get
            {
                if (_filename == null)
                {
                    if (String.IsNullOrWhiteSpace(this.download_url))
                    {
                        this._filename = "";
                    }
                    else
                    {
                        int index = this.download_url.LastIndexOf("/");
                        if (index != -1)
                        {
                            this._filename = this.download_url.Substring(index + 1);
                        }
                        else
                        {
                            this._filename = "";
                        }
                    }
                }
                return this._filename;
            }
            set
            {
                this._filename = value;
            }
        }
        public string gamename { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TwitchGame
    {
        public object id { get; set; }
        public string name { get; set; }
        public string box_art_url { get; set; }
    }

    public class TwitchGameStrict : IItemEnabledAware, INotifyPropertyChanged
    {
        private bool _isEnabled;
        public string id { get; set; }
        public string name { get; set; }
        public string box_art_url { get; set; }

        public TwitchGameStrict()
        {
            _isEnabled = true;
        }
        public bool IsEnabled { get => _isEnabled; set
            {
                if(_isEnabled.Equals(value))
                {
                    return;
                }
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
