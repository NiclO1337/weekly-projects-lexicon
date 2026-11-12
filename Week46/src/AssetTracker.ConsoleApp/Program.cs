using AssetTracker.ConsoleApp.UI;

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

ConsoleMessage.WriteColoredSlow("\nWelcome to Dragon's hoard - guard your products well.\n", Theme.Accent);
ConsoleMessage.WriteColoredSlow("\nA treasure-keeper's ledger for tracking your wares:\n", Theme.Accent);
ConsoleMessage.WriteColoredSlow("add new stock to the hoard, search the vault, edit or\n", Theme.Accent);
ConsoleMessage.WriteColoredSlow("retire old items, and check your riches at a glance.\n", Theme.Accent);
ConsoleInput.Pause();

new MainMenu().Run();

ConsoleMessage.Heading("Closing application");

ConsoleMessage.WriteColoredSlow("The hoard is secure and the ledger is closed... for now.\n\n", Theme.Accent);
ConsoleMessage.WriteColoredSlow("Farewell, treasure keeper!\n\n\n", Theme.Accent);

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
