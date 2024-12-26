using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luval.Marin2.ChatAgent.Tests.Helpers
{
    public class CompletionServiceStub : IChatCompletionService
    {
        private List<StreamingChatMessageContent> _streaming;

        public void Initialize(string jsonWithResponse)
        {
            _streaming = JsonSerializer.Deserialize<List<StreamingChatMessageContent>>(jsonWithResponse) ?? new List<StreamingChatMessageContent>();
        }

        public IReadOnlyDictionary<string, object?> Attributes => throw new NotImplementedException();

        public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            if (_streaming == null) throw new InvalidOperationException("Class not initialize call the Initialize method for executing this method");
            if(!_streaming.Any()) throw new InvalidOperationException("Class was not initialized properly");

            return _streaming.ToAsyncEnumerable();
        }
    }
}
