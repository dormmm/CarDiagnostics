using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarDiagnostics.Services
{
    public class FollowUpQuestionStore
    {
        private static readonly Dictionary<string, List<string>> _store = new();

        public Task SaveQuestionsAsync(string key, List<string> questions)
        {
            _store[key] = questions;
            return Task.CompletedTask;
        }

        public Task<List<string>> GetQuestionsAsync(string key)
        {
            _store.TryGetValue(key, out var questions);
            return Task.FromResult(questions ?? new List<string>());
        }
    }
}
