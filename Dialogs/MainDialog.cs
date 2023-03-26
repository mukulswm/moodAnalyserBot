// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreBotCLU.Interfaces;
using CoreBotCLU.Models;
using CoreBotCLU.Models.Requests;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly MoodRecognizer _cluRecognizer;
        protected readonly ILogger Logger;
        private readonly ITextGenerationService _textGenerationService;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(MoodRecognizer cluRecognizer, ILogger<MainDialog> logger, ITextGenerationService textGenerationService)
            : base(nameof(MainDialog))
        {
            _cluRecognizer = cluRecognizer;
            Logger = logger;
            _textGenerationService = textGenerationService;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_cluRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: CLU is not configured. To enable all capabilities, add 'CluProjectName', 'CluDeploymentName', 'CluAPIKey' and 'CluAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            // // Use the text provided in FinalStepAsync or the default if it is the first time.
            // var weekLaterDate = DateTime.Now.AddDays(7).ToString("MMMM d, yyyy");
            var messageText = stepContext.Options?.ToString() ?? $"How are you feeling today?\nSay something like Good, neutral or low";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var thankYouMessageText = "";
            if (!_cluRecognizer.IsConfigured)
            {
                // CLU is not configured, we just run the MoodDialog path with an empty BookingDetailsInstance.
                var tryAgainMessageText = "Please try again";
                var tryAgainMessage = MessageFactory.Text(tryAgainMessageText, tryAgainMessageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(tryAgainMessage, cancellationToken);
            }

            // Call CLU and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            var cluResult = await _cluRecognizer.RecognizeAsync<MoodAnalyser>(stepContext.Context, cancellationToken);
            switch (cluResult.GetTopIntent().intent)
            {
                case MoodAnalyser.Intent.Happy:
                    var res = await _textGenerationService.GetTextCompletion(GenerateRequest("happy"));
                    thankYouMessageText = res.Choices[0].Message.Content;
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(thankYouMessageText, thankYouMessageText, InputHints.IgnoringInput), cancellationToken);
                    break;
                case MoodAnalyser.Intent.Sad:
                    var mes = "I hope below jokes will make you feel good...";
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(mes, mes, InputHints.IgnoringInput), cancellationToken);
                    res = await _textGenerationService.GetTextCompletion(GenerateRequest("sad"));
                    thankYouMessageText = res.Choices[0].Message.Content;
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(thankYouMessageText, thankYouMessageText, InputHints.IgnoringInput), cancellationToken);
                    break;
                case MoodAnalyser.Intent.Neutral:
                    var message = "Please go through simple meditation to enhance your mood, please wait for a while..";
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(message, message, InputHints.IgnoringInput), cancellationToken);
                    res = await _textGenerationService.GetTextCompletion(GenerateRequest("neutral"));
                    thankYouMessageText = res.Choices[0].Message.Content;
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(thankYouMessageText, thankYouMessageText, InputHints.IgnoringInput), cancellationToken);
                    break;
                default:
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {cluResult.GetTopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is MoodDetail result)
            {
                // Now we have all the booking details call the booking service.

                // If the call to the booking service was successful tell the user.

                // var timeProperty = new TimexProperty(result.TravelDate);
                // var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
                var messageText = $"ok";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            // Restart the main dialog with a different message the second time around
            var promptMessage = "Tell me again";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }

        private TextCompletionsRequest GenerateRequest(string intent)
        {
            return new TextCompletionsRequest
            {
                Model = "gpt-3.5-turbo",
                Temperature = 0.7,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = "user",
                        Content = GetContentBasedOnIntent(intent)
                    }
                }
            };
        }

        private string GetContentBasedOnIntent(string intent)
        {
            var st = "";
            switch (intent)
            {
                case "happy":
                    st = "Say glad to hear that";
                    break;
                case "sad":
                    st = "Tell me some joke";
                    break;
                case "neutral":
                    st = "Give some a meditation exercise";
                    break;
                default:
                    st = "Say I didn't understand you.";
                    break;
            }

            return st;
        }
    }
}
