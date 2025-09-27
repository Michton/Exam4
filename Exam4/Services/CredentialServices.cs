
using Exam4.Models;

namespace Exam4.Services;

public interface ICredentialsService
{
    List<Credentials> GetAllCredentials();
    void AddCredentials(Credentials credentials);
    Credentials? GetCredentialsByToken(string token);
    List<Credentials> GetCredentialsByEmail(string email); // NOW RETURNS LIST
    void UpdateCredentials(Credentials credentials);
    int GetTotalCount();
    int GetUniqueEmailCount(); // NEW: Count unique emails
    List<Credentials> GetLoginHistory(string email); // NEW: Get login history for email
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

    public List<Credentials> GetCredentialsByEmail(string email)
    {
        lock (_lock)
        {
            return _credentials.Where(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(c => c.CreationDate)
                .ToList();
        }
    }

    public Credentials? GetCredentialsByToken(string token)
    {
        lock (_lock)
        {
            return _credentials.FirstOrDefault(c => c.Token == token);
        }
    }

    public int GetUniqueEmailCount()
    {
        lock (_lock)
        {
            return _credentials.Select(c => c.Email.ToLower()).Distinct().Count();
        }
    }

    public List<Credentials> GetLoginHistory(string email)
    {
        lock (_lock)
        {
            return _credentials.Where(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase)
                                           && c.LoginDate.HasValue)
                .OrderByDescending(c => c.LoginDate)
                .ToList();
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