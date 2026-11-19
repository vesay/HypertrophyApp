using HypertrophyApp.Domain.Exercises;
using HypertrophyApp.Domain.Nutrition;
using HypertrophyApp.Domain.Profiles;
using HypertrophyApp.Domain.Workouts;
using Microsoft.EntityFrameworkCore;

namespace HypertrophyApp.Application.Common;

/// <summary>
/// Abstraction over the EF Core context so use cases can query with LINQ while
/// Infrastructure owns the database provider, mappings and migrations.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<UserProfile> Profiles { get; }
    DbSet<WorkoutSession> WorkoutSessions { get; }
    DbSet<FoodEntry> FoodEntries { get; }
    DbSet<Exercise> Exercises { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
