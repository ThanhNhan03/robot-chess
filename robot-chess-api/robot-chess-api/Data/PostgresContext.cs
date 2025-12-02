using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using robot_chess_api.Models;

namespace robot_chess_api.Data;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiSuggestion> AiSuggestions { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<GameMove> GameMoves { get; set; }

    public virtual DbSet<GameType> GameTypes { get; set; }

    public virtual DbSet<PaymentHistory> PaymentHistories { get; set; }

    public virtual DbSet<Robot> Robots { get; set; }

    public virtual DbSet<RobotCommand> RobotCommands { get; set; }

    public virtual DbSet<RobotLog> RobotLogs { get; set; }

    public virtual DbSet<SavedState> SavedStates { get; set; }

    public virtual DbSet<TrainingPuzzle> TrainingPuzzles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    // Robot Management Models
    public virtual DbSet<RobotConfig> RobotConfigs { get; set; }

    public virtual DbSet<RobotMonitoring> RobotMonitorings { get; set; }

    public virtual DbSet<RobotCommandHistory> RobotCommandHistories { get; set; }

    public virtual DbSet<Faq> Faqs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string is configured in Program.cs from appsettings.json
        // Do not override it here
        if (!optionsBuilder.IsConfigured)
        {
            // This is only used for design-time operations (migrations, scaffolding)
            // optionsBuilder.UseNpgsql("Host=db.lyvfqltjhsyjmjmlwweo.supabase.co;Database=postgres;Username=postgres;Password=0oIADgNx5kbykRlB;Port=5432");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone" })
            .HasPostgresEnum("auth", "oauth_authorization_status", new[] { "pending", "approved", "denied", "expired" })
            .HasPostgresEnum("auth", "oauth_client_type", new[] { "public", "confidential" })
            .HasPostgresEnum("auth", "oauth_registration_type", new[] { "dynamic", "manual" })
            .HasPostgresEnum("auth", "oauth_response_type", new[] { "code" })
            .HasPostgresEnum("auth", "one_time_token_type", new[] { "confirmation_token", "reauthentication_token", "recovery_token", "email_change_token_new", "email_change_token_current", "phone_change_token" })
            .HasPostgresEnum("realtime", "action", new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" })
            .HasPostgresEnum("realtime", "equality_op", new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in" })
            .HasPostgresEnum("storage", "buckettype", new[] { "STANDARD", "ANALYTICS" })
            .HasPostgresExtension("extensions", "pg_stat_statements")
            .HasPostgresExtension("extensions", "pgcrypto")
            .HasPostgresExtension("extensions", "uuid-ossp")
            .HasPostgresExtension("graphql", "pg_graphql")
            .HasPostgresExtension("vault", "supabase_vault");

        modelBuilder.Entity<AiSuggestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ai_suggestions_pkey");

            entity.ToTable("ai_suggestions");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Confidence)
                .HasPrecision(5, 2)
                .HasColumnName("confidence");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.MoveId).HasColumnName("move_id");
            entity.Property(e => e.SuggestedMove).HasColumnName("suggested_move");

            entity.HasOne(d => d.Move).WithMany(p => p.AiSuggestions)
                .HasForeignKey(d => d.MoveId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ai_suggestions_move_id_fkey");
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("app_users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username).HasColumnName("username");
            
            // New properties for user management
            entity.Property(e => e.Role)
                .HasDefaultValueSql("'player'::text")
                .HasColumnName("role");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastLoginAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_login_at");
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
            
            // Points System
            entity.Property(e => e.PointsBalance)
                .HasDefaultValue(0)
                .HasColumnName("points_balance");
            
            // Elo Rating System
            entity.Property(e => e.EloRating)
                .HasDefaultValue(1200)
                .HasColumnName("elo_rating");
            entity.Property(e => e.PeakElo)
                .HasDefaultValue(1200)
                .HasColumnName("peak_elo");
            entity.Property(e => e.TotalGamesPlayed)
                .HasDefaultValue(0)
                .HasColumnName("total_games_played");
            entity.Property(e => e.Wins)
                .HasDefaultValue(0)
                .HasColumnName("wins");
            entity.Property(e => e.Losses)
                .HasDefaultValue(0)
                .HasColumnName("losses");
            entity.Property(e => e.Draws)
                .HasDefaultValue(0)
                .HasColumnName("draws");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("feedbacks_pkey");

            entity.ToTable("feedbacks");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("feedbacks_user_id_fkey");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("games_pkey");

            entity.ToTable("games");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EndedAt).HasColumnName("ended_at");
            entity.Property(e => e.FenCurrent).HasColumnName("fen_current");
            entity.Property(e => e.FenStart)
                .HasDefaultValueSql("'startpos'::text")
                .HasColumnName("fen_start");
            entity.Property(e => e.GameTypeId).HasColumnName("game_type_id");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.PuzzleId).HasColumnName("puzzle_id");
            entity.Property(e => e.Result).HasColumnName("result");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'waiting'::text")
                .HasColumnName("status");
            entity.Property(e => e.TotalMoves)
                .HasDefaultValue(0)
                .HasColumnName("total_moves");
            entity.Property(e => e.Difficulty)
                .HasDefaultValueSql("'medium'::text")
                .HasColumnName("difficulty");
            
            // Elo Rating tracking
            entity.Property(e => e.PlayerRatingBefore).HasColumnName("player_rating_before");
            entity.Property(e => e.PlayerRatingAfter).HasColumnName("player_rating_after");
            entity.Property(e => e.RatingChange).HasColumnName("rating_change");

            entity.HasOne(d => d.GameType).WithMany(p => p.Games)
                .HasForeignKey(d => d.GameTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("games_game_type_id_fkey");

            entity.HasOne(d => d.Player).WithMany(p => p.Games)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("games_player_id_fkey");

            entity.HasOne(d => d.Puzzle).WithMany(p => p.Games)
                .HasForeignKey(d => d.PuzzleId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("games_puzzle_id_fkey");
        });

        modelBuilder.Entity<GameMove>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("game_moves_pkey");

            entity.ToTable("game_moves");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.FenStr).HasColumnName("fen_str");
            entity.Property(e => e.FromPiece).HasColumnName("from_piece");
            entity.Property(e => e.FromSquare).HasColumnName("from_square");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.MoveNumber).HasColumnName("move_number");
            entity.Property(e => e.Notation).HasColumnName("notation");
            entity.Property(e => e.PlayerColor).HasColumnName("player_color");
            entity.Property(e => e.ResultsInCheck).HasColumnName("results_in_check");
            entity.Property(e => e.ToPiece).HasColumnName("to_piece");
            entity.Property(e => e.ToSquare).HasColumnName("to_square");
        });

        modelBuilder.Entity<GameType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("game_types_pkey");

            entity.ToTable("game_types");

            entity.HasIndex(e => e.Code, "game_types_code_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<PaymentHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_history_pkey");

            entity.ToTable("payment_history");

            entity.HasIndex(e => e.TransactionId, "payment_history_transaction_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'pending'::text")
                .HasColumnName("status");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.PaymentHistories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("payment_history_user_id_fkey");
        });

        modelBuilder.Entity<Robot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("robots_pkey");

            entity.ToTable("robots");

            entity.HasIndex(e => e.RobotCode, "robots_robot_code_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsOnline)
                .HasDefaultValue(false)
                .HasColumnName("is_online");
            entity.Property(e => e.LastOnlineAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_online_at");
            entity.Property(e => e.Location).HasColumnName("location");
            entity.Property(e => e.MoveSpeedMs)
                .HasDefaultValue(1000)
                .HasColumnName("move_speed_ms");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.RobotCode).HasColumnName("robot_code");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            
            // New fields for robot management
            entity.Property(e => e.IpAddress).HasColumnName("ip_address");
            entity.Property(e => e.TcpConnectionId).HasColumnName("tcp_connection_id");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'idle'::text")
                .HasColumnName("status");
            entity.Property(e => e.CurrentGameId).HasColumnName("current_game_id");
            
            // Navigation properties
            entity.HasOne(d => d.CurrentGame).WithMany()
                .HasForeignKey(d => d.CurrentGameId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("robots_current_game_id_fkey");
        });

        modelBuilder.Entity<RobotCommand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("robot_commands_pkey");

            entity.ToTable("robot_commands");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Command).HasColumnName("command");
            entity.Property(e => e.ExecutedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("executed_at");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.RobotId).HasColumnName("robot_id");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sent_at");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'pending'::text")
                .HasColumnName("status");

            entity.HasOne(d => d.Robot).WithMany(p => p.RobotCommands)
                .HasForeignKey(d => d.RobotId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("robot_commands_robot_id_fkey");
        });

        modelBuilder.Entity<RobotLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("robot_logs_pkey");

            entity.ToTable("robot_logs");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CommandId).HasColumnName("command_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.LogMessage).HasColumnName("log_message");
            entity.Property(e => e.RobotId).HasColumnName("robot_id");

            entity.HasOne(d => d.Command).WithMany(p => p.RobotLogs)
                .HasForeignKey(d => d.CommandId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("robot_logs_command_id_fkey");

            entity.HasOne(d => d.Robot).WithMany(p => p.RobotLogs)
                .HasForeignKey(d => d.RobotId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("robot_logs_robot_id_fkey");
        });

        modelBuilder.Entity<SavedState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("saved_states_pkey");

            entity.ToTable("saved_states");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.FenStr).HasColumnName("fen_str");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.LastMoveId).HasColumnName("last_move_id");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.SavedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("saved_at");

            entity.HasOne(d => d.LastMove).WithMany(p => p.SavedStates)
                .HasForeignKey(d => d.LastMoveId)
                .HasConstraintName("saved_states_last_move_id_fkey");

            entity.HasOne(d => d.Player).WithMany(p => p.SavedStates)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("saved_states_player_id_fkey");
        });

        modelBuilder.Entity<TrainingPuzzle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("training_puzzles_pkey");

            entity.ToTable("training_puzzles");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Difficulty)
                .HasDefaultValueSql("'medium'::text")
                .HasColumnName("difficulty");
            entity.Property(e => e.FenStr).HasColumnName("fen_str");
            entity.Property(e => e.SolutionMove).HasColumnName("solution_move");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users", "auth", tb => tb.HasComment("Auth: Stores user login data within a secure schema."));

            entity.HasIndex(e => e.ConfirmationToken, "confirmation_token_idx")
                .IsUnique()
                .HasFilter("((confirmation_token)::text !~ '^[0-9 ]*$'::text)");

            entity.HasIndex(e => e.EmailChangeTokenCurrent, "email_change_token_current_idx")
                .IsUnique()
                .HasFilter("((email_change_token_current)::text !~ '^[0-9 ]*$'::text)");

            entity.HasIndex(e => e.EmailChangeTokenNew, "email_change_token_new_idx")
                .IsUnique()
                .HasFilter("((email_change_token_new)::text !~ '^[0-9 ]*$'::text)");

            entity.HasIndex(e => e.ReauthenticationToken, "reauthentication_token_idx")
                .IsUnique()
                .HasFilter("((reauthentication_token)::text !~ '^[0-9 ]*$'::text)");

            entity.HasIndex(e => e.RecoveryToken, "recovery_token_idx")
                .IsUnique()
                .HasFilter("((recovery_token)::text !~ '^[0-9 ]*$'::text)");

            entity.HasIndex(e => e.Email, "users_email_partial_key")
                .IsUnique()
                .HasFilter("(is_sso_user = false)");

            entity.HasIndex(e => e.InstanceId, "users_instance_id_idx");

            entity.HasIndex(e => e.IsAnonymous, "users_is_anonymous_idx");

            entity.HasIndex(e => e.Phone, "users_phone_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Aud)
                .HasMaxLength(255)
                .HasColumnName("aud");
            entity.Property(e => e.BannedUntil).HasColumnName("banned_until");
            entity.Property(e => e.ConfirmationSentAt).HasColumnName("confirmation_sent_at");
            entity.Property(e => e.ConfirmationToken)
                .HasMaxLength(255)
                .HasColumnName("confirmation_token");
            entity.Property(e => e.ConfirmedAt)
                .HasComputedColumnSql("LEAST(email_confirmed_at, phone_confirmed_at)", true)
                .HasColumnName("confirmed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.EmailChange)
                .HasMaxLength(255)
                .HasColumnName("email_change");
            entity.Property(e => e.EmailChangeConfirmStatus)
                .HasDefaultValue((short)0)
                .HasColumnName("email_change_confirm_status");
            entity.Property(e => e.EmailChangeSentAt).HasColumnName("email_change_sent_at");
            entity.Property(e => e.EmailChangeTokenCurrent)
                .HasMaxLength(255)
                .HasDefaultValueSql("''::character varying")
                .HasColumnName("email_change_token_current");
            entity.Property(e => e.EmailChangeTokenNew)
                .HasMaxLength(255)
                .HasColumnName("email_change_token_new");
            entity.Property(e => e.EmailConfirmedAt).HasColumnName("email_confirmed_at");
            entity.Property(e => e.EncryptedPassword)
                .HasMaxLength(255)
                .HasColumnName("encrypted_password");
            entity.Property(e => e.InstanceId).HasColumnName("instance_id");
            entity.Property(e => e.InvitedAt).HasColumnName("invited_at");
            entity.Property(e => e.IsAnonymous)
                .HasDefaultValue(false)
                .HasColumnName("is_anonymous");
            entity.Property(e => e.IsSsoUser)
                .HasDefaultValue(false)
                .HasComment("Auth: Set this column to true when the account comes from SSO. These accounts can have duplicate emails.")
                .HasColumnName("is_sso_user");
            entity.Property(e => e.IsSuperAdmin).HasColumnName("is_super_admin");
            entity.Property(e => e.LastSignInAt).HasColumnName("last_sign_in_at");
            entity.Property(e => e.Phone)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("phone");
            entity.Property(e => e.PhoneChange)
                .HasDefaultValueSql("''::character varying")
                .HasColumnName("phone_change");
            entity.Property(e => e.PhoneChangeSentAt).HasColumnName("phone_change_sent_at");
            entity.Property(e => e.PhoneChangeToken)
                .HasMaxLength(255)
                .HasDefaultValueSql("''::character varying")
                .HasColumnName("phone_change_token");
            entity.Property(e => e.PhoneConfirmedAt).HasColumnName("phone_confirmed_at");
            entity.Property(e => e.RawAppMetaData)
                .HasColumnType("jsonb")
                .HasColumnName("raw_app_meta_data");
            entity.Property(e => e.RawUserMetaData)
                .HasColumnType("jsonb")
                .HasColumnName("raw_user_meta_data");
            entity.Property(e => e.ReauthenticationSentAt).HasColumnName("reauthentication_sent_at");
            entity.Property(e => e.ReauthenticationToken)
                .HasMaxLength(255)
                .HasDefaultValueSql("''::character varying")
                .HasColumnName("reauthentication_token");
            entity.Property(e => e.RecoverySentAt).HasColumnName("recovery_sent_at");
            entity.Property(e => e.RecoveryToken)
                .HasMaxLength(255)
                .HasColumnName("recovery_token");
            entity.Property(e => e.Role)
                .HasMaxLength(255)
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // Robot Config Entity
        modelBuilder.Entity<RobotConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("robot_configs_pkey");

            entity.ToTable("robot_configs");

            entity.HasIndex(e => e.RobotId, "robot_configs_robot_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.RobotId).HasColumnName("robot_id");
            entity.Property(e => e.Speed)
                .HasDefaultValue(50)
                .HasColumnName("speed");
            entity.Property(e => e.GripperForce)
                .HasDefaultValue(50)
                .HasColumnName("gripper_force");
            entity.Property(e => e.GripperSpeed)
                .HasDefaultValue(50)
                .HasColumnName("gripper_speed");
            entity.Property(e => e.MaxSpeed)
                .HasDefaultValue(100)
                .HasColumnName("max_speed");
            entity.Property(e => e.EmergencyStop)
                .HasDefaultValue(false)
                .HasColumnName("emergency_stop");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Robot).WithOne(p => p.RobotConfig)
                .HasForeignKey<RobotConfig>(d => d.RobotId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("robot_configs_robot_id_fkey");

            entity.HasOne(d => d.UpdatedByUser).WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("robot_configs_updated_by_fkey");
        });

        // Robot Monitoring Entity
        modelBuilder.Entity<RobotMonitoring>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("robot_monitoring_pkey");

            entity.ToTable("robot_monitoring");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.RobotId).HasColumnName("robot_id");
            entity.Property(e => e.CurrentPositionX)
                .HasPrecision(10, 2)
                .HasColumnName("current_position_x");
            entity.Property(e => e.CurrentPositionY)
                .HasPrecision(10, 2)
                .HasColumnName("current_position_y");
            entity.Property(e => e.CurrentPositionZ)
                .HasPrecision(10, 2)
                .HasColumnName("current_position_z");
            entity.Property(e => e.CurrentRotationRx)
                .HasPrecision(10, 3)
                .HasColumnName("current_rotation_rx");
            entity.Property(e => e.CurrentRotationRy)
                .HasPrecision(10, 3)
                .HasColumnName("current_rotation_ry");
            entity.Property(e => e.CurrentRotationRz)
                .HasPrecision(10, 3)
                .HasColumnName("current_rotation_rz");
            entity.Property(e => e.GripperState).HasColumnName("gripper_state");
            entity.Property(e => e.GripperPosition).HasColumnName("gripper_position");
            entity.Property(e => e.IsMoving).HasColumnName("is_moving");
            entity.Property(e => e.CurrentSpeed).HasColumnName("current_speed");
            entity.Property(e => e.CurrentCommandId).HasColumnName("current_command_id");
            entity.Property(e => e.HasError)
                .HasDefaultValue(false)
                .HasColumnName("has_error");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.RecordedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("recorded_at");

            entity.HasOne(d => d.Robot).WithMany(p => p.RobotMonitorings)
                .HasForeignKey(d => d.RobotId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("robot_monitoring_robot_id_fkey");

            entity.HasOne(d => d.CurrentCommand).WithMany()
                .HasForeignKey(d => d.CurrentCommandId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("robot_monitoring_command_id_fkey");
        });

        // Robot Command History Entity
        modelBuilder.Entity<RobotCommandHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("robot_command_history_pkey");

            entity.ToTable("robot_command_history");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.RobotId).HasColumnName("robot_id");
            entity.Property(e => e.CommandType).HasColumnName("command_type");
            entity.Property(e => e.CommandPayload)
                .HasColumnType("jsonb")
                .HasColumnName("command_payload");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'pending'::text")
                .HasColumnName("status");
            entity.Property(e => e.ResultPayload)
                .HasColumnType("jsonb")
                .HasColumnName("result_payload");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sent_at");
            entity.Property(e => e.StartedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("started_at");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_at");
            entity.Property(e => e.ExecutionTimeMs).HasColumnName("execution_time_ms");
            entity.Property(e => e.ExecutedBy).HasColumnName("executed_by");

            entity.HasOne(d => d.Robot).WithMany(p => p.RobotCommandHistories)
                .HasForeignKey(d => d.RobotId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("robot_command_history_robot_id_fkey");

            entity.HasOne(d => d.ExecutedByUser).WithMany(p => p.RobotCommandHistories)
                .HasForeignKey(d => d.ExecutedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("robot_command_history_executed_by_fkey");
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("faqs_pkey");

            entity.ToTable("faqs");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Question).HasColumnName("question");
            entity.Property(e => e.Answer).HasColumnName("answer");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.IsPublished)
                .HasDefaultValue(false)
                .HasColumnName("is_published");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.HasSequence<int>("seq_schema_version", "graphql").IsCyclic();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
