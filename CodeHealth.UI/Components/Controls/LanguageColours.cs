namespace CodeHealth.UI.Components.Controls;

public static class LanguageColours
{
    private static readonly string DefaultGrey = "#95a5a6";
    private static readonly Dictionary<string, string> languageToColour = new(StringComparer.OrdinalIgnoreCase)
    {
        { "C#", "#178600" },
        { "CSharp", "#178600" }, // identify cyclomatic_complexity.csharp.json as C#
        { "Java", "#b07219" },
        { "Javascript", "#f1e05a" },
        { "Typescript", "#3178c6" },
        { "Kotlin", "#A97BFF"},
        { "Other", "#D3D3D3" },
    };

    public static string GetColour(string language)
    {
        if (!languageToColour.ContainsKey(language))
        {
            return DefaultGrey;
        }

        return languageToColour[language];
    }

}
