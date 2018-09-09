using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace PokemonRedux.Game.Data
{
    class KeyboardData
    {
        public string aButton;
        public string bButton;
        public string startButton;
        public string selectButton;

        [JsonIgnore]
        public Keys AButtonKey => DataHelper.ParseEnum<Keys>(aButton);
        [JsonIgnore]
        public Keys BButtonKey => DataHelper.ParseEnum<Keys>(bButton);
        [JsonIgnore]
        public Keys StartButtonKey => DataHelper.ParseEnum<Keys>(startButton);
        [JsonIgnore]
        public Keys SelectButtonKey => DataHelper.ParseEnum<Keys>(selectButton);

        private static string MapFile
            => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keyboard_map.json");

        private static bool MapFileExists
            => File.Exists(MapFile);

        // creates default keyboard mapping if none exists, and loads it
        public static KeyboardData Load()
        {
            if (!MapFileExists)
            {
                // save default mapping
                var data = new KeyboardData
                {
                    aButton = "Z",
                    bButton = "X",
                    startButton = "Enter",
                    selectButton = "Space",
                };
                Save(data);
                return data;
            }
            else
            {
                var json = File.ReadAllText(MapFile, Encoding.UTF8);
                var data = JsonConvert.DeserializeObject<KeyboardData>(json);
                return data;
            }
        }

        private static void Save(KeyboardData data)
        {
            var json = JsonConvert.SerializeObject(data);
            File.WriteAllText(MapFile, json);
        }
    }
}
