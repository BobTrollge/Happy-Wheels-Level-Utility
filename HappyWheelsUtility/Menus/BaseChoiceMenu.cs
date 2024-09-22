#nullable disable

namespace HappyWheelsUtility.Menus
{
    internal abstract class BaseChoiceMenu : BaseMenu
    {
        public string[] Choices;
        public int SelectedChoice = 0;

        public override void Begin(BaseMenu previous)
        {
            SelectedChoice = 0;
        }

        public void DrawMenu()
        {
            Program.Clear();
            for (int i = 0; i < Choices?.Length; i++)
            {
                DrawOption(i);
            }

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public override void KeyPress(ConsoleKeyInfo key)
        {
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
            }
        }

        protected void DrawOption(int i)
        {
            int y = i + 4;
            Console.CursorTop = y;
            Console.ForegroundColor = i == SelectedChoice ? ConsoleColor.Blue : ConsoleColor.Gray;
            Console.WriteLine($"{(i == SelectedChoice ? '>' : ' ')} {Choices[i]}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void Move(int by)
        {
            int last = SelectedChoice;
            SelectedChoice += by;
            if (SelectedChoice >= Choices.Length)
            {
                SelectedChoice = 0;
            } else if (SelectedChoice < 0)
            {
                SelectedChoice = Choices.Length - 1;
            }
            DrawOption(last);
            DrawOption(SelectedChoice);
        }

        protected abstract void Interact();
    }
}
