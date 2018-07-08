using GameDevCommon.Rendering;
using Microsoft.Xna.Framework;
using PokemonRedux.Game.Overworld.Entities;
using System;

namespace PokemonRedux.Screens.Overworld
{
    class WorldShader : BasicShader
    {
        private const int ENCOUNTER_WHITE_MULTIPLIER = 2;
        private const float ENCOUNTER_ANIMATION_WHITE = 1f / ENCOUNTER_WHITE_MULTIPLIER;
        private const float ENCOUNTER_ANIMATION_SPEED = 0.1f;

        private readonly WorldScreen _screen;

        private bool _encounterAnimationStarted = false;
        private bool _reverseEncounterAnimation = true;
        private float _encounterAnimationState = ENCOUNTER_ANIMATION_WHITE;
        private int _encounterAnimationCounter = 0;

        public event Action EncounterAnimationFinished;

        public WorldShader(WorldScreen screen)
        {
            _screen = screen;
        }

        public void StartEncounter()
        {
            _encounterAnimationStarted = true;
            _encounterAnimationState = ENCOUNTER_ANIMATION_WHITE;
            _reverseEncounterAnimation = true;
            _encounterAnimationCounter = 0;
        }

        public void Update()
        {
            if (_encounterAnimationStarted)
            {
                if (!_reverseEncounterAnimation)
                {
                    _encounterAnimationState += ENCOUNTER_ANIMATION_SPEED;
                    if (_encounterAnimationState >= 1f)
                    {
                        _encounterAnimationState = 1f;
                        _reverseEncounterAnimation = true;
                        _encounterAnimationCounter++;
                    }
                }
                else
                {
                    _encounterAnimationState -= ENCOUNTER_ANIMATION_SPEED;
                    if (_encounterAnimationCounter == 3 && _encounterAnimationState <= ENCOUNTER_ANIMATION_WHITE)
                    {
                        _encounterAnimationStarted = false;
                        EncounterAnimationFinished?.Invoke();
                    }
                    else if (_encounterAnimationState <= -0.5f)
                    {
                        _encounterAnimationState = 0f;
                        _reverseEncounterAnimation = false;
                    }
                }
            }
        }

        public override void ApplyPass(I3DObject obj)
        {
            if (_encounterAnimationStarted)
            {
                if (obj is PlayerCharacter)
                {
                    BE.DiffuseColor = Vector3.One;
                }
                else
                {
                    BE.DiffuseColor = new Vector3(_encounterAnimationState * ENCOUNTER_WHITE_MULTIPLIER);
                }
            }
            else
            {
                BE.DiffuseColor = Vector3.One;
            }

            base.ApplyPass(obj);
        }
    }
}
