using AppClient.Extentions;
using System.Windows.Media.Imaging;

namespace AppClient.Models
{
    class Card : Notifier
    {
        private string title = "";
        private BitmapImage image;
        private long id = -1;
        private bool isSelected;

        public long Id
        {
            get => id;
            set => SetValue(ref id, value);
        }
        public string Title 
        {
            get => title;
            set => SetValue(ref title, value);
        }
        public bool IsSelected
        {
            get => isSelected;
            set => SetValue(ref isSelected, value);
        }

        public BitmapImage Image
        {
            get => image;
            set => SetValue(ref image, value);
        }
        public string ImageBytes { get; set; } = "";

        public SCard GetSCard()
        {
            return new SCard()
            {
                Id = this.Id,
                Title = this.Title,
                ImageBytes = this.ImageBytes,
            };
        }
    }
}
