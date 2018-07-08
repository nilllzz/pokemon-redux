using Newtonsoft.Json;
using PokemonRedux.Game.Battles.Moves;
using PokemonRedux.Game.Pokemons;
using System;
using System.Linq;

namespace PokemonRedux.Game.Data
{
    class PokemonSaveData : ICloneable
    {
        private const int BASE_FRIENDSHIP = 70;
        private const int DEFAULT_TRAINER_ID = 0;

        public const int MAX_NAME_LENGTH = 7;
        public const string DEFAULT_OT = "???????";
        public const int BASE_FRIENDSHIP_EGG = 120;
        public const int BASE_FRIENDSHIP_FRIEND_BALL = 200;

        private static Random _random = new Random();

        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public int id;
        public string nickname;
        public ushort dv;
        public ushort[] ev; // 0 => HP, 1 => ATK, 2 => DEF, 3 => SPC, 5 => SPD
        public int experience;
        public int friendship;
        public int eggCycle;
        public int hp;
        public string status;
        public string item;
        public string itemData; // holds text for mail
        public int trainerId;
        public string ot;
        public PokemonMoveData[] moves;
#pragma warning restore 0649

        public static PokemonSaveData GenerateNew()
        {
            var data = new PokemonSaveData
            {
                dv = (ushort)_random.Next(0, ushort.MaxValue + 1),
                ev = new ushort[5], // init to 0
                experience = 0,
                status = DataHelper.EnumToString(PokemonStatus.OK),
                moves = new PokemonMoveData[0],
                ot = DEFAULT_OT,
                trainerId = DEFAULT_TRAINER_ID,
                friendship = BASE_FRIENDSHIP
            };
            return data;
        }

        public object Clone()
        {
            var clone = (PokemonSaveData)MemberwiseClone();
            clone.ev = (ushort[])ev.Clone();
            clone.moves = (PokemonMoveData[])moves?.Clone();
            return clone;
        }

        #region DV black magic fuckery

        /*
         * 4 DVs are stored like this:
         * short => 16 bits
         * 
         * ATK  DEF  SPD  SPC
         * 0000 0000 0000 0000
         * 
         * HP DV (example):
         *    
         * ATK  DEF  SPD  SPC
         * 0011 1011 0010 1010
         *    ^    ^    ^    ^
         *    1    1    0    0   =>   12
         */

        [JsonIgnore]
        public byte DVHP =>
            (byte)Convert.ToInt32(string.Join("",
                new[] { DVAttack & 1, DVDefense & 1, DVSpeed & 1, DVSpecial & 1 }
                .Select(i => i.ToString())), 2);

        [JsonIgnore]
        public byte DVAttack => (byte)((ushort)(dv & 0b1111_0000_0000_0000) >> 12);
        [JsonIgnore]
        public byte DVDefense => (byte)((ushort)(dv & 0b0000_1111_0000_0000) >> 8);
        [JsonIgnore]
        public byte DVSpeed => (byte)((ushort)(dv & 0b0000_0000_1111_0000) >> 4);
        [JsonIgnore]
        public byte DVSpecial => (byte)((ushort)(dv & 0b0000_0000_0000_1111));

        /*
         * Shiny status is determined by DVs:
         * DEF, SPD and SPC have to equal 10.
         * ATK has to be one of [2, 3, 6, 7, 10, 11, 14, 15].
         */
        private static byte[] ATK_SHINY_VALUES = new byte[] { 2, 3, 6, 7, 10, 11, 14, 15 };
        [JsonIgnore]
        public bool IsShiny
            => DVDefense == 10 &&
               DVSpeed == 10 &&
               DVSpecial == 10 &&
               ATK_SHINY_VALUES.Contains(DVAttack);

        /*
         * Ratio          ATK-DV    Result
         * -----------------------------------
         * Genderless  => N/A    => Genderless
         * Male        => N/A    => Male
         * MostlyMale  => 2-15   => Male
         * OftenMale   => 4-15   => Male
         * Equal       => 8-15   => Male
         * OftenFemale => 12-15  => Male
         * Female      => N/A    => Female
         */
        public PokemonGender GetGender(PokemonGenderNominalRatio ratio)
        {
            switch (ratio)
            {
                case PokemonGenderNominalRatio.Genderless:
                    return PokemonGender.Genderless;
                case PokemonGenderNominalRatio.Male:
                    return PokemonGender.Male;
                case PokemonGenderNominalRatio.MostlyMale:
                    if (DVAttack < 2)
                    {
                        return PokemonGender.Female;
                    }
                    else
                    {
                        return PokemonGender.Male;
                    }
                case PokemonGenderNominalRatio.OftenMale:
                    if (DVAttack < 4)
                    {
                        return PokemonGender.Female;
                    }
                    else
                    {
                        return PokemonGender.Male;
                    }
                case PokemonGenderNominalRatio.Equal:
                    if (DVAttack < 8)
                    {
                        return PokemonGender.Female;
                    }
                    else
                    {
                        return PokemonGender.Male;
                    }
                case PokemonGenderNominalRatio.OftenFemale:
                    if (DVAttack < 12)
                    {
                        return PokemonGender.Female;
                    }
                    else
                    {
                        return PokemonGender.Male;
                    }
                case PokemonGenderNominalRatio.Female:
                    return PokemonGender.Female;

                default:
                    return PokemonGender.Genderless;
            }
        }

        /*
         * formula: typeIndex = 4 x a mod 4 + b mod 4
         * 
         * actual bitwise calculation is:
         * take two least significant bits of Attack and Defense DV,
         * concatenating them and converting that string to a byte
         * 
         * example:
         * 
         *  Bitwise:
         *  Attack DV   Defense DV
         *  0101        1010
         *    ^^          ^^
         *    01          10 => 0110 => 6
         * 
         *  Formula:
         *  Attack DV   Defense DV
         *  5           10
         *  4 * (5 % 4) + (10 % 4) => 4 * 1 + 2 => 6
         * 
         */
        public PokemonType GetHiddenPowerType()
        {
            var a = DVAttack;
            var b = DVDefense;
            var typeIndex = 4 * (a % 4) + (b % 4);
            return HiddenPower.TYPES[typeIndex];
        }

        /*
         * formula: floor((5 * (v + 2 * w + 4 * x + 8 * y) + z) / 2 + 31)
         * where:
         *  v through y are the MSB of each DV
         *  v => Special DV
         *  w => Speed DV
         *  x => Defense DV
         *  y => Attack DV
         * 
         *  z is Special DV mod 4
         */
        public int GetHiddenPowerBasePower()
        {
            int GetMostSignificantBit(byte dv)
            {
                // every DV is a 4-bit structure:
                // 0111 => 7
                // ^ MSB (smaller than 8 is not set)
                return dv < 8 ? 0 : 1;
            }

            var v = GetMostSignificantBit(DVSpecial);
            var w = GetMostSignificantBit(DVSpeed);
            var x = GetMostSignificantBit(DVDefense);
            var y = GetMostSignificantBit(DVAttack);
            var z = DVSpecial % 4;

            return (int)Math.Floor((5 * (v + 2 * w + 4 * x + 8 * y) + z) / 2 + 31D);
        }

        #endregion
    }
}
