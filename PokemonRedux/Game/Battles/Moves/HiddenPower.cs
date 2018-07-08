using PokemonRedux.Game.Pokemons;
using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("HIDDEN POWER")]
    class HiddenPower : BattleMove
    {
        public static readonly PokemonType[] TYPES = new[]
        {
            PokemonType.Fighting,
            PokemonType.Flying,
            PokemonType.Poison,
            PokemonType.Ground,
            PokemonType.Rock,
            PokemonType.Bug,
            PokemonType.Ghost,
            PokemonType.Steel,
            PokemonType.Fire,
            PokemonType.Water,
            PokemonType.Grass,
            PokemonType.Electric,
            PokemonType.Psychic,
            PokemonType.Ice,
            PokemonType.Dragon,
            PokemonType.Dark,
        };

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new HiddenPowerAnimation(target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override PokemonType GetType(BattlePokemon user)
            => user.Pokemon.HiddenPowerType;

        public override int GetBasePower(BattlePokemon user, BattlePokemon target)
            => user.Pokemon.HiddenPowerBasePower;
    }
}
