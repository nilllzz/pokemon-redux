using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class StunSporeAnimation : BattleAnimation
    {
        private const int TOTAL_STAGES = 2;
        private const int STAGE_DELAY = 4;
        private const float SPEED = 0.0075f;
        private const int FRAME_WIDTH = 56;
        private const int FRAME_HEIGHT = 32;

        private readonly BattlePokemon _target;

        private Texture2D _texture;
        private float _progress;
        private int _stage;
        private int _stageDelay = STAGE_DELAY;

        public StunSporeAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/Animations/stunspore.png");
        }

        public override void Show()
        {
            // hide status of user
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), false);
        }

        public override void Draw(SpriteBatch batch)
        {
            var frameWidth = (int)(FRAME_WIDTH * Border.SCALE);
            var frameHeight = (int)(FRAME_HEIGHT * Border.SCALE);
            var center = GetCenter(_target.Side);

            var pos = center;
            pos.Y -= GetPokemonSpriteSize() / 2f + frameHeight / 2f;
            pos.X -= frameWidth / 2f;
            pos.Y += GetPokemonSpriteSize() * _progress;

            var spriteHeight = FRAME_HEIGHT;

            if (_progress >= 0.8f)
            {
                var spriteProgress = 1f - (_progress - 0.8f) * 5;
                frameHeight = (int)(frameHeight * spriteProgress);
                spriteHeight = (int)(spriteHeight * spriteProgress);
            }

            batch.Draw(_texture, new Rectangle((int)pos.X, (int)pos.Y, frameWidth, frameHeight),
                new Rectangle(0, FRAME_HEIGHT * _stage, FRAME_WIDTH, spriteHeight), Color.White);
        }

        public override void Update()
        {
            _stageDelay--;
            if (_stageDelay == 0)
            {
                _stageDelay = STAGE_DELAY;
                _stage++;
                if (_stage == TOTAL_STAGES)
                {
                    _stage = 0;
                }
            }

            _progress += SPEED;
            if (_progress >= 1f)
            {
                _progress = 1f;
                IsFinished = true;
                // show status again
                Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), true);
            }
        }
    }
}
