#nullable disable
using TextCopy;

namespace HappyWheelsUtility.Menus
{
    enum SortLevelsBy
    {
        newest,
        oldest,
        plays,
        rating
    }

    enum Uploaded
    {
        today,
        week,
        month,
        anytime
    }

    internal class LevelMenu : BaseMenu
    {
        int SelectedChoice = 0;
        List<LevelData> Levels;
        bool LevelsLoaded = false;
        static string[] Choices = { "List all levels", "Search by level name", "Search by username", "Get user's levels", "Back" };
        int SelectedFirstChoice = 0;

        public LevelMenu()
        {
            Levels = new();
        }

        private void DrawFirstMenu()
        {
            for (int i=0; i < Choices.Length; i++)
            {
                DrawFirstOption(i);
            }
        }

        private void DrawFirstOption(int i)
        {
            int y = i + 4;
            Console.CursorTop = y;
            Console.ForegroundColor = i == SelectedFirstChoice ? ConsoleColor.Blue : ConsoleColor.Gray;
            Console.WriteLine($"{(i == SelectedFirstChoice ? '>' : ' ')} {Choices[i]}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public override void Begin(BaseMenu previous)
        {
            SelectedFirstChoice = 0;
            DrawFirstMenu();
            int option = -1;
            while (option == -1)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.DownArrow:
                        SelectedFirstChoice++;
                        if (SelectedFirstChoice >= Choices.Length)
                        {
                            SelectedFirstChoice = 0;
                        }
                        DrawFirstMenu();
                        break;

                    case ConsoleKey.UpArrow:
                        SelectedFirstChoice--;
                        if (SelectedFirstChoice < 0)
                        {
                            SelectedFirstChoice = Choices.Length - 1;
                        }
                        DrawFirstMenu();
                        break;

                    case ConsoleKey.Enter:
                        option = SelectedFirstChoice;
                        break;
                }
            }

            Program.Clear();

            switch (option)
            {
                case 0: // list all
                    {
                        uint page = (uint)Math.Max(Program.PromptInt("Page number (first is 1): "), 1);
                        SortLevelsBy sort = Program.PromptEnum<SortLevelsBy>("Sort by (newest, oldest, plays, rating): ");
                        Uploaded uploaded = Program.PromptEnum<Uploaded>("Uploaded (today, week, month, anytime): ");
                        List<LevelData> received = HWLULevelStuff.GetAllLevels(page, sort, uploaded).Result;
                        if (received != null)
                        {
                            Levels.AddRange(received);
                            LevelsLoaded = true;
                            DrawLevelMenu();
                        } else
                        {
                            LevelsLoaded = false;
                        }
                    }
                    break;

                case 1: // level name search
                    {
                        string sterm = Program.Prompt("Search term: ");
                        uint page = (uint)Program.PromptInt("Page number (first is 1): ");
                        SortLevelsBy sort = Program.PromptEnum<SortLevelsBy>("Sort by (newest, oldest, plays, rating): ");
                        List<LevelData> received = HWLULevelStuff.SearchLevelsByName(sterm, page, sort, Uploaded.anytime).Result;
                        if (received != null)
                        {
                            Levels.AddRange(received);
                            LevelsLoaded = true;
                            DrawLevelMenu();
                        }
                        else
                        {
                            LevelsLoaded = false;
                        }
                    }
                    break;

                case 2: // user name search
                    {
                        string sterm = Program.Prompt("Search term: ");
                        uint page = (uint)Program.PromptInt("Page number (first is 1): ");
                        SortLevelsBy sort = Program.PromptEnum<SortLevelsBy>("Sort by (newest, oldest, plays, rating): ");
                        List<LevelData> received = HWLULevelStuff.SearchLevelsByUserName(sterm, page, sort, Uploaded.anytime).Result;
                        if (received != null)
                        {
                            Levels.AddRange(received);
                            LevelsLoaded = true;
                            DrawLevelMenu();
                        }
                        else
                        {
                            LevelsLoaded = false;
                        }
                    }
                    break;

                case 3:  // get user's levels
                    {
                        uint userId = (uint)Math.Max(Program.PromptInt("User ID: "), 1);
                        uint page = (uint)Program.PromptInt("Page number (first is 1): ");
                        List<LevelData> received = HWLULevelStuff.GetLevelsPublishedBy(userId, page).Result;
                        if (received != null)
                        {
                            Levels.AddRange(received);
                            LevelsLoaded = true;
                            DrawLevelMenu();
                        }
                        else
                        {
                            LevelsLoaded = false;
                        }
                    }
                    break;

                case 4:
                    Program.ChangeMenu(MenuStorage.MainMenu);
                    break;
            }
        }

        public override void End(BaseMenu next)
        {
            Levels.Clear();
            SelectedChoice = 0;
            LevelsLoaded = false;
        }

        public override void KeyPress(ConsoleKeyInfo key)
        {
            if (!LevelsLoaded) { return; }

            if (Levels.Count == 0)
            {
                Program.ChangeMenu(MenuStorage.Levels);
                return;
            }

            switch (key.Key)
            {
                case ConsoleKey.DownArrow:
                    Move(1);
                    break;

                case ConsoleKey.UpArrow:
                    Move(-1);
                    break;

                case ConsoleKey.Enter:
                    Interact();
                    break;

                case ConsoleKey.Escape:
                    Program.ChangeMenu(MenuStorage.Levels);
                    break;
            }
        }

        public void DrawLevelMenu()
        {
            Program.Clear();

            if (Levels.Count == 0)
            {
                Console.WriteLine("No levels found");
                Console.WriteLine("Press any key to go back");
            } else
            {
                Program.WriteColor("| Level name           | Creator              | Rating        | Date       \n", ConsoleColor.Cyan);
                for (int i = 0; i < Levels?.Count; i++)
                {
                    DrawLevelOption(i);
                }

                DrawLevelOption(SelectedChoice);
            }
        }

        protected void DrawLevelOption(int i)
        {
            if (!LevelsLoaded || Levels.Count == 0) { return; }
            LevelData entry = Levels[i];
            int y = i + 5;
            Console.CursorTop = y;
            Console.ForegroundColor = i == SelectedChoice ? ConsoleColor.Blue : ConsoleColor.Gray;
            Console.WriteLine($"{(i == SelectedChoice ? '>' : ' ')} {entry.Name,-20} | {entry.CreatorName,-20} | {Program.GetRatingString(entry.Rating),-13} | {entry.UploadDate}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void Move(int by)
        {
            if (!LevelsLoaded || Levels.Count == 0) { return; }
            int last = SelectedChoice;
            SelectedChoice += by;
            if (SelectedChoice >= Levels.Count)
            {
                SelectedChoice = 0;
            }
            else if (SelectedChoice < 0)
            {
                SelectedChoice = Levels.Count - 1;
            }
            DrawLevelOption(last);
            DrawLevelOption(SelectedChoice);
        }

        private void Interact()
        {
            LevelData data = Levels[SelectedChoice];
            Program.Clear();
            Program.WriteLevelData(data);
            if (data.ID == 1)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Cannot decompile Happy Green Hills as it's hardcoded");
                Console.ForegroundColor = ConsoleColor.Gray;
            } else
            {
                bool ans = Program.PromptYesNo("Wanna decompile this level? (Y/N): ");
                if (ans == true)
                {
                    try
                    {
                        byte[] encrypted = HWLULevelStuff.FetchLevelBytes(Levels[SelectedChoice].ID).Result;
                        if (encrypted == null)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("There was an error while fetching the level.");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        string decompiled = HWLULevelStuff.DecompileLevel(encrypted, data.CreatorID);
                        if (Program.PromptYesNo("Copy to clipboard instead of saving to file? (Y/N): "))
                        {
                            new Clipboard().SetText(decompiled);
                        }
                        else
                        {
                            string outPath = Program.Prompt("Input output path: ");
                            File.WriteAllText(outPath, decompiled);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Exception caught: {ex.Message}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }
            }

            Console.WriteLine("Press any key to go back");
            Console.ReadKey(true);
            DrawLevelMenu();
        }
    }
}
