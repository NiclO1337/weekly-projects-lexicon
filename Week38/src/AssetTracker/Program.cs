AssetTracker.UI.OutputTracker.Install();

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
    Console.WriteLine(line);
}

AssetTracker.UI.SlowConsole.WriteLineSlow("\nWelcome to Dragon's hoard - guard your products well.");
Console.WriteLine();
AssetTracker.UI.SlowConsole.WriteLineSlow("A treasure-keeper's ledger for tracking your wares:");
AssetTracker.UI.SlowConsole.WriteLineSlow("add new stock to the hoard, search the vault, edit or");
AssetTracker.UI.SlowConsole.WriteLineSlow("retire old items, and check your riches at a glance.\n");

Console.WriteLine("\nPress any key to continue to main menu...\n");
Console.ReadKey();
const int FetchDelayMs = 800;

AssetTracker.UI.SlowConsole.WriteLineSlow("Fetching data...");
Thread.Sleep(FetchDelayMs);
string dataFilePath = System.IO.Path.Combine(AssetTracker.Services.AppPaths.DataDirectory, "assets.json");

AssetTracker.Services.IAssetRepository assetRepository;
try
{
    assetRepository = new AssetTracker.Services.JsonAssetRepository(dataFilePath);
    int assetCount = assetRepository.GetAll().Count;
    AssetTracker.UI.ConsoleHelpers.DisplaySuccessMessage(
        $"Loaded {assetCount} {AssetTracker.Services.FormatHelpers.Pluralize(assetCount, "asset", "assets")} from {dataFilePath}.");
}
catch (AssetTracker.Exceptions.InvalidAssetDataException ex)
{
    AssetTracker.UI.ConsoleHelpers.DisplayErrorMessage(
        $"{ex.Message} Starting with an empty asset list - fix or delete the file to recover its contents.");
    assetRepository = new AssetTracker.Services.JsonAssetRepository(dataFilePath, skipLoad: true);
}

AssetTracker.UI.SlowConsole.WriteLineSlow("\nFetching exchange rates...");
Thread.Sleep(FetchDelayMs);
string ratesFilePath = System.IO.Path.Combine(AssetTracker.Services.AppPaths.DataDirectory, "exchangeRates.json");
AssetTracker.Services.ApiCurrencyProvider currencyProvider = new(ratesFilePath);

switch (currencyProvider.Source)
{
    case AssetTracker.Services.CurrencyRateSource.TodayCache:
        AssetTracker.UI.ConsoleHelpers.DisplaySuccessMessage($"Using today's cached exchange rates ({currencyProvider.CachedDate}).");
        break;
    case AssetTracker.Services.CurrencyRateSource.LiveApi:
        AssetTracker.UI.ConsoleHelpers.DisplaySuccessMessage("Fetched the latest exchange rates from the API.");
        break;
    case AssetTracker.Services.CurrencyRateSource.StaleCache:
        AssetTracker.UI.ConsoleHelpers.DisplayWarningMessage(
            $"Could not reach the exchange rate API - using cached rates from {currencyProvider.CachedDate} instead.");
        break;
    case AssetTracker.Services.CurrencyRateSource.Hardcoded:
        AssetTracker.UI.ConsoleHelpers.DisplayWarningMessage(
            "Could not reach the exchange rate API and no cached rates were found - using built-in hardcoded rates instead.");
        break;
}

AssetTracker.Services.AssetService assetService = new(assetRepository, currencyProvider);

AssetTracker.UI.MainMenu.RunMainMenu(assetService);

AssetTracker.UI.ConsoleHelpers.Heading("Closing application");

AssetTracker.UI.SlowConsole.WriteLineSlow("The hoard is secure and the ledger is closed... for now.");
Console.WriteLine();
AssetTracker.UI.SlowConsole.WriteLineSlow("Farewell, treasure keeper!");
Console.WriteLine();
Console.WriteLine();

foreach (string line in new[]
{
    "                        \\`-\\`-._",
    "                         \\` )`. `-.__      ,",
    "      '' , . _       _,-._;'_,-`__,-'    ,/",
    "     : `. ` , _' :- '--'._ ' `------._,-;'",
    "      `- ,`- '            `--..__,,---'   hh",
})
{
    Console.WriteLine(line);
}
Console.WriteLine();
