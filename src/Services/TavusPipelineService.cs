using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TavusPrototype.Models;
using TavusPrototype.Options;

namespace TavusPrototype.Services;

public class TavusPipelineService
{
    private readonly HttpClient _httpClient;
    private readonly TavusOptions _options;
    private readonly ILogger<TavusPipelineService> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public TavusPipelineService(HttpClient httpClient, IOptions<TavusOptions> options, ILogger<TavusPipelineService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PersonaSetupResponse> BuildPersonaAsync(CancellationToken cancellationToken)
    {
        var personaRequest = BuildPersonaRequest();
        var personaResponse = await PostAsync<PersonaRequest, PersonaResponse>("personas", personaRequest, cancellationToken);

        return new PersonaSetupResponse(personaResponse.PersonaId, personaRequest.PersonaName);
    }

    public async Task<ConversationLaunchResponse> StartConversationAsync(string personaId, string? conversationName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(personaId))
        {
            throw new ArgumentException("PersonaId is required to start a conversation.", nameof(personaId));
        }

        var conversationRequest = new CreateConversationRequest(personaId, string.IsNullOrWhiteSpace(conversationName) ? "Interview User" : conversationName!);
        var conversation = await PostAsync<CreateConversationRequest, TavusConversationResponse>("conversations", conversationRequest, cancellationToken);

        return new ConversationLaunchResponse(conversation.ConversationId, conversation.ConversationName, conversation.ConversationUrl, conversation.Status, conversation.CreatedAt);
    }

    private PersonaRequest BuildPersonaRequest()
    {
        return new PersonaRequest(
            _options.DefaultPersonaName,
            "As an Interviewer, you are a skilled professional who conducts thoughtful and structured interviews. Your aim is to ask insightful questions, listen carefully, and assess responses objectively to identify the best candidates.",
            "full",
            "You have a track record of conducting interviews that put candidates at ease, draw out their strengths, and help organizations make excellent hiring decisions.",
            _options.DefaultReplicaId,
            new PersonaLayers(new PerceptionLayer("raven-0"), new SttLayer(true))
        );
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string resource, TRequest payload, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(resource, payload, SerializerOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Tavus API call to {Resource} failed with {Status}. Payload: {Payload}. Response: {Response}", resource, response.StatusCode, payload, error);
            throw new InvalidOperationException($"Tavus API call to '{resource}' failed with status {response.StatusCode}.");
        }

        var result = await response.Content.ReadFromJsonAsync<TResponse>(SerializerOptions, cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException($"Tavus API call to '{resource}' returned no body.");
        }

        return result;
    }
}
