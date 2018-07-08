using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRedux
{
    public static class Extensions
    {
        private static Random _random = new Random();

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> e)
        {
            var l = e.ToList();
            int n = l.Count();
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                T value = l[k];
                l[k] = l[n];
                l[n] = value;
            }
            return l;
        }
    }
}
