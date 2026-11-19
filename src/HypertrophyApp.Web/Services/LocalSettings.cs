using Microsoft.JSInterop;

namespace HypertrophyApp.Web.Services;

/// <summary>
/// Per-browser settings kept in localStorage. HypertrophyApp has no accounts by design,
/// so the stored profile id is the only link between a visitor and their data.
/// </summary>
public sealed class LocalSettings(IJSRuntime js)
{
    private const string ProfileKey = "hypertrophyapp.profileId";
    private const string ThemeKey = "hypertrophyapp.theme";

    public async Task<Guid?> GetProfileIdAsync() =>
        Guid.TryParse(await GetAsync(ProfileKey), out var id) ? id : null;

    public Task SetProfileIdAsync(Guid id) => SetAsync(ProfileKey, id.ToString());

    public Task ClearProfileIdAsync() => SetAsync(ProfileKey, null);

    /// <returns><c>true</c>/<c>false</c> if the user picked a theme, <c>null</c> to follow the OS.</returns>
    public async Task<bool?> GetDarkModeAsync() => await GetAsync(ThemeKey) switch
    {
        "dark" => true,
        "light" => false,
        _ => null
    };

    public Task SetDarkModeAsync(bool dark) => SetAsync(ThemeKey, dark ? "dark" : "light");

    private async Task<string?> GetAsync(string key)
    {
        try { return await js.InvokeAsync<string?>("localStorage.getItem", key); }
        catch (JSException) { return null; } // Storage blocked (e.g. private mode).
    }

    private async Task SetAsync(string key, string? value)
    {
        try
        {
            if (value is null) await js.InvokeVoidAsync("localStorage.removeItem", key);
            else await js.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch (JSException) { /* Storage unavailable: works for this session only. */ }
    }
}
