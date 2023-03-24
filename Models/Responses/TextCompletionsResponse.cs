using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoreBotCLU.Models.Responses
{
    public class TextCompletionsResponse
    {
        public string Id { get; set; }
        [JsonPropertyName("object")]
        public string ResponseObject { get; set; }
        public int Created { get; set; }
        public string Model { get; set; }
        public Usage Usage { get; set; }
        public List<Choice> Choices { get; set; }
    }
    public class Choice
    {
        public Message Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }

        public int Index { get; set; }
    }

    public class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }
        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

}
