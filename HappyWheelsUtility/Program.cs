#nullable disable

using HappyWheelsUtility.Menus;
using System.Runtime.InteropServices;
using System.Text;

namespace HappyWheelsUtility
{
    internal class MenuStorage
    {
        public static MainMenu MainMenu = new();
        public static DecompileMenu Decompile = new();
        public static LevelMenu Levels = new();
    }

    internal class Program
    {
        public static string Version = "1.0";
        static BaseMenu Current = null;
        public static bool Run = true;

        static void Main()
        {
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            Console.CursorVisible = false;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = $"Happy Wheels Level Utility {Version}";
            Clear();

            ChangeMenu(MenuStorage.MainMenu);

            while (Run)
            {
                Current?.KeyPress(Console.ReadKey(true));
            }
        }

        public static void Clear()
        {
            string name = $"Happy Wheels Level Utility {Version}";

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"┌{new string('─', name.Length + 2)}┐");
            Console.WriteLine($"│ {name} │");
            Console.WriteLine($"└{new string('─', name.Length + 2)}┘");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static string Prompt(string question)
        {
            Console.Write(question);
            return Console.ReadLine();
        }

        public static int PromptInt(string question)
        {
            while (true)
            {
                Console.Write(question);
                string ans = Console.ReadLine();
                if (int.TryParse(ans, out int ansI))
                {
                    return ansI;
                } else
                {
                    Console.WriteLine("Invalid answer.");
                }
            }
        }

        public static T PromptEnum<T>(string question)
        {
            while (true)
            {
                Console.Write(question);
                string ans = Console.ReadLine();
                if (Enum.TryParse(typeof(T), ans, true, out object thing))
                {
                    return (T)thing;
                }
                else
                {
                    Console.WriteLine("Invalid option.");
                }
            }
        }

        public static bool PromptYesNo(string question)
        {
            Console.Write(question);
            string ans = Console.ReadLine();
            ans = ans.ToLower();
            return ans == "y" || ans == "yes";
        }

        public static void ChangeMenu(BaseMenu menu)
        {
            Clear();
            BaseMenu old = Current;
            Current?.End(menu);
            Current = menu;
            menu?.Begin(old);
        }

        public static string GetRatingString(float rating)
        {
            int stars = (int)rating;
            return $"{new string('●', stars) + new string('○', 5 - stars)} ({Math.Round(rating, 2)})";
        }

        public static void Quit()
        {
            Run = false;
            Console.Clear();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { Console.CursorVisible = true; }
        }

        public static void WriteColor(string text, ConsoleColor color)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = old;
        }

        public static string SplitIntoWords(string str)
        {
            StringBuilder bob = new();
            foreach (char c in str)
            {
                if (char.IsUpper(c) && bob.Length > 0)
                {
                    bob.Append(' ');
                }
                bob.Append(c);
            }
            return bob.ToString();
        }

        public static void WriteLevelData(LevelData data)
        {
            WriteColor("┌──────────────────────────────┐\n", ConsoleColor.DarkBlue);
            WriteColor("│", ConsoleColor.DarkBlue); WriteColor(" Name: ", ConsoleColor.White); Console.Write($"{data.Name,-23}"); WriteColor("│\n", ConsoleColor.DarkBlue);
            WriteColor("│", ConsoleColor.DarkBlue); WriteColor(" ID: ", ConsoleColor.White); Console.Write($"{data.ID,-25}"); WriteColor("│\n", ConsoleColor.DarkBlue);
            WriteColor("│", ConsoleColor.DarkBlue); WriteColor(" Creator: ", ConsoleColor.White); Console.Write($"{data.CreatorName,-20}"); WriteColor("│\n", ConsoleColor.DarkBlue);
            WriteColor("│", ConsoleColor.DarkBlue); WriteColor(" Creator ID: ", ConsoleColor.White); Console.Write($"{data.CreatorID,-17}"); WriteColor("│\n", ConsoleColor.DarkBlue);
            WriteColor("│", ConsoleColor.DarkBlue); WriteColor(" Uploaded: ", ConsoleColor.White); Console.Write($"{data.UploadDate,-19}"); WriteColor("│\n", ConsoleColor.DarkBlue);
            WriteColor("│", ConsoleColor.DarkBlue); WriteColor(" Play Count: ", ConsoleColor.White); Console.Write($"{data.PlayCount,-17}"); WriteColor("│\n", ConsoleColor.DarkBlue);
            WriteColor("│", ConsoleColor.DarkBlue); WriteColor(" Rating: ", ConsoleColor.White); WriteColor($"{GetRatingString(data.Rating),-21}", ConsoleColor.DarkYellow); WriteColor("│\n", ConsoleColor.DarkBlue);
            WriteColor("│", ConsoleColor.DarkBlue); WriteColor(" Character: ", ConsoleColor.White); Console.Write($"{SplitIntoWords(Enum.GetName(data.PlayableCharacter)),-18}"); WriteColor("│\n", ConsoleColor.DarkBlue);
            WriteColor("└──────────────────────────────┘\n", ConsoleColor.DarkBlue);
        }
    }
}
