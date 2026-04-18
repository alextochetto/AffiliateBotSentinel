using ApiBotSentinel.Services;
using Azure.Data.Tables;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

string connectionString = builder.Configuration["AzureTableStorage:ConnectionString"]
    ?? throw new InvalidOperationException("AzureTableStorage:ConnectionString is not configured.");

TableClient tableClient = new TableClient(connectionString, "BotTrack");
builder.Services.AddSingleton<TableClient>(tableClient);
builder.Services.AddSingleton<ITrackBotService, TrackBotService>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

//app.UseHttpsRedirection();
app.MapControllers();

await tableClient.CreateIfNotExistsAsync();

app.Run();
