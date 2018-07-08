using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles.Animations
{
    class PokemonSizeChangeAnimation : BattleAnimation
    {
        private readonly BattlePokemon _target;
        private readonly float _startSize, _targetSize, _speed;

        private float _size;

        public PokemonSizeChangeAnimation(BattlePokemon target, float startSize, float targetSize, float speed)
        {
            _target = target;
            _startSize = startSize;
            _targetSize = targetSize;
            _speed = speed;

            _size = _startSize;
        }

        public override void LoadContent()
        {
            Battle.ActiveBattle.AnimationController.SetPokemonSize(_target.Side, _startSize);
        }

        public override void Draw(SpriteBatch batch)
        { }

        public override void Update()
        {
            if (_size < _targetSize)
            {
                _size += _speed;
                if (_size >= _targetSize)
                {
                    _size = _targetSize;
                    IsFinished = true;
                }
            }
            else if (_size > _targetSize)
            {
                _size -= _speed;
                if (_size <= _targetSize)
                {
                    _size = _targetSize;
                    IsFinished = true;
                }
            }

            Battle.ActiveBattle.AnimationController.SetPokemonSize(_target.Side, _size);
        }
    }
}
