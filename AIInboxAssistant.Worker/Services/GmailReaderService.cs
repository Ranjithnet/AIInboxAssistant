using AIInboxAssistant.Core.DTOs;
using AIInboxAssistant.Core.Entities;
using AIInboxAssistant.Infrastructure.Data;
using AIInboxAssistant.Worker.AI;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace AIInboxAssistant.Worker.Services
{

    public class GmailReaderService
    {
        private static readonly string[] Scopes =
    {
        GmailService.Scope.GmailReadonly
    };

        private const string ApplicationName = "AIInboxAssistant";

        public async Task<GmailService> GetGmailServiceAsync()
        {
            using var stream = new FileStream(
                "credentials.json",
                FileMode.Open,
                FileAccess.Read);

            string tokenPath = "token.json";

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(tokenPath, true));

            return new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
        }

        public async Task ReadEmailsAsync(AppDbContext dbContext)
        {
            var service = await GetGmailServiceAsync();

            var ollamaService = new OllamaService();

            var request = service.Users.Messages.List("me");

            request.MaxResults = 10;

            var response = await request.ExecuteAsync();

            if (response.Messages == null)
            {
                Console.WriteLine("No emails found.");
                return;
            }

            foreach (var message in response.Messages)
            {
                bool exists = dbContext.Emails
                    .Any(x => x.GmailId == message.Id);

                if (exists)
                {
                    Console.WriteLine($"Skipping duplicate: {message.Id}");
                    continue;
                }

                var emailData = await service.Users.Messages
                    .Get("me", message.Id)
                    .ExecuteAsync();

                string subject = "";
                string sender = "";
                string body = "";

                foreach (var header in emailData.Payload.Headers)
                {
                    if (header.Name == "Subject")
                        subject = header.Value;

                    if (header.Name == "From")
                        sender = header.Value;
                }

                // Extract email body
                if (emailData.Payload.Body != null &&
                    !string.IsNullOrEmpty(emailData.Payload.Body.Data))
                {
                    body = DecodeBase64String(
                        emailData.Payload.Body.Data);
                }

                // AI Classification
                EmailClassificationResult aiResult =
                    await ollamaService.ClassifyEmailAsync(
                        subject,
                        sender);

                Console.WriteLine("--------------------------------");
                Console.WriteLine($"Subject: {subject}");
                Console.WriteLine($"From: {sender}");
                Console.WriteLine(
                    $"AI => Category: {aiResult.Category}, Priority: {aiResult.Priority}");

                var emailEntity = new EmailMessage
                {
                    GmailId = message.Id,

                    Subject = subject,

                    Sender = sender,

                    Body = body,

                    ReceivedDate = DateTime.Now,

                    Category = aiResult.Category,

                    Priority = aiResult.Priority,

                    IsProcessed = true,

                    IsRead = true
                };

                dbContext.Emails.Add(emailEntity);
            }

            await dbContext.SaveChangesAsync();

            Console.WriteLine("Emails saved to database.");
        }

        private string DecodeBase64String(string input)
        {
            string incoming = input
                .Replace("-", "+")
                .Replace("_", "/");

            switch (incoming.Length % 4)
            {
                case 2:
                    incoming += "==";
                    break;

                case 3:
                    incoming += "=";
                    break;
            }

            byte[] bytes = Convert.FromBase64String(incoming);

            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}
