using static Core;

namespace PokemonRedux.Game
{
    static class PokedexHelper
    {
        // returns the Prof. Oak rating message based on the amount of caught pokemon
        public static string GetRatingMessage()
        {
            var caught = Controller.ActivePlayer.PokedexCaught.Length;

            if (caught < 10)
            {
                return "Look for POKéMON in grassy areas!"; // TODO: message
            }
            else if (caught < 20)
            {
                return "Good. I see you understand how to use POKé BALLS"; // TODO: message
            }
            else if (caught < 35)
            {
                return "You^re getting good at this. But you have a long way to go."; // TODO: message
            }
            else if (caught < 50)
            {
                return "You need to fill up the POKéDEX. Catch different kinds of POKéMON."; // TODO: message
            }
            else if (caught < 65)
            {
                return "You^re trying--I\ncan see that.\n\nYour POKéDEX is\ncoming together.";
            }
            else if (caught < 80)
            {
                return "To evolve, some POKéMON grow, others use the effects of STONES."; // TODO: message
            }
            else if (caught < 95)
            {
                return "Have you gotten a fishing ROD? You can catch POKéMON by fishing."; // TODO: message
            }
            else if (caught < 110)
            {
                return "Excellent! You seem to like collecting things!"; // TODO: message
            }
            else if (caught < 125)
            {
                return "Some POKéMON only appear during certain times of the day."; // TODO: message
            }
            else if (caught < 140)
            {
                return "Your POKéDEX is filling up. Keep up the good work."; // TODO: message
            }
            else if (caught < 155)
            {
                return "I'm impressed. You're evolving POKéMON, not just catching them."; // TODO: message
            }
            else if (caught < 170)
            {
                return "Have you met KURT? His custom POKé BALLS should help."; // TODO: message
            }
            else if (caught < 185)
            {
                return "Wow. You've found more POKéMON than the last POKéDEX research project."; // TODO: message
            }
            else if (caught < 200)
            {
                return "Are you trading your POKéMON? It's tough to do this alone."; // TODO: message
            }
            else if (caught < 215)
            {
                return "Wow! You've hit 200! Your POKéDEX is looking great!"; // TODO: message
            }
            else if (caught < 230)
            {
                return "You've found so many POKéMON! You've really helped my studies!"; // TODO: message
            }
            else if (caught < 245)
            {
                return "Magnificent! You could become a POKéMON professor right now!"; // TODO: message
            }
            else if (caught < 250)
            {
                return "Your POKéDEX is amazing! You're ready to turn professional!"; // TODO: message
            }
            else
            {
                return "Whoa! A perfect POKéDEX! I've dreamt about this! Congratulations!"; // TODO: message
            }
        }
    }
}
