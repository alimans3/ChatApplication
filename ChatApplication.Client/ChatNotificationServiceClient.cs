using System;
using System.Net.Http;
using System.Threading.Tasks;
using ChatApplication.DataContracts;
using Newtonsoft.Json;

namespace ChatApplication.Client
{
    public class ChatNotificationServiceClient
    {
        HttpClient httpClient;

        public ChatNotificationServiceClient(HttpClient client)
        {
            httpClient = client;
        }

        public ChatNotificationServiceClient(Uri baseUri)
        {
            httpClient = new HttpClient
            {
                BaseAddress = baseUri
            };
        }

        public async Task<PresenceDto> GetPresenceAsync(string username)
        {
            var response = await httpClient.GetAsync($"api/presence/{username}");
            if (!response.IsSuccessStatusCode)
            {
                throw new ChatServiceException("Failed to get presence",response.ReasonPhrase,response.StatusCode);
            }
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PresenceDto>(content);
            
        }
    }
}
