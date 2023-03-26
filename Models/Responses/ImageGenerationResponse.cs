using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoreBotCLU.Models.Responses
{
    public class ImageGenerationResponse
    {
        public int Created { get; set; }

        [JsonPropertyName("data")]
        public List<ImageData> Data { get; set; }
    }

    public class ImageData
    {
        public string Url { get; set; }
    }
}
