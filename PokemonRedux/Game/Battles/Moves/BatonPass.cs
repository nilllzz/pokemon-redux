using PokemonRedux.Screens.Battles.Animations;
using PokemonRedux.Screens.Battles.Animations.Moves;
using System.Linq;
using static Core;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("BATON PASS")]
    class BatonPass : BattleMove
    {
        public override MoveCategory GetCategory(BattlePokemon user) => MoveCategory.Status;
        public override bool ShouldShowAnimation => true;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            if (Controller.GameOptions.BattleAnimations)
            {
                Battle.ActiveBattle.AnimationController.ShowAnimation(new PokemonSizeChangeAnimation(user, 1f, 0f, 0.07f));

                var animation = new BatonPassAnimation(user, target);
                Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
            }
            else
            {
                Battle.ActiveBattle.AnimationController.SetPokemonSize(user.Side, 0f);
            }

            Battle.ActiveBattle.AnimationController.SetPokemonVisibility(user.Side, false);
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(user.Side, false);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            var pokemon = Battle.ActiveBattle.UI.SelectPokemon();
            Battle.ActiveBattle.SendOutPlayerPokemon(pokemon, true);
            return true;
        }

        public override bool StatusMoveCheck(BattlePokemon user, BattlePokemon target)
        {
            if (user.Side == PokemonSide.Player)
            {
                // requires more than 1 pokemon that can battle
                return Controller.ActivePlayer.PartyPokemon.Where(p => p.CanBattle).Count() > 1;
            }
            else
            {
                if (Battle.ActiveBattle.IsWildBattle)
                {
                    // always fails for wild pokemon
                    return false;
                }
                else
                {
                    // TODO: trainer
                    return false;
                }
            }
        }
    }
}
