using GameDevCommon.Input;
using GameDevCommon.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PokemonRedux.Content;
using PokemonRedux.Game.Data;
using PokemonRedux.Game.Data.Entities;
using PokemonRedux.Game.Overworld.Entities;
using PokemonRedux.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using static Core;

namespace PokemonRedux.Game.Overworld
{
    class World
    {
        private static Random _random = new Random();

        private List<Map> _loadedMaps = new List<Map>();
        private RenderObjectCollection<Entity> _entities = new RenderObjectCollection<Entity>();
        private double _daytimeDelayCheck = 0;
        private Daytime _daytime;

        // warp
        private const float FADE_SPEED = 0.05f;
        private WarpData _warpData;
        private bool _warping = false;
        private bool _fadingOut = false;
        private bool _fadingIn = false;

        // if the world camera should zoom in (for warps)
        public bool ZoomCamera { get; set; }
        public float FadeAlpha { get; private set; }

        public PlayerCharacter PlayerEntity { get; private set; }
        public Map ActiveMap { get; private set; }
        public Daytime Daytime
        {
            get => _daytime;
            set
            {
                if (_daytime != value)
                {
                    _daytime = value;
                    for (var i = 0; i < _entities.Count; i++)
                    {
                        _entities[i].LoadTexture();
                    }
                }
            }
        }
        public ScriptManager ScriptManager { get; private set; }

        public bool PlayerCanMove => !_warping && !IsPaused;

        public bool IsPaused { get; set; }

        public event Action MapChanged;
        public event Action<EncounterResult> WildPokemonEncountered;
        public event Action<string> TextDisplayed;

        public World()
        {
            _daytime = DetermineDaytime();
        }

        public void LoadContent()
        {
            PlayerEntity = new PlayerCharacter();
            PlayerEntity.LoadContent();
            AddEntity(PlayerEntity);

            ScriptManager = new ScriptManager();
        }

        public void WarpTo(WarpData warpData)
        {
            _warpData = warpData;
            _warping = true;
            _fadingOut = true;
            _fadingIn = false;
            FadeAlpha = 0f;
            ZoomCamera = true;
        }

        public void ChangeMap(string mapFile)
        {
            mapFile = mapFile.ToLower();
            if (ActiveMap == null || ActiveMap.MapFile != mapFile)
            {
                if (_loadedMaps.Any(l => l.MapFile == mapFile))
                {
                    ActiveMap = _loadedMaps.First(l => l.MapFile == mapFile);
                }
                else
                {
                    ActiveMap = new Map(this, mapFile);
                    ActiveMap.LoadContent();
                    _loadedMaps.Add(ActiveMap);
                }

                // load attached maps
                if (ActiveMap.LoadMaps != null)
                {
                    foreach (var map in ActiveMap.LoadMaps)
                    {
                        LoadMap(map);
                    }
                }
                // clear all maps that are not needed anymore
                ClearLoaded(ActiveMap.LoadMaps);

                PlayerEntity.Map = ActiveMap;

                Daytime = DetermineDaytime();
                MapChanged?.Invoke();
            }
        }

        private void LoadMap(string mapFile)
        {
            mapFile = mapFile.ToLower();
            if (!_loadedMaps.Any(l => l.MapFile == mapFile))
            {
                var map = new Map(this, mapFile);
                map.LoadContent();

                _loadedMaps.Add(map);
            }
        }

        public void ClearLoaded(string[] keep)
        {
            var keepLower = keep?.Select(k => k.ToLower());
            for (var i = 0; i < _loadedMaps.Count; i++)
            {
                if (_loadedMaps[i] != ActiveMap &&
                    (keepLower == null || !keepLower.Contains(_loadedMaps[i].MapFile)))
                {
                    DisposeMap(_loadedMaps[i]);
                    i--;
                }
            }
        }

        public void AddEntity(Entity entity)
        {
            _entities.Add(entity);
        }

        public void DisposeMap(Map map)
        {
            // disposes of all entities belonging to a map
            for (var i = 0; i < _entities.Count; i++)
            {
                if (_entities[i].Map == map && !(_entities[i] is PlayerCharacter))
                {
                    _entities[i].Dispose();
                    _entities.RemoveAt(i);
                    i--;
                }
            }
            _loadedMaps.Remove(map);

            if (ActiveMap == map)
            {
                ActiveMap = null;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (_warping)
            {
                UpdateWarping();
            }
            else
            {
                UpdateMap(gameTime);
            }
        }

        private void UpdateMap(GameTime gameTime)
        {
#if DEBUG
            var kHandler = GetComponent<KeyboardHandler>();
            if (kHandler.KeyPressed(Keys.R))
            {
                ActiveMap.Reload();
            }
#endif

            UpdateDaytime(gameTime);

            for (var i = 0; i < _entities.Count; i++)
            {
                _entities[i].Update();
            }

            _entities.Sort();
        }

        private void UpdateWarping()
        {
            if (_fadingOut)
            {
                if (FadeAlpha < 1f)
                {
                    FadeAlpha += FADE_SPEED;
                    if (FadeAlpha >= 1f)
                    {
                        FadeAlpha = 1f;
                        _fadingOut = false;
                        _fadingIn = true;
                        ZoomCamera = false;

                        // load next map
                        ChangeMap(_warpData.map);
                        // reposition player with map offset and warp data
                        PlayerEntity.Position = _warpData.Position + ActiveMap.Offset;
                        // update and sort to get visuals right
                        PlayerEntity.Update();
                        _entities.Sort();
                    }
                }
            }
            else if (_fadingIn)
            {
                if (FadeAlpha > 0f)
                {
                    FadeAlpha -= FADE_SPEED;
                    if (FadeAlpha <= 0f)
                    {
                        FadeAlpha = 0f;
                        _fadingIn = false;
                        _warping = false;
                        PlayerEntity.WarpComplete();
                    }
                }
            }
        }

        private void UpdateDaytime(GameTime gameTime)
        {
            var kHandler = GetComponent<KeyboardHandler>();
            if (kHandler.KeyPressed(Keys.T))
            {
                switch (Daytime)
                {
                    case Daytime.Morning:
                        Daytime = Daytime.Day;
                        break;
                    case Daytime.Day:
                        Daytime = Daytime.Night;
                        break;
                    case Daytime.Night:
                        Daytime = Daytime.Morning;
                        break;
                }
            }

            _daytimeDelayCheck += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_daytimeDelayCheck >= 1000)
            {
                _daytimeDelayCheck = 0;
                Daytime = DetermineDaytime();
            }
        }

        public void Draw(Shader shader)
        {
            Controller.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            foreach (var e in _entities.OpaqueObjects)
            {
                shader.Render(e);
            }
            Controller.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            foreach (var e in _entities.TransparentObjects)
            {
                shader.Render(e);
            }
        }

        public Entity GetFloor(Vector3 position, Entity other)
        {
            Entity floor = null;
            var height = float.MinValue;
            var v2Pos = new Vector2(position.X, position.Z);

            foreach (var ent in _entities)
            {
                if (ent.IsValidFloor(position, other))
                {
                    var entHeight = ent.GetHeightForPosition(v2Pos);
                    if (entHeight > height)
                    {
                        floor = ent;
                        height = entHeight;
                    }
                }
            }

            return floor;
        }

        public Entity GetCollision(CollisionType collisionType, Vector3 position, Vector3 size, Entity other)
        {
            Entity ent = null;
            for (var i = 0; i < _entities.Count; i++)
            {
                if (_entities[i] != other && _entities[i].DoesCollide(collisionType, position, size, other))
                {
                    ent = _entities[i];
                    // prioritize doors and scripts
                    if (ent is Door || ent is ScriptTrigger)
                    {
                        return ent;
                    }
                }
            }
            return ent;
        }

        public bool IsInGrass(Vector3 position, Vector3 size, Entity other)
        {
            var grass = _entities.Where(e => e is Grass).ToArray();
            for (var i = 0; i < grass.Length; i++)
            {
                if (grass[i].DoesCollide(CollisionType.Grass, position, size, other))
                {
                    return true;
                }
            }
            return false;
        }

        public void StartScript(string file)
        {
            if (!ScriptManager.IsActive)
            {
                var source = Controller.Content.LoadDirect<string>($"Data/Scripts/{file}");
                ScriptManager.RunScript(source);
            }
        }

        public void ShowText(string text)
        {
            TextDisplayed?.Invoke(text);
        }

        public static Daytime DetermineDaytime()
        {
            var hour = DateTime.Now.Hour;

            if (hour >= 18 || hour < 4)
            {
                return Daytime.Night;
            }
            else if (hour >= 4 && hour < 10)
            {
                return Daytime.Morning;
            }
            else
            {
                return Daytime.Day;
            }
        }

        // returns whether a wild encounter has started
        public bool TryWildEncounter(int steps)
        {
            if (_random.Next(0, 250) == 0 && steps > 20)
            {
                var encounter = Encounter.Get(ActiveMap.EncounterData);
                var result = encounter.GetResult(EncounterMethod.Grass, _daytime);
                if (result.HasValue)
                {
                    WildPokemonEncountered?.Invoke(result.Value);
                }
                return true;
            }
            return false;
        }
    }
}
