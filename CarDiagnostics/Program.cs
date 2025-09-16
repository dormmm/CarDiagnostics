using CarDiagnostics.Services;
using CarDiagnostics.Repository;
using CarDiagnostics.Domain.Interfaces;
using CarDiagnostics.API.Middlewares;
using CarDiagnostics.API; // SwaggerFileOperationFilter
using CarDiagnostics.Domain.Models.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Core
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<CarDiagnostics.API.SwaggerFileOperationFilter>();
});
builder.Services.AddHttpClient();        // פעם אחת בלבד
builder.Services.AddMemoryCache();

// Storage service
builder.Services.AddSingleton<IAzureStorageService, AzureStorageService>();

// Repositories
builder.Services.AddScoped<ICarsCallsRepository, CarsCallsRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserRepository>();               // ← הוחזר כדי להתאים ל-UserService שמזריק מחלקה קונקרטית
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<AIService>();
builder.Services.AddSingleton<LicensePlateService>();
builder.Services.AddSingleton<FollowUpQuestionStore>();
builder.Services.AddSingleton<ManualContentFetcher>();

// Concurrency services
builder.Services.AddSingleton<ILinkFetcherService, LinkFetcherService>(); // Singleton כדי להתאים ל-ManualLinkService

// ManualLinkService עם הזרקה של fetcher
builder.Services.AddSingleton<ManualLinkService>(sp =>
{
    var storage = sp.GetRequiredService<IAzureStorageService>();
    var fetcher = sp.GetRequiredService<ILinkFetcherService>();
    return new ManualLinkService(storage, "manual_links.json", fetcher);
});

// VisualDiagnosisService – קבלת מפתח מהקונפיג
builder.Services.AddSingleton<VisualDiagnosisService>(sp =>
{
    var apiKey = builder.Configuration["OpenAI:ApiKey"];
    var licensePlateService = sp.GetRequiredService<LicensePlateService>();
    var manualLinkService   = sp.GetRequiredService<ManualLinkService>();
    return new VisualDiagnosisService(apiKey, licensePlateService, manualLinkService);
});

// ProblemTopicService – קבלת מפתח מהקונפיג
builder.Services.AddSingleton<ProblemTopicService>(sp =>
{
    var openAiKey = builder.Configuration["OpenAI:ApiKey"];
    return new ProblemTopicService(openAiKey);
});

var app = builder.Build();

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
