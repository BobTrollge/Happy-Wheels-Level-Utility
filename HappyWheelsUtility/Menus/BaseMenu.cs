namespace HappyWheelsUtility.Menus
{
    internal abstract class BaseMenu
    {
        public abstract void Begin(BaseMenu previous);
        public abstract void End(BaseMenu next);
        public abstract void KeyPress(ConsoleKeyInfo key);
    }
}
