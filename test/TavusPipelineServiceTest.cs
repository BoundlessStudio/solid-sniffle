using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using TavusPrototype.Options;
using TavusPrototype.Services;

namespace TestTravus
{
    [TestClass]
    public sealed class TavusPipelineServiceTest
    {
        private TavusPipelineService? service;
        private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        };

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new TavusOptions()
            {
                ApiKey = Environment.GetEnvironmentVariable("TAVUS_API_KEY") ?? string.Empty
            };

            var client = new HttpClient();
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);

            var logger = new NullLogger<TavusPipelineService>();
            this.service = new TavusPipelineService(client, Options.Create(options), logger);
        }

        [TestMethod]
        public async Task TestBuildPersona()
        {
            Assert.IsNotNull(this.service);

            var name = "Interviewer";
            var instructions = "As an Interviewer, you are a skilled professional who conducts thoughtful and structured interviews. Your aim is to ask insightful questions, listen carefully, and assess responses objectively to identify the best candidates.";
            var context = "You have a track record of conducting interviews that put candidates at ease, draw out their strengths, and help organizations make excellent hiring decisions.";
            var replicaId = "rfe12d8b9597";
            var result = await this.service.BuildPersonaAsync(name, instructions, context, replicaId);
            Assert.IsNotNull(result);
            Console.WriteLine(JsonSerializer.Serialize(result, this.serializerOptions));
        }

        [TestMethod]
        public async Task TestStartConversation()
        {
            Assert.IsNotNull(this.service);

            var personaId = "p7883022a8a8";
            var title = $"Interviewer - {DateTimeOffset.UtcNow.ToFileTime()}";
            var converstion = await this.service.StartConversationAsync(personaId, title);
            Assert.IsNotNull(converstion);
            Console.WriteLine(JsonSerializer.Serialize(converstion, this.serializerOptions));
        }
    }

}
