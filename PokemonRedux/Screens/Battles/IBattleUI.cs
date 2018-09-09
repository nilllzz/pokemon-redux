using Microsoft.Xna.Framework;
using PokemonRedux.Game.Battles;
using PokemonRedux.Game.Data;
using PokemonRedux.Game.Pokemons;

namespace PokemonRedux.Screens.Battles
{
    interface IBattleUI
    {
        void ShowMessageAndWait(string message);
        void ShowMessageAndKeepOpen(string message, int frames = 0); // frames to keep the message open without continuing
        void ResetMenu();
        void AnimateEnemyHPAndWait();
        void AnimatePlayerHPAndWait();
        void AnimatePlayerExpAndWait(bool instant = false);
        void SetPokemonStatusVisible(PokemonSide side, bool visible);
        void SetPokemonStatusOffset(PokemonSide side, Vector2 offset);
        void SetPokemonArtificialLevelUp(bool active);
        void ShowPokemonStatsAndWait(Pokemon pokemon);
        void ShowLearnMoveScreen(Pokemon pokemon, PokemonMoveData moveData);
        void EndBattle(bool won);
    }
}
