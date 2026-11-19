using System.Net;
using System.Net.Http.Json;
using HypertrophyApp.Shared.Dashboard;
using HypertrophyApp.Shared.Exercises;
using HypertrophyApp.Shared.Profiles;
using HypertrophyApp.Shared.Programs;
using HypertrophyApp.Shared.Workouts;

namespace HypertrophyApp.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class ApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Onboarding_flow_creates_profile_and_builds_dashboard()
    {
        var profile = await CreateProfileAsync(days: 3);

        var dashboard = await _client.GetFromJsonAsync<DashboardDto>($"/api/profiles/{profile.Id}/dashboard");

        Assert.NotNull(dashboard);
        Assert.Equal(3, dashboard.WeeklySchedule.Count(d => d.Workout is not null));
        Assert.True(dashboard.Nutrition.TargetCalories > 0);
        // Built-in program exercises are linked to the exercise library and come with demo images.
        Assert.All(dashboard.WeeklySchedule.Where(d => d.Workout is not null).SelectMany(d => d.Workout!.Exercises),
            e => Assert.NotEmpty(e.ImageUrls ?? []));
    }

    [Fact]
    public async Task Invalid_profile_is_rejected_with_problem_details()
    {
        var response = await _client.PostAsJsonAsync("/api/profiles", new SaveProfileRequest { Name = "", Age = 5 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Logged_sets_show_up_in_history_and_progress()
    {
        var profile = await CreateProfileAsync(days: 4);
        var date = new DateOnly(2026, 1, 15);
        var sets = new List<SetLogDto>
        {
            new("Barbell Bench Press", 1, 8, 60),
            new("Barbell Bench Press", 2, 6, 60),
        };

        var put = await _client.PutAsJsonAsync($"/api/profiles/{profile.Id}/sessions/{date:yyyy-MM-dd}",
            new SaveWorkoutSessionRequest("Upper A", sets));
        put.EnsureSuccessStatusCode();

        var progress = await _client.GetFromJsonAsync<ExerciseProgressDto>(
            $"/api/profiles/{profile.Id}/progress?exercise=Barbell%20Bench%20Press");

        var point = Assert.Single(progress!.Points);
        Assert.Equal(60, point.BestWeightKg);
        Assert.Equal(8, point.BestReps);
        Assert.Equal(840, point.VolumeKg);
    }

    [Fact]
    public async Task Deleting_a_profile_removes_it()
    {
        var profile = await CreateProfileAsync(days: 2);

        (await _client.DeleteAsync($"/api/profiles/{profile.Id}")).EnsureSuccessStatusCode();

        var response = await _client.GetAsync($"/api/profiles/{profile.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Exercise_library_is_seeded_and_searchable()
    {
        var result = await _client.GetFromJsonAsync<ExerciseSearchResultDto>("/api/exercises?q=squat&take=5");

        Assert.NotNull(result);
        Assert.True(result.Total > 20);
        Assert.Equal("Barbell_Squat", result.Items[0].Id);

        var missing = await _client.GetAsync("/api/exercises/does-not-exist");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    private async Task<ProfileDto> CreateProfileAsync(int days)
    {
        var splits = await _client.GetFromJsonAsync<List<SplitSummaryDto>>($"/api/programs/splits?days={days}");
        var request = new SaveProfileRequest
        {
            Name = "Test", Sex = Sex.Female, Age = 30, HeightCm = 168, WeightKg = 62,
            DaysPerWeek = days, SplitId = splits!.Single(s => s.IsRecommended).Id, Goal = Goal.BuildMuscle,
        };
        var response = await _client.PostAsJsonAsync("/api/profiles", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProfileDto>())!;
    }
}
