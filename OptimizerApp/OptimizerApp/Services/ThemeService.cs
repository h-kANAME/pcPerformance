using System.Windows;

namespace OptimizerApp.Services;

public interface IThemeService
{
    bool IsDarkTheme { get; }
    void ToggleTheme();
    event EventHandler? ThemeChanged;
}

public sealed class ThemeService : IThemeService
{
    private bool _isDarkTheme = true;

    public bool IsDarkTheme => _isDarkTheme;

    public event EventHandler? ThemeChanged;

    public void ToggleTheme()
    {
        _isDarkTheme = !_isDarkTheme;
        ApplyTheme();
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyTheme()
    {
        var app = Application.Current;
        if (app?.Resources == null) return;

        var mergedDictionaries = app.Resources.MergedDictionaries;
        
        // Remove existing theme
        var existingTheme = mergedDictionaries.FirstOrDefault(d => 
            d.Source?.OriginalString.Contains("Theme.xaml") == true);
        
        if (existingTheme != null)
        {
            mergedDictionaries.Remove(existingTheme);
        }

        // Add new theme
        var themeUri = new Uri(
            _isDarkTheme 
                ? "Resources/Styles/RazerTheme.xaml" 
                : "Resources/Styles/LightTheme.xaml", 
            UriKind.Relative);

        var newTheme = new ResourceDictionary { Source = themeUri };
        mergedDictionaries.Insert(0, newTheme);
    }

    public ThemeService()
    {
        ApplyTheme();
    }
}
