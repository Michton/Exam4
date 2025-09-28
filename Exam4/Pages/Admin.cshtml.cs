using Exam4.Models;
using Exam4.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using System.ComponentModel.DataAnnotations;

namespace Exam4.Pages
{
    public class AdminModel : PageModel
    {
        private readonly ICredentialsService _credentialsService;

        [BindProperty]
        [Required(ErrorMessage = "Please enter the admin password.")]
        public string Password { get; set; } = string.Empty;

        public bool IsAuthenticated { get; set; } = false;
        public string ErrorMessage { get; set; } = "";
        public List<Credentials> AllCredentials { get; set; } = new List<Credentials>();
        public int TotalCredentials { get; set; } = 0;
        public int UniqueEmails { get; set; } = 0;
        public int ValidatedEmails { get; set; } = 0;
        public int SuccessfulLogins { get; set; } = 0;

        private const string ADMIN_PASSWORD = "password"; // The required password

        public AdminModel(ICredentialsService credentialsService)
        {
            _credentialsService = credentialsService;
        }

        // This runs when the page loads (GET request)
        public void OnGet()
        {
            Console.WriteLine("🔐 Admin page accessed");
            IsAuthenticated = false;
            ErrorMessage = "";
        }

        // This runs when the login form is submitted (POST request)
        public IActionResult OnPost()
        {
            Console.WriteLine("🔐 Admin login attempt");

            // Check if password is provided
            if (string.IsNullOrEmpty(Password))
            {
                Console.WriteLine("❌ No password provided");
                ErrorMessage = "Please enter the admin password.";
                IsAuthenticated = false;
                return Page();
            }

            // Validate password
            if (Password.Trim() != ADMIN_PASSWORD)
            {
                Console.WriteLine($"❌ Invalid password attempt: {Password}");
                ErrorMessage = "Invalid admin password. Access denied.";
                IsAuthenticated = false;
                return Page();
            }

            Console.WriteLine("✅ Admin password correct - loading credentials");

            // Password is correct - load and display credentials
            IsAuthenticated = true;
            ErrorMessage = "";
            LoadCredentialsData();

            return Page();
        }

        private void LoadCredentialsData()
        {
            Console.WriteLine("📊 Loading credentials data for admin view");

            // Get all credentials
            AllCredentials = _credentialsService.GetAllCredentials()
                .OrderByDescending(c => c.CreationDate)
                .ToList();

            // Calculate statistics
            TotalCredentials = _credentialsService.GetTotalCount();
            UniqueEmails = _credentialsService.GetUniqueEmailCount();
            ValidatedEmails = AllCredentials.Count(c => c.EmailStatus == EmailStatus.Validated);
            SuccessfulLogins = AllCredentials.Count(c => c.LoginSuccess);

            Console.WriteLine($"📊 Stats - Total: {TotalCredentials}, Unique: {UniqueEmails}, Validated: {ValidatedEmails}, Logins: {SuccessfulLogins}");
        }

        // Helper method to get status display with color coding
        public string GetStatusClass(EmailStatus status)
        {
            return status switch
            {
                EmailStatus.Created => "status-created",
                EmailStatus.SendSuccessfully => "status-sent",
                EmailStatus.SendWithError => "status-error",
                EmailStatus.Validated => "status-validated",
                _ => "status-unknown"
            };
        }

        // Helper method to format dates nicely
        public string FormatDate(DateTime? date)
        {
            if (date.HasValue)
            {
                return date.Value.ToString("MMM dd, yyyy HH:mm:ss");
            }
            return "N/A";
        }
    }
}