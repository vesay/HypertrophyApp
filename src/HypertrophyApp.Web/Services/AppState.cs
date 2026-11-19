using HypertrophyApp.Shared.Dashboard;
using Microsoft.AspNetCore.Components;

namespace HypertrophyApp.Web.Services;

/// <summary>
/// Client-side session: which profile this browser uses and its cached dashboard,
/// shared by all pages so navigation doesn't refetch everything.
/// </summary>
public sealed class AppState(HypertrophyAppApiClient api, LocalSettings settings, NavigationManager nav)
{
    public Guid ProfileId { get; private set; }
    public DashboardDto? Dashboard { get; private set; }

    /// <summary>Today in the visitor's own time zone – logs are stored per local calendar day.</summary>
    public static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    public event Action? Changed;

    /// <summary>Loads the profile, or sends the visitor to onboarding if there is none.</summary>
    /// <returns><c>true</c> when <see cref="Dashboard"/> is available.</returns>
    public async Task<bool> EnsureLoadedAsync()
    {
        if (Dashboard is not null) return true;

        if (await settings.GetProfileIdAsync() is not { } id)
        {
            nav.NavigateTo("/start", replace: true);
            return false;
        }

        var dashboard = await api.GetDashboardAsync(id);
        if (dashboard is null)
        {
            // The stored profile no longer exists (e.g. it was deleted) – start over.
            await settings.ClearProfileIdAsync();
            nav.NavigateTo("/start", replace: true);
            return false;
        }

        ProfileId = id;
        Dashboard = dashboard;
        Changed?.Invoke();
        return true;
    }

    /// <summary>Reloads the dashboard after the profile or program changed.</summary>
    public async Task RefreshAsync()
    {
        Dashboard = null;
        await EnsureLoadedAsync();
    }

    public async Task UseProfileAsync(Guid id)
    {
        await settings.SetProfileIdAsync(id);
        Dashboard = null;
    }

    public async Task ForgetProfileAsync()
    {
        await settings.ClearProfileIdAsync();
        Dashboard = null;
        ProfileId = Guid.Empty;
    }
}
