using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles.Animations
{
    // animation for after a stat has been lowered
    // shakes status/pokemon for enemy,
    // shakes screen for own (to left once, then to right once), 4 pixels each
    class StatFallAnimation : BattleAnimation
    {
        private const int ENEMY_SHAKE_AMOUNT = 16;
        private const int ENEMY_SHAKE_DELAY = 2;
        private const int PLAYER_X_OFFSET = 4;

        private readonly BattlePokemon _target;

        private int _playerOffsetX = 0;
        private int _playerStage = 0;
        private int _enemyShakeAmount = 0;
        private int _enemyShakeDelay = ENEMY_SHAKE_DELAY;

        public StatFallAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        { }

        public override void Draw(SpriteBatch batch)
        { }

        public override void Update()
        {
            if (_target.Side == PokemonSide.Enemy)
            {
                _enemyShakeDelay--;
                if (_enemyShakeDelay == 0)
                {
                    _enemyShakeDelay = ENEMY_SHAKE_DELAY;
                    _enemyShakeAmount++;
                    if (_enemyShakeAmount == ENEMY_SHAKE_AMOUNT)
                    {
                        Battle.ActiveBattle.AnimationController.SetPokemonOffset(_target.Side, Vector2.Zero);
                        Battle.ActiveBattle.UI.SetPokemonStatusOffset(_target.Side, Vector2.Zero);
                        IsFinished = true;
                    }
                    else
                    {
                        var offset = _enemyShakeAmount % 2 == 0 ? -1 : 1;
                        Battle.ActiveBattle.AnimationController.SetPokemonOffset(_target.Side, new Vector2(offset, 0));
                        Battle.ActiveBattle.UI.SetPokemonStatusOffset(_target.Side, new Vector2(offset, 0));
                    }
                }
            }
            else
            {
                switch (_playerStage)
                {
                    case 0:
                        _playerOffsetX--;
                        if (_playerOffsetX == -PLAYER_X_OFFSET)
                        {
                            _playerStage++;
                        }
                        break;
                    case 1:
                        _playerOffsetX++;
                        if (_playerOffsetX == PLAYER_X_OFFSET)
                        {
                            _playerStage++;
                        }
                        break;
                    case 2:
                        _playerOffsetX--;
                        if (_playerOffsetX == 0)
                        {
                            _playerStage++;
                            IsFinished = true;
                        }
                        break;
                }
                var offset = _playerOffsetX * Border.SCALE;
                Battle.ActiveBattle.AnimationController.SetScreenOffset(new Vector2(offset, 0));
            }
        }
    }
}
