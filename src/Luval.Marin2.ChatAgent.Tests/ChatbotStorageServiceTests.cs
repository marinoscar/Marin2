using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Infrastructure.Logging;
using Luval.Marin2.ChatAgent.Core.Entities;
using Luval.Marin2.ChatAgent.Core.Services;
using Luval.Marin2.ChatAgent.Infrastructure.Interfaces;
using Luval.Marin2.ChatAgent.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Luval.Marin2.ChatAgent.Tests
{
    public class ChatbotStorageServiceTests
    {

        private ChatbotStorageService CreateService(Action<MemoryDataContext>? workOnContext)
        {
            var context = new MemoryDataContext();
            context.Initialize();
            var userResolverMock = new Mock<IUserResolver>();
            userResolverMock.Setup(x => x.GetUserEmail()).Returns("user@email.com");
            var service = new ChatbotStorageService(context, new NullLogger<ChatbotStorageService>(), userResolverMock.Object);

            workOnContext?.Invoke(context);

            return service;
        }

        [Fact]
        public async Task CreateChatbotAsync_ShouldCreateChatbot()
        {
            // Arrange
            var service = CreateService(null);
            var chatbot = new Chatbot
            {
                Name = "Test Chatbot",
                AccountId = 1
            };

            Action<MemoryDataContext> contextSetup = (c) =>
            {
                c.Chatbots.Add(chatbot);
                c.SaveChanges();
            };
            
            // Act
            var result = await service.CreateChatbotAsync(chatbot);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Chatbot", result.Name);
            Assert.NotNull(result.CreatedBy);
            Assert.NotNull(result.UpdatedBy);
            Assert.True(result.Id > 0);
            Assert.True(result.UtcCreatedOn > DateTime.UtcNow.AddHours(-1));
        }
        [Fact]
        public async Task CreateChatbotAsync_ShouldThrowArgumentNullException_WhenChatbotIsNull()
        {
            // Arrange
            var service = CreateService(null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateChatbotAsync(null));
        }
        [Fact]
        public async Task GetChatbotAsync_ShouldReturnChatbot_WhenChatbotExists()
        {
            var chatbotId = 0ul;
            // Arrange
            var expectedChatbot = new Chatbot
            {
                Name = "Test Chatbot",
                AccountId = 1
            };

            var service = CreateService(context =>
            {
                context.Chatbots.Add(expectedChatbot);
                context.SaveChanges();
                chatbotId = context.Chatbots.First().Id;
                var chatSession = new ChatSession
                {
                    ChatbotId = chatbotId,
                    Title = "Test Chat Session",
                    Version = 1
                };
                context.ChatSessions.Add(chatSession);
                context.SaveChanges();
                var message = new ChatMessage
                {
                    ChatSessionId = chatSession.Id,
                    UserMessage = "Test Message",
                    AgentResponse = "Test Response",
                    Version = 1
                };
                context.ChatMessages.Add(message);
                context.SaveChanges();
                var media = new ChatMessageMedia()
                {
                    ChatMessageId = message.Id,
                    ProviderName = "Test Provider",
                    MediaUrl = "http://test.com",
                    Version = 1
                };
                context.ChatMessageMedia.Add(media);
                context.SaveChanges();
            });

            // Act
            var result = await service.GetChatbotAsync(chatbotId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedChatbot.Name, result.Name);
            Assert.Equal(expectedChatbot.AccountId, result.AccountId);
            Assert.NotEmpty(result.ChatSessions);
        }
        [Fact]
        public async Task UpdateChatbotAsync_ShouldUpdateChatbot()
        {
            // Arrange
            var chatbotId = 0ul;
            var service = CreateService(context =>
            {
                var chatbot = new Chatbot
                {
                    Name = "Old Chatbot",
                    AccountId = 1
                };
                context.Chatbots.Add(chatbot);
                context.SaveChanges();
                chatbotId = chatbot.Id;
            });

            var updatedChatbot = new Chatbot
            {
                Id = chatbotId,
                Name = "Updated Chatbot",
                AccountId = 1
            };

            // Act
            var result = await service.UpdateChatbotAsync(updatedChatbot);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Chatbot", result.Name);
            Assert.Equal(1ul, result.AccountId);
            Assert.NotNull(result.UpdatedBy);
            Assert.True(result.UtcUpdatedOn > DateTime.UtcNow.AddMinutes(-10));
        }

        [Fact]
        public async Task UpdateChatbotAsync_ShouldThrowArgumentNullException_WhenChatbotIsNull()
        {
            // Arrange
            var service = CreateService(null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateChatbotAsync(null));
        }

        [Fact]
        public async Task UpdateChatbotAsync_ShouldThrowInvalidOperationException_WhenChatbotDoesNotExist()
        {
            // Arrange
            var service = CreateService(null);
            var nonExistentChatbot = new Chatbot
            {
                Id = 999,
                Name = "Non-existent Chatbot",
                AccountId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => service.UpdateChatbotAsync(nonExistentChatbot));
        }








    }
}
