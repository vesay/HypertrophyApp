using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HypertrophyApp.Shared.Dashboard;
using HypertrophyApp.Shared.Exercises;
using HypertrophyApp.Shared.Nutrition;
using HypertrophyApp.Shared.Profiles;
using HypertrophyApp.Shared.Programs;
using HypertrophyApp.Shared.Workouts;

namespace HypertrophyApp.Web.Services;

/// <summary>Result of a write call: either the saved value or a user-friendly error message.</summary>
public readonly record struct ApiResult<T>(T? Value, string? Error)
{
    public bool IsSuccess => Error is null;
}

/// <summary>Typed wrapper around the HypertrophyApp REST API.</summary>
public sealed class HypertrophyAppApiClient(HttpClient http)
{
    // ---------- Programs ----------
    public async Task<IReadOnlyList<SplitSummaryDto>> GetSplitsAsync(int daysPerWeek) =>
        await http.GetFromJsonAsync<List<SplitSummaryDto>>($"api/programs/splits?days={daysPerWeek}") ?? [];

    // ---------- Profiles ----------
    public Task<ApiResult<ProfileDto>> CreateProfileAsync(SaveProfileRequest request) =>
        SendAsync<ProfileDto>(http.PostAsJsonAsync("api/profiles", request));

    public Task<ApiResult<ProfileDto>> UpdateProfileAsync(Guid id, SaveProfileRequest request) =>
        SendAsync<ProfileDto>(http.PutAsJsonAsync($"api/profiles/{id}", request));

    public async Task DeleteProfileAsync(Guid id) => await http.DeleteAsync($"api/profiles/{id}");

    /// <returns>The dashboard, or <c>null</c> when the profile no longer exists.</returns>
    public async Task<DashboardDto?> GetDashboardAsync(Guid profileId)
    {
        var response = await http.GetAsync($"api/profiles/{profileId}/dashboard");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DashboardDto>();
    }

    public async Task<IReadOnlyList<CustomDayDto>> GetCustomProgramAsync(Guid profileId) =>
        await http.GetFromJsonAsync<List<CustomDayDto>>($"api/profiles/{profileId}/custom-program") ?? [];

    public Task<ApiResult<ProfileDto>> SaveCustomProgramAsync(Guid profileId, SaveCustomProgramRequest request) =>
        SendAsync<ProfileDto>(http.PutAsJsonAsync($"api/profiles/{profileId}/custom-program", request));

    // ---------- Workout log ----------
    public async Task<WorkoutSessionDto?> GetSessionAsync(Guid profileId, DateOnly date)
    {
        var response = await http.GetAsync($"api/profiles/{profileId}/sessions/{date:yyyy-MM-dd}");
        response.EnsureSuccessStatusCode();
        return response.StatusCode == HttpStatusCode.NoContent ? null : await response.Content.ReadFromJsonAsync<WorkoutSessionDto>();
    }

    public Task<ApiResult<WorkoutSessionDto>> SaveSessionAsync(Guid profileId, DateOnly date, SaveWorkoutSessionRequest request) =>
        SendAsync<WorkoutSessionDto>(http.PutAsJsonAsync($"api/profiles/{profileId}/sessions/{date:yyyy-MM-dd}", request));

    public async Task DeleteSessionAsync(Guid profileId, DateOnly date) =>
        await http.DeleteAsync($"api/profiles/{profileId}/sessions/{date:yyyy-MM-dd}");

    public async Task<IReadOnlyList<WorkoutSessionSummaryDto>> GetHistoryAsync(Guid profileId, int take = 60) =>
        await http.GetFromJsonAsync<List<WorkoutSessionSummaryDto>>($"api/profiles/{profileId}/sessions?take={take}") ?? [];

    public async Task<IReadOnlyList<string>> GetLoggedExercisesAsync(Guid profileId) =>
        await http.GetFromJsonAsync<List<string>>($"api/profiles/{profileId}/progress/exercises") ?? [];

    public async Task<ExerciseProgressDto?> GetProgressAsync(Guid profileId, string exercise) =>
        await http.GetFromJsonAsync<ExerciseProgressDto>($"api/profiles/{profileId}/progress?exercise={Uri.EscapeDataString(exercise)}");

    // ---------- Food log ----------
    public async Task<IReadOnlyList<FoodEntryDto>> GetFoodsAsync(Guid profileId, DateOnly date) =>
        await http.GetFromJsonAsync<List<FoodEntryDto>>($"api/profiles/{profileId}/foods?date={date:yyyy-MM-dd}") ?? [];

    public Task<ApiResult<FoodEntryDto>> AddFoodAsync(Guid profileId, AddFoodEntryRequest request) =>
        SendAsync<FoodEntryDto>(http.PostAsJsonAsync($"api/profiles/{profileId}/foods", request));

    public async Task DeleteFoodAsync(Guid profileId, Guid entryId) =>
        await http.DeleteAsync($"api/profiles/{profileId}/foods/{entryId}");

    public async Task<IReadOnlyList<DailyIntakeDto>> GetDailyIntakeAsync(Guid profileId, DateOnly from, DateOnly to) =>
        await http.GetFromJsonAsync<List<DailyIntakeDto>>(
            $"api/profiles/{profileId}/foods/daily-totals?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}") ?? [];

    // ---------- Exercise library ----------
    public async Task<ExerciseSearchResultDto> SearchExercisesAsync(string? query, string? muscle, string? equipment, int skip = 0, int take = 24)
    {
        var url = $"api/exercises?skip={skip}&take={take}"
                  + (string.IsNullOrWhiteSpace(query) ? "" : $"&q={Uri.EscapeDataString(query)}")
                  + (string.IsNullOrWhiteSpace(muscle) ? "" : $"&muscle={Uri.EscapeDataString(muscle)}")
                  + (string.IsNullOrWhiteSpace(equipment) ? "" : $"&equipment={Uri.EscapeDataString(equipment)}");
        return await http.GetFromJsonAsync<ExerciseSearchResultDto>(url) ?? new([], 0);
    }

    public async Task<ExerciseFiltersDto> GetExerciseFiltersAsync() =>
        await http.GetFromJsonAsync<ExerciseFiltersDto>("api/exercises/filters") ?? new([], []);

    public async Task<ExerciseDetailDto?> GetExerciseAsync(string id)
    {
        var response = await http.GetAsync($"api/exercises/{Uri.EscapeDataString(id)}");
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ExerciseDetailDto>() : null;
    }

    // ---------- Helpers ----------
    private static async Task<ApiResult<T>> SendAsync<T>(Task<HttpResponseMessage> request)
    {
        try
        {
            var response = await request;
            return response.IsSuccessStatusCode
                ? new ApiResult<T>(await response.Content.ReadFromJsonAsync<T>(), null)
                : new ApiResult<T>(default, await ReadProblemAsync(response));
        }
        catch (HttpRequestException)
        {
            return new ApiResult<T>(default, "Could not reach the server. Please check your connection and try again.");
        }
    }

    /// <summary>Extracts a readable message from an RFC 7807 ProblemDetails / ValidationProblemDetails body.</summary>
    private static async Task<string> ReadProblemAsync(HttpResponseMessage response)
    {
        try
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = doc.RootElement;

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                return string.Join(" ", errors.EnumerateObject().SelectMany(e => e.Value.EnumerateArray()).Select(m => m.GetString()));

            if (root.TryGetProperty("detail", out var detail) && detail.GetString() is { Length: > 0 } message)
                return message;
        }
        catch (JsonException) { }

        return $"Something went wrong ({(int)response.StatusCode}). Please try again.";
    }
}
