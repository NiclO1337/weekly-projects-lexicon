Console.WriteLine("Welcome to the product list manager!\n");

Console.WriteLine("Enter products to save them, type \"exit\" to finish.");

List<string> products = [];

while (true)
{   
    Console.Write("Product: ");
    string? input = Console.ReadLine();

    if (input?.Trim().ToLower() == "exit")
    {
        break;
    }
    products.Add(input.Trim());
}

Console.WriteLine("\nProducts entered:\n");
Console.WriteLine(String.Join("\n", products));

Console.WriteLine("\nPress any key to continue...");
Console.ReadKey();