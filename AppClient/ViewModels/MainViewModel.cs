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
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Threading;

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

            Task.Factory.StartNew(() => 
            {
                GetCards(null);
            });
        }

        #region Fields

        private bool isEditing;
        private string editingCard;
        private string title;
        private ObservableCollection<Card> items;
        private bool isPopupOpen;
        private BitmapImage image;
        private string buttonContent;
        private string address = "localhost:44351";

        #endregion

        #region Properties

        public string Title
        {
            get => title;
            set => SetValue(ref title, value);
        }
        public ObservableCollection<Card> Items
        {
            get => items;
            set => SetValue(ref items, value);
        }
        public bool IsPopupOpen
        {
            get => isPopupOpen;
            set => SetValue(ref isPopupOpen, value);
        }
        public BitmapImage Image
        {
            get => image;
            set => SetValue(ref image, value);
        }
        public string ButtonContent
        {
            get => buttonContent;
            set => SetValue(ref buttonContent, value);
        }
        public string Address
        {
            get => address;
            set
            {
                SetValue(ref address, value);
                WebAPI.ServerName = value;
            }
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
            GetCommand = new RelayCommand(async (p) =>
            {
                await Task.Run(() =>
                {
                    GetCards(null);
                });
            });
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

            if (!this.isEditing && Items.Any(card => card.Title == Title))
            {
                MessageBox.Show("The card named " + Title + " already exists!");
                return;
            }

            Card newCard = new Card();

            if (this.isEditing)
            {
                newCard = Items.Where(c => c.Title == editingCard).FirstOrDefault();
                if (newCard == null)
                {
                    MessageBox.Show("Cannot edit card: '" + editingCard + "'");
                    return;
                }
                if (Items.Any(c => c.Title == Title && newCard.Title != editingCard))
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

            bool isEditing = this.isEditing;
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
                var details = await WebAPI.PostCall(card.GetSCard());
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
                var details = await WebAPI.PutCall(card.GetSCard());
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

            try
            {

                var cardDetails = WebAPI.GetCall();

                if (cardDetails != null && cardDetails.Result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        var items = cardDetails.Result.Content.ReadAsAsync<ObservableCollection<Card>>().Result;
                        foreach (var item in items)
                        {
                            byte[] bytes = Convert.FromBase64String(item.ImageBytes);
                            item.Image = BytesToImage(bytes);
                        }
                        Items = items;
                    }));
                }
                else
                {
                    MessageBox.Show("Operation failed!");
                }
            }
            catch (HttpRequestException re)
            {
                MessageBox.Show(nameof(HttpRequestException) + ": " + re.Message);
            }
            catch (Exception e) 
            {
                MessageBox.Show(nameof(Exception) + ": " + e.Message);
            }

            if (Items == null)
            {
                Items = new ObservableCollection<Card>();
            }
        }

    private void TogglePopup(object param)
        {
            IsPopupOpen = Convert.ToBoolean(param);
            ButtonContent = "Create";
            Title = "";
            Image = null;
            isEditing = false;
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
            if (card == null)
            {
                MessageBox.Show("Item is not found!");
                return;
            }
            try
            {
                var response = await WebAPI.DeleteCall(card.Id.ToString());
                if (ShowResult(response))
                {
                    await Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Background,
                        new Action(() =>
                        {
                            if (!Items.Remove(card))
                            {
                                MessageBox.Show("Cannot delete card: '" + title + "'");
                                return;
                            }
                        })
                    );
                }
            }
            catch (HttpRequestException e)
            {
                MessageBox.Show(nameof(HttpRequestException) + ": " + e.Message);
                return;
            }
            catch (Exception e)
            {
                MessageBox.Show(nameof(Exception) + ": " + e.Message);
            }
        }
        private void EditCard(object param)
        {
            TogglePopup(true);
            ButtonContent = "Edit";
            isEditing = true;

            string title = editingCard = Convert.ToString(param);
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
                    var response = await WebAPI.DeleteCall(arr);
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
