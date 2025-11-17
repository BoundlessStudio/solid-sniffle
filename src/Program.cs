using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
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

app.MapPost("/personas", async Task<IResult> (TavusPipelineService service, CancellationToken ct) =>
    {
        var name = "Interviewer";
        var instructions = "As an Interviewer, you are a skilled professional who conducts thoughtful and structured interviews. Your aim is to ask insightful questions, listen carefully, and assess responses objectively to identify the best candidates.";
        var context = "You have a track record of conducting interviews that put candidates at ease, draw out their strengths, and help organizations make excellent hiring decisions.";
        var replica_id = "r9fa0878977a";
        var persona = await service.BuildPersonaAsync(name, instructions, context, replica_id,  ct);
        return Results.Ok(persona);
    })
    .WithName("CreatePersona")
    .WithSummary("Creates a Tavus persona and returns its identifying information.")
    .Produces<PersonaSetupResponse>();

app.MapPost("/conversations", async Task<IResult> (CreateConversationRequest request, TavusPipelineService service, CancellationToken ct) =>
    {
        var result = await service.StartConversationAsync(request.PersonaId, request.ConversationName, ct);
        return Results.Ok(result);
    })
    .WithName("CreateConversation")
    .WithSummary("Starts a conversation for an existing persona and returns the conversation URL to join.")
    .Produces<ConversationLaunchResponse>();

app.MapGet("/", () => Results.Redirect("/swagger", permanent: false)).ExcludeFromDescription();

app.Run();
