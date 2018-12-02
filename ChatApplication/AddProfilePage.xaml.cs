using System;
using System.Collections.Generic;
using ChatApplication.Client;
using ChatApplication.DataContracts;
using Xamarin.Forms;

namespace ChatApplication
{
    public partial class AddProfilePage : ContentPage
    {
        private IChatServiceClient client = (ChatServiceClient)Application.Current.Properties["ChatServiceClient"];
        public AddProfilePage()
        {
            InitializeComponent();
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                await client.CreateProfile(new CreateProfileDto
                {
                    FirstName = firstnameEntry.Text,
                    LastName = lastnameEntry.Text,
                    Username=usernameEntry.Text
                });

                await DisplayAlert("Created", $"Profile of {usernameEntry.Text} was created", "Continue");
                ChatServiceUriPage.CreateOrReplaceObject("CurrentProfile",new UserProfile(usernameEntry.Text, firstnameEntry.Text, lastnameEntry.Text));
                await Navigation.PushAsync(new ChatPage());

            }
            catch(ChatServiceException ex)
            {
                await DisplayAlert("Error", ex.Message, "Cancel");
            }
        }
    }
}
