#nullable disable
using HappyWheelsUtility.Menus;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace HappyWheelsUtility
{
    enum HWCharacter
    {
        Any,
        WheelchairGuy,
        SegwayGuy,
        IrresponsibleDad,
        EffectiveShopper,
        MopedCouple,
        LawnmowerMan,
        ExplorerGuy,
        SantaClaus,
        PogostickMan,
        IrresponsibleMom,
        HelicopterMan
    }

    internal class LevelData
    {
        public byte[] EncryptedData;
        public uint ID;
        public uint CreatorID;
        public string CreatorName;
        public string Name;
        public string Description;
        public string UploadDate;
        public uint PlayCount;
        public float Rating;
        public HWCharacter PlayableCharacter;

        public override string ToString()
        {
            return $@"Level Info:
Name: {Name}
ID: {ID}
Creator: {CreatorName} ({CreatorID})
Upload Date: {UploadDate}
Play Count: {PlayCount}
Rating: {Math.Round(Rating, 2)}";
        }
    }

    internal static class HWLULevelStuff
    {
        static HttpClient Client;
        static readonly Random Random = new();

        public static string DecompileLevel(string path, uint creatorId)
        {
            byte[] data = File.ReadAllBytes(path);
            return DecompileLevel(data, creatorId);
        }

        public static string DecompileLevel(LevelData lvlData)
        {
            return DecompileLevel(lvlData.EncryptedData, lvlData.CreatorID);
        }

        public static string DecompileLevel(byte[] data, uint creatorId)
        {
            byte[] decrypted = LevelEncryptor.DecryptByteArray(data, $"eatshit{creatorId}");
            byte[] decryptedNoheader = new ArraySegment<byte>(decrypted).Slice(2).ToArray();

            byte[] xmlData;

            using (var inputStream = new MemoryStream(decryptedNoheader))
            using (var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                deflateStream.CopyTo(outputStream);
                xmlData = outputStream.ToArray();
            }

            try
            {
                XDocument doc = XDocument.Parse(Encoding.ASCII.GetString(xmlData));
                doc.Root.Element("info").SetAttributeValue("e", "1");
                return doc.ToString();
            }
            catch (Exception ex2)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Tried to make XML editable but failed");
                Console.WriteLine("--------");
                Console.WriteLine(ex2.Message);
                Console.WriteLine("--------");
                Console.WriteLine("If the XML file isn't corrupted, just add 'e=\"1\"' as an attribute of the info element inside levelXML");
                Console.ForegroundColor = ConsoleColor.Gray;

                return Encoding.ASCII.GetString(xmlData);
            }
        }

        public static async Task<byte[]> FetchLevelBytes(uint id)
        {
            Client ??= new();

            Dictionary<string, string> values;
            FormUrlEncodedContent content;
            HttpResponseMessage responseLevel;

            values = new()
            {
                { "level_id", id.ToString()},
                { "action", "get_record"},
                { "ip_tracking", Random.Next(0, 1000000).ToString()}
            };
            content = new(values);

            responseLevel = await Client.PostAsync("https://totaljerkface.com/get_level.hw", content);
            if (!responseLevel.IsSuccessStatusCode)
            {
                return null;
            }

            return await responseLevel.Content.ReadAsByteArrayAsync();
        }

        public static async Task<LevelData> FetchLevel(uint id)
        {
            Client ??= new();

            Dictionary<string, string> values;
            FormUrlEncodedContent content;
            HttpResponseMessage responseLevel, responseInfo;

            values = new()
            {
                { "level_id", id.ToString()},
                { "action", "get_record"},
                { "ip_tracking", Random.Next(0, 1000000).ToString()}
            };
            content = new(values);

            responseLevel = await Client.PostAsync("https://totaljerkface.com/get_level.hw", content);
            if (!responseLevel.IsSuccessStatusCode)
            {
                return null;
            }

            values = new()
            {
                { "level_id", id.ToString()},
                { "action", "get_level"},
                { "ip_tracking", Random.Next(0, 1000000).ToString()}
            };
            content = new(values);

            responseInfo = await Client.PostAsync("https://totaljerkface.com/get_level.hw", content);
            if (!responseInfo.IsSuccessStatusCode)
            {
                return null;
            }

            try
            {
                XDocument doc = XDocument.Parse(responseInfo.Content.ReadAsStringAsync().Result);
                var root = doc.Root;
                var lv = root.Element("lv");
                return ParseLevelElement(lv, await responseLevel.Content.ReadAsByteArrayAsync());
            } catch (Exception ex)
            {
                Debug.WriteLine($"Exception when fetching level: {ex.Message}");
                return null;
            }
        }

        public static LevelData ParseLevelElement(XElement lv, byte[] data=null)
        {
            if (lv == null)
            {
                return null;
            }

            uint lvlId = uint.Parse(lv.Attribute("id").Value);
            string lvlName = lv.Attribute("ln").Value;
            uint creatorId = uint.Parse(lv.Attribute("ui").Value);
            string creatorName = lv.Attribute("un").Value;
            string uploadDate = lv.Attribute("dp").Value;
            uint playCount = uint.Parse(lv.Attribute("ps").Value);
            float rating = float.Parse(lv.Attribute("rg").Value);
            string desc = lv.Element("uc")?.Value;
            HWCharacter character = (HWCharacter)Math.Max(int.Parse(lv.Attribute("pc").Value), 0);

            return new LevelData()
            {
                ID = lvlId,
                EncryptedData = data,
                CreatorID = creatorId,
                CreatorName = creatorName,
                UploadDate = uploadDate,
                PlayCount = playCount,
                Name = lvlName,
                Rating = rating,
                Description = desc,
                PlayableCharacter = character
            };
        }

        public static async Task<List<LevelData>> GetLevelsPublishedBy(uint userId, uint page, SortLevelsBy sortby = SortLevelsBy.newest, Uploaded uploaded = Uploaded.anytime)
        {
            Client ??= new();
            List<LevelData> levels = new();

            Dictionary<string, string> values = new()
            {
                { "action", "get_pub_by_user"},
                { "page", page.ToString() },
                { "user_id", userId.ToString()},
                { "uploaded", Enum.GetName(uploaded) },
                { "sortby", Enum.GetName(sortby) },
            };
            FormUrlEncodedContent content = new(values);
            HttpResponseMessage response = await Client.PostAsync("https://totaljerkface.com/get_level.hw", content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            try
            {
                XDocument doc = XDocument.Parse(await response.Content.ReadAsStringAsync());
                foreach (XElement lv in doc.Root.Elements("lv"))
                {
                    levels.Add(ParseLevelElement(lv));
                }
            } catch (Exception ex) {
                Debug.WriteLine($"Exception when fetching levels by user: {ex.Message}");
            }

            return levels;
        }

        public static async Task<List<LevelData>> GetAllLevels(uint page, SortLevelsBy sortby, Uploaded uploaded)
        {
            Client ??= new();
            List<LevelData> levels = new();

            Dictionary<string, string> values = new()
            {
                { "action", "get_all"},
                { "page", page.ToString() },
                { "uploaded", Enum.GetName(uploaded) },
                { "sortby", Enum.GetName(sortby) },
            };
            FormUrlEncodedContent content = new(values);
            HttpResponseMessage response = await Client.PostAsync("https://totaljerkface.com/get_level.hw", content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            try
            {
                XDocument doc = XDocument.Parse(await response.Content.ReadAsStringAsync());
                foreach (XElement lv in doc.Root.Elements("lv"))
                {
                    levels.Add(ParseLevelElement(lv));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception when fetching all levels: {ex.Message}");
            }

            return levels;
        }

        public static async Task<List<LevelData>> SearchLevelsByName(string sterm, uint page, SortLevelsBy sortby, Uploaded uploaded)
        {
            Client ??= new();
            List<LevelData> levels = new();

            Dictionary<string, string> values = new()
            {
                { "action", "search_by_name"},
                { "page", page.ToString() },
                { "uploaded", Enum.GetName(uploaded) },
                { "sortby", Enum.GetName(sortby) },
                { "sterm", sterm }
            };
            FormUrlEncodedContent content = new(values);
            HttpResponseMessage response = await Client.PostAsync("https://totaljerkface.com/get_level.hw", content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            try
            {
                XDocument doc = XDocument.Parse(await response.Content.ReadAsStringAsync());
                foreach (XElement lv in doc.Root.Elements("lv"))
                {
                    levels.Add(ParseLevelElement(lv));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception when searching levels by name: {ex.Message}");
            }

            return levels;
        }

        public static async Task<List<LevelData>> GetLocalFavorites(SortLevelsBy sortby)
        {
            List<LevelData> levels = new();

            try
            {
                foreach (uint id in FavoriteLevels.LevelIDs)
                {
                    LevelData data = await FetchLevel(id);
                    if (data != null)
                    {
                        levels.Add(data);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception when getting local favorites: {ex.Message}\n{ex.StackTrace}");
                return levels;
            }

            return sortby switch
            {
                SortLevelsBy.newest => levels.OrderByDescending(lvl => lvl.UploadDate).ToList(),
                SortLevelsBy.oldest => levels.OrderBy(lvl => lvl.UploadDate).ToList(),
                SortLevelsBy.plays => levels.OrderByDescending(lvl => lvl.PlayCount).ToList(),
                SortLevelsBy.rating => levels.OrderByDescending(lvl => lvl.Rating).ToList(),
                _ => levels,
            };
        }

        public static async Task<List<LevelData>> SearchLevelsByUserName(string sterm, uint page, SortLevelsBy sortby, Uploaded uploaded)
        {
            Client ??= new();
            List<LevelData> levels = new();

            Dictionary<string, string> values = new()
            {
                { "action", "search_by_user"},
                { "page", page.ToString() },
                { "uploaded", Enum.GetName(uploaded) },
                { "sortby", Enum.GetName(sortby) },
                { "sterm", sterm }
            };
            FormUrlEncodedContent content = new(values);
            HttpResponseMessage response = await Client.PostAsync("https://totaljerkface.com/get_level.hw", content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            try
            {
                XDocument doc = XDocument.Parse(await response.Content.ReadAsStringAsync());
                foreach (XElement lv in doc.Root.Elements("lv"))
                {
                    levels.Add(ParseLevelElement(lv));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception when searching levels by username: {ex.Message}");
            }

            return levels;
        }
    }
}
