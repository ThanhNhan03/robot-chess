using Microsoft.EntityFrameworkCore;
using robot_chest_api.Data;
using robot_chest_api.Services.Interface;
using robot_chest_api.Services.Implement;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ✅ Register DbContext (PostgreSQL via Supabase)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PostgresContext>(options =>
    options.UseNpgsql(connectionString)
);

// ✅ Register Supabase Client
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseAnonKey = builder.Configuration["Supabase:AnonKey"];

if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseAnonKey))
{
    throw new Exception("Supabase configuration is missing in appsettings.json");
}

var supabaseClient = new Supabase.Client(supabaseUrl, supabaseAnonKey);
builder.Services.AddSingleton(supabaseClient);

// ✅ Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppUserService, AppUserService>();

// ✅ Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Use CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🚀 Robot Chess API is running...");
Console.WriteLine($"📊 Swagger UI: {(app.Environment.IsDevelopment() ? "https://localhost:7xxx/swagger" : "Available")}");

app.Run();
