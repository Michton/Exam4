

using Exam4.Models;

namespace Exam4.Services
{
    public interface ICredentialsService
    {
        List<Credentials> GetAllCredentials();
        void AddCredentials(Credentials credentials);
        Credentials? GetCredentialsByToken(string token);
        Credentials? GetCredentialsByEmail(string email);
        void UpdateCredentials(Credentials credentials);
        int GetTotalCount();
    }

    public class CredentialsService : ICredentialsService
    {
        private static readonly List<Credentials> _credentials = new List<Credentials>();
        private static readonly object _lock = new object();

        public List<Credentials> GetAllCredentials()
        {
            lock (_lock)
            {
                return _credentials.ToList(); // Return a copy to avoid external modifications
            }
        }

        public void AddCredentials(Credentials credentials)
        {
            lock (_lock)
            {
                _credentials.Add(credentials);
            }
        }

        public Credentials? GetCredentialsByToken(string token)
        {
            lock (_lock)
            {
                return _credentials.FirstOrDefault(c => c.Token == token);
            }
        }

        public Credentials? GetCredentialsByEmail(string email)
        {
            lock (_lock)
            {
                return _credentials.FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            }
        }

        public void UpdateCredentials(Credentials credentials)
        {
            lock (_lock)
            {
                var existing = _credentials.FirstOrDefault(c => c.Token == credentials.Token);
                if (existing != null)
                {
                    var index = _credentials.IndexOf(existing);
                    _credentials[index] = credentials;
                }
            }
        }

        public int GetTotalCount()
        {
            lock (_lock)
            {
                return _credentials.Count;
            }
        }
    }
}