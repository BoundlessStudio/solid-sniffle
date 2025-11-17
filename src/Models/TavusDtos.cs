using System.Text.Json.Serialization;

namespace TavusPrototype.Models;

public record PersonaRequest(
    [property: JsonPropertyName("persona_name")] string PersonaName,
    [property: JsonPropertyName("system_prompt")] string SystemPrompt,
    [property: JsonPropertyName("pipeline_mode")] string PipelineMode,
    [property: JsonPropertyName("context")] string Context,
    [property: JsonPropertyName("default_replica_id")] string DefaultReplicaId,
    [property: JsonPropertyName("layers")] PersonaLayers Layers);

public record PersonaLayers(
    [property: JsonPropertyName("perception")] PerceptionLayer Perception,
    [property: JsonPropertyName("stt")] SttLayer SpeechToText);

public record PerceptionLayer([property: JsonPropertyName("perception_model")] string PerceptionModel);

public record SttLayer([property: JsonPropertyName("smart_turn_detection")] bool SmartTurnDetection);

public record PersonaResponse([property: JsonPropertyName("persona_id")] string PersonaId);

public record CreateConversationRequest(
    [property: JsonPropertyName("persona_id")] string PersonaId,
    [property: JsonPropertyName("conversation_name")] string ConversationName);

public record TavusConversationResponse(
    [property: JsonPropertyName("conversation_id")] string ConversationId,
    [property: JsonPropertyName("conversation_name")] string ConversationName,
    [property: JsonPropertyName("conversation_url")] string ConversationUrl,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("callback_url")] string? CallbackUrl,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);

public record ConversationLaunchResponse(string ConversationId, string ConversationName, string ConversationUrl, string Status,
    DateTimeOffset CreatedAt);

public record PersonaSetupResponse(string PersonaId, string PersonaName);

public record StartConversationRequest(string PersonaId, string? ConversationName);
