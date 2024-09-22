namespace HappyWheelsUtility.Menus
{
    internal class MainMenu : BaseChoiceMenu
    {
        public MainMenu()
        {
            Choices = new string[]{ "View level metadata", "Decompile level", "View levels page", "Quit" };
        }

        public override void Begin(BaseMenu previous)
        {
            if (previous == MenuStorage.Decompile)
            {
                SelectedChoice = 1;
            } else if (previous == MenuStorage.Levels)
            {
                SelectedChoice = 2;
            } else
            {
                SelectedChoice = 0;
            }
            DrawMenu();
        }

        public override void End(BaseMenu next) {}

        protected override void Interact()
        {
            switch (SelectedChoice)
            {
                case 0:
                    {
                        bool leave = false;
                        Program.Clear();
                        uint id = 0;
                        while (true)
                        {
                            string strId = Program.Prompt("Input level ID (or \"back\" to go back): ");
                            
                            if (strId.ToLower() == "back") { leave = true; break; } 

                            if (uint.TryParse(strId, out id))
                            {
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Invalid ID, try again.");
                            }
                        }

                        if (leave) { DrawMenu(); break; }

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
                                Program.WriteLevelData(data);
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
                        DrawMenu();
                    }
                    break;

                case 1:
                    Program.ChangeMenu(MenuStorage.Decompile);
                    break;

                case 2:
                    Program.ChangeMenu(MenuStorage.Levels);
                    break;

                case 3:
                    Program.Quit();
                    break;
            }
        }
    }
}
