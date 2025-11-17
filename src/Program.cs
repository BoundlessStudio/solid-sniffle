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

app.MapPost("/tavus/conversations", async Task<IResult> (StartConversationRequest request, TavusPipelineService service, CancellationToken cancellationToken) =>
    {
        var result = await service.StartConversationAsync(request.ConversationName, cancellationToken);
        return Results.Ok(result);
    })
    .WithName("CreateTavusConversation")
    .WithSummary("Creates a persona using the full Tavus pipeline and returns the conversation URL to join.")
    .Produces<ConversationLaunchResponse>();

app.MapGet("/", () => Results.Redirect("/swagger", permanent: false));

app.Run();
