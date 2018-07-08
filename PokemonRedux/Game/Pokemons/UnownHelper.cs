using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRedux.Game.Pokemons
{
    static class UnownHelper
    {
        private const int UNOWN_AMOUNT = 26;
        private static readonly int[] BIT_INDICES = new[] { 14, 13, 10, 9, 6, 5, 2, 1 };
        public const int UNOWN_ID = 201; // id of Unown in the national pokedex

        // unown letters are ints from 0-25, representing the letters A-Z in the latin alphabet.
        /*
         * Determining Unown's letter:
         * 1. get bits from the positions in the dv defined in BIT_INDICES
         * 2. combine them into a new byte
         * 3. divide that byte by 10 and cut off the remainder by flooring the result
         * 
         * Example:
         * DV:    1010 1001 0001 1110
         * Bits:   ^^   ^^   ^^   ^^
         *         01   00   00   11  => 01000011 => 67 => floor(67 / 10) => 6
         */
        public static int GetUnownLetterFromDV(ushort dv)
        {
            var bits = new BitArray(BitConverter.GetBytes(dv));

            byte combination = 0;
            for (int i = 0; i < BIT_INDICES.Length; i++)
            {
                var bitIndex = BIT_INDICES[i];
                var bit = bits.Get(bitIndex);
                if (bit)
                {
                    var mask = 1 << (7 - i);
                    combination |= (byte)mask;
                }
            }

            return (int)Math.Floor((double)combination / 10);
        }

        private static byte[] UNOWN_LETTER_COLOR_RED;
        private static byte[] UNOWN_LETTER_COLOR_GREEN;
        private static byte[] UNOWN_LETTER_COLOR_BLUE;

        public static Color GetUnownDexColor(int unownLetter)
        {
            if (UNOWN_LETTER_COLOR_RED == null)
            {
                // initialze color ranges
                // represent a rainbow like this:
                // red, yellow, green, blue, purple, red
                // each color contains 256 * 6 bytes representing their color value at a time

                var up = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
                var down = up.Reverse().ToArray();
                var upper = Enumerable.Repeat(255, 256).Select(i => (byte)i).ToArray();
                var lower = Enumerable.Repeat(0, 256).Select(i => (byte)i).ToArray();

                var red = new List<byte>();
                red.AddRange(upper);
                red.AddRange(down);
                red.AddRange(lower);
                red.AddRange(lower);
                red.AddRange(up);
                red.AddRange(upper);

                var green = new List<byte>();
                green.AddRange(up);
                green.AddRange(upper);
                green.AddRange(upper);
                green.AddRange(down);
                green.AddRange(lower);
                green.AddRange(lower);

                var blue = new List<byte>();
                blue.AddRange(down);
                blue.AddRange(down);
                blue.AddRange(up);
                blue.AddRange(upper);
                blue.AddRange(upper);
                blue.AddRange(down);

                UNOWN_LETTER_COLOR_RED = red.ToArray();
                UNOWN_LETTER_COLOR_GREEN = green.ToArray();
                UNOWN_LETTER_COLOR_BLUE = blue.ToArray();
            }

            // get color index from unown letter position in the alphabet
            var index = (int)Math.Floor((unownLetter / (double)(UNOWN_AMOUNT - 1)) * (UNOWN_LETTER_COLOR_RED.Length - 1));
            return new Color(UNOWN_LETTER_COLOR_RED[index], UNOWN_LETTER_COLOR_GREEN[index], UNOWN_LETTER_COLOR_BLUE[index]);
        }
    }
}
