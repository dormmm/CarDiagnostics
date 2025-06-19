using CarDiagnostics.Services;
using CarDiagnostics.Repository;
using CarDiagnostics.Domain.Interfaces;
using CarDiagnostics.API.Middlewares;
using CarDiagnostics.API; // כדי שיכיר את SwaggerFileOperationFilter


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<CarDiagnostics.API.SwaggerFileOperationFilter>();
});

builder.Services.AddHttpClient();

// Repositories
builder.Services.AddScoped<ICarsCallsRepository, CarsCallsRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<VehicleRepository>();
builder.Services.AddScoped<CarsCallsRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<AIService>();
builder.Services.AddSingleton<LicensePlateService>();
builder.Services.AddSingleton<FollowUpQuestionStore>();

// ManualLinkService עם פרמטר לקובץ
builder.Services.AddSingleton(new ManualLinkService("manual_links.json"));

// VisualDiagnosisService – תיקון והזרקה עם פרמטרים
builder.Services.AddSingleton<VisualDiagnosisService>(sp =>
{
    var apiKey = builder.Configuration["OpenAI:ApiKey"];
    var licensePlateService = sp.GetRequiredService<LicensePlateService>();
    var manualLinkService = sp.GetRequiredService<ManualLinkService>();

    return new VisualDiagnosisService(apiKey, licensePlateService, manualLinkService);
});

// ProblemTopicService – גם כן עם Key
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
