using CarDiagnostics.Services;
using CarDiagnostics.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient(); // ⬅️ שורה חדשה – חובה!

// Register IUserService
builder.Services.AddScoped<IUserService, UserService>();

// Register Services
builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<AIService>(); // עכשיו יקבל את IHttpClientFactory

// Register Repositories
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<VehicleRepository>();
builder.Services.AddScoped<CarsCallsRepository>();

var app = builder.Build();

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
