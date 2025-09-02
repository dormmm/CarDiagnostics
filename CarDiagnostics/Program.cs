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
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

// Storage service (ממשק בלבד)
builder.Services.AddSingleton<IAzureStorageService, AzureStorageService>();

// Repositories (ממשקים בלבד)
builder.Services.AddScoped<ICarsCallsRepository, CarsCallsRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Repositories
builder.Services.AddScoped<UserRepository>();           // ← הוסף שורה זו

builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<AIService>();
builder.Services.AddSingleton<LicensePlateService>();
builder.Services.AddSingleton<FollowUpQuestionStore>();
builder.Services.AddSingleton<ManualContentFetcher>();

// ManualLinkService – הזרקה דרך IAzureStorageService
builder.Services.AddSingleton<ManualLinkService>(sp =>
{
    var storage = sp.GetRequiredService<IAzureStorageService>();
    return new ManualLinkService(storage, "manual_links.json");
});

// VisualDiagnosisService – קבלת מפתח מהקונפיג
builder.Services.AddSingleton<VisualDiagnosisService>(sp =>
{
    var apiKey = builder.Configuration["OpenAI:ApiKey"];
    var licensePlateService = sp.GetRequiredService<LicensePlateService>();
    var manualLinkService = sp.GetRequiredService<ManualLinkService>();
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
