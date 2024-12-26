using Luval.Marin2.ChatAgent.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Luval.Marin2.ChatAgent.Infrastructure.Interfaces
{
    public interface IChatDbContext
    {
        DbSet<Chatbot> Chatbots { get; set; }
        DbSet<ChatMessage> ChatMessages { get; set; }
        DbSet<ChatSession> ChatSessions { get; set; }

        /// <summary>
        /// The DbSet representing the ChatMessageMedia table.
        /// </summary>
        DbSet<ChatMessageMedia> ChatMessageMedia { get; set; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}