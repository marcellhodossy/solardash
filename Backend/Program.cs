using SolarDash.Config;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<PostgreSQL>();
builder.Services.AddSingleton<authentication>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()
    );
});

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");

app.MapControllers();

app.Run();
