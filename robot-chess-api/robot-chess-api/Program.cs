using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using robot_chess_api.Data;
using robot_chess_api.Services.Interface;
using robot_chess_api.Services.Implement;
using robot_chess_api.Middleware;
using robot_chess_api.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\n\nExample: 'Bearer abcdef12345'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

//Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "https://localhost:5173",
                "http://192.168.1.85:5173",
                "https://192.168.1.85:5173",
                "http://192.168.1.85:7096",
                "https://192.168.1.85:7096"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for SignalR
    });
});

// Add SignalR
builder.Services.AddSignalR();

// Configure Authentication (for [Authorize] attribute support)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // We're using custom JWT validation in JwtMiddleware
        // This is just to satisfy the [Authorize] attribute requirement
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Skip default JWT validation - our middleware handles it
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                // Token validation is done by JwtMiddleware
                return Task.CompletedTask;
            }
        };
        options.RequireHttpsMetadata = false;
        options.SaveToken = false;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false,
            RequireExpirationTime = false,
            RequireSignedTokens = false
        };
    });

builder.Services.AddAuthorization();

// Configure Npgsql to handle UTC DateTime with timestamp without timezone
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Register DbContext (PostgreSQL via Supabase)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PostgresContext>(options =>
    options.UseNpgsql(connectionString)
);

// Register Supabase Client
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseAnonKey = builder.Configuration["Supabase:AnonKey"];

if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseAnonKey))
{
    throw new Exception("Supabase configuration is missing in appsettings.json");
}

var supabaseClient = new Supabase.Client(supabaseUrl, supabaseAnonKey);
builder.Services.AddSingleton(supabaseClient);

//Register Repositories
builder.Services.AddScoped<IRobotRepository, RobotRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFaqRepository, FaqRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameMoveRepository, GameMoveRepository>();
builder.Services.AddScoped<ITrainingPuzzleRepository, TrainingPuzzleRepository>();
builder.Services.AddScoped<ISavedStateRepository, SavedStateRepository>();
builder.Services.AddScoped<IPointPackageRepository, PointPackageRepository>();
builder.Services.AddScoped<IPaymentHistoryRepository, PaymentHistoryRepository>();
builder.Services.AddScoped<IPointTransactionRepository, PointTransactionRepository>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppUserService, AppUserService>();
builder.Services.AddScoped<IRobotService, RobotService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFaqService, FaqService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITrainingPuzzleService, TrainingPuzzleService>();
builder.Services.AddScoped<IPointPackageService, PointPackageService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAiSuggestionService, AiSuggestionService>();

// Register Helpers
builder.Services.AddScoped<robot_chess_api.Helpers.ChessEngineHelper>();

// Register HttpClient for communication with TCP Server
builder.Services.AddHttpClient();

// Register IMemoryCache for rate limiting (AI suggestions)
builder.Services.AddMemoryCache();

// Logging
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

// Use CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();

// Use JWT Middleware (custom validation with Supabase)
app.UseMiddleware<JwtMiddleware>();

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
app.MapHub<robot_chess_api.Hubs.AuthHub>("/authHub");

Console.WriteLine("Robot Chess API is running...");
Console.WriteLine($"Swagger UI: {(app.Environment.IsDevelopment() ? "https://localhost:7096/swagger" : "Available")}");

app.Run();