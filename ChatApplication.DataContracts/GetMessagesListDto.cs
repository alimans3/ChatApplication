using System.Collections.Generic;

namespace ChatApplication.DataContracts
{
    public class GetMessagesListDto
    {
        public GetMessagesListDto(List<GetMessageDto> messages, string nextUri,string previousUri)
        {
            Messages = messages;
            NextUri = nextUri;
            PreviousUri = previousUri;
        }
        public List<GetMessageDto> Messages { get; set; }

        public string NextUri { get; }
        public string PreviousUri { get; }
    }
}