using SimpleBettingExchange.Markets;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleansClient(static builder =>
{
    builder.UseLocalhostClustering();
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();
//builder.Services.AddScoped<IMarketRepository, OrleansMarketRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCreateMarketEndpoint();
app.UseChangeMarketNameEndpoint();
app.UseAddRunnersEndpoint();
app.UseSuspendMarketEndpoint();
app.UseResumeMarketEndpoint();
app.UseCloseMarketEndpoint();


app.Run();