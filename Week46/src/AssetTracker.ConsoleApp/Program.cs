using AssetTracker.ConsoleApp.UI;
using AssetTracker.Core.Exceptions;
using AssetTracker.Core.Services;
using AssetTracker.Infrastructure.Currency;
using AssetTracker.Infrastructure.Persistence;

// Must run before the first Spectre call: Spectre binds to Console.Out the first time it is used.
SlowConsole.Install();

foreach (string line in new[]
{
    "",
    "                                               _   __,----'~~~~~~~~~`-----.__",
    "                                        .  .    `//====-              ____,-'~`",
    "                        -.            \\_|// .   /||\\\\  `~~~~`---.___./",
    "                  ______-==.       _-~o  `\\/    |||  \\\\           _,'`",
    "            __,--'   ,=='||\\=_    ;_,_,/ _-'|-   |`\\   \\\\        ,'",
    "         _-'      ,='    | \\\\`.    '',/~7  /-   /  ||   `\\.     /",
    "       .'       ,'       |  \\\\  \\_  \"  /  /-   /   ||      \\   /",
    "      / _____  /         |     \\\\.`-_/  /|- _/   ,||       \\ /",
    "     ,-'     `-|--'~~`--_ \\     `==-/  `| \\'--===-'       _/`",
    "               '         `-|      /|    )-'\\~'      _,--\"'",
    "                           '-~^\\_/ |    |   `\\_   ,^             /\\",
    "                                /  \\     \\__   \\/~               `\\__",
    "                            _,-' _/'\\ ,-'~____-'`-/                 ``===\\",
    "                           ((->/'    \\|||' `.     `\\.  ,                _||",
    "             ./                       \\_     `\\      `~---|__i__i__\\--~'_/",
    "            <_n_                     __-^-_    `)  \\-.______________,-~'",
    "             `B'\\)                  ///,-'~`__--^-  |-------~~~~^'",
    "             /^>                           ///,--~`-\\",
    "            `  `                                       -Tua Xiong",
})
{
    ConsoleMessage.WriteColoredLine(line, Theme.Accent);
}

//ConsoleMessage.WriteColoredSlow("\nWelcome to Dragon's hoard - guard your products well.\n", Theme.Accent);
//ConsoleMessage.WriteColoredSlow("\nA treasure-keeper's ledger for tracking your wares:\n", Theme.Accent);
//ConsoleMessage.WriteColoredSlow("add new stock to the hoard, search the vault, edit or\n", Theme.Accent);
//ConsoleMessage.WriteColoredSlow("retire old items, and check your riches at a glance.\n", Theme.Accent);

using var context = new AppDbContext();

ConsoleMessage.NewLine();
ConsoleMessage.WriteColoredLine("Connecting to database...", Theme.Accent);
try
{
    DatabaseInitializer.Initialize(context);
}
catch (DataStoreException ex)
{
    ConsoleMessage.DisplayErrorMessage(ex.Message);
    ConsoleMessage.DisplayErrorMessage("Is SQL Server Express running, and is APP_DB_CONNECTION correct if you have set it?");
    ConsoleInput.Pause();
    return 1;
}

ConsoleMessage.DisplaySuccessMessage("Database ready.");

ConsoleMessage.NewLine();
ConsoleMessage.WriteColoredLine("Fetching exchange rates...", Theme.Accent);
var currencyProvider = new ApiCurrencyProvider();
switch (currencyProvider.Source)
{
    case CurrencyRateSource.TodayCache:
        ConsoleMessage.DisplaySuccessMessage($"Using today's cached exchange rates ({currencyProvider.CachedDate}).");
        break;
    case CurrencyRateSource.LiveApi:
        ConsoleMessage.DisplaySuccessMessage("Fetched the latest exchange rates.");
        break;
    case CurrencyRateSource.StaleCache:
        ConsoleMessage.DisplayWarningMessage($"Could not reach the exchange rate service - using cached rates from {currencyProvider.CachedDate}.");
        break;
    case CurrencyRateSource.Hardcoded:
        ConsoleMessage.DisplayWarningMessage("Could not reach the exchange rate service and no cached rates were found - using built-in rates.");
        break;
}

ConsoleInput.Pause();

var assetRepository = new EfAssetRepository(context);
var officeRepository = new EfOfficeRepository(context);
var assetService = new AssetService(assetRepository, officeRepository, currencyProvider);
var reportService = new ReportService(assetRepository, officeRepository, currencyProvider);

new MainMenu(new AssetBrowser(assetService), new AssetMenu(assetService), new ReportMenu(reportService)).Run();

ConsoleMessage.Heading("Closing application");

//ConsoleMessage.WriteColoredSlow("The hoard is secure and the ledger is closed... for now.\n\n", Theme.Accent);
//ConsoleMessage.WriteColoredSlow("Farewell, treasure keeper!\n\n\n", Theme.Accent);

foreach (string line in new[]
{
    "                        \\`-\\`-._",
    "                         \\` )`. `-.__      ,",
    "      '' , . _       _,-._;'_,-`__,-'    ,/",
    "     : `. ` , _' :- '--'._ ' `------._,-;'",
    "      `- ,`- '            `--..__,,---'   hh",
})
{
    ConsoleMessage.WriteColoredLine(line, Theme.Accent);
}

ConsoleMessage.NewLine();
return 0;
