using HypertrophyApp.Application.Exercises;
using HypertrophyApp.Application.Nutrition;
using HypertrophyApp.Application.Profiles;
using HypertrophyApp.Application.Workouts;
using Microsoft.Extensions.DependencyInjection;

namespace HypertrophyApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, ExerciseMediaOptions media)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(media);
        services.AddScoped<ProfileService>();
        services.AddScoped<WorkoutLogService>();
        services.AddScoped<FoodLogService>();
        services.AddScoped<ExerciseLibraryService>();
        return services;
    }
}
