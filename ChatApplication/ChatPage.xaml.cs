using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatApplication.Client;
using ChatApplication.DataContracts;
using Microsoft.AspNetCore.SignalR.Client;
using Xamarin.Forms;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace ChatApplication
{
    public partial class ChatPage : ContentPage
    {
        private IChatServiceClient client = (ChatServiceClient)Application.Current.Properties["ChatServiceClient"];
        private UserProfile profile = (UserProfile)Application.Current.Properties["CurrentProfile"];
        private ChatNotificationServiceClient notificationClient = (ChatNotificationServiceClient)Application.Current.Properties["ChatNotificationServiceClient"];
        private ObservableCollection<ConversationModel> CurrentConversations = new ObservableCollection<ConversationModel>();
        private ObservableCollection<MessageModel> CurrentMessages= new ObservableCollection<MessageModel>();
        private GetConversationsListDto CurrentConversationsDto;
        private ConcurrentDictionary<string, bool> onlineDictionary = new ConcurrentDictionary<string, bool>();
        private bool conversationFlag = false;
        private bool messageFlag = false;
        private string selectedRecipient;
        private string selectedConversationId;
        private string nextMessageUri = null;
        private string previousMessageUri = null;
        private HubConnection hubConnection = (HubConnection) Application.Current.Properties["HubConnection"];
        Converter<GetConversationsDto, ConversationModel> converter;

        public ChatPage()
        {
            converter  = new Converter<GetConversationsDto, ConversationModel>((input) =>
            {
                Color convColor;
                onlineDictionary.TryGetValue(input.Recipient.Username, out bool value);
                if (value)
                {
                    convColor = Color.Green;
                }
                else
                {
                    convColor = Color.Red;
                }
                return new ConversationModel
                {
                    Recipient = input.Recipient.Username,
                    Id = input.Id,
                    Color = convColor
                };
            });
            InitializeComponent();
        }

        protected override async void OnDisappearing()
        {
            await hubConnection.InvokeAsync("LeaveGroup", profile.Username);
            await hubConnection.StopAsync();
        }

        protected override async void OnAppearing()
        {
            RefreshConversations(); 
            await hubConnection.StartAsync();
            await hubConnection.InvokeAsync("JoinGroup", profile.Username);
            hubConnection.On("ReceiveNotification", (string payloadString) =>
             {
                var payload = JsonConvert.DeserializeObject<Payload>(payloadString);
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
                    RefreshConversations();
                    messageFlag = false;
                }
                return true;
            });

            Device.StartTimer(TimeSpan.FromSeconds(3), () =>
             {
                 GetPresences();
                 CurrentConversations = new ObservableCollection<ConversationModel>(CurrentConversationsDto.Conversations.ConvertAll(converter));
                 ConversationsList.ItemsSource = CurrentConversations;
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
                CurrentConversationsDto = await client.GetConversations(profile.Username);
                GetPresences();
                CurrentConversations = new ObservableCollection<ConversationModel>(CurrentConversationsDto.Conversations.ConvertAll(converter));
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
                    var messageConverter = new Converter<GetMessageDto, MessageModel>((input) => {
                        return new MessageModel
                        {
                            MessageText = $"{input.SenderUsername} : {input.Text}"
                        };
                    });
                    messages.Messages.Reverse();
                    var messageModels = new ObservableCollection<MessageModel>(messages.Messages.ConvertAll(messageConverter));
                    CurrentMessages = new ObservableCollection<MessageModel>(CurrentMessages.Concat(messageModels));
                    MessagesList.ItemsSource = CurrentMessages;
                }
                catch (ChatServiceException ex)
                {
                    await DisplayAlert("Error", ex.Message, "Cancel");
                }
            }
        }

        async void GetPresences()
        {
            foreach(var dto in CurrentConversationsDto.Conversations)
            {
                var recipientuser = dto.Recipient.Username;
                var presenceDto = await notificationClient.GetPresenceAsync(recipientuser);
                if (!onlineDictionary.TryAdd(recipientuser, presenceDto.isOnline))
                {
                    onlineDictionary.TryGetValue(recipientuser, out bool value);
                    onlineDictionary.TryUpdate(recipientuser, presenceDto.isOnline, value);
                }
            }
        }

    }
}
