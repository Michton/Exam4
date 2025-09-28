using Exam4.Models;
using Exam4.Services;

using Exam4.Models;
using Exam4.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Exam4.Pages;

public class ValidationModel : PageModel
{
    private readonly ICredentialsService _credentialsService;

    public bool IsSuccess { get; set; } = false;
    public string ErrorMessage { get; set; } = "";
    public Credentials? Credentials { get; set; }

    public ValidationModel(ICredentialsService credentialsService)
    {
        _credentialsService = credentialsService;
    }

    // This runs when someone clicks the email link: /Validation?token=xyz
    public IActionResult OnGet(string? token)
    {
        Console.WriteLine($"🔗 Validation page accessed with token: {token}");

        // Check if token is provided
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("❌ No token provided");
            IsSuccess = false;
            ErrorMessage = "No validation token provided. Please check your email link.";
            return Page();
        }

        // Find credentials by token
        var credentials = _credentialsService.GetCredentialsByToken(token);

        if (credentials == null)
        {
            Console.WriteLine($"❌ Token not found: {token}");
            IsSuccess = false;
            ErrorMessage = "Invalid or expired validation token. Please request a new validation email.";
            return Page();
        }

        Console.WriteLine($"✅ Token found for email: {credentials.Email}");

        // Check if already validated
        if (credentials.EmailStatus == EmailStatus.Validated)
        {
            Console.WriteLine("⚠️ Email already validated");
            IsSuccess = false;
            ErrorMessage = "This token has already be used for login. As for a new one to re-login.";
            Credentials = credentials;
            //// Still record this as a successful login attempt
            //credentials.RecordLoginAttempt(true);
            //_credentialsService.UpdateCredentials(credentials);
            return Page();
        }

        // Mark as validated AND record successful login
        Console.WriteLine("🎉 Marking email as validated and recording login");
        credentials.MarkAsValidated();
        credentials.RecordLoginAttempt(true); // Record successful login
        _credentialsService.UpdateCredentials(credentials);

        Console.WriteLine($"✅ Email validation and login completed for: {credentials.Email}");

        IsSuccess = true;
        Credentials = credentials;

        return Page();
    }
}