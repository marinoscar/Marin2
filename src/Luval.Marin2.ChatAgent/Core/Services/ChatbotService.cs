using Luval.Marin2.ChatAgent.Core.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.Marin2.ChatAgent.Core.Services
{
    public class ChatbotService
    {

        private readonly IChatCompletionService _chatService;
        private readonly ILogger<ChatbotService> _logger;
        private readonly Chatbot _chatbot;
        private ChatHistory _chatHistory;

        public void Initialize()
        {
            _chatHistory = new ChatHistory(_chatbot.SystemPrompt);
        }

        public async Task RunUserMessageAsync(string message, string? mediaUrl, CancellationToken cancellationToken = default)
        {
            var settings = new OpenAIPromptExecutionSettings()
            {
                Temperature = 0d
            };
            if (string.IsNullOrEmpty(mediaUrl))
                _chatHistory.AddUserMessage(message);
            else
            {
                _chatHistory.AddUserMessage(new ChatMessageContentItemCollection
                {
                    new ImageContent(mediaUrl),
                    new TextContent(message)
                });
            }

            await foreach (var content in _chatService.GetStreamingChatMessageContentsAsync(_chatHistory, settings, null, cancellationToken))
            {
                content.ToString();
                content.
            }
        }


    }
}
