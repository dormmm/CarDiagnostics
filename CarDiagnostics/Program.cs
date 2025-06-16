using CarDiagnostics.Services;
using CarDiagnostics.Repository;
using CarDiagnostics.Domain.Interfaces;
using CarDiagnostics.API.Middlewares;

// יצירת builder לאפליקציה
var builder = WebApplication.CreateBuilder(args);

// הוספת שירותי MVC (Controllers) למערכת
builder.Services.AddControllers();

// הוספת תמיכה ב-EndPoints ו-Swagger (תיעוד API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// הוספת HttpClient לשימוש בשירותים חיצוניים (למשל, קריאות API)
builder.Services.AddHttpClient(); // לאי-סיודיי

// רישום שירותים (Services) ל-Injection
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<AIService>();

// רישום רפוזיטוריים (Repositories) ל-Injection
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<VehicleRepository>();
builder.Services.AddScoped<CarsCallsRepository>();
builder.Services.AddScoped<ICarsCallsRepository, CarsCallsRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();

// רישום שירות Singleton (אובייקט יחיד לכל חיי האפליקציה)
builder.Services.AddSingleton<LicensePlateService>();
builder.Services.AddSingleton<LicensePlateService>(); // שורה כפולה, מיותר - אפשר למחוק אחת

builder.Services.AddSingleton(new ManualLinkService("manual_links.json"));
builder.Services.AddSingleton<FollowUpQuestionStore>();



builder.Services.AddSingleton<ProblemTopicService>(sp =>
    new ProblemTopicService(builder.Configuration["OpenAI:ApiKey"]));



// בניית האפליקציה
var app = builder.Build();

// הוספת Middleware לטיפול בחריגות (שגיאות)
app.UseMiddleware<CarDiagnostics.API.Middlewares.ExceptionHandlingMiddleware>();

// קונפיגורציה של ה-Pipeline
if (app.Environment.IsDevelopment())
{
    // הפעלת Swagger רק בסביבת פיתוח
    app.UseSwagger();
    app.UseSwaggerUI();
}

// אפשרות לבטל הפניה ל-HTTPS (מושבת כרגע)
// app.UseHttpsRedirection();

// הוספת Middleware לאימות והרשאות
app.UseAuthorization();

// מיפוי ה-Controllers לנתיבים
app.MapControllers();

// הפעלת האפליקציה
app.Run();