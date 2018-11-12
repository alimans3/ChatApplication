using System.Collections.Generic;

namespace ChatApplication.DataContracts
{
    public class GetConversationsListDto
    {
        public GetConversationsListDto(List<GetConversationsDto> conversations, string nextUri, string previousUri)
        {
            Conversations = conversations;
            NextUri = nextUri;
            PreviousUri = previousUri;
        }
        
        public List<GetConversationsDto> Conversations{get; set; }
        public string NextUri { get; }
        public string PreviousUri { get; }
    }
}