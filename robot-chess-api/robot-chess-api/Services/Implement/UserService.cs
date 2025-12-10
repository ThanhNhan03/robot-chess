using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using robot_chess_api.Data;
using robot_chess_api.DTOs;
using robot_chess_api.Models;
using robot_chess_api.Repositories;
using robot_chess_api.Services.Interface;
using System.Security.Cryptography;

namespace robot_chess_api.Services.Implement;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly PostgresContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository, 
        PostgresContext context, 
        IConfiguration configuration,
        IEmailService emailService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetAllUsersAsync(bool includeInactive = false)
    {
        var users = await _userRepository.GetAllUsersAsync(includeInactive);
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        // Validate email uniqueness in app_users
        if (await _userRepository.EmailExistsAsync(dto.Email))
        {
            throw new InvalidOperationException($"Email '{dto.Email}' is already in use");
        }

        // Validate username uniqueness
        if (await _userRepository.UsernameExistsAsync(dto.Username))
        {
            throw new InvalidOperationException($"Username '{dto.Username}' is already in use");
        }

        try
        {
            // Create user in Supabase Auth using Admin API
            var authResponse = await CreateSupabaseAuthUserAsync(dto);
            
            if (authResponse == null || string.IsNullOrEmpty(authResponse.Id))
            {
                throw new InvalidOperationException("Failed to create user in Supabase Auth");
            }

            // Create user in app_users table with the same ID from Supabase Auth
            var user = new AppUser
            {
                Id = Guid.Parse(authResponse.Id),
                Email = dto.Email,
                Username = dto.Username,
                FullName = dto.FullName,
                AvatarUrl = dto.AvatarUrl,
                Role = dto.Role,
                PhoneNumber = dto.PhoneNumber,
                IsActive = true,
                EmailVerified = false,
                EmailVerificationToken = GenerateVerificationToken(),
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
            };

            var createdUser = await _userRepository.CreateUserAsync(user);

            // Send verification email
            try
            {
                await _emailService.SendVerificationEmailAsync(
                    createdUser.Email,
                    createdUser.Username,
                    createdUser.EmailVerificationToken!
                );
                _logger.LogInformation($"Verification email sent to admin-created user: {createdUser.Email}");
            }
            catch (Exception emailEx)
            {
                _logger.LogError($"Failed to send verification email to {createdUser.Email}: {emailEx.Message}");
                // Continue anyway - user is created
            }

            return MapToDto(createdUser);
        }
        catch (Exception ex)
        {
            // If app_users creation fails, we should ideally delete the auth user
            // For now, just log and rethrow
            throw new InvalidOperationException($"Error creating user: {ex.Message}", ex);
        }
    }

    public async Task<UserDto> AdminCreateUserAsync(AdminCreateUserDto dto)
    {
        // Validate email uniqueness in app_users
        if (await _userRepository.EmailExistsAsync(dto.Email))
        {
            throw new InvalidOperationException($"Email '{dto.Email}' is already in use");
        }

        // Validate username uniqueness
        if (await _userRepository.UsernameExistsAsync(dto.Username))
        {
            throw new InvalidOperationException($"Username '{dto.Username}' is already in use");
        }

        try
        {
            // Generate random password
            var generatedPassword = GenerateRandomPassword();
            
            // Create user in Supabase Auth using Admin API
            var authResponse = await CreateSupabaseAuthUserForAdminAsync(dto, generatedPassword);
            
            if (authResponse == null || string.IsNullOrEmpty(authResponse.Id))
            {
                throw new InvalidOperationException("Failed to create user in Supabase Auth");
            }

            // Create user in app_users table with the same ID from Supabase Auth
            var user = new AppUser
            {
                Id = Guid.Parse(authResponse.Id),
                Email = dto.Email,
                Username = dto.Username,
                FullName = dto.FullName,
                AvatarUrl = dto.AvatarUrl,
                Role = dto.Role,
                PhoneNumber = dto.PhoneNumber,
                IsActive = true,
                EmailVerified = true, // Auto-verify for admin-created accounts
                EmailVerificationToken = null,
                EmailVerificationTokenExpiry = null
            };

            var createdUser = await _userRepository.CreateUserAsync(user);

            // Send account credentials email
            try
            {
                await _emailService.SendAccountCreatedEmailAsync(
                    createdUser.Email,
                    createdUser.Username,
                    generatedPassword
                );
                _logger.LogInformation($"Account credentials email sent to: {createdUser.Email}");
            }
            catch (Exception emailEx)
            {
                _logger.LogError($"Failed to send credentials email to {createdUser.Email}: {emailEx.Message}");
                // Continue anyway - user is created
            }

            return MapToDto(createdUser);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error creating user: {ex.Message}", ex);
        }
    }

    private string GenerateRandomPassword(int length = 12)
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
        var random = new Random();
        var password = new char[length];
        
        for (int i = 0; i < length; i++)
        {
            password[i] = validChars[random.Next(validChars.Length)];
        }
        
        return new string(password);
    }

    private async Task<SupabaseAuthUserResponse?> CreateSupabaseAuthUserForAdminAsync(AdminCreateUserDto dto, string password)
    {
        try
        {
            var serviceRoleKey = _configuration["ServiceRoleKey"] 
                ?? throw new InvalidOperationException("ServiceRoleKey not configured");
            
            var supabaseUrl = _configuration["Supabase:Url"] 
                ?? throw new InvalidOperationException("Supabase:Url not configured");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("apikey", serviceRoleKey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {serviceRoleKey}");

            var payload = new
            {
                email = dto.Email,
                password = password,
                email_confirm = true, // Auto-confirm email for admin-created users
                user_metadata = new
                {
                    username = dto.Username,
                    full_name = dto.FullName,
                    role = dto.Role
                }
            };

            var response = await httpClient.PostAsJsonAsync(
                $"{supabaseUrl}/auth/v1/admin/users",
                payload
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Supabase Auth error: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<SupabaseAuthUserResponse>();
            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create Supabase Auth user: {ex.Message}", ex);
        }
    }

    private async Task<SupabaseAuthUserResponse?> CreateSupabaseAuthUserAsync(CreateUserDto dto)
    {
        try
        {
            // Get Supabase service role key from configuration
            var serviceRoleKey = _configuration["ServiceRoleKey"] 
                ?? throw new InvalidOperationException("ServiceRoleKey not configured in appsettings.json");
            
            var supabaseUrl = _configuration["Supabase:Url"] 
                ?? throw new InvalidOperationException("Supabase:Url not configured in appsettings.json");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("apikey", serviceRoleKey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {serviceRoleKey}");

            var payload = new
            {
                email = dto.Email,
                password = dto.Password,
                email_confirm = true, // Auto-confirm email for admin-created users
                user_metadata = new
                {
                    username = dto.Username,
                    full_name = dto.FullName,
                    role = dto.Role
                }
            };

            var response = await httpClient.PostAsJsonAsync(
                $"{supabaseUrl}/auth/v1/admin/users",
                payload
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Supabase Auth error: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<SupabaseAuthUserResponse>();
            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create Supabase Auth user: {ex.Message}", ex);
        }
    }

    // Helper class for Supabase Auth response
    private class SupabaseAuthUserResponse
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found");
        }

        // Update email if provided and different
        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
        {
            if (await _userRepository.EmailExistsAsync(dto.Email, id))
            {
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use");
            }
            user.Email = dto.Email;
        }

        // Update username if provided and different
        if (!string.IsNullOrEmpty(dto.Username) && dto.Username != user.Username)
        {
            if (await _userRepository.UsernameExistsAsync(dto.Username, id))
            {
                throw new InvalidOperationException($"Username '{dto.Username}' is already in use");
            }
            user.Username = dto.Username;
        }

        // Update other fields
        if (dto.FullName != null) user.FullName = dto.FullName;
        if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;
        if (dto.Role != null) user.Role = dto.Role;
        if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;

        var updatedUser = await _userRepository.UpdateUserAsync(user);
        return MapToDto(updatedUser);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found");
        }

        return await _userRepository.DeleteUserAsync(id);
    }

    public async Task<bool> UpdateUserStatusAsync(Guid id, bool isActive)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found");
        }

        user.IsActive = isActive;
        await _userRepository.UpdateUserAsync(user);
        return true;
    }

    public async Task<List<UserDto>> GetUsersByRoleAsync(string role)
    {
        var users = await _userRepository.GetUsersByRoleAsync(role);
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserActivityDto?> GetUserActivityAsync(Guid id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null) return null;

        // Get game statistics
        var games = await _context.Games
            .Where(g => g.PlayerId == id)
            .ToListAsync();

        var lastGame = games.OrderByDescending(g => g.CreatedAt).FirstOrDefault();

        return new UserActivityDto
        {
            UserId = user.Id,
            Username = user.Username,
            TotalGames = games.Count,
            GamesWon = games.Count(g => g.Result == "win"),
            GamesLost = games.Count(g => g.Result == "lose"),
            GamesDraw = games.Count(g => g.Result == "draw"),
            LastLoginAt = user.LastLoginAt,
            LastGameAt = lastGame?.CreatedAt
        };
    }

    public async Task<UserStatsDto> GetUserStatsAsync()
    {
        var allUsers = await _userRepository.GetAllUsersAsync(includeInactive: true);
        var activeUsers = allUsers.Where(u => u.IsActive).ToList();
        
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var newUsersThisWeek = activeUsers
            .Where(u => u.CreatedAt.HasValue && u.CreatedAt.Value >= weekAgo)
            .Count();

        return new UserStatsDto
        {
            TotalUsers = activeUsers.Count,
            ActiveUsers = activeUsers.Count, // Same as active users (IsActive = true)
            AdminUsers = activeUsers.Count(u => u.Role == "admin"),
            NewUsersThisWeek = newUsersThisWeek
        };
    }

    public async Task<UserDto> UpdateUserEloAsync(Guid id, int elo)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found");
        }

        user.EloRating = elo;
        
        // Update peak elo if this is a new high or initial set
        if (!user.PeakElo.HasValue || elo > user.PeakElo.Value)
        {
            user.PeakElo = elo;
        }

        var updatedUser = await _userRepository.UpdateUserAsync(user);
        return MapToDto(updatedUser);
    }

    private static UserDto MapToDto(AppUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt,
            PointsBalance = user.PointsBalance,
            EloRating = user.EloRating,
            PeakElo = user.PeakElo,
            TotalGamesPlayed = user.TotalGamesPlayed,
            Wins = user.Wins,
            Losses = user.Losses,
            Draws = user.Draws
        };
    }

    private static string GenerateVerificationToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
