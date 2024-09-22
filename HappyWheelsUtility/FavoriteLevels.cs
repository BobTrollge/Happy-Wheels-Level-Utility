#nullable disable

using System.Text;

namespace HappyWheelsUtility
{
    internal class FavoriteLevels
    {
        public static List<uint> LevelIDs = new();

        static FavoriteLevels()
        {
            if (File.Exists("Favorites.txt"))
            {
                string file = File.ReadAllText("Favorites.txt");
                foreach (string line in file.Split('\n'))
                {
                    if (string.IsNullOrWhiteSpace(line)) { continue; }
                    uint id = uint.Parse(line.Trim());
                    if (!LevelIDs.Contains(id)) { LevelIDs.Add(id); }
                }
            }
        }

        public static void Favorite(uint id)
        {
            if (!LevelIDs.Contains(id))
            {
                LevelIDs.Add(id);
                WriteToDisk();
            }
        }

        public static void Unfavorite(uint id)
        {
            if (LevelIDs.Contains(id))
            {
                LevelIDs.Remove(id);
                WriteToDisk();
            }
        }

        private static void WriteToDisk()
        {
            StringBuilder bob = new();
            foreach (uint id in LevelIDs)
            {
                bob.Append($"{id}\n");
            }
            File.WriteAllText("Favorites.txt", bob.ToString());
        }
    }
}
