using System.ComponentModel.DataAnnotations;

namespace Exam4.Models
{
    public class Credentials
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public DateTime CreationDate { get; set; } = DateTime.Now;

        public string Token { get; set; } = string.Empty;

        public DateTime? ValidationDate { get; set; } = null;

        public EmailStatus EmailStatus { get; set; } = EmailStatus.Created;

        // Constructor
        public Credentials()
        {
            // Generate a unique token when creating credentials
            Token = Guid.NewGuid().ToString();
        }

        // Constructor with email
        public Credentials(string email) : this()
        {
            Email = email;
        }

        // Method to mark as validated
        public void MarkAsValidated()
        {
            EmailStatus = EmailStatus.Validated;
            ValidationDate = DateTime.Now;
        }

        // Method to mark as sent successfully
        public void MarkAsSentSuccessfully()
        {
            EmailStatus = EmailStatus.SendSuccessfully;
        }

        // Method to mark as sent with error
        public void MarkAsSentWithError()
        {
            EmailStatus = EmailStatus.SendWithError;
        }
    }
}