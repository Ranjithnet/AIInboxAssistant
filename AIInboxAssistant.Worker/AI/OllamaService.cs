using AIInboxAssistant.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIInboxAssistant.Worker.AI
{
    public class OllamaService
    {
        private readonly HttpClient _httpClient;

        public OllamaService()
        {
            _httpClient = new HttpClient();

            _httpClient.BaseAddress =
                new Uri("http://localhost:11434");
        }

        public async Task<EmailClassificationResult>
            ClassifyEmailAsync(
                string subject,
                string sender)
        {
            var prompt = $@"
You are an enterprise email classifier.

Classify the email accurately.

Rules:
- Security alerts -> Security
- Instagram notifications -> Social
- YouTube updates -> Promotions
- Banking emails -> Banking
- Shopping/order emails -> Shopping

Return ONLY valid JSON.

Format:
{{
  ""Category"": ""Security"",
  ""Priority"": ""High""
}}
";

            var requestBody = new
            {
                model = "phi3",
                prompt = prompt,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                "/api/generate",
                content);

            response.EnsureSuccessStatusCode();

            var responseJson =
                await response.Content.ReadAsStringAsync();

            using var document =
                JsonDocument.Parse(responseJson);

            var aiResponse =
                document.RootElement
                    .GetProperty("response")
                    .GetString();

            Console.WriteLine(aiResponse);

            try
            {
                return JsonSerializer.Deserialize
                    <EmailClassificationResult>(aiResponse!)
                       ?? new EmailClassificationResult();
            }
            catch
            {
                return new EmailClassificationResult();
            }
        }
    }
}
