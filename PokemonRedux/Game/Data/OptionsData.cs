using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace PokemonRedux.Game.Data
{
    class OptionsData
    {
        public int textSpeed; // 0 => fast, 1 => mid, 2 => slow
        public bool battleScene; // battle animations on/off
        public bool battleStyle; // false => Shift, true => Set
        public bool menuAccount; // start menu descriptions on/off
        public int frame; // 0-7 frames

        public static OptionsData CreateNew()
        {
            var data = new OptionsData
            {
                textSpeed = 1,
                battleScene = true,
                battleStyle = false,
                menuAccount = true,
                frame = 0,
            };
            return data;
        }

        public static OptionsData Load()
        {
            if (OptionsFileExists())
            {
                try
                {
                    var optionsContents = File.ReadAllText(OptionsFile, Encoding.UTF8);
                    var data = JsonConvert.DeserializeObject<OptionsData>(optionsContents);
                    return data;
                }
                catch (Exception ex)
                {
                    throw new FileLoadException("Failed to load options file. Refer to inner for more details.", ex);
                }
            }
            else
            {
                throw new FileNotFoundException("Tried to load non-existing options file.");
            }
        }

        public void Save()
        {
            try
            {
                var saveContents = JsonConvert.SerializeObject(this);
                File.WriteAllText(OptionsFile, saveContents, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to save options. Refer to inner for more details.", ex);
            }
        }

        private static string OptionsFile
            => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "options.json");

        public static bool OptionsFileExists()
            => File.Exists(OptionsFile);
    }
}
