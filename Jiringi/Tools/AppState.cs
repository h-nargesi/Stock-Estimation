using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Media;

namespace Photon.Jiringi.Tools
{
    public class AppState : INotifyPropertyChanged
    {
        readonly StringBuilder full_info = new StringBuilder();
        MessageState state = MessageState.Info;

        public event PropertyChangedEventHandler PropertyChanged;

        public void ChangeStatus(MessageState state, string value)
        {
            if (this.state != state)
            {
                this.state = state;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color)));
            }

            Message = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
        }
        public void ChangeStatusWithSave(MessageState state, string value)
        {
            ChangeStatus(state, value);

            full_info.Append(value).Append("\r\n");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FullInfo)));
        }
        public void ChangeStatusWithSave(MessageState state, string value, string report)
        {
            ChangeStatus(state, value);

            full_info.Append("\r\n").Append(report);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FullInfo)));
        }

        public string FullInfo
        {
            get { return full_info.ToString(); }
        }
        public string Message { get; private set; } = "Loading application ...";
        public Brush Color
        {
            get
            {
                return state switch
                {
                    MessageState.Info => INFO,
                    MessageState.Error => ERROR,
                    MessageState.Done => DONE,
                    _ => BLANK,
                };
            }
        }

        static readonly SolidColorBrush
            BLANK = new SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 64, 64)),
            INFO = new SolidColorBrush(System.Windows.Media.Color.FromRgb(47, 117, 181)),
            ERROR = new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 0, 50)),
            DONE = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 176, 80));
    }

    public enum MessageState { None, Error, Info, Done }
}
