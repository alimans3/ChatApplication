using System;
using System.Collections.Generic;
using ChatApplication.Client;
using ChatApplication.DataContracts;
using Xamarin.Forms;

namespace ChatApplication
{
    public partial class AddConversationPage : ContentPage
    {
        private IChatServiceClient client = (ChatServiceClient)Application.Current.Properties["ChatServiceClient"];
        private UserProfile profile = (UserProfile)Application.Current.Properties["CurrentProfile"];
        public AddConversationPage()
        {
            InitializeComponent();
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(recipientEntry.Text))
                {
                    await client.GetProfile(recipientEntry.Text);
                    await client.PostConversation(new AddConversationDto
                    {
                        Participants = new List<string> { profile.Username, recipientEntry.Text }
                    });
                }
                await DisplayAlert("Created", $"Conversation with {recipientEntry.Text} is Created", "Continue");
                await Navigation.PopAsync();
            }
            catch(ChatServiceException)
            {
                await DisplayAlert("Error", "Profile doesn't exist or Chat Service could not be reached", "Try Again");
            }

        }
    }
}
