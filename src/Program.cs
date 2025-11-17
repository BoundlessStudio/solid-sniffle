using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using TavusPrototype.Models;
using TavusPrototype.Options;
using TavusPrototype.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<TavusOptions>(builder.Configuration.GetSection("Tavus"));

builder.Services.AddHttpClient<TavusPipelineService>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<TavusOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.ApiKey))
    {
        throw new InvalidOperationException("Configure Tavus:ApiKey before calling the pipeline.");
    }

    var baseUrl = string.IsNullOrWhiteSpace(options.BaseUrl) ? "https://tavusapi.com/v2/" : options.BaseUrl.TrimEnd('/') + "/";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/tavus/personas", async Task<IResult> (TavusPipelineService service, CancellationToken cancellationToken) =>
    {
        var persona = await service.BuildPersonaAsync(cancellationToken);
        return Results.Ok(persona);
    })
    .WithName("CreateTavusPersona")
    .WithSummary("Creates a Tavus persona and returns its identifying information.")
    .Produces<PersonaSetupResponse>();

app.MapPost("/tavus/conversations", async Task<IResult> (StartConversationRequest request, TavusPipelineService service, CancellationToken cancellationToken) =>
    {
        var result = await service.StartConversationAsync(request.PersonaId, request.ConversationName, cancellationToken);
        return Results.Ok(result);
    })
    .WithName("CreateTavusConversation")
    .WithSummary("Starts a conversation for an existing persona and returns the conversation URL to join.")
    .Produces<ConversationLaunchResponse>();

app.MapGet("/", () => Results.Redirect("/swagger", permanent: false));

app.Run();
