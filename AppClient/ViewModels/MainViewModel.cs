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
using System.Collections.Generic;

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
            //Items = Data.Items;
            
            GetCards(null);
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
        public RelayCommand SortCardsCommand { get; set; }
        public RelayCommand DeleteSelectedCommand { get; set; }

        #endregion

        #region Methods
        private void InitializeCommands()
        {
            GetCommand = new RelayCommand(GetCards);
            CreateCardCommand = new RelayCommand(CreateCard);
            TogglePopupCommand = new RelayCommand(TogglePopup);
            SelectImageCommand = new RelayCommand(SelectImage);
            DeleteCardCommand = new RelayCommand(DeleteCardAsync);
            EditCardCommand = new RelayCommand(EditCard);
            SortCardsCommand = new RelayCommand(SortCards);
            DeleteSelectedCommand = new RelayCommand(DeleteSelectedAsync);
        }
        private void CreateCard(object param)
        {
            if (string.IsNullOrEmpty(Title) || Image == null)
            {
                MessageBox.Show("Enter title and image!");
                return;
            }

            if (!_isEditing && Items.Any(card => card.Title == Title))
            {
                MessageBox.Show("The card named " + Title + " already exists!");
                return;
            }

            Card newCard = new Card();

            if (_isEditing)
            {
                newCard = Items.Where(c => c.Title == _editingCard).FirstOrDefault();
                if (newCard == null)
                {
                    MessageBox.Show("Cannot edit card: '" + _editingCard + "'");
                    return;
                }
                if (Items.Any(c => c.Title == Title && newCard.Title != _editingCard))
                {
                    MessageBox.Show("This name already exists!");
                    return;
                }
            }
            newCard.Title = Title;
            newCard.Image = Image;

            if (Image != null)
            {
                byte[] bytes = ConvertPngToBytes(Image);
                string stringBytes = Convert.ToBase64String(bytes);
                newCard.ImageBytes = stringBytes;
            }

            bool isEditing = _isEditing;
            TogglePopup(false);

            if (isEditing)
            {
                PutAsync(newCard);
            }
            else
            {
                PostAsync(newCard);
            }
        }
        private async void PostAsync(Card card)
        {
            try
            {
                var details = await WebAPI.PostCall(WebAPI.CardsUri, card.GetSCard());
                if (ShowResult(details))
                {
                    card.Id = GetIdOfResponse(details);
                    Items.Add(card);
                }
            }
            catch (HttpRequestException re)
            {
                MessageBox.Show("HttpRequestException: " + re.Message);
            }
            catch (Exception)
            {

            }
        }
        private async void PutAsync(Card card)
        {
            try
            {
                var details = await WebAPI.PutCall(WebAPI.CardsUri, card.GetSCard());
                card.Id = GetIdOfResponse(details);

                ShowResult(details);
            }
            catch (HttpRequestException re)
            {
                MessageBox.Show("HttpRequestException: " + re.Message);
            }
            catch (Exception)
            {

            }
        }
        
        private int GetIdOfResponse(HttpResponseMessage response)
        {
            return response == null ? -1 : Convert.ToInt32(response.Headers.Location.Segments.Last());
        }
        private bool ShowResult(HttpResponseMessage response)
        {
            if (response != null && response.IsSuccessStatusCode)
            {
                // MessageBox.Show("Operation succed!");
                return true;
            }
            else
            {
                MessageBox.Show("Operation failed: " + response?.ReasonPhrase);
                return false;
            }
        }

        private void GetCards(object param)
        {
            GetAsync();
            if (Items == null)
            {
                Items = new ObservableCollection<Card>();
            }
        }
        private async void GetAsync()
        {
            try
            {
                var cardDetails = await WebAPI.GetCall(WebAPI.CardsUri);

                if (cardDetails != null && cardDetails.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var items = cardDetails.Content.ReadAsAsync<ObservableCollection<Card>>().Result;
                    foreach (var item in items)
                    {
                        byte[] bytes = Convert.FromBase64String(item.ImageBytes);
                        item.Image = BytesToImage(bytes);
                    }
                    Items = items;
                }
                else
                {
                    MessageBox.Show("Operation failed!");
                }
            }
            catch (HttpRequestException re)
            {
                MessageBox.Show("HttpRequestException: " + re.Message);
            }
            catch (Exception) { }
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
        private async void DeleteCardAsync(object param)
        {
            string title = Convert.ToString(param);
            Card card = Items.Where(c => c.Title == title).FirstOrDefault();

            try
            {
                var response = await WebAPI.DeleteCall(WebAPI.CardsUri, card.Id.ToString());
                if (ShowResult(response))
                {
                    if (!Items.Remove(card))
                    {
                        MessageBox.Show("Cannot delete card: '" + title + "'");
                        return;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                MessageBox.Show(nameof(HttpRequestException) + ": " + e.Message);
            }
            catch (Exception) { }
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
        private void SortCards(object param)
        {
            Items = new ObservableCollection<Card>(Items.OrderBy(card => card.Title));
        }
        private async void DeleteSelectedAsync(object param)
        {
            List<long> ids = new List<long>();
            foreach(Card card in Items)
            {
                if (card.IsSelected) 
                { 
                    ids.Add(card.Id);
                }
            }
            if (ids.Count > 0)
            { 
                try
                {
                    long[] arr = ids.ToArray<long>();
                    var response = await WebAPI.DeleteCall(WebAPI.CardsUri + "/delete", arr);
                    if (ShowResult(response))
                    {
                        foreach (long id in ids)
                        {
                            Card card = Items.Where(c => c.Id == id).First();
                            Items.Remove(card);
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show(nameof(HttpRequestException) + ": " + e.Message);
                }
                catch (Exception) { }
            }
            else
            {
                MessageBox.Show("Select at least 1 card!");
            }
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
            if (array == null || array.Length == 0)
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
