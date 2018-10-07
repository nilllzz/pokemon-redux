using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Data;
using PokemonRedux.Game.Items;
using System.Linq;
using static Core;

namespace PokemonRedux.Game.Pokemons
{
    class Pokemon
    {
        public const int MAX_LEVEL = 100;
        public const int MAX_MOVES = 4;
        public const int TYPES_AMOUNT = 17;
        public const int MAX_NAME_LENGTH = 10;

        private PokemonData _data;
        private PokemonSaveData _saveData;

        private Pokemon(PokemonData data, PokemonSaveData saveData)
        {
            _data = data;
            _saveData = saveData;
        }

        public static Pokemon Get(PokemonSaveData saveData)
        {
            var data = PokemonData.Get(saveData.id);
            var p = new Pokemon(data, saveData);
            p.Level = PokemonStatHelper.GetLevelForExperience(p._data.ExperienceType, p._saveData.experience);
            return p;
        }

        public static Pokemon Get(int id, int level)
        {
            // generate save data
            var saveData = PokemonSaveData.GenerateNew();
            saveData.id = id;
            var p = Get(saveData);
            p.Experience = PokemonStatHelper.GetExperienceForLevel(p._data.ExperienceType, level);
            p.HP = p.MaxHP;

            // learn moves up to level
            var moves = p._data.moves
                .Where(m => !m.breeding && !m.tm && m.level <= level)
                .Select(m => m.ToPokemonMoveData());
            var learned = p._saveData.moves.ToList();
            var i = learned.Count;
            if (i == MAX_MOVES)
            {
                i = 0;
            }
            foreach (var learnMove in moves)
            {
                if (learned.All(m => m.name != learnMove.name))
                {
                    if (learned.Count < MAX_MOVES)
                    {
                        learned.Add(learnMove);
                    }
                    else
                    {
                        learned[i] = learnMove;
                    }
                    i++;
                    if (i == MAX_MOVES)
                    {
                        i = 0;
                    }
                }
            }
            p._saveData.moves = learned.ToArray();

            return p;
        }

        public int Id => _data.id;
        public string Name => _data.name;
        public string DisplayName => _saveData.nickname ?? Name;
        public string Nickname => _saveData.nickname;
        public bool IsShiny => _saveData.IsShiny;
        public PokemonGender Gender => _saveData.GetGender(_data.GenderRatio);
        public int Experience
        {
            get => _saveData.experience;
            set
            {
                var newExp = value;
                var maxExp = PokemonStatHelper.GetExperienceForLevel(_data.ExperienceType, MAX_LEVEL);

                _saveData.experience = MathHelper.Clamp(newExp, 0, maxExp);
                Level = PokemonStatHelper.GetLevelForExperience(_data.ExperienceType, _saveData.experience);
            }
        }
        public ExperienceType ExperienceType => _data.ExperienceType;
        // needed exp for next level up
        public int NeededExperience
        {
            get
            {
                if (Level == MAX_LEVEL)
                {
                    return 0;
                }
                else
                {
                    return PokemonStatHelper.GetExperienceForLevel(_data.ExperienceType, Level + 1) - Experience;
                }
            }
        }

        public int Level { get; private set; }
        public int HP
        {
            get => _saveData.hp;
            set => _saveData.hp = MathHelper.Clamp(value, 0, MaxHP);
        }
        public int MaxHP => PokemonStatHelper.CalcHPStat(Level, _data.baseStats[0], _saveData.DVHP, _saveData.ev[0]);
        public int Attack => PokemonStatHelper.CalcStat(Level, _data.baseStats[1], _saveData.DVAttack, _saveData.ev[1]);
        public int Defense => PokemonStatHelper.CalcStat(Level, _data.baseStats[2], _saveData.DVDefense, _saveData.ev[2]);
        public int SpecialAttack => PokemonStatHelper.CalcStat(Level, _data.baseStats[3], _saveData.DVSpecial, _saveData.ev[3]);
        public int SpecialDefense => PokemonStatHelper.CalcStat(Level, _data.baseStats[4], _saveData.DVSpecial, _saveData.ev[3]);
        public int Speed => PokemonStatHelper.CalcStat(Level, _data.baseStats[5], _saveData.DVSpeed, _saveData.ev[4]);
        public PokemonStatus Status
        {
            get => DataHelper.ParseEnum<PokemonStatus>(_saveData.status);
            set => _saveData.status = DataHelper.EnumToString(value);
        }
        public Item HeldItem
        {
            get
            {
                if (_saveData.item == null)
                {
                    return null;
                }
                return Item.Get(_saveData.item);
            }
            set => _saveData.item = value?.Name;
        }
        public string ItemData
        {
            get => _saveData.itemData;
            set => _saveData.itemData = value;
        }
        public PokemonType Type1 => DataHelper.ParseEnum<PokemonType>(_data.type1);
        public PokemonType Type2 => DataHelper.ParseEnum<PokemonType>(_data.type2);
        public string OT => _saveData.ot;
        public int TrainerID => _saveData.trainerId;
        public PokemonMoveData[] Moves => _saveData.moves;
        public bool IsEgg => _saveData.eggCycle > 0;
        public bool IsUnown => UnownHelper.UNOWN_ID == _data.id;
        public int UnownLetter => UnownHelper.GetUnownLetterFromDV(_saveData.dv);

        public bool IsOwn => TrainerID == Controller.ActivePlayer.IDNo; // if the trainer has caught this pokemon
        public int BaseExperience => _data.experienceYield;
        public bool CanBattle => Status != PokemonStatus.FNT && !IsEgg;
        public double FleeRate => _data.wildFleeRate;
        public PokemonType HiddenPowerType => _saveData.GetHiddenPowerType();
        public int HiddenPowerBasePower => _saveData.GetHiddenPowerBasePower();

        internal void SwapMoves(int moveIndex1, int moveIndex2)
        {
            var move1 = _saveData.moves[moveIndex1];
            var move2 = _saveData.moves[moveIndex2];

            _saveData.moves[moveIndex1] = move2;
            _saveData.moves[moveIndex2] = move1;
        }

        public void SetOT(int trainerId, string ot)
        {
            if (_saveData.ot == PokemonSaveData.DEFAULT_OT)
            {
                _saveData.ot = ot;
                _saveData.trainerId = trainerId;
            }
        }

        // creates a shallow clone of the save data, no one but this class can write to it
        public PokemonSaveData GetSaveData()
        {
            return (PokemonSaveData)_saveData.Clone();
        }

        // returns all learnable moves by level up for the spe
        public MovesetEntryData[] GetMovesForCurrentLevel()
        {
            return _data.moves.Where(m => !m.tm && !m.breeding && m.level == Level).ToArray();
        }

        public void AddMove(params PokemonMoveData[] moves)
        {
            _saveData.moves = _saveData.moves.Concat(moves).ToArray();
        }

        public void RemoveMove(int index)
        {
            var moves = _saveData.moves.ToList();
            moves.RemoveAt(index);
            _saveData.moves = moves.ToArray();
        }

        #region Sprites and palettes

        public Color[] GetPalette()
            => PokemonTextureManager.GetPalette(IsShiny ? _data.colors.shiny : _data.colors.normal);

        public Texture2D GetFrontSprite() => GetFrontSprite(GetPalette());
        public Texture2D GetFrontSprite(Color[] palette) => PokemonTextureManager.GetFront(Id, palette, UnownLetter);
        public Texture2D GetBackSprite() => GetBackSprite(GetPalette());
        public Texture2D GetBackSprite(Color[] palette) => PokemonTextureManager.GetBack(Id, palette, UnownLetter);
        public Texture2D GetMenuSprite() => PokemonTextureManager.GetMenu(Id);

        #endregion
    }
}
