using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatApplication.Client;
using ChatApplication.DataContracts;
using Xamarin.Forms;

namespace ChatApplication
{
    public partial class ChatPage : ContentPage
    {
        private IChatServiceClient client = (ChatServiceClient)Application.Current.Properties["ChatServiceClient"];
        private UserProfile profile = (UserProfile)Application.Current.Properties["CurrentProfile"];
        private GetConversationsListDto conversations;
        private GetMessagesListDto messages; 
        private string selectedRecipient;
        private string selectedConversationId;

        public ChatPage()
        {
            InitializeComponent();
        }
      
        protected override void OnAppearing()
        {
            RefreshConversations();
            Device.StartTimer(new TimeSpan(0, 0, 5), () => {

                RefreshConversations();
                RefreshMessages();
                return true;
            });
        }

        void Handle_ConversationSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            var model = e.SelectedItem as ConversationModel;
            selectedRecipient = model.Recipient ;
            selectedConversationId = conversations.Conversations.Find((obj) => obj.Recipient.Username == selectedRecipient).Id;

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
                conversations = await client.GetConversations(profile.Username);
                var converter = new Converter<GetConversationsDto, ConversationModel>((input) =>
                {
                    return new ConversationModel
                    {
                        Recipient = input.Recipient.Username
                    };
                });
                var conversationModels = new ObservableCollection<ConversationModel>(conversations.Conversations.ConvertAll(converter));
                ConversationsList.ItemsSource = conversationModels;
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
                    messages = await client.GetMessages(selectedConversationId);
                    var converter = new Converter<GetMessageDto, MessageModel>((input) => {
                        return new MessageModel
                        {
                            MessageText = $"{input.SenderUsername} : {input.Text}"
                        };
                    });
                    messages.Messages.Reverse();
                    var messageModels = new ObservableCollection<MessageModel>(messages.Messages.ConvertAll(converter));
                    MessagesList.ItemsSource = messageModels;
                }
                catch (ChatServiceException ex)
                {
                    await DisplayAlert("Error", ex.Message, "Cancel");
                }
            }
        }


    }
}
