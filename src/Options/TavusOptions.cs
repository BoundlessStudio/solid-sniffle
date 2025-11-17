namespace TavusPrototype.Options;

public class TavusOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://tavusapi.com/v2";
    public string DefaultReplicaId { get; set; } = "rfe12d8b9597";
    public string DefaultPersonaName { get; set; } = "Interviewer";
}
