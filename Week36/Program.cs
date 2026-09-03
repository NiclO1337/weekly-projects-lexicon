using Week36;

Action[] levels =
[
    Level1.Run
];

Console.WriteLine("" +
    "------------------------------------" +
    "\n   Mini project week 36\n" +
    "------------------------------------");

Console.WriteLine($"There are {levels.Length} levels avalible to run here.");
Console.Write($"Enter 1 - {levels.Length}, or 'a' to run all: ");
var input = Console.ReadLine();

if (input == "a")
{
    for (int i = 0; i < levels.Length; i++)
    {
        Console.WriteLine("" +
            "------------------------------------" +
            $"\n   PRODUCT LIST MANAGER - LEVEL {i + 1}\n" +
            "------------------------------------");

        levels[i]();
        if (i < levels.Length - 1)
        {
            Console.WriteLine("\nPress ENTER for next level.");
            Console.ReadKey();
        }
        else
        {
            Console.WriteLine("\nPress any key to continue...");
        }
    }
}
else if (int.TryParse(input, out int n) && n >= 1 && n <= levels.Length)
{
    Console.WriteLine("" +
        "------------------------------------" +
        $"\n   PRODUCT LIST MANAGER - LEVEL {n}\n" +
        "------------------------------------");
    levels[n - 1]();
    Console.WriteLine("\nPress any key to continue...");
}
else
    Console.WriteLine("No such level.");

Console.ReadKey();