using System.Linq;

namespace PokemonRedux.Game.Battles.AIs
{
    class WildAI : AI
    {
        public override BattleAction TakeAction()
        {
            var actor = Battle.ActiveBattle.EnemyPokemon;

            // wild pokemon can flee
            if (actor.Pokemon.FleeRate > 0 && actor.GetCanFlee())
            {
                var r = Battle.ActiveBattle.Random.NextDouble();
                if (r <= actor.Pokemon.FleeRate)
                {
                    return new BattleAction
                    {
                        ActionType = BattleActionType.Run
                    };
                }
            }

            // select a move at random
            // if no PP are left, use Struggle instead
            var validMoves = actor.Pokemon.Moves.Where(m => m.pp > 0 && actor.DisabledMove != m).ToArray();
            if (validMoves.Length == 0)
            {
                return new BattleAction
                {
                    ActionType = BattleActionType.Move,
                    MoveName = "STRUGGLE"
                };
            }
            else
            {
                var moveIndex = Battle.ActiveBattle.Random.Next(0, validMoves.Length);
                return new BattleAction
                {
                    ActionType = BattleActionType.Move,
                    MoveName = validMoves[moveIndex].name
                };
            }
        }
    }
}
