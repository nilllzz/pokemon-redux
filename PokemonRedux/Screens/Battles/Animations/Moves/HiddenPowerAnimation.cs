using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System;
using System.Linq;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class HiddenPowerAnimation : BattleMoveAnimation
    {
        private const int BALL_AMOUNT = 8;
        private const double ROTATION_SPEED = 0.015;
        private const int ELLIPSE_STAGES = 3;
        private const int ELLIPSE_HEIGHT = 16;
        private const int ELLIPSE_WIDTH = 56;
        private const int BALL_SIZE = 8;
        private const double ELLIPSE_GROW_SPEED = 0.2;
        private const double ELLIPSE_TARGET_SIZE = 4;
        private const int HIT_STAGES = 3;
        private const int HIT_DELAY = 4;
        private const int HIT_SIZE = 32;

        private double[] _positions;
        private double _totalRotation = 0;
        private int _stage = 0;
        private double _ellipseSize = 1;
        private int _hitStage = 0;
        private int _hitDelay = HIT_DELAY;

        public HiddenPowerAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("hiddenpower");
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_stage <= ELLIPSE_STAGES)
            {
                var center = GetCenter(_user.Side);
                var width = (int)(ELLIPSE_WIDTH * Border.SCALE * _ellipseSize);
                var height = (int)(ELLIPSE_HEIGHT * Border.SCALE * _ellipseSize);
                var ballSize = BALL_SIZE * Border.SCALE;

                var texX = _stage;
                if (_stage == ELLIPSE_STAGES)
                {
                    texX--; // last stage uses stage 2 textures
                }

                foreach (var position in _positions)
                {
                    var rAngle = MathHelper.TwoPi * position;
                    var posX = width / 2 * Math.Cos(rAngle);
                    var posY = height / 2 * Math.Sin(rAngle);

                    var x = (int)(center.X + posX - ballSize / 2);
                    var y = (int)(center.Y + posY - ballSize / 2);

                    batch.Draw(_texture, new Rectangle(x, y, (int)ballSize, (int)ballSize),
                        new Rectangle(texX * BALL_SIZE, 32, BALL_SIZE, BALL_SIZE), Color.White);
                }
            }
            else
            {
                var center = GetCenter(_target.Side);
                var hitSize = HIT_SIZE * Border.SCALE;
                var x = (int)(center.X - hitSize / 2);
                var y = (int)(center.Y - hitSize / 2 + 10 * Border.SCALE);
                batch.Draw(_texture, new Rectangle(x, y, (int)hitSize, (int)hitSize),
                    new Rectangle((_hitStage % 2) * HIT_SIZE, 0, HIT_SIZE, HIT_SIZE), Color.White);
            }
        }

        public override void Update()
        {
            if (_stage < ELLIPSE_STAGES)
            {
                for (var i = 0; i < _positions.Length; i++)
                {
                    var newPosition = _positions[i] - ROTATION_SPEED;
                    if (newPosition < 0)
                    {
                        newPosition += 1;
                    }
                    _positions[i] = newPosition;
                }
                _totalRotation += ROTATION_SPEED;
                switch (_stage)
                {
                    case 0:
                        if (_totalRotation > 0.25)
                        {
                            _stage++;
                        }
                        break;
                    case 1:
                        if (_totalRotation > 0.5)
                        {
                            _stage++;
                        }
                        break;
                    case 2:
                        if (_totalRotation >= 2)
                        {
                            _stage++;
                            _positions = new double[BALL_AMOUNT]
                                .Select((v, i) => (double)i / BALL_AMOUNT).ToArray();
                        }
                        break;
                }
            }
            else if (_stage == ELLIPSE_STAGES)
            {
                _ellipseSize += ELLIPSE_GROW_SPEED;
                if (_ellipseSize >= ELLIPSE_TARGET_SIZE)
                {
                    _stage++;
                }
            }
            else
            {
                _hitDelay--;
                if (_hitDelay == 0)
                {
                    _hitDelay = HIT_DELAY;
                    _hitStage++;
                    if (_hitStage == HIT_STAGES)
                    {
                        Finish();
                    }
                }
            }
        }
    }
}
