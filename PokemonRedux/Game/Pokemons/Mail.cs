using PokemonRedux.Game.Data;
using PokemonRedux.Game.Items;
using PokemonRedux.Screens;

namespace PokemonRedux.Game.Pokemons
{
    class Mail
    {
        public const int MESSAGE_MAX_LENGTH = 32;
        public const int MESSAGE_CHARS_PER_LINE = 16;

        public static Mail Get(MailData data)
        {
            return new Mail(data);
        }

        public static Mail Get(string template, string data)
        {
            // data stores 3 lines, author and two text lines (stored as single line, breaks on character limit per line)
            var lines = data.Split('\n');
            var author = lines[0];
            var text = lines[1];

            return new Mail(new MailData
            {
                author = author,
                message = text,
                template = template
            });
        }

        public string Author => Data.author;
        public string Template => Data.template;
        public int PokemonId
        {
            get => Data.pokemonId;
            set => Data.pokemonId = value;
        }
        public string[] Lines => GetLinesFromMessage(Data.message);
        public MailData Data { get; }

        private Mail(MailData data)
        {
            Data = data;
        }

        public Item GetItem()
            => Item.Get(Template);

        public string GetItemData()
            => GenerateItemData(Author, Data.message);

        public static string[] GetLinesFromMessage(string message)
        {
            var messageLength = PokemonFontRenderer.PrintableCharAmount(message);
            var line1 = message;
            string line2 = null;
            // split in two lines
            if (messageLength >= MESSAGE_CHARS_PER_LINE)
            {
                line1 = "";
                line2 = "";
                var line1Chars = 0;
                // split at correct positions and incorporate escape chars
                foreach (var c in message)
                {
                    if (line1Chars < MESSAGE_CHARS_PER_LINE)
                    {
                        line1 += c;
                        if (c == PokemonFontRenderer.ESCAPE_CHAR)
                        {
                            line1Chars--;
                        }
                        else
                        {
                            line1Chars++;
                        }
                    }
                    else
                    {
                        line2 += c;
                    }
                }
            }

            return new[] { line1, line2 };
        }

        public static string GenerateItemData(string author, string message)
        {
            return author + "\n" + message;
        }
    }
}
