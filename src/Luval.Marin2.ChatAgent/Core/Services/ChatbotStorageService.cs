﻿using Luval.AuthMate.Core;
using Luval.AuthMate.Core.Interfaces;
using Luval.Marin2.ChatAgent.Core.Entities;
using Luval.Marin2.ChatAgent.Infrastructure.Data;
using Luval.Marin2.ChatAgent.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luval.Marin2.ChatAgent.Core.Services
{
    /// <summary>
    /// Service for managing chatbot storage operations.
    /// </summary>
    public class ChatbotStorageService
    {
        private readonly ChatDbContext _dbContext;
        private readonly ILogger<ChatbotStorageService> _logger;
        private readonly IUserResolver _userResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatbotStorageService"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="userResolver">The user resolver instance.</param>
        public ChatbotStorageService(ChatDbContext dbContext, ILogger<ChatbotStorageService> logger, IUserResolver userResolver)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userResolver = userResolver ?? throw new ArgumentNullException(nameof(userResolver));
        }

        #region Chatbot Methods

        /// <summary>
        /// Creates a new chatbot and saves it to the database.
        /// </summary>
        /// <param name="chatbot">The chatbot entity to create.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The created chatbot entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the chatbot is null.</exception>
        public async Task<Chatbot> CreateChatbotAsync(Chatbot chatbot, CancellationToken cancellationToken = default)
        {
            if (chatbot == null)
            {
                _logger.LogError("Chatbot cannot be null.");
                throw new ArgumentNullException(nameof(chatbot));
            }

            try
            {
                chatbot.CreatedBy = _userResolver.GetUserEmail();
                chatbot.UtcCreatedOn = DateTime.UtcNow;
                chatbot.UpdatedBy = _userResolver.GetUserEmail();
                chatbot.UtcUpdatedOn = DateTime.UtcNow;
                chatbot.Version = 1;

                await _dbContext.Chatbots.AddAsync(chatbot, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Chatbot created successfully with ID {ChatbotId}.", chatbot.Id);
                return chatbot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the chatbot.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a chatbot by its unique identifier.
        /// </summary>
        /// <param name="chatbotId">The unique identifier of the chatbot.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The chatbot entity if found; otherwise, null.</returns>
        public async Task<Chatbot?> GetChatbotAsync(ulong chatbotId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Chatbots
                .Include(x => x.ChatSessions)
                .SingleOrDefaultAsync(x => x.Id == chatbotId, cancellationToken);
        }


        /// <summary>
        /// Updates an existing chatbot and saves the changes to the database.
        /// </summary>
        /// <param name="chatbot">The chatbot entity to update.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The updated chatbot entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the chatbot is null.</exception>
        public async Task<Chatbot> UpdateChatbotAsync(Chatbot chatbot, CancellationToken cancellationToken = default)
        {
            if (chatbot == null)
            {
                _logger.LogError("Chatbot cannot be null.");
                throw new ArgumentNullException(nameof(chatbot));
            }
            try
            {
                var isTracked = _dbContext.ChangeTracker.Entries<Chatbot>().Any(e => e.Entity.Id == chatbot.Id);
                chatbot.UpdatedBy = _userResolver.GetUserEmail();
                chatbot.UtcUpdatedOn = DateTime.UtcNow;
                chatbot.Version++;

                if(!isTracked)
                    _dbContext.Chatbots.Update(chatbot);

                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Chatbot updated successfully with ID {ChatbotId}.", chatbot.Id);
                return chatbot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the chatbot.");
                throw;
            }
        }

        /// <summary>
        /// Deletes a chatbot by its unique identifier.
        /// </summary>
        /// <param name="chatbotId">The unique identifier of the chatbot.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        public async Task DeleteChatbotAsync(ulong chatbotId, CancellationToken cancellationToken = default)
        {
            var chatbot = await GetChatbotAsync(chatbotId, cancellationToken);
            if (chatbot == null)
            {
                _logger.LogWarning("Chatbot with ID {ChatbotId} not found.", chatbotId);
                return;
            }
            try
            {
                _dbContext.Chatbots.Remove(chatbot);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Chatbot with ID {ChatbotId} deleted successfully.", chatbotId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the chatbot with ID {ChatbotId}.", chatbotId);
                throw;
            }
        }

        #endregion

        #region ChatSession Methods

        /// <summary>
        /// Creates a new chat session and saves it to the database.
        /// </summary>
        /// <param name="chatSession">The chat session entity to create.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The created chat session entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the chat session is null.</exception>
        public async Task<ChatSession> CreateChatSessionAsync(ChatSession chatSession, CancellationToken cancellationToken = default)
        {
            if (chatSession == null)
            {
                _logger.LogError("ChatSession cannot be null.");
                throw new ArgumentNullException(nameof(chatSession));
            }
            if (chatSession.ChatbotId <= 0)
            {
                _logger.LogError("ChatSession needs a valid chatbot reference");
                throw new ArgumentException(nameof(chatSession.Chatbot));
            }

            try
            {
                await _dbContext.ChatSessions.AddAsync(chatSession, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Chat session created successfully with ID {0}.", chatSession.Id);
                return chatSession;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the chat session.");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing chat session and saves the changes to the database.
        /// </summary>
        /// <param name="chatSession">The chat session entity to update.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The updated chat session entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the chat session is null.</exception>
        public async Task<ChatSession> UpdateChatSessionAsync(ChatSession chatSession, CancellationToken cancellationToken = default)
        {
            if (chatSession == null)
            {
                _logger.LogError("ChatSession cannot be null.");
                throw new ArgumentNullException(nameof(chatSession));
            }
            try
            {
                chatSession.UpdatedBy = _userResolver.GetUserEmail();
                chatSession.UtcUpdatedOn = DateTime.UtcNow.ForceUtc();
                chatSession.Version++;
                _dbContext.ChatSessions.Update(chatSession);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Chat session updated successfully with ID {0}.", chatSession.Id);
                return chatSession;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the chat session.");
                throw;
            }
        }

        /// <summary>
        /// Deletes a chat session by its unique identifier.
        /// </summary>
        /// <param name="chatSessionId">The unique identifier of the chat session.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        public async Task DeleteChatSessionAsync(ulong chatSessionId, CancellationToken cancellationToken = default)
        {
            var chatSession = await GetChatSessionAsync(chatSessionId, cancellationToken);
            if (chatSession == null)
            {
                _logger.LogWarning("Chat session with ID {ChatSessionId} not found.", chatSessionId);
                return;
            }
            try
            {
                _dbContext.ChatSessions.Remove(chatSession);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Chat session with ID {ChatSessionId} deleted successfully.", chatSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the chat session with ID {ChatSessionId}.", chatSessionId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a chat session by its unique identifier.
        /// </summary>
        /// <param name="chatSessionId">The unique identifier of the chat session.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The chat session entity if found; otherwise, null.</returns>
        public async Task<ChatSession?> GetChatSessionAsync(ulong chatSessionId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ChatSessions
                .Include(x => x.Chatbot)
                .ThenInclude(c => c.ChatSessions)
                .Include(x => x.ChatMessages)
                .ThenInclude(m => m.Media)
                .SingleOrDefaultAsync(x => x.Id == chatSessionId, cancellationToken);
        }

        #endregion

        #region ChatMessage Methods

        /// <summary>
        /// Adds a new chat message to a specified chat session and saves it to the database.
        /// </summary>
        /// <param name="chatSessionId">The unique identifier of the chat session to which the message belongs.</param>
        /// <param name="chatMessage">The chat message entity to add.</param>
        /// <param name="media">A collection of media entities associated with the chat message. Default to null</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The created chat message entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the chat message is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the chat session ID is invalid.</exception>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Validates the input parameters. If the chat message is null, it logs an error and throws an ArgumentNullException.
        ///    If the chat session ID is less than or equal to zero, it logs an error and throws an ArgumentException.
        /// 2. Sets the creation and update metadata for the chat message, including the creation and update timestamps,
        ///    the user who created and updated the message, and the version number.
        /// 3. Sets the chat session ID for the chat message.
        /// 4. Adds the chat message to the database context and saves the changes asynchronously.
        /// 5. Logs an informational message indicating the successful creation of the chat message.
        /// 6. If there are any media entities associated with the chat message, it iterates through each media entity,
        ///    sets the chat message ID for the media, adds the media to the database context, and saves the changes asynchronously.
        /// 7. Returns the created chat message entity.
        /// 
        /// If an exception occurs during the process, it logs the exception and rethrows it.
        /// </remarks>
        public async Task<ChatMessage> AddChatMessageAsync(ulong chatSessionId, ChatMessage chatMessage, IEnumerable<ChatMessageMedia>? media = null, CancellationToken cancellationToken = default)
        {
            if (chatMessage == null)
            {
                _logger.LogError("ChatMessage cannot be null.");
                throw new ArgumentNullException(nameof(chatMessage));
            }
            if (chatSessionId <= 0)
            {
                _logger.LogError("ChatMessage needs a valid chat session reference");
                throw new ArgumentException(nameof(chatSessionId));
            }
            try
            {
                chatMessage.UtcCreatedOn = DateTime.UtcNow.ForceUtc();
                chatMessage.UpdatedBy = _userResolver.GetUserEmail();
                chatMessage.UtcUpdatedOn = DateTime.UtcNow.ForceUtc();
                chatMessage.CreatedBy = _userResolver.GetUserEmail();
                chatMessage.Version = 1;

                chatMessage.ChatSessionId = chatSessionId;
                await _dbContext.ChatMessages.AddAsync(chatMessage, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Chat message created successfully with ID {0}.", chatMessage.Id);

                // Add child media
                if (media != null && media.Any())
                    foreach (var m in media)
                    {
                        m.ChatMessageId = chatMessage.Id;
                        await AddMessageMediaAsync(chatMessage.Id, m, cancellationToken);
                        chatMessage.Media.Add(m);
                    }
                return chatMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the chat message.");
                throw;
            }
        }

        /// <summary>
        /// Adds media to a chat message and saves it to the database.
        /// </summary>
        /// <param name="chatMessageId">The unique identifier of the chat message.</param>
        /// <param name="media">The media entity to add.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The created chat message media entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the media is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the chat message ID is invalid.</exception>
        public async Task<ChatMessageMedia> AddMessageMediaAsync(ulong chatMessageId, ChatMessageMedia media, CancellationToken cancellationToken = default)
        {
            if (media == null)
            {
                _logger.LogError("ChatMessageMedia cannot be null.");
                throw new ArgumentNullException(nameof(media));
            }
            if (chatMessageId <= 0)
            {
                _logger.LogError("ChatMessageMedia needs a valid chat message reference");
                throw new ArgumentException(nameof(chatMessageId));
            }
            try
            {
                media.UtcCreatedOn = DateTime.UtcNow.ForceUtc();
                media.UpdatedBy = _userResolver.GetUserEmail();
                media.UtcUpdatedOn = DateTime.UtcNow.ForceUtc();
                media.CreatedBy = _userResolver.GetUserEmail();
                media.Version = 1;
                media.ChatMessageId = chatMessageId;
                await _dbContext.ChatMessageMedia.AddAsync(media, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Chat message media created successfully with ID {0}.", media.Id);
                return media;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the chat message media.");
                throw;
            }
        } 

        #endregion

    }
}