#nullable disable
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
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
        readonly List<LevelData> Levels = new();
        bool LevelsLoaded = false;
        static readonly string[] Choices = { "List all levels", "Search by level name", "Search by username", "Get user's levels", "Get local favorites", "Back" };
        int SelectedFirstChoice = 0;

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

                case 4:  // get local favorites
                    {
                        SortLevelsBy sort = Program.PromptEnum<SortLevelsBy>("Sort by (newest, oldest, plays, rating): ");
                        List<LevelData> received = HWLULevelStuff.GetLocalFavorites(sort).Result;
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

                case 5:  // back
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

        private void DrawInteractionMenu(int chosen, string[] options)
        {
            for (int i = 0; i < options.Length; i++)
            {
                int y = 14 + i;
                Console.SetCursorPosition(0, y);
                Console.WriteLine(new string(' ', 15));
                Console.SetCursorPosition(0, y);
                Console.ForegroundColor = i == chosen ? ConsoleColor.Blue : ConsoleColor.Gray;
                Console.WriteLine($"{(i == chosen ? '>' : ' ')} {options[i]}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        private void Interact()
        {
            LevelData data = Levels[SelectedChoice];
            Program.Clear();
            Program.WriteLevelData(data);

            int chosen = 0;
            string[] options;
            if (data.ID == 1)
            {
                options = new string[]{ "Favorite", "Back" };
            } else
            {
                options = new string[] { "Play", "Decompile", "Favorite", "Back" };
            }

            if (FavoriteLevels.LevelIDs.Contains(Levels[SelectedChoice].ID))
            {
                int index = Array.IndexOf(options, "Favorite");
                options[index] = "Unfavorite";
            }

            DrawInteractionMenu(chosen, options);

            bool run = true;
            while (run)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.DownArrow:
                        chosen++;
                        if (chosen >= options.Length)
                        {
                            chosen = 0;
                        }
                        DrawInteractionMenu(chosen, options);
                        break;

                    case ConsoleKey.UpArrow:
                        chosen--;
                        if (chosen < 0)
                        {
                            chosen = options.Length-1;
                        }
                        DrawInteractionMenu(chosen, options);
                        break;

                    case ConsoleKey.Enter:
                        switch (options[chosen])
                        {
                            case "Play":
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = $"https://totaljerkface.com/happy_wheels.tjf?level_id={Levels[SelectedChoice].ID}",
                                    UseShellExecute = true
                                });
                                break;

                            case "Decompile":
                                Program.Clear();
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
                                Console.WriteLine("Press any key to go back");
                                Console.ReadKey(true);
                                run = false;
                                break;

                            case "Favorite":
                                FavoriteLevels.Favorite(Levels[SelectedChoice].ID);
                                options[chosen] = "Unfavorite";
                                DrawInteractionMenu(chosen, options);
                                break;

                            case "Unfavorite":
                                FavoriteLevels.Unfavorite(Levels[SelectedChoice].ID);
                                options[chosen] = "Favorite";
                                DrawInteractionMenu(chosen, options);
                                break;

                            case "Back":
                                run = false;
                                break;
                        }
                        break;
                }
            }

            DrawLevelMenu();
        }
    }
}
