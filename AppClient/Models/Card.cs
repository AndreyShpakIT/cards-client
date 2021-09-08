using AppClient.Extentions;
using System.Windows.Media.Imaging;

namespace AppClient.Models
{
    class Card : Notifier
    {
        private string title;
        private BitmapImage image;

        public long Id { get; set; }
        public string Title 
        {
            get => title;
            set => SetValue(ref title, value);
        }

        public BitmapImage Image
        {
            get => image;
            set => SetValue(ref image, value);
        }
        public string ImageBytes { get; set; }
    }
}
