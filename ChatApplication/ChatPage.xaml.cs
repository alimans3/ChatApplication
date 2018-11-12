using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatApplication.Client;
using ChatApplication.DataContracts;
using Microsoft.AspNetCore.SignalR.Client;
using Xamarin.Forms;
using System.Linq;

namespace ChatApplication
{
    public partial class ChatPage : ContentPage
    {
        private IChatServiceClient client = (ChatServiceClient)Application.Current.Properties["ChatServiceClient"];
        private UserProfile profile = (UserProfile)Application.Current.Properties["CurrentProfile"];
        private Uri notificationUri = (Uri)Application.Current.Properties["NotificationUri"];
        private ObservableCollection<ConversationModel> CurrentConversations = new ObservableCollection<ConversationModel>();
        private ObservableCollection<MessageModel> CurrentMessages= new ObservableCollection<MessageModel>();
        private bool conversationFlag = false;
        private bool messageFlag = false;
        private string selectedRecipient;
        private string selectedConversationId;
        private string nextConvUri = null;
        private string previousConvUri = null;
        private string nextMessageUri = null;
        private string previousMessageUri = null;

        public ChatPage()
        {
            InitializeComponent();
        }
      
        protected override async void OnAppearing()
        {
            RefreshConversations();
            var hubConnection = new HubConnectionBuilder().WithUrl(notificationUri).Build();
            await hubConnection.StartAsync();
            await hubConnection.InvokeAsync("JoinGroup", profile.Username);
            hubConnection.On("RecieveNotification", (Payload payload) =>
             {
                if (payload.Type == Payload.ConversationType)
                {
                     conversationFlag = true;
                }
                else if (payload.Type == Payload.MessageType && payload.ConversationId == selectedConversationId)
                {
                     messageFlag = true;
                }
             });
            Device.StartTimer(new TimeSpan(0,0,0,0,100), () =>
            {
                if (conversationFlag == true)
                {
                    RefreshConversations();
                    conversationFlag = false;
                }
                if (messageFlag == true)
                {
                    RefreshMessages();
                    messageFlag = false;
                }
                return true;
            });
            Device.StartTimer(new TimeSpan(0, 1, 0), () => {

                RefreshConversations();
                RefreshMessages();
                return true;
            });
        }

        void Handle_ConversationSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            var model = e.SelectedItem as ConversationModel;
            selectedRecipient = model.Recipient ;
            selectedConversationId = model.Id;
            nextMessageUri = null;
            previousMessageUri = null;
            CurrentMessages = new ObservableCollection<MessageModel>();

            RefreshMessages();

        }

        async void Handle_Clicked_SendButton(object sender, System.EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(MessageEntry.Text))
                {
                    await client.PostMessage(selectedConversationId, new AddMessageDto(MessageEntry.Text, profile.Username));
                    MessageEntry.Text = "";
                }
            }
            catch(ChatServiceException ex)
            {
                await DisplayAlert("Error", ex.Message, "Cancel");
            }
        }

        void Handle_Clicked_AddConversation(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new AddConversationPage());
        }

        async void RefreshConversations()
        {
            try
            {
                GetConversationsListDto conversations;
                if (nextConvUri == null)
                {
                    conversations = await client.GetConversations(profile.Username);
                    nextConvUri = conversations.NextUri;
                    previousConvUri = conversations.PreviousUri;
                }
                else
                {
                    conversations = await client.GetConversationsFromUri(nextConvUri);
                    nextConvUri = conversations.NextUri;
                    previousConvUri = conversations.PreviousUri;
                }

                var converter = new Converter<GetConversationsDto, ConversationModel>((input) =>
                {
                    return new ConversationModel
                    {
                        Recipient = input.Recipient.Username,
                        Id = input.Id
                    };
                });
                var conversationModels = new ObservableCollection<ConversationModel>(conversations.Conversations.ConvertAll(converter));
                CurrentConversations = new ObservableCollection<ConversationModel>(CurrentConversations.Concat(conversationModels));

                ConversationsList.ItemsSource = CurrentConversations;
            }
            catch (ChatServiceException ex)
            {
                await DisplayAlert("Error", ex.Message, "Cancel");
            }
        }

        async void RefreshMessages()
        {
            if (!string.IsNullOrEmpty(selectedRecipient))
            {
                try
                {
                    GetMessagesListDto messages;
                    if (nextMessageUri == null)
                    {
                        messages = await client.GetMessages(selectedConversationId);
                        nextMessageUri = messages.NextUri;
                        previousMessageUri = messages.NextUri;
                    }
                    else {
                        messages = await client.GetMessagesFromUri(nextMessageUri);
                        nextMessageUri = messages.NextUri;
                        previousMessageUri = messages.PreviousUri;
                    }
                    var converter = new Converter<GetMessageDto, MessageModel>((input) => {
                        return new MessageModel
                        {
                            MessageText = $"{input.SenderUsername} : {input.Text}"
                        };
                    });
                    messages.Messages.Reverse();
                    var messageModels = new ObservableCollection<MessageModel>(messages.Messages.ConvertAll(converter));
                    CurrentMessages = new ObservableCollection<MessageModel>(CurrentMessages.Concat(messageModels));
                    MessagesList.ItemsSource = CurrentMessages;
                }
                catch (ChatServiceException ex)
                {
                    await DisplayAlert("Error", ex.Message, "Cancel");
                }
            }
        }


    }
}
