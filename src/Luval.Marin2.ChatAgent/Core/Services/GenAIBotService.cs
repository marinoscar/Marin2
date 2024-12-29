﻿using Luval.Marin2.ChatAgent.Core.Entities;
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
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Luval.Marin2.ChatAgent.Core.Services
{
    public class GenAIBotService
    {

        private readonly IChatCompletionService _chatService;
        private readonly IGenAIBotStorageService _chatbotStorageService;
        private readonly ILogger<GenAIBotService> _logger;
        private readonly IMediaService _mediaService;

        /// <summary>
        /// Occurs when a chat message is completed.
        /// </summary>
        public event EventHandler<ChatMessageCompletedEventArgs>? ChatMessageCompleted;
        /// <summary>
        /// Occurs when a chat message is streamed.
        /// </summary>
        public event EventHandler<ChatMessageStreamEventArgs>? ChatMessageStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenAIBotService"/> class.
        /// </summary>
        /// <param name="chatCompletionService">The chat completion service used to process chat messages.</param>
        /// <param name="chatbotStorageService">The chatbot storage service used to manage chat sessions and messages.</param>
        /// <param name="mediaService">The media service used to handle media file operations.</param>
        /// <param name="logger">The logger instance used for logging information and errors.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the required dependencies are null.</exception>
        public GenAIBotService(IChatCompletionService chatCompletionService, IGenAIBotStorageService chatbotStorageService, IMediaService mediaService, ILogger<GenAIBotService> logger)
        {
            _chatService = chatCompletionService ?? throw new ArgumentNullException(nameof(chatCompletionService), "Chat completion service cannot be null.");
            _chatbotStorageService = chatbotStorageService ?? throw new ArgumentNullException(nameof(chatbotStorageService), "Chatbot storage service cannot be null.");
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService), "Media service cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
        }



        /// <summary>
        /// Submits a user message to a new chat session, processes the message using the chat service, and persists the resulting chat message.
        /// </summary>
        /// <param name="chatbotId">The unique identifier of the chatbot.</param>
        /// <param name="message">The user message to submit to the new chat session.</param>
        /// <param name="sessionTitle">The title of the new chat session. Defaults to "New Session".</param>
        /// <param name="files">Optional collection of files to be uploaded and associated with the message.</param>
        /// <param name="temperature">The temperature setting for the OpenAI prompt execution, which controls the randomness of the response.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The created chat message entity.</returns>
        /// <exception cref="ArgumentException">Thrown when the message is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the chatbotId is invalid.</exception>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Validates the input parameters. If the message is null or empty, it throws an ArgumentException.
        ///    If the chatbotId is invalid, it throws an ArgumentNullException.
        /// 2. Creates a new chat session with the provided title and chatbotId.
        /// 3. Calls the AppendMessageToSession method to process the user message, stream the response content, invoke events, and persist the user message and the assistant's response to the database.
        /// 4. Returns the created chat message entity.
        /// </remarks>
        public async Task<ChatMessage> SubmitMessageToNewSession(ulong chatbotId, string message, string sessionTitle = "New Session", IEnumerable<UploadFile>? files = default, double temperature = 0, CancellationToken cancellationToken = default)
        {
            if (chatbotId <= 0)
            {
                _logger.LogError("Invalid chatbot ID.");
                throw new ArgumentNullException(nameof(chatbotId), "Chatbot ID must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogError("Message cannot be null or empty.");
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            }

            try
            {
                var chatSession = new ChatSession()
                {
                    Title = sessionTitle,
                    GenAIBotId = chatbotId
                };
                

                await _chatbotStorageService.CreateChatSessionAsync(chatSession, cancellationToken);
                _logger.LogInformation("Chat session created successfully with ID {ChatSessionId}.", chatSession.Id);

                var result = await AppendMessageToSession(message, chatSession, files, temperature, cancellationToken);
                _logger.LogInformation("Message submitted to new session successfully.");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while submitting the message to a new session.");
                throw;
            }
        }



        /// <summary>
        /// Appends a user message to an existing chat session by its ID, processes the message using the chat service, and persists the resulting chat message.
        /// </summary>
        /// <param name="message">The user message to append to the chat session.</param>
        /// <param name="chatSessionId">The unique identifier of the chat session to which the message will be appended.</param>
        /// <param name="files">Optional collection of files to be uploaded and associated with the message.</param>
        /// <param name="temperature">The temperature setting for the OpenAI prompt execution, which controls the randomness of the response.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The created chat message entity.</returns>
        /// <exception cref="ArgumentException">Thrown when the message is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the chat session is not found.</exception>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Retrieves the chat session by its unique identifier. If the chat session is not found, it logs an error and throws an ArgumentNullException.
        /// 2. Calls the overloaded AppendMessageToSession method with the retrieved chat session.
        /// 3. The overloaded method validates the input parameters, applies the temperature setting, prepares the chat history, processes the user message, streams the response content, invokes events, and persists the user message and the assistant's response to the database.
        /// 4. Returns the created chat message entity.
        /// </remarks>
        public async Task<ChatMessage> AppendMessageToSession(string message, ulong chatSessionId, IEnumerable<UploadFile>? files = default, double temperature = 0, CancellationToken cancellationToken = default)
        {
            var chatSession = await _chatbotStorageService.GetChatSessionAsync(chatSessionId, cancellationToken);
            if (chatSession == null)
            {
                _logger.LogError($"Chat session {chatSessionId} not found");
                throw new ArgumentNullException(nameof(chatSession));
            }
            return await AppendMessageToSession(message, chatSession, files, temperature, cancellationToken);
        }

        /// <summary>
        /// Appends a user message to an existing chat session, processes the message using the chat service, and persists the resulting chat message.
        /// </summary>
        /// <param name="message">The user message to append to the chat session.</param>
        /// <param name="chatSession">The chat session to which the message will be appended.</param>
        /// <param name="files">Optional collection of files to be uploaded and associated with the message.</param>
        /// <param name="temperature">The temperature setting for the OpenAI prompt execution, which controls the randomness of the response.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The created chat message entity.</returns>
        /// <exception cref="ArgumentException">Thrown when the message is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the chat session is null.</exception>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Validates the input parameters. If the message is null or empty, it throws an ArgumentException.
        ///    If the chat session is null, it throws an ArgumentNullException.
        /// 2. Applies the temperature setting to the OpenAI prompt execution settings.
        /// 3. Prepares the chat history by loading previous messages and uploading any provided files.
        /// 4. Processes the user message using the chat service, streaming the response content and appending it to a StringBuilder.
        /// 5. Invokes the ChatMessageStream event for each streamed content.
        /// 6. Invokes the ChatMessageCompleted event when the message processing is completed.
        /// 7. Adds the assistant's response to the chat history.
        /// 8. Persists the user message and the assistant's response to the database, including any associated media files.
        /// 9. Returns the created chat message entity.
        /// </remarks>
        public async Task<ChatMessage> AppendMessageToSession(string message, ChatSession chatSession, IEnumerable<UploadFile>? files = default, double temperature = 0, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            if (chatSession == null)
                throw new ArgumentNullException(nameof(chatSession));

            //Apply tge temperature to the settings
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
                    if (content.Metadata != null && content.Metadata.ContainsKey("FinishReason") && !string.IsNullOrEmpty(Convert.ToString(content.Metadata["FinishReason"])))
                        finishReason = Convert.ToString(content.Metadata["FinishReason"]);

                    lastContent = content;
                    OnMessageStream(content);
                }

                OnMessageCompleted(lastContent, sb.ToString(), finishReason);
                prepHistory.History.AddAssistantMessage(sb.ToString());
                var result = await PersistMessage(message, chatSession, prepHistory, lastContent, sb, cancellationToken);

                return result;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while running user message.");
                throw;
            }
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

        #region Supporting Methods

        private async Task<ChatMessage> PersistMessage(string message, ChatSession chatSession, ChatHistoryPrep prepHistory, StreamingChatMessageContent? content, StringBuilder sb, CancellationToken cancellationToken = default)
        {
            //updates the chat session with the new message
            var chatInfo = ChatMessageCompletedEventArgs.Create(content, string.Empty, string.Empty);
            List<ChatMessageMedia>? mediaFiles = null;
            var chatMessage = new ChatMessage()
            {
                ChatSessionId = chatSession.Id,
                UserMessage = message,
                AgentResponse = sb.ToString(),
                Model = content?.ModelId,
                ProviderName = "OpenAI",
                InputTokens = chatInfo.InputTokenCount,
                OutputTokens = chatInfo.OutputTokenCount
            };

            if (prepHistory.Files != null && prepHistory.Files.Any())
            {
                mediaFiles = prepHistory.Files.Select(i => new ChatMessageMedia()
                {
                    ChatMessageId = chatMessage.Id,
                    ChatMessage = chatMessage,
                    ContentMD5 = i.ContentMD5,
                    ContentType = i.ContentType,
                    FileName = i.FileName,
                    MediaUrl = i.PublicUri.ToString(),
                    ProviderFileName = i.ProviderFileName,
                    ProviderName = i.ProviderName,
                }).ToList();
            }

            var result = await _chatbotStorageService.AddChatMessageAsync(chatSession.Id, chatMessage, mediaFiles, cancellationToken);
            return result;
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


        private string GetDefaultSystemPrompt()
        {
            return "You are a helpful assistant trained to provide information and answer questions about our products and services. Always be polite and professional. Keep your answers concise and relevant. Do not provide personal opinions or guess answers to questions outside your training. If you cannot provide an answer, guide the user on how they can get further assistance. Remember to respect user privacy and do not ask for personal information unless necessary for the service.";

        }

        private class ChatHistoryPrep
        {
            public ChatHistory History { get; set; } = default!;
            public List<MediaFileInfo> Files { get; set; } = new List<MediaFileInfo>();
        } 

        #endregion

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
            var usage = JsonObject.Parse(content.Metadata["Usage"].ToString());
            if (usage != null)
            {
                res.InputTokenCount = usage["InputTokenCount"]?.GetValue<int>() ?? 0;
                res.OutputTokenCount = usage["OutputTokenCount"]?.GetValue<int>() ?? 0;
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