using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TwitchClipArchiveViewer
{
    public class OrderSelector: INotifyPropertyChanged
    {
        public delegate int compareDelegate(TwitchClip twitchClip1, TwitchClip twitchClip2);

        private string _Name;
        private bool _accentDescent;

        public string Name
        {
            get { return _Name; }
            set
            {
                if((_Name??"").Equals(value) == false)
                {
                    this._Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public bool accentDescent
        {
            get { return _accentDescent; }
            set
            {
                if(_accentDescent.Equals(value) == false)
                {
                    this._accentDescent = value;
                    OnPropertyChanged(nameof(accentDescent));
                    OnPropertyChanged(nameof(DisplayAccentDescent));
                }
            }
        }

        public string DisplayAccentDescent
        {
            get
            {
                if(accentDescent == false)
                {
                    return "▲";
                }  else
                {
                    return "▼";
                }
            }
        }

        private compareDelegate _compareFunc;
        public compareDelegate compareFunc
        {
            set
            {
                this._compareFunc = value;
            }
        }

        public int compareTo(TwitchClip twitchClip1, TwitchClip twitchClip2)
        {
            if(accentDescent == false)
            {
                return _compareFunc(twitchClip1, twitchClip2);
            } else
            {
                return -1*_compareFunc(twitchClip1, twitchClip2);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
