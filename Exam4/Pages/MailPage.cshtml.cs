
using Exam4.Models;
using Exam4.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Exam4.Pages
{
    public class MailPageModel : PageModel
    {
        private readonly ICredentialsService _credentialsService;
        private readonly IEmailService _emailService;

        [BindProperty]
        [Required(ErrorMessage = "Please enter an email address.")]
        public string Email { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = false;
        public int TotalCredentials { get; set; } = 0;

        // Constructor - injects the services we need
        public MailPageModel(ICredentialsService credentialsService, IEmailService emailService)
        {
            _credentialsService = credentialsService;
            _emailService = emailService;
        }

        // This runs when the page loads (GET request)
        public void OnGet()
        {
            Message = "";
            IsSuccess = false;
            TotalCredentials = _credentialsService.GetTotalCount();
        }

        // THIS METHOD RUNS WHEN THE "SEND" BUTTON IS CLICKED (POST request)
        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("🔘 SEND BUTTON WAS CLICKED! Starting email process...");

            // Get current count
            TotalCredentials = _credentialsService.GetTotalCount();

            // Step 1: Check if email field is empty
            if (string.IsNullOrEmpty(Email))
            {
                Console.WriteLine("❌ Email field is empty");
                Message = "Please enter an email address.";
                IsSuccess = false;
                return Page();
            }

            Console.WriteLine($"📧 Email entered: {Email}");

            // Step 2: Validate email format using regex
            if (!IsValidEmailFormat(Email.Trim()))
            {
                Console.WriteLine("❌ Email format is invalid");
                Message = "Please enter a valid email address.";
                IsSuccess = false;
                return Page();
            }

            Console.WriteLine("✅ Email format is valid");

            // Step 3: Check if this email already exists in our system
            var existingCredentials = _credentialsService.GetCredentialsByEmail(Email.Trim());
            if (existingCredentials != null)
            {
                Console.WriteLine("⚠️ Email already exists in system");
                Message = $"Email already registered! Token: {existingCredentials.Token}";
                IsSuccess = false;
                return Page();
            }

            // Step 4: Create new credentials object
            Console.WriteLine("🆕 Creating new credentials...");
            var newCredentials = new Credentials(Email.Trim());
            Console.WriteLine($"🎫 Generated Token: {newCredentials.Token}");

            // Step 5: Add to our credentials list
            _credentialsService.AddCredentials(newCredentials);
            Console.WriteLine("💾 Credentials saved to list");

            // Step 6: **SEND THE EMAIL** - This is the main action!
            Console.WriteLine("📤 SENDING EMAIL VIA AWS SES...");

            try
            {
                bool emailWasSent = await _emailService.SendValidationEmailAsync(newCredentials);

                if (emailWasSent)
                {
                    // EMAIL SENT SUCCESSFULLY! 🎉
                    Console.WriteLine("✅ EMAIL SENT SUCCESSFULLY!");

                    newCredentials.MarkAsSentSuccessfully();
                    _credentialsService.UpdateCredentials(newCredentials);

                    Message = $"🎉 SUCCESS! Validation email sent to {Email}! Check your inbox. Token: {newCredentials.Token}";
                    IsSuccess = true;
                }
                else
                {
                    // EMAIL SENDING FAILED 😞
                    Console.WriteLine("❌ EMAIL SENDING FAILED!");

                    newCredentials.MarkAsSentWithError();
                    _credentialsService.UpdateCredentials(newCredentials);

                    Message = $"❌ Failed to send email to {Email}. Please check AWS SES configuration. Token: {newCredentials.Token}";
                    IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                // EXCEPTION OCCURRED DURING EMAIL SENDING
                Console.WriteLine($"💥 EXCEPTION during email sending: {ex.Message}");

                newCredentials.MarkAsSentWithError();
                _credentialsService.UpdateCredentials(newCredentials);

                Message = $"💥 Error occurred: {ex.Message}. Token: {newCredentials.Token}";
                IsSuccess = false;
            }

            // Step 7: Update the total count
            TotalCredentials = _credentialsService.GetTotalCount();
            Console.WriteLine($"📊 Total credentials now: {TotalCredentials}");

            return Page();
        }

        // Helper method to validate email format
        private bool IsValidEmailFormat(string email)
        {
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailPattern);
        }
    }
}