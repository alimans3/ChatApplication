using System;
using Newtonsoft.Json;

namespace ChatApplication.DataContracts
{

    public class GetMessageDto
    {
        
        [JsonConstructor]
        public GetMessageDto(string text, string senderUsername,DateTime utcNow)
        {
            Text = text;
            SenderUsername = senderUsername;
            UtcTime = utcNow;
        }

        public string Text { get; }
        public string SenderUsername { get; }
        public DateTime UtcTime { get; }
    }
}