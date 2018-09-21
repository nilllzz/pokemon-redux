using PokemonRedux.Game.Overworld;
using System;
using System.Globalization;

namespace PokemonRedux.Game
{
    static class DateHelper
    {
        public static string GetDisplayDayOfWeek(DateTime dateTime)
        {
            switch (dateTime.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return "Sunday";
                case DayOfWeek.Monday:
                    return "Monday";
                case DayOfWeek.Tuesday:
                    return "Tuesday";
                case DayOfWeek.Wednesday:
                    return "Wednesday";
                case DayOfWeek.Thursday:
                    return "Thursday";
                case DayOfWeek.Friday:
                    return "Friday";
                case DayOfWeek.Saturday:
                    return "Saturday";
            }

            return "";
        }

        public static string GetDisplayDaytime(Daytime daytime)
        {
            switch (daytime)
            {
                case Daytime.Morning:
                    return "Morn";
                case Daytime.Day:
                    return "Day";
                case Daytime.Night:
                    return "Nite";
            }

            return "";
        }

        public static string GetAmPmTime(DateTime dateTime)
        {
            return dateTime.ToString("hh:mm tt", CultureInfo.InvariantCulture).ToUpper();
        }
    }
}
