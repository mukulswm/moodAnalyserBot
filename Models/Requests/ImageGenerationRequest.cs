using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoreBotCLU.Models.Requests
{
    public class ImageGenerationRequest
    {
        public string Prompt { get; set; }

        [JsonPropertyName("n")]
        public int NumberOfImages { get; set; }

        public string Size { get; set; }
    }
}
