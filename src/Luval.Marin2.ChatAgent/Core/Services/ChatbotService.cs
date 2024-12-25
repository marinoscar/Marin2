using Luval.Marin2.ChatAgent.Core.Entities;
using Luval.Marin2.ChatAgent.Infrastructure.Data;
using Luval.Marin2.ChatAgent.Infrastructure.Interfaces;
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
        private readonly IMediaService _mediaService;

        /// <summary>
        /// Occurs when a chat message is completed.
        /// </summary>
        public EventHandler<ChatMessageCompletedEventArgs>? ChatMessageCompleted;
        /// <summary>
        /// Occurs when a chat message is streamed.
        /// </summary>
        public EventHandler<ChatMessageStreamEventArgs>? ChatMessageStream;

        public ChatbotService(IChatCompletionService chatCompletionService, IMediaService mediaService, ILogger<ChatbotService> logger)
        {
            _chatService = chatCompletionService;
            _logger = logger; ;
            _mediaService = mediaService;
        }

        public async Task<ChatMessage> RunUserMessageAsync(string message, ChatSession chatSession, IEnumerable<UploadFile>? files = default, double temperature = 0, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            if (chatSession == null)
                throw new ArgumentNullException(nameof(chatSession));

            var settings = new OpenAIPromptExecutionSettings()
            {
                Temperature = temperature
            };

            try
            {
                var prepHistory = await PrepareHistoryAsync(chatSession, message, files, cancellationToken);
                var finishReason = string.Empty;
                StreamingChatMessageContent? lastContent = null;
                var sb = new StringBuilder();

                await foreach (var content in _chatService.GetStreamingChatMessageContentsAsync(prepHistory.History, settings, null, cancellationToken))
                {
                    sb.Append(content.Content);
                    if (content.Metadata != null && content.Metadata.ContainsKey("FinishReason"))
                        finishReason = Convert.ToString(content.Metadata["FinishReason"]);
                    lastContent = content;
                    OnMessageStream(content);
                }

                OnMessageCompleted(lastContent, sb.ToString(), finishReason);
                prepHistory.History.AddAssistantMessage(sb.ToString());
                return new ChatMessage();
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while running user message.");
                throw;
            }
        }

        private async Task<ChatHistoryPrep> PrepareHistoryAsync(ChatSession chatSession, string message, IEnumerable<UploadFile>? files = default, CancellationToken cancellationToken = default)
        {
            if (chatSession.Chatbot == null)
                throw new ArgumentNullException("Chatbot is required", nameof(chatSession.Chatbot));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));

            var history = new ChatHistory(chatSession.Chatbot.SystemPrompt ?? GetDefaultSystemPrompt());
            var collection = new ChatMessageContentItemCollection();
            var mediaUploaded = new List<MediaFileInfo>();

            //Load previous messages
            if (chatSession.ChatMessages != null && chatSession.ChatMessages.Any())
            {
                //TODO: Determine what to do with previous messages that have images
                foreach (var msg in chatSession.ChatMessages)
                {
                    history.AddUserMessage(msg.UserMessage);
                    history.AddAssistantMessage(msg.AgentResponse);
                }
            }

            if (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    var info = await _mediaService.UploadMediaAsync(file.Content, file.Name, cancellationToken);
                    info.PublicUri = new Uri(await _mediaService.GetPublicUrlAsync(info.ProviderFileName, cancellationToken));
                    collection.Add(new ImageContent(info.PublicUri));
                    mediaUploaded.Add(info);
                }
            }

            collection.Add(new TextContent(message));
            history.AddUserMessage(collection);

            return new ChatHistoryPrep()
            {
                History = history,
                Files = mediaUploaded
            };
        }

        /// <summary>
        /// Invokes the ChatMessageStream event when a chat message is streamed.
        /// </summary>
        /// <param name="streamingChat">The streaming chat message content.</param>
        protected virtual void OnMessageStream(StreamingChatMessageContent? streamingChat)
        {
            if (streamingChat == null) return;
            ChatMessageStream?.Invoke(this, new ChatMessageStreamEventArgs(streamingChat.Content));
        }

        /// <summary>
        /// Invokes the ChatMessageCompleted event when a chat message is completed.
        /// </summary>
        /// <param name="streamingChat">The streaming chat message content.</param>
        /// <param name="content">The complete content of the chat message.</param>
        /// <param name="finishReason">The reason the chat message stream was finished.</param>
        protected virtual void OnMessageCompleted(StreamingChatMessageContent? streamingChat, string? content, string? finishReason)
        {
            if (streamingChat == null) return;
            ChatMessageCompleted?.Invoke(this, ChatMessageCompletedEventArgs.Create(streamingChat, content, finishReason));
        }

        private string GetDefaultSystemPrompt()
        {
            return "You are a helpful assistant trained to provide information and answer questions about our products and services. Always be polite and professional. Keep your answers concise and relevant. Do not provide personal opinions or guess answers to questions outside your training. If you cannot provide an answer, guide the user on how they can get further assistance. Remember to respect user privacy and do not ask for personal information unless necessary for the service.";

        }

        private class ChatHistoryPrep
        {
            public ChatHistory History { get; set; } = default!;
            public List<MediaFileInfo> Files { get; set; } = new List<MediaFileInfo>();
        }
    }

    public class ChatMessageCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// The content of the chat message.
        /// </summary>
        public string Content { get; set; } = default!;
        /// <summary>
        /// The model ID used to generate the content.
        /// </summary>
        public string ModelId { get; set; } = default!;
        /// <summary>
        /// The reason the stream was finished.
        /// </summary>
        public string FinishReason { get; set; } = default!;
        /// <summary>
        /// The number of output tokens in the content.
        /// </summary>
        public int OutputTokenCount { get; set; } = 0;
        /// <summary>
        /// The number of input tokens in the content.
        /// </summary>
        public int InputTokenCount { get; set; } = 0;
        /// <summary>
        /// The total number of tokens in the content.
        /// </summary>
        public int TotalTokenCount => OutputTokenCount + InputTokenCount;

        /// <summary>
        /// Creates a new instance of the <see cref="ChatMessageCompletedEventArgs"/> class.
        /// </summary>
        /// <param name="content">The <see cref="StreamingChatMessageContent"/></param>
        /// <param name="contentResult">A string with all of the chat response</param>
        /// <param name="finishReason">The reason the streaming ended</param>
        /// <returns></returns>
        public static ChatMessageCompletedEventArgs Create(StreamingChatMessageContent content, string? contentResult, string? finishReason)
        {
            var res = new ChatMessageCompletedEventArgs()
            {
                FinishReason = finishReason ?? default!,
                Content = contentResult ?? default!,
                ModelId = content.ModelId ?? default!
            };

            if (content.Metadata == null) return res;
            var usage = content.Metadata["Usage"] as OpenAI.Chat.ChatTokenUsage;
            if (usage != null)
            {
                res.InputTokenCount = usage.InputTokenCount;
                res.OutputTokenCount = usage.OutputTokenCount;
            }
            return res;
        }
    }

    public class ChatMessageStreamEventArgs : EventArgs
    {
        /// <summary>
        /// The content of the chat message.
        /// </summary>
        public string? Content { get; set; }

        public ChatMessageStreamEventArgs(string? content)
        {
            Content = content;
        }
    }
}