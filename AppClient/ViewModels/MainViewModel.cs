using AppClient.Extentions;
using AppClient.Models;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Linq;
using System.IO;
using System.Drawing;

namespace AppClient.ViewModels
{
    static class Data 
    {
        public static ObservableCollection<Card> Items = new ObservableCollection<Card>
        {
            new Card()
            {
                Title = "Birds",
            },
            new Card()
            {
                Title = "Animals",
            },
            new Card()
            {
                Title = "Fishes",
            },
        };
    }


    class MainViewModel : Notifier
    {
        public MainViewModel()
        {
            InitializeCommands();
            Items = Data.Items;
        }

        #region Fields

        private bool _isEditing;
        private string _editingCard;
        private string _title;
        private ObservableCollection<Card> _items;
        private bool _isPopupOpen;
        private BitmapImage _image;
        private string _buttonContent;

        #endregion

        #region Properties

        public string Title
        {
            get => _title;
            set => SetValue(ref _title, value);
        }
        public ObservableCollection<Card> Items
        {
            get => _items;
            set => SetValue(ref _items, value);
        }
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set => SetValue(ref _isPopupOpen, value);
        }
        public BitmapImage Image
        {
            get => _image;
            set => SetValue(ref _image, value);
        }
        public string ButtonContent
        {
            get => _buttonContent;
            set => SetValue(ref _buttonContent, value);
        }

        #endregion


        #region Commands

        public RelayCommand GetCommand { get; set; }
        public RelayCommand CreateCardCommand { get; set; }
        public RelayCommand TogglePopupCommand { get; set; }
        public RelayCommand SelectImageCommand { get; set; }
        public RelayCommand DeleteCardCommand { get; set; }
        public RelayCommand EditCardCommand { get; set; }

        #endregion

        #region Methods
        private void InitializeCommands()
        {
            GetCommand = new RelayCommand(GetCards);
            CreateCardCommand = new RelayCommand(CreateCard);
            TogglePopupCommand = new RelayCommand(TogglePopup);
            SelectImageCommand = new RelayCommand(SelectImage);
            DeleteCardCommand = new RelayCommand(DeleteCard);
            EditCardCommand = new RelayCommand(EditCard);
        }
        private void CreateCard(object param)
        {
            if (string.IsNullOrEmpty(Title) /*|| Image == null*/)
            {
                MessageBox.Show("Enter title and image!");
                return;
            }

            if (!_isEditing && Items.Any(card => card.Title == Title))
            {
                MessageBox.Show("The card named " + Title + " already exists!");
                return;
            }

            Card newCard;

            if (!_isEditing)
            {
                newCard = new Card
                {
                    Title = Title,
                    Image = Image
                };

                byte[] bytes = ConvertPngToBytes(Image);
                newCard.ImageBytes = bytes;

                //string stringBytes = Convert.ToString(bytes);
                //newCard.ImageBytes = stringBytes;

                Items.Add(newCard);
            }
            else
            {
                newCard = Items.Where(c => c.Title == _editingCard).FirstOrDefault();
                if (newCard == null)
                {
                    MessageBox.Show("Cannot edit card: '" + _editingCard + "'");
                    return;
                }
                if (Items.Any(c => c.Title == Title))
                {
                    MessageBox.Show("This name already exists!");
                    return;
                }

                newCard.Title = Title;
                newCard.Image = Image;
                
                byte[] bytes = ConvertPngToBytes(Image);
                newCard.ImageBytes = bytes;
                
                //string stringBytes = Convert.ToString(bytes);
                //newCard.ImageBytes = stringBytes;
            }

            TogglePopup(false);
            var cardDetails = WebAPI.PostCall(WebAPI.CardsUri, newCard);
            if (cardDetails != null && cardDetails.Result.StatusCode == System.Net.HttpStatusCode.Created)
            {
                MessageBox.Show("Card has successfully been added!");
            }
            else
            {
                MessageBox.Show("Failed to update details.");
            }

        }
        private void GetCards(object param)
        {
            var cardDetails = WebAPI.GetCall(WebAPI.CardsUri);
            
            if (cardDetails != null && cardDetails.Result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Items = cardDetails.Result.Content.ReadAsAsync<ObservableCollection<Card>>().Result;
            }
            else
            {
                MessageBox.Show("failed");
            }
        }
        private void TogglePopup(object param)
        {
            IsPopupOpen = Convert.ToBoolean(param);
            ButtonContent = "Create";
            Title = "";
            Image = null;
            _isEditing = false;
        }
        private void SelectImage(object param)
        {
            

            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.InitialDirectory = "c:\\";
                dialog.Filter = "Image files | *.png;";
                dialog.RestoreDirectory = true;

                if (dialog.ShowDialog() == true)
                {
                    string selectedFileName = dialog.FileName;

                    
                    System.Drawing.Image image = System.Drawing.Image.FromFile(selectedFileName);

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    Image = bitmap;     
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void DeleteCard(object param)
        {
            string title = Convert.ToString(param);
            Card card = Items.Where(c => c.Title == title).FirstOrDefault();
            if (!Items.Remove(card))
            {
                MessageBox.Show("Cannot delete card: '" + title + "'");
            }
        }
        private void EditCard(object param)
        {
            TogglePopup(true);
            ButtonContent = "Edit";
            _isEditing = true;

            string title = _editingCard = Convert.ToString(param);
            Card card = Items.Where(c => c.Title == title).FirstOrDefault();
            if (card == null)
            {
                MessageBox.Show("Cannot edit card: '" + title + "'");
                return;
            }

            Title = card.Title;
            Image = card.Image;
        }

        public byte[] ImageToBytes(BitmapImage image)
        {
            if (image == null) return new byte[0];
            //image.BeginInit();
            Stream stream = image.StreamSource;
            //image.EndInit();
            byte[] buffer = null;
            if (stream != null && stream.Length > 0)
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    buffer = br.ReadBytes((int)stream.Length);
                }
            }

            return buffer;
        }
        public BitmapImage BytesToImage(byte[] array)
        {
            if (array == null)
            {
                return null;
            }
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        public byte[] ConvertPngToBytes(BitmapImage imageC)
        {
            MemoryStream memStream = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageC));
            encoder.Save(memStream);
            return memStream.ToArray();
        }

        #endregion
    }
}
