using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApplication.Client;
using ChatApplication.DataContracts;
using Xamarin.Forms;

namespace ChatApplication
{
    public partial class ChatServiceUriPage : ContentPage
    {

        public ChatServiceUriPage()
        {
            InitializeComponent();
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            var client = new ChatServiceClient(new Uri(uriEntry.Text));
            CreateOrReplaceChatClient(client);

            try
            {
                CreateOrReplaceCurrentProfile(await client.GetProfile(usernameEntry.Text));
            }
            catch (ChatServiceException ex)
            {
                await DisplayAlert("Error", ex.Message, "Cancel");
            }
            await Navigation.PushAsync(new ChatPage());
        }

        async void Handle_Clicked_CreateProfile(object sender, System.EventArgs e)
        {
            var client = new ChatServiceClient(new Uri(uriEntry.Text));
            CreateOrReplaceChatClient(client);
            await Navigation.PushAsync(new AddProfilePage());
        }

        public static void CreateOrReplaceCurrentProfile(UserProfile profile)
        {
            if (!Application.Current.Properties.TryGetValue("CurrentProfile", out Object obj1))
            {
                Application.Current.Properties.Add("CurrentProfile", profile);
            }
            else
            {
                Application.Current.Properties["CurrentProfile"] = profile;
            }
        }

        void CreateOrReplaceChatClient(IChatServiceClient client)
        {
            if (!Application.Current.Properties.TryGetValue("ChatServiceClient", out Object obj))
            {
                Application.Current.Properties.Add("ChatServiceClient", client);
            }
            else
            {
                Application.Current.Properties["ChatServiceClient"] = client;
            }
        }
    }
}
