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

    public async Task<PersonaSetupResponse> BuildPersonaAsync(string name, string instructions, string context, string replica_id, CancellationToken cancellationToken = default)
    {
        var request = new PersonaRequest(name, instructions, "full", context, replica_id, new PersonaLayers(new PerceptionLayer("raven-0"), new SttLayer(true)));
        var response = await PostAsync<PersonaRequest, PersonaResponse>("personas", request, cancellationToken);

        return new PersonaSetupResponse(response.PersonaId, request.PersonaName);
    }

    public async Task<ConversationLaunchResponse> StartConversationAsync(string personaId, string? conversationName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(personaId))
        {
            throw new ArgumentException("PersonaId is required to start a conversation.", nameof(personaId));
        }

        var conversationRequest = new CreateConversationRequest(personaId, string.IsNullOrWhiteSpace(conversationName) ? "Interview User" : conversationName!);
        var conversation = await PostAsync<CreateConversationRequest, TavusConversationResponse>("conversations", conversationRequest, cancellationToken);

        return new ConversationLaunchResponse(conversation.ConversationId, conversation.ConversationName, conversation.ConversationUrl, conversation.Status, conversation.CreatedAt);
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
