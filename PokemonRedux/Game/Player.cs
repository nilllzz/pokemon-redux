using Microsoft.Xna.Framework;
using PokemonRedux.Game.Data;
using PokemonRedux.Game.Items;
using PokemonRedux.Game.Overworld.Entities;
using PokemonRedux.Game.Pokemons;
using System;
using System.Linq;
using static Core;

namespace PokemonRedux.Game
{
    class Player
    {
        private const int MAX_HALL_OF_FAME_ENTRIES = 180; // max amount of entries to be stored simultaniously
        private const int MAX_HALL_OF_FAME_ENTRY_COUNT = 999; // highest number a hall of fame entry can display as "x-time famer"
        public const int MAX_POKEMON = 6;
        public const int MAX_NAME_LENGTH = 7;

        private PlayerData _data;
        private TimeSpan _elapsedSinceLastSave; // elapsed time since last game save

        public string Name => _data.name;
        public int[] Badges => _data.badges;
        public int IDNo => _data.idNo;
        public int Money
        {
            get => _data.money;
            set => _data.money = MathHelper.Clamp(_data.money + value, 0, 999999);
        }
        public TimeSpan TimePlayed =>
            _elapsedSinceLastSave + new TimeSpan(0, 0, _data.secondsPlayed);
        public int[] PokedexSeen => _data.pokedexSeen; // only includes non-caught seen pokemon
        public int[] PokedexCaught => _data.pokedexCaught;
        public bool VisitedKanto => _data.visitedKanto;
        public string Map => _data.map;
        public Vector3 Position => DataHelper.GetVector3(_data.position, 0f);
        public bool HasPokedex
        {
            get => _data.hasPokedex;
            set => _data.hasPokedex = value;
        }
        public bool HasPokegear
        {
            get => _data.hasPokegear;
            set => _data.hasPokegear = value;
        }
        public bool HasMapModule
        {
            get => _data.hasMapModule;
            set => _data.hasMapModule = value;
        }
        public bool HasRadioModule
        {
            get => _data.hasRadioModule;
            set => _data.hasRadioModule = value;
        }
        public bool HasUnownMode
        {
            get => _data.hasUnownMode;
            set => _data.hasUnownMode = value;
        }
        public string[] Contacts => _data.contacts;
        public int[] UnownsCaught => _data.unownsCaught;
        public HallOfFameEntryData[] HallOfFame => _data.hallOfFame;
        public int ActiveBoxIndex
        {
            get => _data.activeBox;
            set => _data.activeBox = value;
        }
        public Mail[] Mails => _data.mails.Select(m => Mail.Get(m)).ToArray();

        public StorageBox[] Boxes { get; private set; }
        public Pokemon[] PartyPokemon { get; private set; }
        public MenuStates MenuStates { get; private set; }

        public void Update(GameTime gameTime)
        {
            _elapsedSinceLastSave += gameTime.ElapsedGameTime;
        }

        public void Load(PlayerData data = null)
        {
            if (data == null)
            {
                _data = PlayerData.Load();
            }
            else
            {
                _data = data;
            }

            // load pokemon from save data
            PartyPokemon = _data.pokemon.Select(p => Pokemon.Get(p)).ToArray();

            // load boxes from save data
            Boxes = _data.boxes.Select(b => new StorageBox(b)).ToArray();

            _elapsedSinceLastSave = new TimeSpan();

            MenuStates = new MenuStates();
            MenuStates.Reset();
        }

        public void Save(PlayerCharacter playerEntity)
        {
            var pos = playerEntity.Position - playerEntity.Map.Offset;
            _data.position = new[]
            {
                pos.X,
                pos.Y,
                pos.Z,
            };
            _data.map = playerEntity.Map.MapFile;
            _data.secondsPlayed = (int)TimePlayed.TotalSeconds;

            _data.pokemon = PartyPokemon.Select(p => p.GetSaveData()).ToArray();

            _data.Save();

            _elapsedSinceLastSave = new TimeSpan();

            // also save current global options when player data is saved
            Controller.GameOptions.Save();
        }

        public void SeenPokemon(int id)
        {
            if (!_data.pokedexSeen.Contains(id))
            {
                var seen = _data.pokedexSeen.ToList();
                seen.Add(id);
                _data.pokedexSeen = seen.ToArray();
            }
        }

        public void CaughtPokemon(int id)
        {
            // remove from seen first
            if (_data.pokedexSeen.Contains(id))
            {
                var seen = _data.pokedexSeen.ToList();
                seen.Remove(id);
                _data.pokedexSeen = seen.ToArray();
            }

            if (!_data.pokedexCaught.Contains(id))
            {
                var caught = _data.pokedexCaught.ToList();
                caught.Add(id);
                _data.pokedexCaught = caught.ToArray();
            }
        }

        public (string hours, string minutes) GetDisplayTime()
        {
            var hours = TimePlayed.TotalHours;
            var minutes = TimePlayed.Minutes;
            if (hours > 999)
            {
                hours = 999;
                minutes = (int)(TimePlayed.TotalMinutes - (999 * 60));
                if (minutes > 99)
                {
                    minutes = 99;
                }
            }

            return (Math.Floor(hours).ToString(), minutes.ToString("D2"));
        }

        public void DeleteContact(string name)
        {
            if (_data.contacts.Contains(name))
            {
                var contacts = _data.contacts.ToList();
                contacts.Remove(name);
                _data.contacts = contacts.ToArray();
            }
        }

        public Item[] GetItems(bool stored)
        {
            var items = stored ? _data.storedItems : _data.items;
            return items.Select(i =>
            {
                var item = Item.Get(i.name);
                item.Amount = i.amount;
                return item;
            }).ToArray();
        }

        public void AddItem(string name, int amount)
        {
            if (_data.items.Any(i => i.name == name))
            {
                _data.items.First(i => i.name == name).amount += amount;
            }
            else
            {
                var items = _data.items.ToList();
                items.Add(new PlayerItemData
                {
                    name = name,
                    amount = amount
                });
                _data.items = items.ToArray();
            }
        }

        public void RemoveItem(string name, int amount)
        {
            if (_data.items.Any(i => i.name == name))
            {
                var item = _data.items.First(i => i.name == name);
                if (item.amount <= amount)
                {
                    // 0 or less: remove item entirely
                    var items = _data.items.ToList();
                    items.Remove(item);
                    _data.items = items.ToArray();
                }
                else
                {
                    // deduct amount
                    item.amount -= amount;
                }
            }
        }

        public void SwapItems(string name1, string name2)
        {
            if (_data.items.Any(i => i.name == name1) && _data.items.Any(i => i.name == name2))
            {
                var item1 = _data.items.First(i => i.name == name1);
                var item2 = _data.items.First(i => i.name == name2);

                var items = _data.items.ToList();
                var index1 = items.IndexOf(item1);
                var index2 = items.IndexOf(item2);

                _data.items[index1] = item2;
                _data.items[index2] = item1;
            }
        }

        public void WithdrawItem(string name, int amount)
        {
            if (_data.storedItems.Any(i => i.name == name))
            {
                var item = _data.storedItems.First(i => i.name == name);
                if (item.amount <= amount)
                {
                    // 0 or less: remove item entirely
                    var items = _data.storedItems.ToList();
                    items.Remove(item);
                    _data.storedItems = items.ToArray();
                }
                else
                {
                    // deduct amount
                    item.amount -= amount;
                }
            }
        }

        public void DepositItem(string name, int amount)
        {
            if (_data.storedItems.Any(i => i.name == name))
            {
                _data.storedItems.First(i => i.name == name).amount += amount;
            }
            else
            {
                var items = _data.storedItems.ToList();
                items.Add(new PlayerItemData
                {
                    name = name,
                    amount = amount
                });
                _data.storedItems = items.ToArray();
            }
        }

        public void SwapPokemon(int pIndex1, int pIndex2)
        {
            var pokemon1 = PartyPokemon[pIndex1];
            var pokemon2 = PartyPokemon[pIndex2];

            PartyPokemon[pIndex1] = pokemon2;
            PartyPokemon[pIndex2] = pokemon1;
        }

        public void MovePokemon(int oldIndex, int newIndex)
        {
            var party = PartyPokemon.ToList();
            var pokemon = party[oldIndex];
            party.RemoveAt(oldIndex);
            // shift index to accomodate for removed item
            if (newIndex > oldIndex)
            {
                newIndex--;
            }
            party.Insert(newIndex, pokemon);
            PartyPokemon = party.ToArray();
        }

        // index of -1 means add to end, anything else inserts at index location
        public void AddPokemon(Pokemon pokemon, int insertIndex = -1)
        {
            pokemon.SetOT(IDNo, Name);

            // TODO: handle PC storage system
            var party = PartyPokemon.ToList();
            if (insertIndex == -1)
            {
                party.Add(pokemon);
            }
            else
            {
                party.Insert(insertIndex, pokemon);
            }
            PartyPokemon = party.ToArray();

            // register as caught if not an egg
            if (!pokemon.IsEgg)
            {
                CaughtPokemon(pokemon.Id);
            }

            // register unown type
            if (pokemon.IsUnown)
            {
                var unownType = pokemon.UnownLetter;
                if (!_data.unownsCaught.Contains(unownType))
                {
                    var unownsCaught = _data.unownsCaught.ToList();
                    unownsCaught.Add(unownType);
                    _data.unownsCaught = unownsCaught.ToArray();
                }
            }
        }

        public void RemoveFromParty(int index)
        {
            var party = PartyPokemon.ToList();
            party.RemoveAt(index);
            PartyPokemon = party.ToArray();
        }

        // registers the current party as hall of fame pokemon
        public void RegisterHallOfFame()
        {
            var hallOfFame = _data.hallOfFame.ToList();
            var entryNumber = 1;
            if (_data.hallOfFame.Length > 0)
            {
                entryNumber = MathHelper.Clamp(_data.hallOfFame.Max(e => e.number) + 1, 1, MAX_HALL_OF_FAME_ENTRY_COUNT);
            }

            // reverse order for party pokemon so they appear in order on the screen
            var party = PartyPokemon.Reverse();
            foreach (var partyPokemon in party)
            {
                var newEntry = HallOfFameEntryData.CreateFromPokemon(entryNumber, partyPokemon);
                if (hallOfFame.Count == MAX_HALL_OF_FAME_ENTRIES)
                {
                    hallOfFame.RemoveAt(0); // first in first out
                }
                hallOfFame.Add(newEntry);
            }

            _data.hallOfFame = hallOfFame.ToArray();
        }

        public void AddMailToPC(string mailTemplate, string itemData, int pokemonId)
        {
            var mail = Mail.Get(mailTemplate, itemData);
            // set the pokemon id
            mail.PokemonId = pokemonId;
            var mails = _data.mails.ToList();
            mails.Add(mail.Data);
            _data.mails = mails.ToArray();
        }

        public void RemoveMailFromPC(Mail mail)
        {
            var mails = _data.mails.ToList();
            mails.Remove(mail.Data);
            _data.mails = mails.ToArray();
        }
    }
}
