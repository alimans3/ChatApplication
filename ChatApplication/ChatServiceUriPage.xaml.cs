using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApplication.Client;
using ChatApplication.DataContracts;
using Microsoft.AspNetCore.SignalR.Client;
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
            var notificationUri = new Uri(new Uri(notificationEntry.Text), "notificationHub");
            var hubConnection = new HubConnectionBuilder().WithUrl(notificationUri).Build();
            var notificationClient = new ChatNotificationServiceClient(new Uri(notificationEntry.Text));
            CreateOrReplaceObject("HubConnection", hubConnection);
            CreateOrReplaceObject("ChatNotificationServiceClient",notificationClient);
            CreateOrReplaceObject("ChatServiceClient",client);
            CreateOrReplaceObject("NotificationUri",notificationUri);

            try
            {
                CreateOrReplaceObject("CurrentProfile",await client.GetProfile(usernameEntry.Text));
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
            CreateOrReplaceObject("ChatServiceClient",client);
            await Navigation.PushAsync(new AddProfilePage());
        }

        public static void CreateOrReplaceObject(string objectName, object obj)
        {
            if (!Application.Current.Properties.TryGetValue(objectName, out Object obj1))
            {
                Application.Current.Properties.Add(objectName, obj);
            }
            else
            {
                Application.Current.Properties[objectName] = obj;
            }
        }

    }
}
