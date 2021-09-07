using AppClient.Extentions;
using AppClient.Models;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;

namespace AppClient.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            InitializeCommands();
            ImageUri = "<image uri>";
        }

        #region Fields

        private string _title;
        private string _imageUri;
        private ObservableCollection<Card> _items;
        private bool _isPopupOpen;

        #endregion

        #region Properties

        public string Title
        {
            get => _title;
            set => SetValue(ref _title, value);
        }
        public string ImageUri
        {
            get => _imageUri;
            set => SetValue(ref _imageUri, value);
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

        #endregion


        #region Commands

        public RelayCommand GetCommand { get; set; }
        public RelayCommand PostCommand { get; set; }
        public RelayCommand OpenPopupCommand { get; set; }

        #endregion

        #region Methods
        private void InitializeCommands()
        {
            GetCommand = new RelayCommand(GetCards);
            //PostCommand = new RelayCommand(PostCard);
            PostCommand = new RelayCommand(PostCard);
            OpenPopupCommand = new RelayCommand(OpenPopup);
        }
        private void PostCard(object param)
        {
            Card newCard = new Card
            {
                Title = Title,
                ImageUri = ImageUri
            };

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
        private void OpenPopup(object param)
        {
            IsPopupOpen = true;
        }
        
        #endregion
    }
}
