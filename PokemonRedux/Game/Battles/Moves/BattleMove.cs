﻿using PokemonRedux.Game.Data;
using PokemonRedux.Game.Pokemons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Core;

namespace PokemonRedux.Game.Battles.Moves
{
    abstract class BattleMove
    {
        private static Dictionary<string, BattleMove> _moveBuffer;

        private static void InitializeMoveBuffer()
        {
            if (_moveBuffer == null)
            {
                _moveBuffer = typeof(BattleMove).Assembly.GetTypes()
                    .Where(t => t.GetCustomAttributes<BattleMoveAttribute>().Count() > 0)
                    .ToDictionary(t => t.GetCustomAttribute<BattleMoveAttribute>().Name,
                                  t => (BattleMove)Activator.CreateInstance(t));
            }
        }

        public static BattleMove Get(string name)
        {
            InitializeMoveBuffer();
            return _moveBuffer[name];
        }

        public static string GetMoveName<MoveClass>() where MoveClass : BattleMove
        {
            InitializeMoveBuffer();
            var searchType = typeof(MoveClass);

            var move = _moveBuffer.Values.FirstOrDefault(m => m.GetType() == searchType);
            if (move == null)
            {
                throw new Exception($"No move for the move class {searchType.Name} found.");
            }
            return move.Name;
        }

        private MoveData _data;

        protected BattleMove()
        {
            // set data
            var name = GetType().GetCustomAttribute<BattleMoveAttribute>().Name;
            _data = MoveData.Get(name);
        }

        public string Name => _data.name;

        public virtual int Priority => 0; // -1 - 2
        public virtual bool IncreasedCriticalHitRatio => false;
        public virtual bool KingsRockAffected => true;
        public virtual double Accuracy => 1; // 0-1
        public virtual double EffectChance => 0; // 0-1
        public virtual bool ShouldShowAnimation => Controller.GameOptions.BattleAnimations;

        public virtual int GetHitAmount() => 1;
        public virtual PokemonType GetType(BattlePokemon user) => DataHelper.ParseEnum<PokemonType>(_data.type);
        public virtual int GetBasePower(BattlePokemon user, BattlePokemon target) => _data.attk;
        public virtual MoveCategory GetCategory(BattlePokemon user) =>
            PokemonTypeHelper.IsPhysical(GetType(user)) ? MoveCategory.Physical : MoveCategory.Special;

        // returns true for all non-status moves, false if a status move does not succeed, true if it does
        public virtual bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            throw new NotImplementedException($"This move ({Name}) does not have a secondary effect.");
        }
        public abstract void ShowAnimation(BattlePokemon user, BattlePokemon target);
        public virtual bool StatusMoveCheck(BattlePokemon user, BattlePokemon target) => true;
        public virtual bool HasAccuracyCheck(BattlePokemon user) => true;
        public virtual void MoveFailed(BattlePokemon user, BattlePokemon target) { /** move failed to hit target **/ }
        public virtual bool ShouldShowUsedText(BattlePokemon user) => true; // controls whether "<pokemon> used <move>" is shown
        public virtual bool ShouldConsumePP(BattlePokemon user) => true; // reduces PP when used, used to control e.g. Fly not consuming two PP
    }
}
