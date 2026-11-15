using FluentValidation;

using PaymentGateway.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.AddConsole();

builder.Services.AddProblemDetails();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<PaymentsRepository>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSingleton<IBankingClient, BankingClient>();

builder.Services.AddHttpClient<IBankingClient, BankingClient>((_, client) =>
{
    var bankBaseUrl = builder.Configuration["BankingClient:BaseUrl"]
        ?? throw new InvalidOperationException("BankingClient:BaseUrl is not configured.");

    client.BaseAddress = new Uri(bankBaseUrl);
});

var app = builder.Build();

app.UseExceptionHandler("/error");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
