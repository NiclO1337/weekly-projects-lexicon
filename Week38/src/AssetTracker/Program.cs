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

//Console.WriteLine();
//AssetTracker.UI.SlowConsole.WriteLineSlow("Welcome to Dragon's hoard - guard your products well.");
//Console.WriteLine();
//AssetTracker.UI.SlowConsole.WriteLineSlow("A treasure-keeper's ledger for tracking your wares:");
//AssetTracker.UI.SlowConsole.WriteLineSlow("add new stock to the hoard, search the vault, edit or");
//AssetTracker.UI.SlowConsole.WriteLineSlow("retire old items, and check your riches at a glance.");
//Console.WriteLine();

string dataFilePath = System.IO.Path.Combine(AssetTracker.Services.AppPaths.DataDirectory, "assets.json");

AssetTracker.Services.IAssetRepository assetRepository;
try
{
    assetRepository = new AssetTracker.Services.JsonAssetRepository(dataFilePath);
}
catch (AssetTracker.Exceptions.InvalidAssetDataException ex)
{
    AssetTracker.UI.ConsoleHelpers.DisplayErrorMessage(
        $"{ex.Message} Starting with an empty asset list - fix or delete the file to recover its contents.");
    assetRepository = new AssetTracker.Services.JsonAssetRepository(dataFilePath, skipLoad: true);
}

AssetTracker.Services.AssetService assetService = new(assetRepository);

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
