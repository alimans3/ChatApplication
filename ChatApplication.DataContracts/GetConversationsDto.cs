using System;
using Newtonsoft.Json;

namespace ChatApplication.DataContracts
{
    public class GetConversationsDto
    {
        [JsonConstructor]
        public GetConversationsDto(string id, UserProfile recipient, DateTime lastModifiedDateUtc)
        {
            Id = id;
            Recipient = recipient;
            LastModifiedDateUtc = lastModifiedDateUtc;
        }

        public string Id { get; }
        public UserProfile Recipient { get; }
        public DateTime LastModifiedDateUtc { get; }
        
    }
}