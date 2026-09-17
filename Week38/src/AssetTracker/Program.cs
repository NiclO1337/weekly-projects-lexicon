AssetTracker.UI.OutputTracker.Install();

Console.WriteLine("\r\n                                               _   __,----'~~~~~~~~~`-----.__\r\n                                        .  .    `//====-              ____,-'~`\r\n                        -.            \\_|// .   /||\\\\  `~~~~`---.___./\r\n                  ______-==.       _-~o  `\\/    |||  \\\\           _,'`\r\n            __,--'   ,=='||\\=_    ;_,_,/ _-'|-   |`\\   \\\\        ,'\r\n         _-'      ,='    | \\\\`.    '',/~7  /-   /  ||   `\\.     /\r\n       .'       ,'       |  \\\\  \\_  \"  /  /-   /   ||      \\   /\r\n      / _____  /         |     \\\\.`-_/  /|- _/   ,||       \\ /\r\n     ,-'     `-|--'~~`--_ \\     `==-/  `| \\'--===-'       _/`\r\n               '         `-|      /|    )-'\\~'      _,--\"'\r\n                           '-~^\\_/ |    |   `\\_   ,^             /\\\r\n                                /  \\     \\__   \\/~               `\\__\r\n                            _,-' _/'\\ ,-'~____-'`-/                 ``===\\\r\n                           ((->/'    \\|||' `.     `\\.  ,                _||\r\n             ./                       \\_     `\\      `~---|__i__i__\\--~'_/\r\n            <_n_                     __-^-_    `)  \\-.______________,-~'\r\n             `B'\\)                  ///,-'~`__--^-  |-------~~~~^'\r\n             /^>                           ///,--~`-\\\r\n            `  `                                       -Tua Xiong");

Console.WriteLine("\nWelcome to Dragon's hoard - guard your products well.\n");
Console.WriteLine("A treasure-keeper's ledger for tracking your wares:");
Console.WriteLine("add new stock to the hoard, search the vault, edit or");
Console.WriteLine("retire old items, and check your riches at a glance.\n");

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

Console.WriteLine("The hoard is secure and the ledger is closed... for now.\n" +
    "Farewell, treasure keeper!\n\n\n" +
    "                        \\`-\\`-._\r\n                         \\` )`. `-.__      ,\r\n      '' , . _       _,-._;'_,-`__,-'    ,/\r\n     : `. ` , _' :- '--'._ ' `------._,-;'\r\n      `- ,`- '            `--..__,,---'   hh\n\n");
