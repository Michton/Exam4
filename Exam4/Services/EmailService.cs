using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

using Exam4.Models;

using Exam4.Models;

namespace Exam4.Services
{
    public interface IEmailService
    {
        Task<bool> SendValidationEmailAsync(Credentials credentials);
        string GenerateValidationLink(string token);
    }

    public class EmailService : IEmailService
    {
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly IConfiguration _configuration;
        private readonly string _senderEmail;
        private readonly string _baseUrl;

        public EmailService(IAmazonSimpleEmailService sesClient, IConfiguration configuration)
        {
            _sesClient = sesClient;
            _configuration = configuration;
            _senderEmail = _configuration["AWS:SES:SenderEmail"] ?? "noreply@yourdomain.com";
            _baseUrl = _configuration["Application:BaseUrl"] ?? "https://localhost:7000";
        }

        public async Task<bool> SendValidationEmailAsync(Credentials credentials)
        {
            try
            {
                Console.WriteLine($"🔧 DEBUG: Starting email send process...");
                Console.WriteLine($"🔧 DEBUG: Sender Email: {_senderEmail}");
                Console.WriteLine($"🔧 DEBUG: Recipient Email: {credentials.Email}");
                Console.WriteLine($"🔧 DEBUG: AWS Region: {_sesClient.Config.RegionEndpoint}");

                var validationLink = GenerateValidationLink(credentials.Token);
                Console.WriteLine($"🔧 DEBUG: Validation Link: {validationLink}");

                var emailBody = CreateEmailContent(validationLink);
                Console.WriteLine($"🔧 DEBUG: Email body created, length: {emailBody.Length} characters");

                var sendRequest = new SendEmailRequest
                {
                    Source = _senderEmail,
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { credentials.Email }
                    },
                    Message = new Message
                    {
                        Subject = new Content("Aspectiva Test Email - Please Validate"),
                        Body = new Body
                        {
                            Html = new Content(emailBody),
                            Text = new Content(CreateTextEmailContent(validationLink))
                        }
                    }
                };

                Console.WriteLine($"🔧 DEBUG: Sending request to AWS SES...");
                var response = await _sesClient.SendEmailAsync(sendRequest);
                Console.WriteLine($"🔧 DEBUG: AWS Response - MessageId: {response.MessageId}");

                // If we get here without exception, email was sent successfully
                return !string.IsNullOrEmpty(response.MessageId);
            }
            catch (Exception ex)
            {
                // Log the exception with full details
                Console.WriteLine($"💥 ERROR sending email: {ex.Message}");
                Console.WriteLine($"💥 ERROR Type: {ex.GetType().Name}");
                Console.WriteLine($"💥 ERROR Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"💥 INNER ERROR: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public string GenerateValidationLink(string token)
        {
            return $"{_baseUrl}/Validation?token={token}";
        }

        private string CreateEmailContent(string validationLink)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; }}
        .header {{ text-align: center; color: #333; margin-bottom: 30px; }}
        .content {{ line-height: 1.6; color: #555; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; color: #888; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to the Aspectiva Test Email</h1>
        </div>
        <div class='content'>
            <p>Hello,</p>
            <p>Welcome to the Aspectiva Test email system!</p>
            <p>Please press the following link to login:</p>
            <p style='text-align: center;'>
                <a href='{validationLink}' class='button'>Click Here to Login</a>
            </p>
            <p>Or copy and paste this link in your browser:</p>
            <p style='word-break: break-all; background: #f8f8f8; padding: 10px; border-radius: 5px;'>
                {validationLink}
            </p>
        </div>
        <div class='footer'>
            <p>This is an automated message from Aspectiva Test System.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string CreateTextEmailContent(string validationLink)
        {
            return $@"
Welcome to the Aspectiva Test Email

Hello,

Welcome to the Aspectiva Test email system!

Please press the following link to login:
{validationLink}

This is an automated message from Aspectiva Test System.
";
        }
    }
}