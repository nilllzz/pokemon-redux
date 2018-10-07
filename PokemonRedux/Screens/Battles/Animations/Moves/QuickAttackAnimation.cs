using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class QuickAttackAnimation : BattleMoveAnimation
    {
        private const int TEXTURE_SIZE = 56;
        private const int TEXTURE_ANIMATION_DELAY = 3;
        private const int TEXTURE_STAGES = 4;
        private const int HIT_DELAY = 10;
        private const int HIT_SIZE = 24;
        private const int FINISH_DELAY = 40;

        private int _textureStage = 0; // 0-4
        private bool _textureVisible = true;
        private int _textureDelay = TEXTURE_ANIMATION_DELAY;
        private bool _hitVisible = false;
        private int _hitDelay = HIT_DELAY;
        private bool _finishing = false;
        private int _finishDelay = FINISH_DELAY;

        public QuickAttackAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("quickattack");
        }

        public override void Show()
        {
            base.Show();

            Battle.ActiveBattle.AnimationController.SetPokemonVisibility(BattlePokemon.ReverseSide(_target.Side), false);
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_textureVisible)
            {
                var center = GetCenter(_user.Side);
                var textureSize = (int)(TEXTURE_SIZE * Border.SCALE);
                var x = (int)(center.X - textureSize / 2f);
                var y = (int)(center.Y - textureSize / 2f);
                batch.Draw(_texture, new Rectangle(x, y, textureSize, textureSize),
                    new Rectangle(_textureStage * TEXTURE_SIZE, 0, TEXTURE_SIZE, TEXTURE_SIZE), Color.White);
            }
            else if (_hitVisible)
            {
                var center = GetCenter(_target.Side);
                var hitSize = (int)(HIT_SIZE * Border.SCALE);
                var x = (int)(center.X - hitSize / 2f);
                var y = (int)(center.Y - hitSize / 2f);
                batch.Draw(_texture, new Rectangle(x, y, hitSize, hitSize),
                    new Rectangle(0, TEXTURE_SIZE, HIT_SIZE, HIT_SIZE), Color.White);
            }
        }

        public override void Update()
        {
            if (_textureVisible)
            {
                _textureDelay--;
                if (_textureDelay == 0)
                {
                    _textureDelay = TEXTURE_ANIMATION_DELAY;
                    _textureStage++;
                    if (_textureStage == TEXTURE_STAGES)
                    {
                        _textureVisible = false;
                    }
                }
            }
            else if (_finishing)
            {
                _finishDelay--;
                if (_finishDelay == 0)
                {
                    Finish();
                }
            }
            else
            {
                _hitDelay--;
                if (_hitDelay == 0)
                {
                    _hitDelay = HIT_DELAY;
                    _hitVisible = !_hitVisible;
                    if (!_hitVisible) // after hit appeared and disappeared
                    {
                        _finishing = true;
                        Battle.ActiveBattle.AnimationController.SetPokemonVisibility(BattlePokemon.ReverseSide(_target.Side), true);
                    }
                }
            }
        }
    }
}
