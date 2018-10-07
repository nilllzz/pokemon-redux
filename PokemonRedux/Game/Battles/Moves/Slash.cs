using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("SLASH")]
    class Slash : BattleMove
    {
        public override bool IncreasedCriticalHitRatio => true;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new SlashAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }
    }
}
