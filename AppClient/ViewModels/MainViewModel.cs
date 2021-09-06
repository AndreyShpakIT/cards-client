using AppClient.Extentions;
using AppClient.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;

namespace AppClient.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            InitializeCommands();
        }

        #region Fields

        private string _title;
        private string _imageUri;

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

        #endregion


        #region Commands

        public RelayCommand GetCommand { get; set; }
        public RelayCommand PostCommand { get; set; }

        #endregion

        #region Methods
        private void InitializeCommands()
        {
            GetCommand = new RelayCommand(GetCards);
            PostCommand = new RelayCommand(PostCard);
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
                var cards = cardDetails.Result.Content.ReadAsAsync<List<Card>>().Result;
            }
        }
        #endregion
    }
}
