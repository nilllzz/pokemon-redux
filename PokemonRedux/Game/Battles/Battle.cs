using PokemonRedux.Game.Battles.AIs;
using PokemonRedux.Game.Battles.Moves;
using PokemonRedux.Game.Data;
using PokemonRedux.Game.Pokemons;
using PokemonRedux.Screens.Battles;
using PokemonRedux.Screens.Battles.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Core;

namespace PokemonRedux.Game.Battles
{
    // contains state of a battle
    class Battle
    {
        // TODO: replace move name comparisons with (move is class) calls
        // TODO: replace all item name comparisons with (item is class) calls

        public static Battle ActiveBattle { get; set; }

        private int _runAttempts; // amount of times the player tried to run
        // maps the player's pokemon to enemy's pokemon, which have participated in a battle against each
        private readonly Dictionary<Pokemon, List<Pokemon>> _participation = new Dictionary<Pokemon, List<Pokemon>>();
        private readonly AI _enemyAI;

        // the active thread the battle runs in 
        public Thread BattleThread { get; private set; }
        public Random Random { get; } = new Random();
        public IBattleUI UI { get; }
        public IBattleAnimationController AnimationController { get; }

        public BattlePokemon PlayerPokemon { get; set; }
        public BattlePokemon EnemyPokemon { get; set; }
        public bool IsWildBattle { get; }
        public int WeatherTurns { get; set; }
        public Weather CurrentWeather { get; set; } = Weather.Clear;

        public Battle(IBattleUI ui, IBattleAnimationController animationController, Pokemon wildPokemon)
        {
            UI = ui;
            AnimationController = animationController;

            EnemyPokemon = new BattlePokemon(wildPokemon, PokemonSide.Enemy);
            IsWildBattle = true;
            _enemyAI = new WildAI();

            // first pokemon in party is starting player pokemon
            PlayerPokemon = new BattlePokemon(Controller.ActivePlayer.PartyPokemon[0], PokemonSide.Player);
        }

        public void StartRound(BattleAction playerAction)
        {
            // run in secondary thread, wait for player input on main thread if needed
            BattleThread = new Thread(() =>
            {
                RecordParticipation();

                var enemyAction = _enemyAI.TakeAction();
                var playerGoesFirst = true;

                // order of actions:
                // prioritize player run attempts and item usages
                // then, do player switches, except when the enemy pokemon uses pursuit
                // then, do enemy switches, except when the player pokemon uses pursuit
                // then, do enemy item uses
                // if both pokemon use moves, prioritize based on speed and moves used
                // finally, do enemy wild pokemon run attempts

                // both pokemon used a move 
                if (playerAction.ActionType == BattleActionType.Move &&
                    enemyAction.ActionType == BattleActionType.Move)
                {
                    var playerMove = BattleMove.Get(playerAction.MoveName);
                    var enemyMove = BattleMove.Get("SLEEP POWDER"); //BattleMove.Get(enemyAction.MoveName);

                    playerGoesFirst = PlayerMoveGoesFirst(playerMove, enemyMove);
                    if (playerGoesFirst)
                    {
                        var secondaryUser = EnemyPokemon;
                        UseMove(playerMove, PlayerPokemon, EnemyPokemon);
                        EndOfAttack(PlayerPokemon);
                        // only use move if user has not changed
                        // also, if the target fainted during its turn, do not act
                        if (secondaryUser == EnemyPokemon &&
                            PlayerPokemon.Pokemon.Status != PokemonStatus.FNT &&
                            EnemyPokemon.Pokemon.Status != PokemonStatus.FNT)
                        {
                            UseMove(enemyMove, EnemyPokemon, PlayerPokemon);
                            EndOfAttack(EnemyPokemon);
                        }
                    }
                    else
                    {
                        var secondaryUser = PlayerPokemon;
                        UseMove(enemyMove, EnemyPokemon, PlayerPokemon);
                        EndOfAttack(EnemyPokemon);
                        // only use move if user has not changed
                        // also, if the target fainted during its turn, do not act
                        if (secondaryUser == PlayerPokemon &&
                            PlayerPokemon.Pokemon.Status != PokemonStatus.FNT &&
                            EnemyPokemon.Pokemon.Status != PokemonStatus.FNT)
                        {
                            UseMove(playerMove, PlayerPokemon, EnemyPokemon);
                            EndOfAttack(PlayerPokemon);
                        }
                    }
                }
                else
                {
                    var usedPursuit = false; // tracks if pursuit has been used

                    // player attempts to run
                    if (playerAction.ActionType == BattleActionType.Run)
                    {
                        var canRun = AttemptRun();
                        if (canRun)
                        {
                            UI.ShowMessageAndWait("Got away safely!");
                            UI.EndBattle(true);
                        }
                        else
                        {
                            _runAttempts++;
                            UI.ShowMessageAndWait("Can^'t escape!");
                            EndOfAttack(PlayerPokemon);
                        }
                    }
                    else
                    {
                        _runAttempts = 0;
                    }

                    if (playerAction.ActionType == BattleActionType.Item)
                    {
                        // item action has been taken from menu directly before round start.
                        EndOfAttack(PlayerPokemon);
                    }
                    else if (playerAction.ActionType == BattleActionType.Switch)
                    {
                        // pursuit
                        if (enemyAction.ActionType == BattleActionType.Move &&
                            enemyAction.MoveName == "PURSUIT")
                        {
                            usedPursuit = true;
                            playerGoesFirst = false;
                            var moveUsed = BattleMove.Get(enemyAction.MoveName);
                            UseMove(moveUsed, EnemyPokemon, PlayerPokemon);
                        }

                        if (PlayerPokemon.Pokemon.Status != PokemonStatus.FNT)
                        {
                            UI.ShowMessageAndKeepOpen(PlayerPokemon.GetDisplayName() + ", that^'s\nenough! Come back!", 60);
                            AnimationController.ShowAnimationAndWait(new PokemonSizeChangeAnimation(PlayerPokemon, 1f, 0f, 0.07f));
                            UI.SetPokemonStatusVisible(PokemonSide.Player, false);
                        }

                        var newPokemon = Controller.ActivePlayer.PartyPokemon[playerAction.SwitchToIndex];
                        SendOutPlayerPokemon(newPokemon, false);

                        if (usedPursuit)
                        {
                            EndOfAttack(EnemyPokemon);
                        }
                        EndOfAttack(PlayerPokemon);
                    }

                    if (enemyAction.ActionType == BattleActionType.Item)
                    {
                        // TODO: enemy item usage
                    }
                    else if (enemyAction.ActionType == BattleActionType.Switch)
                    {
                        // pursuit
                        if (playerAction.ActionType == BattleActionType.Move &&
                            playerAction.MoveName == "PURSUIT")
                        {
                            usedPursuit = true;
                        }

                        // TODO: enemy switching
                    }

                    // player used a move, enemy didn't
                    if (playerAction.ActionType == BattleActionType.Move && !usedPursuit)
                    {
                        playerGoesFirst = false;
                        if (EnemyPokemon.Pokemon.Status != PokemonStatus.FNT)
                        {
                            var moveUsed = BattleMove.Get(playerAction.MoveName);
                            UseMove(moveUsed, PlayerPokemon, EnemyPokemon);
                            EndOfAttack(PlayerPokemon);
                        }
                    }

                    // enemy used a move, player didn't
                    if (enemyAction.ActionType == BattleActionType.Move && !usedPursuit)
                    {
                        if (PlayerPokemon.Pokemon.Status != PokemonStatus.FNT)
                        {
                            var moveUsed = BattleMove.Get(enemyAction.MoveName);
                            UseMove(moveUsed, EnemyPokemon, PlayerPokemon);
                            EndOfAttack(EnemyPokemon);
                        }
                    }

                    // when the enemy action is run, it was already determined to be successful
                    if (enemyAction.ActionType == BattleActionType.Run)
                    {
                        UI.ShowMessageAndWait(EnemyPokemon.GetDisplayName() + "\nfled!");
                        UI.EndBattle(true);
                    }
                }

                EndOfTurn(playerGoesFirst);
            });

            BattleThread.Name = "Battle Thread";
            BattleThread.IsBackground = true; // kills the thread when main thread dies
            BattleThread.Start();
        }

        private void RecordParticipation()
        {
            if (!_participation.ContainsKey(EnemyPokemon.Pokemon))
            {
                _participation.Add(EnemyPokemon.Pokemon, new List<Pokemon>());
            }
            if (!_participation[EnemyPokemon.Pokemon].Contains(PlayerPokemon.Pokemon))
            {
                _participation[EnemyPokemon.Pokemon].Add(PlayerPokemon.Pokemon);
            }
        }

        // Run from a wild pokemon.
        // returns true if successful
        private bool AttemptRun()
        {
            // when player pokemon's speed is higher, running is successful
            // also, if the enemy pokemon' speed is 0.
            if (PlayerPokemon.Pokemon.Speed >= EnemyPokemon.Pokemon.Speed)
            {
                return true;
            }

            // calc if running is successful
            var a = PlayerPokemon.Pokemon.Speed;
            var b = (EnemyPokemon.Pokemon.Speed / 4D) % 256;
            if (b == 0)
            {
                return true;
            }
            var c = _runAttempts;
            var x = (int)(a * 32 / b) + (30 * c);
            var r = Random.Next(0, 256);
            return r <= x;
        }

        private void SendOutPlayerPokemon(Pokemon pokemon, bool batonPass)
        {
            var battlePokemon = new BattlePokemon(pokemon, PokemonSide.Player);
            PlayerPokemon.TransferStateTo(battlePokemon, batonPass);

            PlayerPokemon = battlePokemon;

            UI.ShowMessageAndKeepOpen("Go!" + PlayerPokemon.GetDisplayName() + "!", 60);
            AnimationController.ShowAnimation(new PokemonSizeChangeAnimation(ActiveBattle.PlayerPokemon, 0f, 1f, 0.07f), 6);
            AnimationController.ShowAnimationAndWait(new PokeballOpeningAnimation(ActiveBattle.PlayerPokemon));
            AnimationController.SetPokemonVisibility(PokemonSide.Player, true);
            UI.SetPokemonStatusVisible(PokemonSide.Player, true);
        }

        private bool PlayerMoveGoesFirst(BattleMove playerMove, BattleMove enemyMove)
        {
            if (playerMove.Priority > enemyMove.Priority)
            {
                return true;
            }
            else if (playerMove.Priority < enemyMove.Priority)
            {
                return false;
            }
            else // same priority
            {
                // determine quick claw, 23.4% chance
                var playerQuickclaw =
                    PlayerPokemon.Pokemon.HeldItem?.Name == "QUICK CLAW" &&
                    Random.Next(0, 257) <= 60;
                var enemyQuickclaw =
                    EnemyPokemon.Pokemon.HeldItem?.Name == "QUICK CLAW" &&
                    Random.Next(0, 257) <= 60;

                // both quick claws succeed, determine randomly
                if (playerQuickclaw && enemyQuickclaw)
                {
                    return Random.Next(0, 2) == 0;
                }
                else if (playerQuickclaw)
                {
                    return true;
                }
                else if (enemyQuickclaw)
                {
                    return false;
                }

                var playerSpeed = PlayerPokemon.GetStat(PokemonStat.Speed);
                // if the player has the plain badge, it increases the speed by 0.125x
                if (Controller.ActivePlayer.Badges.Contains(BadgeHelper.PLAIN_BADGE))
                {
                    playerSpeed *= 1.125D;
                }
                // divide by 4 if paralyzed
                if (PlayerPokemon.Pokemon.Status == PokemonStatus.PAR)
                {
                    playerSpeed *= 0.25D;
                }

                var enemySpeed = EnemyPokemon.GetStat(PokemonStat.Speed);
                // divide by 4 if paralyzed
                if (EnemyPokemon.Pokemon.Status == PokemonStatus.PAR)
                {
                    enemySpeed *= 0.25D;
                }

                if (playerSpeed > enemySpeed)
                {
                    return true;
                }
                else if (playerSpeed < enemySpeed)
                {
                    return false;
                }
                else
                {
                    // if speed is the same, determine randomly
                    return Random.Next(0, 2) == 0;
                }
            }
        }

        private void UseMove(BattleMove moveUsed, BattlePokemon user, BattlePokemon target, bool disobedientMove = false)
        {
            if (user.Pokemon.Status == PokemonStatus.SLP)
            {
                if (user.SleepTurns == 0)
                {
                    // pokemon wakes up
                    UI.ShowMessageAndWait(user.GetDisplayName() + "\nwoke up."); // TODO: message
                    user.Pokemon.Status = PokemonStatus.OK;
                }
                else
                {
                    user.SleepTurns--;
                    UI.ShowMessageAndWait(user.GetDisplayName() + "\nis fast asleep!");
                    AnimationController.ShowAnimationAndWait(new AsleepAnimation(user));
                    // if pokemon used sleep talk/snore, it will continue the turn
                    if (moveUsed.Name != "SLEEP TALK" && moveUsed.Name != "SNORE")
                    {
                        // pokemon is asleep, skip turn
                        return;
                    }
                }
            }
            else if (user.Pokemon.Status == PokemonStatus.FRZ)
            {
                // frozen pokemon thaw out if the pokemon uses Flame Wheel or Sacred Fire
                if (moveUsed.Name == "FLAME WHEEL" ||
                    moveUsed.Name == "SACRED FIRE")
                {
                    UI.ShowMessageAndWait(user.GetDisplayName() + "\nthawed out!"); // TODO: message
                    user.Pokemon.Status = PokemonStatus.OK;
                }
                // 10% to thaw out and end turn
                else if (Random.Next(0, 10) == 0)
                {
                    UI.ShowMessageAndWait(user.GetDisplayName() + "\nthawed out!"); // TODO: message
                    user.Pokemon.Status = PokemonStatus.OK;
                    return;
                }
                else
                {
                    // still frozen, skip turn
                    UI.ShowMessageAndWait(user.GetDisplayName() + "\nis frozen solid!"); // TODO: message
                    return;
                }
            }
            else
            {
                if (user.Flinched)
                {
                    user.Flinched = false;
                    UI.ShowMessageAndWait(user.GetDisplayName() + "\nflinched!");
                    return;
                }
            }

            // TODO: move this to before choosing a move
            //if (user.HyperbeamRecharge)
            //{
            //    user.HyperbeamRecharge = false;
            //    _screen.ShowMessageAndWait(user.GetDisplayName() + "\n must recharge!");
            //    return;
            //}

            // disable turn reduction
            if (user.DisabledTurns > 0)
            {
                user.DisabledTurns--;
                UI.ShowMessageAndWait(user.GetDisplayName() + "^'s\ndisabled no more!");
            }

            // confusion
            if (user.ConfusionTurns > 0)
            {
                user.ConfusionTurns--;
                if (user.ConfusionTurns == 0)
                {
                    UI.ShowMessageAndWait(user.GetDisplayName() + "^'s\nconfused no more!");
                }
                else
                {
                    UI.ShowMessageAndKeepOpen(user.GetDisplayName() + "\nis confused!");
                    AnimationController.ShowAnimationAndWait(new ConfusedAnimation(user));
                    // confusion, 50% chance of hitting itself
                    if (Random.Next(0, 2) == 0)
                    {
                        // hits self with 40 power move
                        UI.ShowMessageAndKeepOpen("It hurt itself in\nits confusion!");
                        // TODO: confusion hit animation
                        var damage = GetConfusionDamage(user);
                        DealDamage(damage, user, true, false);
                        return; // end turn after hitting self in confusion
                    }
                }
            }

            // infatuation, 50% of skipping turn
            if (user.IsInfatuated && Random.Next(0, 2) == 0)
            {
                UI.ShowMessageAndWait(user.GetDisplayName() + "\nis in love."); // TODO: message
                return;
            }

            // if the move has been disabled by the enemy in the same turn
            if (user.DisabledTurns > 0 && moveUsed.Name == user.DisabledMove.name)
            {
                UI.ShowMessageAndWait(user.GetDisplayName() + "^'s\n" + user.DisabledMove.name + " is\ndisabled!"); // TODO: message
                return;
            }

            // paralyzed, 50% to skip turn
            if (user.Pokemon.Status == PokemonStatus.PAR &&
                Random.Next(0, 2) == 0)
            {
                UI.ShowMessageAndWait(user.GetDisplayName() + "^'s\nfully paralyzed.");
                return;
            }

            // obedience check for player
            if (user == PlayerPokemon)
            {
                var obedienceResult = ObedienceCheck(moveUsed);
                if (!obedienceResult)
                {
                    user.LastUsedMove = null;
                    // TODO: stop encore
                    return;
                }
            }

            if (Controller.ActivePlayer.BattleAnimations)
            {
                UI.ShowMessageAndKeepOpen(user.GetDisplayName() + "\nused " + moveUsed.Name + "!", 20);
            }
            else
            {
                UI.ShowMessageAndWait(user.GetDisplayName() + "\nused " + moveUsed.Name + "!");
            }

            user.ReducePP(moveUsed.Name);

            // determine if move hits or misses
            if (moveUsed.AccuracyCheck)
            {
                var lockedOn = false;
                // dream eater
                if (moveUsed.Name == "DREAM EATER" && target.Pokemon.Status != PokemonStatus.SLP)
                {
                    UI.ShowMessageAndWait("But it failed!");
                    return;
                }
                // protect/detect
                else if (target.Protected)
                {
                    UI.ShowMessageAndWait("But it failed!");
                    return;
                }
                // lock on guarantees hit
                if (target.LockedOn)
                {
                    target.LockedOn = false;
                    // earthquake, fissure and magnitude will still fail when target is flying
                    if ((moveUsed.Name == "EARTHQUAKE" || moveUsed.Name == "FISSURE" || moveUsed.Name == "MAGNITUDE") &&
                        target.IsFlying)
                    {
                        UI.ShowMessageAndWait("It doesn^'t affect\n" + target.GetDisplayName() + "!");
                        return;
                    }
                    else
                    {
                        // no accuracy check
                        lockedOn = true;
                    }
                }
                // life drain moves fail against a substitute
                if (target.SubstituteHP > 0 && moveUsed.Name == "ABSORB" ||
                    moveUsed.Name == "DREAM EATER" ||
                    moveUsed.Name == "GIGA DRAIN" ||
                    moveUsed.Name == "LEECH LIFE")
                {
                    UI.ShowMessageAndWait("But it failed!");
                    return;
                }
                // flying targets can only be hit by these moves, others miss
                else if (target.IsFlying && !(
                    moveUsed.Name == "GUST" ||
                    moveUsed.Name == "THUNDER" ||
                    moveUsed.Name == "TWISTER" ||
                    moveUsed.Name == "WHIRLWIND"))
                {
                    UI.ShowMessageAndWait(user.GetDisplayName() + "^'s\nattack missed!");
                    return;
                }
                // dug under targets cano only be hit by these moves, others miss
                else if (target.IsUnderground && !(
                    moveUsed.Name == "EARTHQUAKE" ||
                    moveUsed.Name == "MAGNITUDE"))
                {
                    UI.ShowMessageAndWait(user.GetDisplayName() + "^'s\nattack missed!");
                    return;
                }
                // during rain, thunder will not miss
                else if (moveUsed.Name == "THUNDER" && CurrentWeather == Weather.Rain)
                {
                    lockedOn = true;
                }

                // check if move misses its target due to accuracy
                if (!lockedOn && CheckMoveMisses(moveUsed, user, target))
                {
                    if (moveUsed.GetCategory(user) == MoveCategory.Status)
                    {
                        UI.ShowMessageAndWait("It didn^'t affect\n" + target.GetDisplayName() + "!");
                    }
                    else
                    {
                        UI.ShowMessageAndWait(user.GetDisplayName() + "^'s\nattack missed!");
                    }
                    return;
                }
            }

            // do damage if move is damaging
            if (moveUsed.GetCategory(user) != MoveCategory.Status)
            {
                var fainted = false;
                var typeMultiplier = PokemonTypeHelper.GetMultiplier(moveUsed.GetType(user), target);
                if (typeMultiplier != 0)
                {
                    var hits = moveUsed.GetHitAmount();
                    for (var i = 0; i < hits; i++)
                    {
                        var isCriticalHit = IsCriticalHit(moveUsed, user);
                        var damage = GetDamage(moveUsed, user, target, isCriticalHit);
                        if (damage == 0)
                        {
                            UI.ShowMessageAndWait("It doesn^'t affect\n" + target.GetDisplayName() + "!");
                            return;
                        }
                        else
                        {
                            if (Controller.ActivePlayer.BattleAnimations)
                            {
                                moveUsed.ShowAnimation(user, target);
                            }
                            DealDamage(damage, target, true, i < hits - 1);

                            if (isCriticalHit)
                            {
                                UI.ShowMessageAndWait("A critical hit!");
                            }

                            if (hits == 1)
                            {
                                // effectivenss messages
                                // show this outside the loop if multi-hit attack
                                ShowTypeEffectivenessMessage(typeMultiplier);
                            }
                        }

                        // reset rage counter
                        if (moveUsed.Name != "RAGE")
                        {
                            user.RageCounter = 1;
                        }

                        // execute secondary effect
                        var r = Random.NextDouble();
                        if (r < moveUsed.EffectChance)
                        {
                            moveUsed.ExecuteSecondaryEffect(user, target);
                            fainted |= CheckFainted(target);
                            fainted |= CheckFainted(user);
                        }
                        else
                        {
                            fainted |= CheckFainted(target);
                        }

                        // TODO: destiny bond

                        // TODO: target rage

                        if (fainted)
                        {
                            // if any of the involved pokemon faint, break up a multi hit attack
                            break;
                        }
                    }

                    if (hits > 1)
                    {
                        ShowTypeEffectivenessMessage(typeMultiplier);
                        UI.ShowMessageAndWait($"Hit {hits} times!");
                    }
                }
                else
                {
                    // move has no effect due to types, show message
                    UI.ShowMessageAndWait("It doesn^'t affect\n" + target.GetDisplayName() + "!");
                    return;
                }
            }
            else
            {
                // execute status move
                var canExecute = moveUsed.StatusMoveCheck(user, target);
                if (canExecute)
                {
                    if (Controller.ActivePlayer.BattleAnimations)
                    {
                        moveUsed.ShowAnimation(user, target);
                    }
                    moveUsed.ExecuteSecondaryEffect(user, target);
                }
            }
        }

        private void ShowTypeEffectivenessMessage(double typeMultiplier)
        {
            if (typeMultiplier < 1)
            {
                UI.ShowMessageAndWait("It^'s not very\neffective^..");
            }
            else if (typeMultiplier > 1)
            {
                UI.ShowMessageAndWait("It^'s super-\neffective!");
            }
        }

        // true if move is successful, false if not
        private bool ObedienceCheck(BattleMove moveUsed)
        {
            if (PlayerPokemon.Pokemon.IsOwn)
            {
                return true;
            }

            var obedience = BadgeHelper.GetObedienceLevel();
            if (PlayerPokemon.Pokemon.Level <= obedience)
            {
                return true;
            }

            var r = Random.Next(0, PlayerPokemon.Pokemon.Level + obedience);
            if (r < obedience)
            {
                return true;
            }

            // disobedient
            if (moveUsed.Name == "SLEEP TALK" || moveUsed.Name == "SNORE")
            {
                UI.ShowMessageAndWait(PlayerPokemon.GetDisplayName() + "\nignores orders\nsleeping."); // TODO: message
            }
            else
            {
                // TODO: rest of the fucking disobedience check
            }
            return false;
        }

        private bool IsCriticalHit(BattleMove moveUsed, BattlePokemon user)
        {
            var c = 0;
            if (user.FocusEnergyTurns > 0)
            {
                c++; // haha yes
            }
            if (moveUsed.IncreasedCriticalHitRatio)
            {
                c += 2;
            }
            if (user.Pokemon.HeldItem?.Name == "SCOPE LENS")
            {
                c++;
            }
            else if ((user.Pokemon.Id == 113 && user.Pokemon.HeldItem?.Name == "LUCKY PUNCH") ||
                     (user.Pokemon.Id == 83 && user.Pokemon.HeldItem?.Name == "STICK"))
            {
                c = 2;
            }

            var chance = 17; // c == 0
            if (c == 1)
            {
                chance = 32;
            }
            else if (c == 2)
            {
                chance = 64;
            }
            else if (c == 3)
            {
                chance = 85;
            }
            else if (c >= 4)
            {
                chance = 128;
            }

            var r = Random.Next(0, 256);
            return chance >= r;
        }

        private int GetDamage(BattleMove moveUsed, BattlePokemon user, BattlePokemon target, bool isCriticalHit = false)
        {
            // TODO: set HP damage moves

            double atk, def;
            bool ignoreForCriticalHit;
            if (moveUsed.GetCategory(user) == MoveCategory.Physical)
            {
                atk = user.GetStat(PokemonStat.Attack);
                def = target.GetStat(PokemonStat.Defense);
                ignoreForCriticalHit = isCriticalHit && user.Pokemon.Attack <= target.Pokemon.Defense;
            }
            else
            {
                atk = user.GetStat(PokemonStat.SpecialAttack);
                def = target.GetStat(PokemonStat.SpecialDefense);
                ignoreForCriticalHit = isCriticalHit && user.Pokemon.SpecialAttack <= target.Pokemon.SpecialDefense;
            }

            if (!ignoreForCriticalHit)
            {
                // raise stats from badges
                if (user == PlayerPokemon)
                {
                    if (moveUsed.GetCategory(user) == MoveCategory.Physical)
                    {
                        if (Controller.ActivePlayer.Badges.Contains(BadgeHelper.ZEPHYR_BADGE))
                        {
                            atk *= 1.125;
                        }
                    }
                    else
                    {
                        if (Controller.ActivePlayer.Badges.Contains(BadgeHelper.GLACIER_BADGE))
                        {
                            atk *= 1.125;
                        }
                    }
                }
                else
                {
                    if (moveUsed.GetCategory(user) == MoveCategory.Physical)
                    {
                        if (Controller.ActivePlayer.Badges.Contains(BadgeHelper.MINERAL_BADGE))
                        {
                            def *= 1.125;
                        }
                    }
                    else
                    {
                        if (Controller.ActivePlayer.Badges.Contains(BadgeHelper.GLACIER_BADGE))
                        {
                            def *= 1.125;
                        }
                    }
                }

                // burn halves atk
                if (user.Pokemon.Status == PokemonStatus.BRN)
                {
                    atk *= 0.5;
                }

                // reflect/light screen
                if (moveUsed.GetCategory(user) == MoveCategory.Physical)
                {
                    if (target.ReflectTurns > 0)
                    {
                        def *= 2;
                    }
                }
                else
                {
                    if (target.LightScreenTurns > 0)
                    {
                        def *= 2;
                    }
                }
            }

            if (moveUsed.Name == "SELFDESTRUCT" || moveUsed.Name == "EXPLOSION")
            {
                def *= 0.5;
            }

            // light ball
            if (moveUsed.GetCategory(user) == MoveCategory.Special &&
                user.Pokemon.Name == "PIKACHU" && user.Pokemon.HeldItem?.Name == "LIGHT BALL")
            {
                atk *= 2;
            }
            // thick club
            else if (moveUsed.GetCategory(user) == MoveCategory.Physical &&
                (user.Pokemon.Name == "CUBONE" || user.Pokemon.Name == "MAROWAK") &&
                user.Pokemon.HeldItem?.Name == "THICK CLUB")
            {
                atk *= 2;
            }

            // truncate over bytes
            if (atk > 255 || def > 255)
            {
                atk /= 4;
                def /= 4;
            }

            // metal powder
            if (target.Pokemon.Name == "DITTO" && target.Pokemon.HeldItem?.Name == "METAL POWDER")
            {
                def *= 1.5;
            }

            // always have at least 1 atk/def
            if (atk == 0)
            {
                atk = 1;
            }
            if (def == 0)
            {
                def = 1;
            }

            // base damage calc
            var l = user.Pokemon.Level;
            var p = moveUsed.GetBasePower(user, target);
            var damage = (double)((int)((int)((int)(2D * l / 5D + 2D) * atk * p / def) / 50D));

            if (isCriticalHit)
            {
                damage *= 2;
            }

            // TODO: other type enhancing items
            if (moveUsed.GetType(user) == PokemonType.Fire && user.Pokemon.HeldItem?.Name == "CHARCOAL")
            {
                damage *= 1.1;
            }

            if (damage > 997)
            {
                damage = 997;
            }
            damage += 2;

            // weather
            if (CurrentWeather == Weather.Sunny)
            {
                if (moveUsed.GetType(user) == PokemonType.Fire)
                {
                    damage *= 1.5;
                }
                else if (moveUsed.GetType(user) == PokemonType.Water)
                {
                    damage /= 1.5;
                }
            }
            else if (CurrentWeather == Weather.Rain)
            {
                if (moveUsed.GetType(user) == PokemonType.Water)
                {
                    damage *= 1.5;
                }
                else if (moveUsed.GetType(user) == PokemonType.Fire)
                {
                    damage /= 1.5;
                }
            }

            // badge type multiplier
            if (user == PlayerPokemon)
            {
                damage *= BadgeHelper.GetBadgeTypeMultilplier(moveUsed.GetType(user));
            }

            // STAB
            if (moveUsed.GetType(user) != PokemonType.None)
            {
                if (moveUsed.GetType(user) == user.Pokemon.Type1 ||
                    moveUsed.GetType(user) == user.Pokemon.Type2)
                {
                    damage *= 1.5;
                }
            }

            // type multiplier
            var typeMultiplier = PokemonTypeHelper.GetMultiplier(moveUsed.GetType(user), target);
            // foresight
            // ignore type multiplier for fighting/normal moves against ghost type pokemon
            if (target.Foresighted &&
                (target.Pokemon.Type1 == PokemonType.Ghost || target.Pokemon.Type2 == PokemonType.Ghost) &&
                (moveUsed.GetType(user) == PokemonType.Normal || moveUsed.GetType(user) == PokemonType.Fighting))
            {
                typeMultiplier = 1;
            }
            damage *= typeMultiplier;

            // damage variance
            var variance = Random.Next(217, 256);
            damage *= variance;
            damage /= 255;

            return (int)Math.Ceiling(damage);
        }

        private int GetConfusionDamage(BattlePokemon target)
        {
            var atk = target.GetStat(PokemonStat.Attack);
            var def = target.GetStat(PokemonStat.Defense);

            // reflect
            if (target.ReflectTurns > 0)
            {
                def *= 2;
            }

            // burn halves atk
            if (target.Pokemon.Status == PokemonStatus.BRN)
            {
                atk *= 0.5;
            }

            // truncate over bytes
            if (atk > 255 || def > 255)
            {
                atk /= 4;
                def /= 4;
            }

            // always have at least 1 atk/def
            if (atk == 0)
            {
                atk = 1;
            }
            if (def == 0)
            {
                def = 1;
            }

            // base damage calc
            var l = target.Pokemon.Level;
            var p = 40; // base power
            var damage = (double)((int)((int)((int)(2D * l / 5D + 2D) * atk * p / def) / 50D));

            if (damage > 997)
            {
                damage = 997;
            }
            damage += 2;

            return (int)Math.Ceiling(damage);
        }

        // true: move misses, false: hits
        private bool CheckMoveMisses(BattleMove moveUsed, BattlePokemon user, BattlePokemon target)
        {
            var accuracy = user.GetStat(PokemonStat.Accuracy);
            var evasion = target.GetStat(PokemonStat.Evasion);

            var moveAccuracy = Math.Round(moveUsed.Accuracy * 255); // 100% => 255 etc.
            moveAccuracy *= accuracy;
            if (moveAccuracy < 1)
            {
                moveAccuracy = 1;
            }
            moveAccuracy *= evasion;
            if (moveAccuracy < 1)
            {
                moveAccuracy = 1;
            }
            if (moveAccuracy > 255)
            {
                moveAccuracy = 255;
            }

            if (target.Pokemon.HeldItem?.Name == "BRIGHTPOWDER")
            {
                moveAccuracy -= 20;
                if (moveAccuracy < 0)
                {
                    moveAccuracy = 0;
                }
            }

            if (moveAccuracy == 255)
            {
                return false;
            }
            else
            {
                var r = Random.Next(0, 256);
                return r >= moveAccuracy;
            }
        }

        private void DealDamage(int damage, BattlePokemon target, bool substituteAffected, bool multiHit)
        {
            // TODO: substitute

            // within a multi strike move, the blinking/shaking animation does not play until the last hit
            if (!multiHit)
            {
                AnimationController.ShowAnimationAndWait(new DamageAnimation(target));
            }

            target.Pokemon.HP -= damage;
            if (target.Pokemon.HP < 0)
            {
                target.Pokemon.HP = 0;
            }

            if (target == PlayerPokemon)
            {
                UI.AnimatePlayerHPAndWait();
            }
            else
            {
                UI.AnimateEnemyHPAndWait();
            }
        }

        private bool CheckFainted(BattlePokemon target)
        {
            if (target.Pokemon.HP == 0 && target.Pokemon.Status != PokemonStatus.FNT)
            {
                target.Pokemon.Status = PokemonStatus.FNT;
                AnimationController.ShowAnimationAndWait(new FaintedAnimation(target));

                UI.SetPokemonStatusVisible(target.Side, false);
                UI.ShowMessageAndWait(target.GetDisplayName() + "\nfainted!");

                // exp
                if (target.Side == PokemonSide.Enemy && PlayerPokemon.Pokemon.HP > 0)
                {
                    // first, give exp to pokemon in the field
                    // second, give exp to all participated pokemon
                    // third, give exp to all pokemon in party with exp share.
                    // if that pokemon has participated, add that in second
                    // if that pokemon is in the field, add that in first
                    var field = PlayerPokemon.Pokemon;
                    var participated = _participation[EnemyPokemon.Pokemon];
                    var expShared = Controller.ActivePlayer.PartyPokemon
                        .Where(p => p.Status != PokemonStatus.FNT && p.HP > 0 && p.HeldItem?.Name == "EXP.SHARE").ToList();

                    var fieldExp = GetExp(field, false);
                    if (field.HeldItem?.Name == "EXP.SHARE")
                    {
                        expShared.Remove(field);
                        fieldExp += GetExp(field, true);
                    }
                    GainExp(field, fieldExp);
                    foreach (var participant in participated)
                    {
                        if (participant != field)
                        {
                            var participantExp = GetExp(participant, false);
                            if (participant.HeldItem?.Name == "EXP.SHARE")
                            {
                                expShared.Remove(participant);
                                participantExp += GetExp(participant, true);
                            }
                            GainExp(participant, participantExp);
                        }
                    }
                    foreach (var expShareHolder in expShared)
                    {
                        var sharedExp = GetExp(expShareHolder, true);
                        GainExp(expShareHolder, sharedExp);
                    }
                }

                // clear participation record for this pokemon after exp is yielded
                _participation[EnemyPokemon.Pokemon].Clear();

                return true;
            }
            return false;
        }

        private int GetExp(Pokemon participant, bool expShare)
        {
            var participated = _participation[EnemyPokemon.Pokemon];
            var l = EnemyPokemon.Pokemon.Level;
            var g = EnemyPokemon.Pokemon.BaseExperience;
            var x = IsWildBattle ? 1 : 1.5;
            var y = participant.IsOwn ? 1 : 1.5;
            var z = participant.HeldItem?.Name == "LUCKY EGG" ? 1.5 : 1;
            var e = expShare ?
                Controller.ActivePlayer.PartyPokemon.Count(p => p.Status != PokemonStatus.FNT && p.HP > 0 && p.HeldItem?.Name == "EXP.SHARE") :
                participated.Count(p => p.Status != PokemonStatus.FNT && p.HP > 0);
            var f = Controller.ActivePlayer.PartyPokemon.Any(p => p.HeldItem?.Name == "EXP.SHARE") ? 2 : 1;
            return (int)(((l * (g / (e * f))) / 7) * x * y * z);
        }

        private void GainExp(Pokemon pokemon, int exp)
        {
            if (pokemon.IsOwn)
            {
                UI.ShowMessageAndWait(pokemon.DisplayName + " gained\n" + exp.ToString() + " EXP. Points!");
            }
            else
            {
                UI.ShowMessageAndWait(pokemon.DisplayName + " gained\na boosted" + exp.ToString() + "\nEXP. Points!");
            }

            var hasLeveledUp = false;
            var queuedMoves = new List<MovesetEntryData>();

            while (exp > 0)
            {
                var expThisLevel = exp;
                var expToNextLevel = PokemonStatHelper.GetExperienceForLevel(pokemon.ExperienceType, pokemon.Level + 1) - pokemon.Experience;
                if (expToNextLevel <= exp)
                {
                    // level up
                    expThisLevel = expToNextLevel;
                    exp -= expThisLevel;

                    if (pokemon == PlayerPokemon.Pokemon)
                    {
                        pokemon.Experience += expThisLevel - 1;
                        UI.AnimatePlayerExpAndWait();

                        UI.SetPokemonArtificialLevelUp(true);
                        AnimationController.ShowAnimationAndWait(new LevelUpAnimation());

                        UI.ShowMessageAndWait($"{pokemon.DisplayName} grew to\nlevel {pokemon.Level + 1}!");

                        pokemon.Experience++;
                        UI.SetPokemonArtificialLevelUp(false);
                        UI.AnimatePlayerExpAndWait(instant: true); // resets exp bar to empty
                    }
                    else
                    {
                        pokemon.Experience += expThisLevel;
                    }

                    hasLeveledUp = true;
                    queuedMoves.AddRange(pokemon.GetMovesForCurrentLevel());
                }
                else
                {
                    pokemon.Experience += exp;
                    exp = 0;
                    if (pokemon == PlayerPokemon.Pokemon)
                    {
                        UI.AnimatePlayerExpAndWait();
                    }
                }
            }

            if (hasLeveledUp)
            {
                // if a pokemon levels up that isn't the active pokemon,
                // the "Pokemon reached level X" is shown after all exp is given.
                if (pokemon != PlayerPokemon.Pokemon)
                {
                    UI.ShowMessageAndWait($"{pokemon.DisplayName} grew to\nlevel {pokemon.Level}!");
                }

                UI.ShowPokemonStatsAndWait(pokemon);

                foreach (var move in queuedMoves)
                {
                    if (pokemon.Moves.Length < Pokemon.MAX_MOVES)
                    {
                        pokemon.AddMove(move.ToPokemonMoveData());
                    }
                    else
                    {
                        UI.ShowLearnMoveScreen(pokemon, move.ToPokemonMoveData());
                    }
                }
            }
        }

        private void HandleFainted()
        {
            // game decision
            // 1. if the player has no pokemon left, lose the battle
            if (Controller.ActivePlayer.PartyPokemon.All(p => !p.CanBattle))
            {
                // TODO: losing battle
            }

            // 2. if the battle was wild and the wild pokemon is defeated, win the battle
            if (IsWildBattle)
            {
                if (EnemyPokemon.Pokemon.Status == PokemonStatus.FNT)
                {
                    UI.EndBattle(true);
                    return;
                }
            }
            else // if the trainer has no pokemon left, win the battle
            {
                // TODO: winning trainer battle
            }

            // switch in player's pokemon first
            if (PlayerPokemon.Pokemon.Status == PokemonStatus.FNT)
            {

            }
            if (EnemyPokemon.Pokemon.Status == PokemonStatus.FNT)
            {

            }
        }

        private void EndOfAttack(BattlePokemon pokemon)
        {
            if (pokemon.Pokemon.Status != PokemonStatus.FNT)
            {
                if (pokemon.Pokemon.Status == PokemonStatus.PSN)
                {
                    var damage = (int)Math.Ceiling(pokemon.Pokemon.MaxHP / 8D);
                    DealDamage(damage, pokemon, false, false);
                    UI.ShowMessageAndWait(pokemon.GetDisplayName() + "\nis hurt by poison!");
                    // TODO: animation
                }
                else if (pokemon.Pokemon.Status == PokemonStatus.TOX)
                {
                    pokemon.ToxicCounter++;

                    var damage = (int)Math.Ceiling(pokemon.Pokemon.MaxHP * 0.0625 * pokemon.ToxicCounter);
                    DealDamage(damage, pokemon, false, false);
                    UI.ShowMessageAndWait(pokemon.GetDisplayName() + "\nis hurt by poison!");
                    // TODO: animation
                }
                else if (pokemon.Pokemon.Status == PokemonStatus.BRN)
                {
                    var damage = (int)Math.Ceiling(pokemon.Pokemon.MaxHP / 8D);
                    DealDamage(damage, pokemon, false, false);
                    UI.ShowMessageAndWait(pokemon.GetDisplayName() + "^'s\nhurt by its burn!");
                    // TODO: animation
                }

                // TODO: leech seed
                // TODO: nightmare
                // TODO: curse
            }
        }

        private void EndOfTurn(bool playerIsLeader)
        {
            var leader = playerIsLeader ? PlayerPokemon : EnemyPokemon;
            var runnerUp = playerIsLeader ? EnemyPokemon : PlayerPokemon;

            void applyFutureSight(BattlePokemon target)
            {
                // TODO: future sight
            }
            // switch apply order
            applyFutureSight(runnerUp);
            applyFutureSight(leader);
            CheckFainted(runnerUp);
            CheckFainted(leader);
            HandleFainted();

            void applySandstorm(BattlePokemon target)
            {
                // TODO: sandstorm
            }
            applySandstorm(leader);
            applySandstorm(runnerUp);
            CheckFainted(leader);
            CheckFainted(runnerUp);
            HandleFainted();

            // TODO: multi turn moves (Sand Tomb, Fire Spin, etc)
            // TODO: perish song
            // TODO: leftovers
            // TODO: end of reflect, light screen and safeguard
            // TODO: berries
            // TODO: end of encore

            UI.ResetMenu();
        }

        public bool ChangeStat(BattlePokemon target, PokemonStat stat, PokemonStatChange change)
        {
            var canChange = StatusMoveChecks.CheckStatChange(target, stat, change);
            if (!canChange)
            {
                return false;
            }

            var statName = PokemonStatHelper.GetDisplayString(stat);
            var newStat = target.StatModifications[stat];
            switch (change)
            {
                case PokemonStatChange.Increase:
                    newStat++;
                    UI.ShowMessageAndWait(target.GetDisplayName() + "^'s\n" + statName + " went up!");
                    break;
                case PokemonStatChange.SharpIncrease:
                    newStat += 2;
                    UI.ShowMessageAndWait(target.GetDisplayName() + "^'s\n" + statName + "\nwent way up!");
                    break;
                case PokemonStatChange.Decrease:
                    newStat--;
                    AnimationController.ShowAnimationAndWait(new StatFallAnimation(target));
                    UI.ShowMessageAndWait(target.GetDisplayName() + "^'s\n" + statName + " fell!");
                    break;
                case PokemonStatChange.SharpDecrease:
                    newStat -= 2;
                    AnimationController.ShowAnimationAndWait(new StatFallAnimation(target));
                    UI.ShowMessageAndWait(target.GetDisplayName() + "^'s\n" + statName + "\nsharply fell!");
                    break;
                case PokemonStatChange.Reset:
                    newStat = 0; // no message
                    break;
            }
            if (newStat > 6)
            {
                newStat = 6;
            }
            else if (newStat < -6)
            {
                newStat = -6;
            }
            target.StatModifications[stat] = newStat;
            return true;
        }

        public bool TryInflictStatusEffect(BattlePokemon target, PokemonStatus status)
        {
            if (target.Pokemon.Status == PokemonStatus.OK && target.Pokemon.HP > 0)
            {
                // TODO: BRN, FRZ, TOX
                target.Pokemon.Status = status;
                switch (status)
                {
                    case PokemonStatus.PAR:
                        UI.ShowMessageAndWait(target.GetDisplayName() + "^'s\nparalyzed! Maybe\nit can^'t attack!");
                        break;
                    case PokemonStatus.PSN:
                        UI.ShowMessageAndWait(target.GetDisplayName() + "\nwas poisoned!");
                        break;
                    case PokemonStatus.SLP:
                        UI.ShowMessageAndWait(target.GetDisplayName() + "\nfell asleep!");
                        target.SetAsleep();
                        break;
                }
                return true;
            }
            return false;
        }

        public bool TryInflictConfusion(BattlePokemon target)
        {
            if (target.Pokemon.HP > 0 && target.ConfusionTurns == 0)
            {
                target.ConfusionTurns = Random.Next(2, 6);
                AnimationController.ShowAnimationAndWait(new ConfusedAnimation(target));
                UI.ShowMessageAndWait(target.GetDisplayName() + "\nbecame confused!");
                return true;
            }
            return false;
        }
    }
}
