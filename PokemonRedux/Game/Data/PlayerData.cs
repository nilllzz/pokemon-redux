using Newtonsoft.Json;
using PokemonRedux.Game.Pokemons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PokemonRedux.Game.Data
{
    class PlayerData
    {
        public const int TEXT_SPEEDS = 3;

        // basic data
        public string name;
        public int[] badges;
        public int secondsPlayed;
        public int idNo;
        public int money;
        public PlayerItemData[] items;
        public PlayerItemData[] storedItems;

        // pokedex/gear
        public int[] pokedexSeen;
        public int[] pokedexCaught;
        public bool hasPokedex;
        public bool hasPokegear;
        public bool hasMapModule;
        public bool hasRadioModule;
        public bool hasUnownMode;
        public string[] contacts; // names of registered people

        // pokemon
        public PokemonSaveData[] pokemon;
        public HallOfFameEntryData[] hallOfFame;
        public int[] unownsCaught; // also stores the order in which unowns are caught
        public int activeBox; // selected box in the storage system
        public StorageBoxData[] boxes; // storage system boxes
        public MailData[] mails; // mails on the player's PC

        // map
        public string map;
        public float[] position;

        // progression
        public bool visitedKanto; // if the player has ever been to Kanto

        public void Save()
        {
            try
            {
                var saveContents = JsonConvert.SerializeObject(this);
                File.WriteAllText(SaveFile, saveContents, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to save game. Refer to inner for more details.", ex);
            }
        }

        private static string SaveFile
            => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "save.json");

        public static bool SaveFileExists()
            => File.Exists(SaveFile);

        public static PlayerData Load()
        {
            if (SaveFileExists())
            {
                try
                {
                    var saveContents = File.ReadAllText(SaveFile, Encoding.UTF8);
                    var data = JsonConvert.DeserializeObject<PlayerData>(saveContents);
                    return data;
                }
                catch (Exception ex)
                {
                    throw new FileLoadException("Failed to load save file. Refer to inner for more details.", ex);
                }
            }
            else
            {
                throw new FileNotFoundException("Tried to load non-existing save file.");
            }
        }

        public static PlayerData CreateNew(string name)
        {
            return new PlayerData
            {
                name = name,
                badges = new int[0],
                secondsPlayed = 0,
                idNo = GenerateIDNo(),
                money = 3000,
                items = new PlayerItemData[0],
                storedItems = new PlayerItemData[0],

                pokedexSeen = new int[0],
                pokedexCaught = new int[0],
                hasPokedex = false,
                hasPokegear = false,
                hasMapModule = false,
                hasRadioModule = false,
                hasUnownMode = false,
                contacts = new string[0],

                pokemon = new PokemonSaveData[0],
                hallOfFame = new HallOfFameEntryData[0],
                unownsCaught = new int[0],
                activeBox = 0,
                boxes = GenerateBoxes(),
                mails = new MailData[0],

                map = "Maps/Johto/NewBark/main.json",
                position = new float[] { 9, 0, 12 },

                visitedKanto = false,
            };
        }

        private static StorageBoxData[] GenerateBoxes()
        {
            var boxes = new List<StorageBoxData>();
            for (int i = 0; i < StorageBox.BOX_COUNT; i++)
            {
                boxes.Add(new StorageBoxData
                {
                    name = $"BOX{i + 1}",
                    pokemon = new PokemonSaveData[0]
                });
            }
            return boxes.ToArray();
        }

        private static int GenerateIDNo()
        {
            var r = new Random();
            return r.Next(0, ushort.MaxValue + 1);
        }
    }
}
