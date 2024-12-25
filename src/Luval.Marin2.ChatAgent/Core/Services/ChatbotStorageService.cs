using Luval.Marin2.ChatAgent.Core.Entities;
using Luval.Marin2.ChatAgent.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luval.Marin2.ChatAgent.Core.Services
{
    public class ChatbotStorageService
    {
        private readonly IChatDbContext _dbContext;
        private readonly ILogger<ChatbotStorageService> _logger;

        
        public Task<IQueryable<Chatbot>> GetChatbotsByAccountIdAsync(ulong accountId, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => _dbContext.Chatbots.Where(c => c.AccountId == accountId), cancellationToken);
        }

        public async Task<Chatbot> GetChatbotByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Chatbots
                .Include(c => c.Chats)
                .SingleAsync(c => c.Name == name, cancellationToken);
        }

        public Task<IQueryable<ChatMessage>> GetChatMessagesByChatIdAsync(ulong chatId, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => _dbContext.ChatMessages
                              .Include(m => m.Chat)
                              .Where(i => i.ChatId == chatId), cancellationToken);
        }

    }
}
