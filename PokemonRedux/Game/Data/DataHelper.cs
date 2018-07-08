using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace PokemonRedux.Game.Data
{
    static class DataHelper
    {
        public static T ParseEnum<T>(string member)
        {
            // upper case first char
            member = member[0].ToString().ToUpper() + member.Substring(1);
            return (T)Enum.Parse(typeof(T), member);
        }

        public static string EnumToString<T>(T enumMember)
        {
            var member = enumMember.ToString();
            // lower case first char
            member = member[0].ToString().ToLower() + member.Substring(1);
            return member;
        }

        public static Rectangle[] GetRectangles(int[][] source)
        {
            if (source == null || source.Length == 0)
                return null;

            return source.Select(t => new Rectangle(t[0], t[1], t[2], t[3])).ToArray();
        }

        public static Vector2 GetVector2(float[] source, float defaultValue)
        {
            if (source == null || source.Length == 0)
                return new Vector2(defaultValue);

            switch (source.Length)
            {
                case 1:
                    return new Vector2(source[0]);
                default:
                    return new Vector2(source[0], source[1]);
            }
        }

        public static Vector3 GetVector3(float[] source, float defaultValue)
        {
            if (source == null || source.Length == 0)
                return new Vector3(defaultValue);

            switch (source.Length)
            {
                case 1:
                    return new Vector3(source[0]);
                case 2:
                    return new Vector3(source[0], defaultValue, source[1]);
                default:
                    return new Vector3(source[0], source[1], source[2]);
            }
        }

        public static Vector3[] GetVector3Arr(float[][] source, float defaultValue)
        {
            return source?.Select(s => GetVector3(s, defaultValue)).ToArray();
        }
    }
}
