using HypertrophyApp.Application.Common;
using HypertrophyApp.Domain.Exercises;
using HypertrophyApp.Domain.Nutrition;
using HypertrophyApp.Domain.Profiles;
using HypertrophyApp.Domain.Workouts;
using Microsoft.EntityFrameworkCore;

namespace HypertrophyApp.Infrastructure.Persistence;

public sealed class HypertrophyAppDbContext(DbContextOptions<HypertrophyAppDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<UserProfile> Profiles => Set<UserProfile>();
    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
    public DbSet<FoodEntry> FoodEntries => Set<FoodEntry>();
    public DbSet<Exercise> Exercises => Set<Exercise>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>(b =>
        {
            b.ToTable("exercises");
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).HasMaxLength(100);
            b.Property(e => e.Name).HasMaxLength(120).IsRequired();
            b.Property(e => e.Category).HasMaxLength(40);
            b.Property(e => e.Level).HasMaxLength(20);
            b.Property(e => e.Equipment).HasMaxLength(40);
            b.Property(e => e.Force).HasMaxLength(20);
            b.Property(e => e.Mechanic).HasMaxLength(20);
            b.HasIndex(e => e.Name);
            // string[] properties map to PostgreSQL text[] columns.
        });

        modelBuilder.Entity<UserProfile>(b =>
        {
            b.ToTable("profiles");
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).HasMaxLength(UserProfile.MaxNameLength).IsRequired();
            b.Property(p => p.HeightCm).HasPrecision(5, 1);
            b.Property(p => p.WeightKg).HasPrecision(5, 1);
            b.Property(p => p.SplitId).HasMaxLength(50).IsRequired();
            b.Property(p => p.Goal).HasConversion<string>().HasMaxLength(20);
            b.Property(p => p.Sex).HasConversion<string>().HasMaxLength(10);
            b.Ignore(p => p.Bmi);
            b.Ignore(p => p.UsesCustomProgram);

            // The custom program is always read and written as a whole, so it lives in one jsonb column.
            b.OwnsMany(p => p.CustomProgram, day =>
            {
                day.ToJson("custom_program");
                day.Property(d => d.Day).HasConversion<string>();
                day.OwnsMany(d => d.Exercises);
            });
        });

        modelBuilder.Entity<WorkoutSession>(b =>
        {
            b.ToTable("workout_sessions");
            b.HasKey(s => s.Id);
            b.HasIndex(s => new { s.ProfileId, s.Date }).IsUnique();
            b.Property(s => s.Title).HasMaxLength(WorkoutSession.MaxTitleLength).IsRequired();
            b.HasOne<UserProfile>().WithMany().HasForeignKey(s => s.ProfileId).OnDelete(DeleteBehavior.Cascade);
            b.Ignore(s => s.TotalReps);
            b.Ignore(s => s.TotalVolumeKg);

            // Sets get their own table so they stay queryable for progress reports.
            b.OwnsMany(s => s.Sets, set =>
            {
                set.ToTable("workout_sets");
                set.WithOwner().HasForeignKey("SessionId");
                set.Property<int>("Id");
                set.HasKey("Id");
                set.Property(x => x.ExerciseName).HasMaxLength(80).IsRequired();
                set.Property(x => x.WeightKg).HasPrecision(6, 2);
                set.Ignore(x => x.Estimated1RmKg);
            });
        });

        modelBuilder.Entity<FoodEntry>(b =>
        {
            b.ToTable("food_entries");
            b.HasKey(f => f.Id);
            b.HasIndex(f => new { f.ProfileId, f.Date });
            b.Property(f => f.Name).HasMaxLength(FoodEntry.MaxNameLength).IsRequired();
            b.Property(f => f.Meal).HasConversion<string>().HasMaxLength(20);
            b.Property(f => f.ProteinGrams).HasPrecision(6, 1);
            b.Property(f => f.CarbGrams).HasPrecision(6, 1);
            b.Property(f => f.FatGrams).HasPrecision(6, 1);
            b.HasOne<UserProfile>().WithMany().HasForeignKey(f => f.ProfileId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
