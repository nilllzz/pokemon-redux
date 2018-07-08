using Microsoft.Xna.Framework;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles
{
    interface IBattleUI
    {
        void ShowMessageAndWait(string message);
        void ShowMessageAndKeepOpen(string message, int frames = 0); // frames to keep the message open without continuing
        void ResetMenu();
        void AnimateEnemyHPAndWait();
        void AnimatePlayerHPAndWait();
        void SetPokemonStatusVisible(PokemonSide side, bool visible);
        void SetPokemonStatusOffset(PokemonSide side, Vector2 offset);
        void EndBattle(bool won);
    }
}
