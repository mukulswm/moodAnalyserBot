using CoreBotCLU.Interfaces;
using CoreBotCLU.Models.Requests;
using CoreBotCLU.Models.Responses;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CoreBotCLU.Services
{
    public class TextGenerationService: ITextGenerationService
    {
        private IConfiguration _configuration;
        public TextGenerationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<TextCompletionsResponse> GetTextCompletion(TextCompletionsRequest request)
        {
            using(var client = new HttpClient())
            {
                SetRequestParameters(client);
                var response = await client.PostAsync("chat/completions", GetContentToPost(request));
                var result = JsonConvert.DeserializeObject<TextCompletionsResponse>(await response.Content.ReadAsStringAsync());
                return result;
            }            
        }

        private void SetRequestParameters(HttpClient client)
        {
            client.BaseAddress = new Uri(_configuration["OpenAIHostName"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["OpenAIKey"]);
        }

        private StringContent GetContentToPost(TextCompletionsRequest request)
        {
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            var content = JsonConvert.SerializeObject(request, serializerSettings);
            return new StringContent(content, Encoding.Default, "application/json");
        }
    }
}
