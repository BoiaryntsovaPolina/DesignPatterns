using System.Text;
using ClanBattle;

Console.OutputEncoding = Encoding.UTF8;
try { Console.InputEncoding = Encoding.UTF8; }
catch (IOException) { }

new MainMenu(new GameSettings()).Run();
