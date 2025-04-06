using CarDiagnostics.Services;
using CarDiagnostics.Repository;
using CarDiagnostics.Domain.Interfaces;
using CarDiagnostics.API.Middlewares;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); // לאי-סיודיי

// Register Application Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<AIService>();

// Register Repositories
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<VehicleRepository>();
builder.Services.AddScoped<CarsCallsRepository>();
builder.Services.AddScoped<ICarsCallsRepository, CarsCallsRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();


var app = builder.Build();
app.UseMiddleware<CarDiagnostics.API.Middlewares.ExceptionHandlingMiddleware>();


// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Optional: disable HTTPS redirect if not needed locally
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
