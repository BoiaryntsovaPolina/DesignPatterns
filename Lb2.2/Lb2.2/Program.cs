using ClanBattle;

Console.OutputEncoding = System.Text.Encoding.UTF8;

IClanRenderer renderer = new ConsoleClanRenderer();

Console.Write("Введіть назву першого клану (Enter — назва за замовчуванням): ");
string? inputA = Console.ReadLine();
string nameA = string.IsNullOrWhiteSpace(inputA) ? "Клан Багряного Місяця" : inputA;

Clan clanA = new(nameA);
renderer.RenderReport(clanA);
renderer.RenderMap(clanA);

Clan clanB = new("Клан Крижаних Тіней");
renderer.RenderReport(clanB);
renderer.RenderMap(clanB);

// Демонстрація Singleton: один реєстр обслуговує обидва клани
Console.WriteLine($"\n>>> Главу клану «{clanA.Name}» переможено в бою! Визначаємо спадкоємницю...\n");

bool hasNewLeader = clanA.DefeatLeader();
Console.WriteLine(hasNewLeader
    ? ClanLeader.GetInstance().GetInfo(clanA)
    : $"Клан «{clanA.Name}» розгромлено повністю — живих воїнів не лишилось.");
