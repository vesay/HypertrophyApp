using MudBlazor;

namespace HypertrophyApp.Web.Services;

/// <summary>Light/dark mode shared between the layout (which renders the theme) and the toggle button.</summary>
public sealed class ThemeState(LocalSettings settings)
{
    public bool IsDarkMode { get; private set; }

    public event Action? Changed;

    /// <summary>Uses the saved choice, or the operating system preference if the user never picked one.</summary>
    public async Task InitializeAsync(Func<Task<bool>> systemPreference)
    {
        IsDarkMode = await settings.GetDarkModeAsync() ?? await systemPreference();
        Changed?.Invoke();
    }

    public async Task ToggleAsync()
    {
        IsDarkMode = !IsDarkMode;
        await settings.SetDarkModeAsync(IsDarkMode);
        Changed?.Invoke();
    }

    /// <summary>
    /// Navy + energetic orange. The orange accent is validated against both surfaces
    /// (lightness band and ≥3:1 contrast) because the progress chart draws with it.
    /// </summary>
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#FF5A36",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#0F172A",
            Tertiary = "#10B981",
            Info = "#3B82F6",
            Success = "#10B981",
            Warning = "#F59E0B",
            Error = "#EF4444",
            Background = "#F5F6FA",
            BackgroundGray = "#EEF0F5",
            Surface = "#FFFFFF",
            AppbarBackground = "#0F172A",
            AppbarText = "#FFFFFF",
            DrawerBackground = "#0F172A",
            DrawerText = "#E2E8F0",
            DrawerIcon = "#94A3B8",
            TextPrimary = "#0F172A",
            TextSecondary = "#64748B",
            ActionDefault = "#64748B",
            LinesDefault = "#E2E8F0",
            LinesInputs = "#CBD5E1",
            TableLines = "#EEF0F5",
            TableHover = "#F8FAFC",
            Divider = "#E2E8F0",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#F4511E",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#F1F5F9",
            Tertiary = "#34D399",
            Info = "#60A5FA",
            Success = "#34D399",
            Warning = "#FBBF24",
            Error = "#F87171",
            Background = "#0B1120",
            BackgroundGray = "#1E293B",
            Surface = "#151E32",
            AppbarBackground = "#020617",
            AppbarText = "#F1F5F9",
            DrawerBackground = "#020617",
            DrawerText = "#E2E8F0",
            DrawerIcon = "#94A3B8",
            TextPrimary = "#F1F5F9",
            TextSecondary = "#94A3B8",
            ActionDefault = "#94A3B8",
            LinesDefault = "#1E293B",
            LinesInputs = "#334155",
            TableLines = "#1E293B",
            TableHover = "#1A2540",
            Divider = "#1E293B",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = ["Inter", "Roboto", "sans-serif"] },
            Button = new ButtonTypography { FontFamily = ["Inter", "Roboto", "sans-serif"], FontWeight = "700" },
        },
        LayoutProperties = new LayoutProperties { DefaultBorderRadius = "12px", AppbarHeight = "64px" },
    };
}
