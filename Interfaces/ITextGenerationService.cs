using CoreBotCLU.Models.Requests;
using CoreBotCLU.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBotCLU.Interfaces
{
    public interface ITextGenerationService
    {
        Task<TextCompletionsResponse> GetTextCompletion(TextCompletionsRequest request);
    }
}
