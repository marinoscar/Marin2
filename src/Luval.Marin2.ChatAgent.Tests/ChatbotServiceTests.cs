using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Infrastructure.Logging;
using Luval.Marin2.ChatAgent.Core.Entities;
using Luval.Marin2.ChatAgent.Core.Services;
using Luval.Marin2.ChatAgent.Infrastructure.Data;
using Luval.Marin2.ChatAgent.Infrastructure.Interfaces;
using Luval.Marin2.ChatAgent.Tests.Helpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.Marin2.ChatAgent.Tests
{
    public class ChatbotServiceTests
    {

        private ChatbotService CreateService(Action<MemoryDataContext>? workOnContext, Action<CompletionServiceStub>? workOnCompletion)
        {
            var userResolverMock = new Mock<IUserResolver>();
            userResolverMock.Setup(x => x.GetUserEmail()).Returns("user@email.com");
            var mediaService = new MediaServiceStub();

            var context = new MemoryDataContext();
            var completionService = new CompletionServiceStub();
            var storageService = new ChatbotStorageService(context, new NullLogger<ChatbotStorageService>(), userResolverMock.Object);
            var service = new ChatbotService(completionService, storageService, mediaService, new NullLogger<ChatbotService>());

            context.Initialize();
            completionService.Initialize(GetStreamingString());
            workOnContext?.Invoke(context);
            workOnCompletion?.Invoke(completionService);

            return service;
        }


        [Fact]
        public async Task SubmitMessageToNewSession_ShouldCreateChatSessionAndReturnChatMessage()
        {
            // Arrange
            ulong chatbotId = 1;
            string message = "Hello, world!";
            string sessionTitle = "Test Session";
            IEnumerable<UploadFile>? files = null;

            var service = CreateService((c) => {
                var cb = new Chatbot()
                {
                    AccountId = 1,
                    Name = "Test Chatbot"
                };
                c!.Chatbots.Add(cb);
                c!.SaveChanges();
                chatbotId = cb.Id;
            }, null);

            // Act
            var result = await service.SubmitMessageToNewSession(chatbotId, message, sessionTitle, files);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(message, result.UserMessage);
            Assert.NotNull(result.AgentResponse);
            Assert.NotNull(result.ChatSession);
            Assert.Equal(chatbotId, result.ChatSession.ChatbotId);
            Assert.True(result.OutputTokens > 0);
        }

        private string GetStreamingString()
        {
            return @"
[
  {
    ""Content"": """",
    ""Role"": {
      ""Label"": ""Assistant""
    },
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": null
    }
  },
  {
    ""Content"": ""Why"",
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": null
    }
  },
  {
    ""Content"": "" don\u0027t"",
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": null
    }
  },
  {
    ""Content"": "" skeleton"",
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": null
    }
  },
  {
    ""Content"": ""s"",
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": null
    }
  },
  {
    ""Content"": "" fight"",
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": null
    }
  },
  {
    ""Content"": "" each"",
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": null
    }
  },
  {
    ""Content"": "" other"",
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": null
    }
  },
  {
    ""Content"": ""?\n\n"",
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": null
    }
  },
  {
    ""Content"": ""They"",
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": null
    }
  },
  {
    ""Content"": "" don\u0027t"",
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": null
    }
  },
  {
    ""Content"": "" have"",
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": null
    }
  },
  {
    ""Content"": "" the"",
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": null
    }
  },
  {
    ""Content"": "" guts"",
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": null
    }
  },
  {
    ""Content"": ""!"",
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": null
    }
  },
  {
    ""Content"": null,
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": null,
      ""FinishReason"": ""Stop""
    }
  },
  {
    ""Content"": null,
    ""Role"": null,
    ""ChoiceIndex"": 0,
    ""ModelId"": ""gpt-4o"",
    ""Metadata"": {
      ""CompletionId"": ""chatcmpl-AiYM3R7A0KoiwwFxhhekRU5RGww78"",
      ""CreatedAt"": ""2024-12-26T02:57:35+00:00"",
      ""SystemFingerprint"": ""fp_5f20662549"",
      ""RefusalUpdate"": null,
      ""Usage"": {
        ""OutputTokenCount"": 14,
        ""InputTokenCount"": 26,
        ""TotalTokenCount"": 40,
        ""OutputTokenDetails"": {
          ""ReasoningTokenCount"": 0,
          ""AudioTokenCount"": 0
        },
        ""InputTokenDetails"": {
          ""AudioTokenCount"": 0,
          ""CachedTokenCount"": 0
        }
      },
      ""FinishReason"": null
    }
  }
]
";
        }

    }
}
