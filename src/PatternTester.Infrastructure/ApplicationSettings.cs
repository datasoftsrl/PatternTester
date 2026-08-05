namespace PatternTester.Infrastructure;

public sealed class ApplicationSettings
{
    // "last" = ripristina l'ultima configurazione
    // "defaults" = usa i valori predefiniti
    public string StartupMode { get; set; } = "last";

    // "dark" = tema scuro, "light" = tema chiaro
    public string Theme { get; set; } = "dark";

    // Codice lingua esterna, ad esempio "it" o "en".
    public string LanguageCode { get; set; } = "it";

    // Mostra automaticamente il pattern all'avvio
    public bool AutoShowPattern { get; set; } = false;

    // "on_exit" = salva alla chiusura
    // "on_change" = salva ad ogni modifica
    // "manual" = salva solo con il pulsante Salva
    public string SaveMode { get; set; } = "on_exit";

    // Valori predefiniti dell'applicazione
   
    public int DefaultPatternIndex { get; set; } = 0;
    public int DefaultMonitor { get; set; } = 1;
    public int DefaultHorizontalScreens { get; set; } = 3;
    public int DefaultVerticalScreens { get; set; } = 3;
}
