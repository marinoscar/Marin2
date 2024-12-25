using Luval.AuthMate.Core;
using Luval.AuthMate.Core.Interfaces;
using Luval.Marin2.ChatAgent.Core.Entities;
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
        private readonly IChatDbContext _dbContext;
        private readonly ILogger<ChatbotStorageService> _logger;
        private readonly IUserResolver _userResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatbotStorageService"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="userResolver">The user resolver instance.</param>
        public ChatbotStorageService(IChatDbContext dbContext, ILogger<ChatbotStorageService> logger, IUserResolver userResolver)
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
                chatbot.UpdatedBy = _userResolver.GetUserEmail();
                chatbot.UtcUpdatedOn = DateTime.UtcNow;
                chatbot.Version++;
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
    }
}
