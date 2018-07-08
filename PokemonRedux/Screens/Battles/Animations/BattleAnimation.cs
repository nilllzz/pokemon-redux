using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using PokemonRedux.Game.Pokemons;

namespace PokemonRedux.Screens.Battles.Animations
{
    abstract class BattleAnimation
    {
        public int WaitDelay { get; set; } // delays the execution of the animation in the queue
        public bool IsFinished { get; protected set; }

        protected (int startX, int startY, int unit) GetScreenValues()
        {
            var (unit, startX, _, _) = Border.GetDefaultScreenValues();
            return (startX, BattleScreen.StartY, unit);
        }

        protected Vector2 EnemyPokemonCenter
        {
            get
            {
                var (startX, startY, unit) = GetScreenValues();
                var x = startX + unit * 15.5f;
                var y = startY + unit * 3.5f;
                return new Vector2(x, y);
            }
        }

        protected Vector2 PlayerPokemonCenter
        {
            get
            {
                var (startX, startY, unit) = GetScreenValues();
                var x = startX + unit * 5.5f;
                var y = startY + unit * 8.5f;
                return new Vector2(x, y);
            }
        }

        protected Vector2 GetCenter(PokemonSide side)
        {
            if (side == PokemonSide.Enemy)
            {
                return EnemyPokemonCenter;
            }
            else
            {
                return PlayerPokemonCenter;
            }
        }

        protected int GetPokemonSpriteSize()
        {
            return (int)(PokemonTextureManager.TEXTURE_SIZE * Border.SCALE);
        }

        public abstract void LoadContent();
        public abstract void Draw(SpriteBatch batch);
        public abstract void Update();
        public virtual void Show() { }
    }
}
