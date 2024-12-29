﻿using Luval.Marin2.ChatAgent.Core.Entities;
using Luval.Marin2.ChatAgent.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Luval.Marin2.ChatAgent.Infrastructure.Data
{
    /// <summary>
    /// The database context for managing Chat, ChatMessage, and ChatAgent entities.
    /// </summary>
    public class ChatDbContext : DbContext, IChatDbContext
    {
        /// <summary>
        /// The DbSet representing the Chat table.
        /// </summary>
        public DbSet<ChatSession> ChatSessions { get; set; }

        /// <summary>
        /// The DbSet representing the ChatMessage table.
        /// </summary>
        public DbSet<ChatMessage> ChatMessages { get; set; }

        /// <summary>
        /// The DbSet representing the ChatAgent table.
        /// </summary>
        public DbSet<GenAIBot> Chatbots { get; set; }

        /// <summary>
        /// The DbSet representing the ChatMessageMedia table.
        /// </summary>
        public DbSet<ChatMessageMedia> ChatMessageMedia { get; set; }

        /// <summary>
        /// Configures the database context options.
        /// </summary>
        /// <param name="options">The options to be configured.</param>
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configures the model creation process to define relationships and constraints.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure entity relationships and properties.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Configure ChatMessage relationship with Chatbot
            modelBuilder.Entity<ChatSession>()
               .HasOne(cm => cm.Chatbot)
               .WithMany(c => c.ChatSessions)
               .OnDelete(DeleteBehavior.Cascade);

            // Configure ChatMessage relationship with Chat
            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.ChatSession)
                .WithMany(c => c.ChatMessages)
                .HasForeignKey(cm => cm.ChatSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ChatMessageMedia relationship with ChatMessage
            modelBuilder.Entity<ChatMessageMedia>()
                .HasOne(cmm => cmm.ChatMessage)
                .WithMany(cm => cm.Media)
                .HasForeignKey(cmm => cmm.ChatMessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add additional configurations as needed
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
        }

        public ChatDbContext(DbContextOptions options) : base(options)
        {
        }
    }

}
