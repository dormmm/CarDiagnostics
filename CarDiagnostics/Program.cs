using CarDiagnostics.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register IUserService
builder.Services.AddScoped<IUserService, UserService>();

// Register CarService
builder.Services.AddScoped<CarService>();  // ׳”׳•׳¡׳₪׳× ׳¨׳™׳©׳•׳ ׳©׳ CarService
builder.Services.AddScoped<AIService>();  // נ”¹ ׳”׳•׳¡׳£ ׳׳× ׳–׳”

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
