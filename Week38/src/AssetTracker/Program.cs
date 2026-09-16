AssetTracker.UI.OutputTracker.Install();

Console.WriteLine("\r\n                                               _   __,----'~~~~~~~~~`-----.__\r\n                                        .  .    `//====-              ____,-'~`\r\n                        -.            \\_|// .   /||\\\\  `~~~~`---.___./\r\n                  ______-==.       _-~o  `\\/    |||  \\\\           _,'`\r\n            __,--'   ,=='||\\=_    ;_,_,/ _-'|-   |`\\   \\\\        ,'\r\n         _-'      ,='    | \\\\`.    '',/~7  /-   /  ||   `\\.     /\r\n       .'       ,'       |  \\\\  \\_  \"  /  /-   /   ||      \\   /\r\n      / _____  /         |     \\\\.`-_/  /|- _/   ,||       \\ /\r\n     ,-'     `-|--'~~`--_ \\     `==-/  `| \\'--===-'       _/`\r\n               '         `-|      /|    )-'\\~'      _,--\"'\r\n                           '-~^\\_/ |    |   `\\_   ,^             /\\\r\n                                /  \\     \\__   \\/~               `\\__\r\n                            _,-' _/'\\ ,-'~____-'`-/                 ``===\\\r\n                           ((->/'    \\|||' `.     `\\.  ,                _||\r\n             ./                       \\_     `\\      `~---|__i__i__\\--~'_/\r\n            <_n_                     __-^-_    `)  \\-.______________,-~'\r\n             `B'\\)                  ///,-'~`__--^-  |-------~~~~^'\r\n             /^>                           ///,--~`-\\\r\n            `  `                                       -Tua Xiong");

Console.WriteLine("\nWelcome to Dragon's hoard - guard your products well.\n");
Console.WriteLine("A treasure-keeper's ledger for tracking your wares:");
Console.WriteLine("add new stock to the hoard, search the vault, edit or");
Console.WriteLine("retire old items, and check your riches at a glance.\n");

AssetTracker.Services.IAssetRepository assetRepository = new AssetTracker.Services.InMemoryAssetRepository();
AssetTracker.Services.AssetService assetService = new(assetRepository);

// TODO: remove this temporary seed data once manual testing is done.
assetService.AddAsset(new AssetTracker.Models.Computer(0, "Apple", "MacBook Pro", DateTime.Today.AddYears(-1), 1800m, AssetTracker.Models.Office.Germany, AssetTracker.Models.ComputerType.Laptop));
assetService.AddAsset(new AssetTracker.Models.MobilePhone(0, "Samsung", "Galaxy S21", DateTime.Today.AddYears(-2).AddMonths(-8), 650m, AssetTracker.Models.Office.Germany));
assetService.AddAsset(new AssetTracker.Models.Computer(0, "Dell", "OptiPlex 7090", DateTime.Today.AddYears(-3).AddMonths(-1), 900m, AssetTracker.Models.Office.Sweden, AssetTracker.Models.ComputerType.Desktop));
assetService.AddAsset(new AssetTracker.Models.Tablet(0, "Apple", "iPad Air", DateTime.Today.AddMonths(-6), 700m, AssetTracker.Models.Office.Sweden));
assetService.AddAsset(new AssetTracker.Models.Computer(0, "Lenovo", "ThinkPad X1", DateTime.Today.AddYears(-2).AddMonths(-7), 1400m, AssetTracker.Models.Office.Usa, AssetTracker.Models.ComputerType.Laptop));
assetService.AddAsset(new AssetTracker.Models.MobilePhone(0, "Google", "Pixel 6", DateTime.Today.AddYears(-3).AddMonths(-2), 550m, AssetTracker.Models.Office.Usa));
assetService.AddAsset(new AssetTracker.Models.Tablet(0, "Samsung", "Galaxy Tab S8", DateTime.Today.AddYears(-1).AddMonths(-6), 600m, AssetTracker.Models.Office.Turkey));
assetService.AddAsset(new AssetTracker.Models.Computer(0, "HP", "EliteDesk 800", DateTime.Today.AddMonths(-8), 850m, AssetTracker.Models.Office.Turkey, AssetTracker.Models.ComputerType.Desktop));
assetService.AddAsset(new AssetTracker.Models.Tablet(0, "Microsoft", "Surface Pro 8", DateTime.Today.AddYears(-2).AddMonths(-10), 1100m, AssetTracker.Models.Office.Germany));
assetService.AddAsset(new AssetTracker.Models.MobilePhone(0, "OnePlus", "9 Pro", DateTime.Today.AddYears(-2).AddMonths(-6), 600m, AssetTracker.Models.Office.Sweden));

AssetTracker.UI.MainMenu.RunMainMenu(assetService);

AssetTracker.UI.ConsoleHelpers.Heading("Closing application");

Console.WriteLine("The hoard is secure and the ledger is closed... for now.\n" +
    "Farewell, treasure keeper!\n\n\n" +
    "                        \\`-\\`-._\r\n                         \\` )`. `-.__      ,\r\n      '' , . _       _,-._;'_,-`__,-'    ,/\r\n     : `. ` , _' :- '--'._ ' `------._,-;'\r\n      `- ,`- '            `--..__,,---'   hh\n\n");
