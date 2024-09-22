using TextCopy;

namespace HappyWheelsUtility.Menus
{
    internal class DecompileMenu : BaseChoiceMenu
    {
        public DecompileMenu()
        {
            Choices = new string[] { "Decompile from ID", "Decompile from file", "Back" };
        }

        public override void Begin(BaseMenu previous)
        {
            SelectedChoice = 0;
            DrawMenu();
        }

        public override void End(BaseMenu next) {}

        protected override void Interact()
        {
            switch (SelectedChoice)
            {
                case 0:
                    {
                        Program.Clear();
                        uint id;
                        while (true)
                        {
                            string strId = Program.Prompt("Input level ID (or \"back\" to go back): ");
                            if (strId.ToLower() == "back") { Program.ChangeMenu(MenuStorage.Decompile); return; }
                            if (uint.TryParse(strId, out id))
                            {
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Invalid ID, try again.");
                            }
                        }

                        try
                        {
                            LevelData data = HWLULevelStuff.FetchLevel(id).Result;
                            if (data == null)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("There was an error while fetching the level.");
                                Console.ForegroundColor = ConsoleColor.Gray;
                            } else
                            {
                                string decompiled = HWLULevelStuff.DecompileLevel(data);
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
                        } catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Exception caught: {ex.Message}");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }

                        Console.WriteLine("Press any key to go back");
                        Console.ReadKey(true);
                        DrawMenu();
                    }
                    break;

                case 1:
                    {
                        Program.Clear();
                        string path;
                        uint id;
                        while (true)
                        {
                            path = Program.Prompt("Input level path (or \"back\" to go back): ");
                            if (path.ToLower() == "back") { Program.ChangeMenu(MenuStorage.Decompile); return; }
                            if (File.Exists(path))
                            {
                                break;
                            } else
                            {
                                Console.WriteLine("File doesn't exist.");
                            }
                        }

                        while (true)
                        {
                            string strId = Program.Prompt("Input level creator ID: ");
                            if (uint.TryParse(strId, out id))
                            {
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Invalid ID, try again.");
                            }
                        }

                        try
                        {
                            byte[] data = File.ReadAllBytes(path);
                            string decompiled = HWLULevelStuff.DecompileLevel(data, id);
                            if (Program.PromptYesNo("Copy to clipboard instead of saving to file? (Y/N): "))
                            {
                                new Clipboard().SetText(decompiled);
                            } else
                            {
                                string outPath = Program.Prompt("Input output path: ");
                                File.WriteAllText(outPath, decompiled);
                            }
                        } catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Exception caught: {ex.Message}");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }

                        Console.WriteLine("Press any key to go back");
                        Console.ReadKey(true);
                        DrawMenu();
                    }
                    break;

                case 2:
                    Program.ChangeMenu(MenuStorage.MainMenu);
                    break;
            }
        }
    }
}
