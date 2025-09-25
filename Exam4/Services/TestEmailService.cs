using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace Exam4.Services
{
    public interface ITestEmailService
    {
        Task<bool> SendSimpleTestEmailAsync(string recipientEmail);
    }

    public class TestEmailService : ITestEmailService
    {
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly IConfiguration _configuration;
        private readonly string _senderEmail;

        public TestEmailService(IAmazonSimpleEmailService sesClient, IConfiguration configuration)
        {
            _sesClient = sesClient;
            _configuration = configuration;
            _senderEmail = _configuration["AWS:SES:SenderEmail"] ?? "noreply@yourdomain.com";
        }

        public async Task<bool> SendSimpleTestEmailAsync(string recipientEmail)
        {
            try
            {
                Console.WriteLine($"🧪 TEST: Starting simple email send...");
                Console.WriteLine($"🧪 TEST: From: {_senderEmail}");
                Console.WriteLine($"🧪 TEST: To: {recipientEmail}");
                Console.WriteLine($"🧪 TEST: Region: {_sesClient.Config.RegionEndpoint}");

                var simpleMessage = new SendEmailRequest
                {
                    Source = _senderEmail,
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { recipientEmail }
                    },
                    Message = new Message
                    {
                        Subject = new Content("Simple Test Email"),
                        Body = new Body
                        {
                            Text = new Content("Hello! This is a simple test email from AWS SES. If you receive this, everything is working correctly!")
                        }
                    }
                };

                Console.WriteLine($"🧪 TEST: Calling AWS SES...");
                var response = await _sesClient.SendEmailAsync(simpleMessage);

                Console.WriteLine($"🧪 TEST: SUCCESS! MessageId: {response.MessageId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🧪 TEST: ERROR - {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"🧪 TEST: INNER ERROR: {ex.InnerException.Message}");
                }
                Console.WriteLine($"🧪 TEST: Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}