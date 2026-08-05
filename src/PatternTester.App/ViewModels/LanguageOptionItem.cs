namespace PatternTester.App.ViewModels;

public sealed class LanguageOptionItem
{
    public LanguageOptionItem(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    public string Code { get; }
    public string DisplayName { get; }

    public override string ToString() => DisplayName;
}
