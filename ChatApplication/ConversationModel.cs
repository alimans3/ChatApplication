using System;
using Xamarin.Forms;

namespace ChatApplication
{
    public class ConversationModel
    {
        public ConversationModel()
        {
        }
        public string Recipient { get; set; }
        public string Id { get; set; }
        public Color Color { get; set; } = Color.Gray;
    }
}
