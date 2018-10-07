using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System.Collections.Generic;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class HydroPumpAnimation : BattleMoveAnimation
    {
        private class Pillar
        {
            public int Position = 0;
            public int Stage = 0;
            public int StageDelay = STAGE_DELAY;
            public bool Retracting;
        }

        private const float EFFECT_OFFSET_SPEED = 1f;
        private const int TOTAL_PILLARS = 7;
        private const int PILLAR_WIDTH = 8; // only the pillar, actual texture is 16px
        private const int FRAME_WIDTH = 16;
        private const int FRAME_HEIGHT = 53;
        private const int TOTAL_STAGES = 5;
        private const int STAGE_DELAY = 4;

        private Effect _shader;

        private List<Pillar> _pillars = new List<Pillar>();
        private int _totalPillars = 0;
        private float _effectOffset = 0f;

        public HydroPumpAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("hydropump");
            _shader = Controller.Content.Load<Effect>("Shaders/Battle/underwater");
            _pillars.Add(new Pillar());
            _totalPillars++;
        }

        public override void Show()
        {
            base.Show();

            Battle.ActiveBattle.AnimationController.SetScreenEffect(_shader);

            _shader.Parameters["offset"].SetValue(_effectOffset);

            var center = GetCenter(_target.Side);
            _shader.Parameters["startY"].SetValue(center.Y - GetPokemonSpriteSize() / 2f);
            _shader.Parameters["endY"].SetValue(center.Y + GetPokemonSpriteSize() / 2f);
        }

        public override void Draw(SpriteBatch batch)
        {
            var frameWidth = (int)(FRAME_WIDTH * Border.SCALE);
            var frameHeight = (int)(FRAME_HEIGHT * Border.SCALE);
            var pillarWidth = (int)(PILLAR_WIDTH * Border.SCALE);
            var center = GetCenter(_target.Side);

            foreach (var pillar in _pillars)
            {
                var x = (int)(center.X - GetPokemonSpriteSize() / 2f + pillarWidth * pillar.Position);
                var y = (int)(center.Y + GetPokemonSpriteSize() / 2f - frameHeight);
                batch.Draw(_texture, new Rectangle(x, y, frameWidth, frameHeight),
                    new Rectangle(pillar.Stage * FRAME_WIDTH, 0, FRAME_WIDTH, FRAME_HEIGHT), Color.White);
            }
        }

        public override void Update()
        {
            _effectOffset += EFFECT_OFFSET_SPEED;
            _shader.Parameters["offset"].SetValue(_effectOffset);

            for (var i = 0; i < _pillars.Count; i++)
            {
                var pillar = _pillars[i];
                pillar.StageDelay--;
                if (pillar.StageDelay == 0)
                {
                    pillar.StageDelay = STAGE_DELAY;
                    if (pillar.Retracting)
                    {
                        pillar.Stage--;
                        if (pillar.Stage == -1)
                        {
                            _pillars.RemoveAt(i);
                            i--;
                            if (_pillars.Count == 0)
                            {
                                Finish();
                                Battle.ActiveBattle.AnimationController.SetScreenEffect();
                            }
                        }
                    }
                    else
                    {
                        pillar.Stage++;
                        if (pillar.Stage == TOTAL_STAGES)
                        {
                            pillar.Stage--;
                            pillar.Retracting = true;
                            if (_totalPillars < TOTAL_PILLARS)
                            {
                                _pillars.Add(new Pillar { Position = _totalPillars });
                                _totalPillars++;
                            }
                        }
                    }
                }
            }
        }
    }
}
