using Microsoft.Xna.Framework;
using PokemonRedux.Game.Data.Entities;

namespace PokemonRedux.Game.Overworld.Entities
{
    class Floor : Prop
    {
        public Floor(EntityData data)
            : base(data)
        { }

        public override float GetHeightForPosition(Vector2 position)
        {
            if (_heights[0] == _heights[1] &&
                _heights[0] == _heights[2] &&
                _heights[0] == _heights[3])
                return _heights[0] + Position.Y;

            var p1 = new Vector3(-Size.X / 2f, _heights[0], -Size.Z / 2f) + Position;
            var p2 = new Vector3(Size.X / 2f, _heights[1], -Size.Z / 2f) + Position;
            var p3 = new Vector3(-Size.X / 2f, _heights[2], Size.Z / 2f) + Position;
            var p4 = new Vector3(Size.X / 2f, _heights[3], Size.Z / 2f) + Position;

            var topLeft = new Vector2(-Size.X / 2f, -Size.Z / 2f) + new Vector2(Position.X, Position.Z);
            var bottomRight = new Vector2(Size.X / 2f, Size.Z / 2f) + new Vector2(Position.X, Position.Z);

            if (position.X < topLeft.X)
            {
                position.X = topLeft.X;
            }
            else if (position.X > bottomRight.X)
            {
                position.X = bottomRight.X;
            }
            if (position.Y < topLeft.Y)
            {
                position.Y = topLeft.Y;
            }
            else if (position.Y > bottomRight.Y)
            {
                position.Y = bottomRight.Y;
            }

            if (Vector2.Distance(position, topLeft) > Vector2.Distance(position, bottomRight))
            {
                // use triangle 1
                return GetHeightAtTriangle(p1, p2, p3, position);
            }
            else
            {
                // use triangle 2
                return GetHeightAtTriangle(p2, p3, p4, position);
            }
        }

        private static float GetHeightAtTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector2 pos)
        {
            float det = (p2.Z - p3.Z) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Z - p3.Z);
            float l1 = ((p2.Z - p3.Z) * (pos.X - p3.X) + (p3.X - p2.X) * (pos.Y - p3.Z)) / det;
            float l2 = ((p3.Z - p1.Z) * (pos.X - p3.X) + (p1.X - p3.X) * (pos.Y - p3.Z)) / det;
            float l3 = 1.0f - l1 - l2;
            return l1 * p1.Y + l2 * p2.Y + l3 * p3.Y;
        }

        public override bool IsValidFloor(Vector3 position, Entity other)
        {
            return
                position.X + other.Size.X / 2f >= Position.X - Size.X / 2f &&
                position.X - other.Size.X / 2f <= Position.X + Size.X / 2f &&

                position.Z + other.Size.Z / 2f >= Position.Z - Size.Z / 2f &&
                position.Z - other.Size.Z / 2f <= Position.Z + Size.Z / 2f;
        }

        public override bool DoesCollide(CollisionType collisionType, Vector3 position, Vector3 size, Entity other)
            => false;
    }
}
