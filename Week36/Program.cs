Console.WriteLine("" +
    "------------------------------------" +
    "\n   PRODUCT LIST MANAGER - LEVEL 1\n" +
    "------------------------------------");

Console.WriteLine("\nEnter products to save them, type \"exit\" to finish.");

List<string> products = [];

while (true)
{
    Console.Write("Product: ");
    string? input = Console.ReadLine();

    if (input?.Trim().ToLower() == "exit")
    {
        break;
    }
    if (input.Trim().Length > 0) { products.Add(input.Trim()); }
}

Console.WriteLine("\nProducts entered:\n");
Console.WriteLine(String.Join("\n", products));

Console.WriteLine("\nPress any key to continue...");
Console.ReadKey();