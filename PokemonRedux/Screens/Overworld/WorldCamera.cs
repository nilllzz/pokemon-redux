using GameDevCommon.Rendering;
using Microsoft.Xna.Framework;
using PokemonRedux.Game.Overworld;
using PokemonRedux.Game.Overworld.Entities;

namespace PokemonRedux.Screens.Overworld
{
    class WorldCamera : PerspectiveCamera
    {
        private const float WARP_ZOOM_MAX = 1.5f;
        private const float WARP_ZOOM_SPEED = 0.1f;

        private readonly float _zoomLevel = 3.5f;
        private Entity _lockEntity;
        private World _world;
        private float _warpZoom = 0f;

        public WorldCamera(World world, Entity lockEntity)
        {
            _world = world;
            _lockEntity = lockEntity;

            Pitch = -0.85f;
            FOV = 45f;

            CreateProjection();
            UpdateView();
        }

        public override void Update()
        {
            UpdateView();
        }

        private void UpdateView()
        {
            var targetPosition = GetPosition();
            if (Position != targetPosition)
            {
                Position = targetPosition;
                CreateView();
            }
        }

        private Vector3 GetPosition()
        {
            var pos = _lockEntity.Position + new Vector3(0, _zoomLevel, _zoomLevel);

            if (_world.ZoomCamera)
            {
                if (_warpZoom < GetWarpZoomMax())
                {
                    _warpZoom += GetWarpZoomSpeed();
                    Pitch += GetWarpZoomSpeed() * 0.25f;
                    if (_warpZoom >= GetWarpZoomMax())
                    {
                        _warpZoom = GetWarpZoomMax();
                    }
                }
            }
            else
            {
                _warpZoom = 0f;
                Pitch = -0.85f;
            }

            return pos - new Vector3(0, _warpZoom, 0);
        }

        private float GetWarpZoomMax()
        {
            return _zoomLevel / 7f * WARP_ZOOM_MAX;
        }
        private float GetWarpZoomSpeed()
        {
            return _zoomLevel / 7f * WARP_ZOOM_SPEED;
        }
    }
}
