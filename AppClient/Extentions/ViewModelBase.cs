using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AppClient.Extentions
{
    public class Notifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetValue<T>(ref T property, T value, [CallerMemberName] string name = null)
        {
            if (!EqualityComparer<T>.Default.Equals(property, value))
            {
                property = value;
                OnPropertyChanged(name);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
