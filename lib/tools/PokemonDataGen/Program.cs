using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace PokemonDataGen
{
    class Program
    {
        private static string[] PokemonNames = new[] { "Bulbasaur", "Ivysaur", "Venusaur", "Charmander", "Charmeleon", "Charizard", "Squirtle", "Wartortle", "Blastoise", "Caterpie", "Metapod", "Butterfree", "Weedle", "Kakuna", "Beedrill", "Pidgey", "Pidgeotto", "Pidgeot", "Rattata", "Raticate", "Spearow", "Fearow", "Ekans", "Arbok", "Pikachu", "Raichu", "Sandshrew", "Sandslash", "Nidoran♀", "Nidorina", "Nidoqueen", "Nidoran♂", "Nidorino", "Nidoking", "Clefairy", "Clefable", "Vulpix", "Ninetales", "Jigglypuff", "Wigglytuff", "Zubat", "Golbat", "Oddish", "Gloom", "Vileplume", "Paras", "Parasect", "Venonat", "Venomoth", "Diglett", "Dugtrio", "Meowth", "Persian", "Psyduck", "Golduck", "Mankey", "Primeape", "Growlithe", "Arcanine", "Poliwag", "Poliwhirl", "Poliwrath", "Abra", "Kadabra", "Alakazam", "Machop", "Machoke", "Machamp", "Bellsprout", "Weepinbell", "Victreebel", "Tentacool", "Tentacruel", "Geodude", "Graveler", "Golem", "Ponyta", "Rapidash", "Slowpoke", "Slowbro", "Magnemite", "Magneton", "Farfetch'd", "Doduo", "Dodrio", "Seel", "Dewgong", "Grimer", "Muk", "Shellder", "Cloyster", "Gastly", "Haunter", "Gengar", "Onix", "Drowzee", "Hypno", "Krabby", "Kingler", "Voltorb", "Electrode", "Exeggcute", "Exeggutor", "Cubone", "Marowak", "Hitmonlee", "Hitmonchan", "Lickitung", "Koffing", "Weezing", "Rhyhorn", "Rhydon", "Chansey", "Tangela", "Kangaskhan", "Horsea", "Seadra", "Goldeen", "Seaking", "Staryu", "Starmie", "Mr. Mime", "Scyther", "Jynx", "Electabuzz", "Magmar", "Pinsir", "Tauros", "Magikarp", "Gyarados", "Lapras", "Ditto", "Eevee", "Vaporeon", "Jolteon", "Flareon", "Porygon", "Omanyte", "Omastar", "Kabuto", "Kabutops", "Aerodactyl", "Snorlax", "Articuno", "Zapdos", "Moltres", "Dratini", "Dragonair", "Dragonite", "Mewtwo", "Mew", "Chikorita", "Bayleef", "Meganium", "Cyndaquil", "Quilava", "Typhlosion", "Totodile", "Croconaw", "Feraligatr", "Sentret", "Furret", "Hoothoot", "Noctowl", "Ledyba", "Ledian", "Spinarak", "Ariados", "Crobat", "Chinchou", "Lanturn", "Pichu", "Cleffa", "Igglybuff", "Togepi", "Togetic", "Natu", "Xatu", "Mareep", "Flaaffy", "Ampharos", "Bellossom", "Marill", "Azumarill", "Sudowoodo", "Politoed", "Hoppip", "Skiploom", "Jumpluff", "Aipom", "Sunkern", "Sunflora", "Yanma", "Wooper", "Quagsire", "Espeon", "Umbreon", "Murkrow", "Slowking", "Misdreavus", "Unown", "Wobbuffet", "Girafarig", "Pineco", "Forretress", "Dunsparce", "Gligar", "Steelix", "Snubbull", "Granbull", "Qwilfish", "Scizor", "Shuckle", "Heracross", "Sneasel", "Teddiursa", "Ursaring", "Slugma", "Magcargo", "Swinub", "Piloswine", "Corsola", "Remoraid", "Octillery", "Delibird", "Mantine", "Skarmory", "Houndour", "Houndoom", "Kingdra", "Phanpy", "Donphan", "Porygon2", "Stantler", "Smeargle", "Tyrogue", "Hitmontop", "Smoochum", "Elekid", "Magby", "Miltank", "Blissey", "Raikou", "Entei", "Suicune", "Larvitar", "Pupitar", "Tyranitar", "Lugia", "Ho-Oh", "Celebi" };
        private static string[] BaseStatColors = new[] { "FF5959", "F5AC78", "FAE078", "9DB7F5", "A7DB8D", "FA92B2" };
        private const int START_INDEX = 0;

        public static void Main(string[] args)
        {
            var eggDatas = EggData.Load();
            Console.WriteLine("Loaded egg data");

            var outputFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
                Console.WriteLine("Created output folder");
            }

            // advanced html crawl black magic commences
            var client = new WebClient();

            for (int no = START_INDEX; no < PokemonNames.Length; no++)
            {
                var name = PokemonNames[no];
                var id = no + 1;

                var data = new PokemonData
                {
                    name = name.ToUpper(),
                    id = id
                };

                Console.Write($"{name.PadRight(20)} ({id.ToString("D3")}/{PokemonNames.Length})");

                var bulbapediaUrl = $"https://bulbapedia.bulbagarden.net/wiki/{name}_(Pok%C3%A9mon)";
                var html = client.DownloadString(bulbapediaUrl);

                Console.Write(".");

                // gender
                {
                    var genderMatch = Regex.Match(html, "<span style=\"color:#0000FF;\">(\\d|\\.)*% male<\\/span>");
                    if (genderMatch.Success)
                    {
                        var genderStr = genderMatch.Value;
                        var maleRatio = genderStr.Remove(0, genderStr.IndexOf(">") + 1);
                        maleRatio = maleRatio.Remove(maleRatio.IndexOf("%"));
                        switch (maleRatio)
                        {
                            case "100":
                                data.gender = "male";
                                break;
                            case "87.5":
                                data.gender = "mostlyMale";
                                break;
                            case "75":
                                data.gender = "oftenMale";
                                break;
                            case "50":
                                data.gender = "equal";
                                break;
                            case "25":
                                data.gender = "oftenFemale";
                                break;
                        }
                    }
                    else
                    {
                        // no male gender, find if female or genderless
                        var femaleMatch = Regex.Match(html, "<span style=\"color:#FF6060;\">");
                        if (femaleMatch.Success)
                        {
                            data.gender = "female";
                        }
                        else
                        {
                            data.gender = "genderless";
                        }
                    }

                    Console.Write(".");
                }

                // base stats
                {
                    data.baseStats = new int[BaseStatColors.Length];
                    for (int i = 0; i < BaseStatColors.Length; i++)
                    {
                        var searchString = $"<th style=\"background: #{BaseStatColors[i]}; width: 30px;\"> ";
                        var stat = html.Remove(0, html.IndexOf(searchString) + searchString.Length);
                        stat = stat.Remove(stat.IndexOf("<")).Trim();
                        data.baseStats[i] = Convert.ToInt32(stat);
                    }

                    Console.Write(".");
                }

                // experience yield and type
                {
                    var yieldSearchStr = "<b><a href=\"/wiki/Experience\" title=\"Experience\"><span style=\"color:#000;\">Base experience yield</span></a></b>";
                    var yieldIndex = html.IndexOf(yieldSearchStr);
                    var yieldMatch = Regex.Match(html.Remove(0, yieldIndex + yieldSearchStr.Length), "<td width=\"(\\d|\\.)*%\">");
                    var yieldStr = yieldMatch.Value;
                    var yieldFieldIndex = html.IndexOf(yieldStr, yieldIndex) + yieldStr.Length;
                    var yield = html.Substring(yieldFieldIndex, html.IndexOf("<", yieldFieldIndex) - yieldFieldIndex).Trim();

                    data.experienceYield = Convert.ToInt32(yield);

                    Console.Write(".");

                    var typeStr = "<b><a href=\"/wiki/Experience\" title=\"Experience\"><span style=\"color:#000;\">Leveling rate</span></a></b>";
                    var typeFieldIndex = html.IndexOf("<td> ", html.IndexOf(typeStr)) + 5;
                    var expType = html.Remove(0, typeFieldIndex);
                    expType = expType.Remove(expType.IndexOf("<")).Trim();

                    expType = expType.Replace(" ", "");
                    expType = expType[0].ToString().ToLower() + expType.Substring(1);

                    data.experienceType = expType;

                    Console.Write(".");
                }

                // catch rate
                {
                    var catchRateSearchStr = "<a href=\"/wiki/Catch_rate\" title=\"Catch rate\"><span style=\"color:#000;\">Catch rate</span></a>";
                    var catchRateIndex = html.IndexOf(catchRateSearchStr) + catchRateSearchStr.Length;
                    catchRateIndex = html.IndexOf("<td>", catchRateIndex) + 4;
                    var catchRateStr = html.Remove(0, catchRateIndex);
                    catchRateStr = catchRateStr.Remove(catchRateStr.IndexOf("<")).Trim();

                    data.catchRate = Convert.ToInt32(catchRateStr);

                    Console.Write(".");
                }

                // types
                {
                    var typeStr = "<b><a href=\"/wiki/Type\" title=\"Type\"><span style=\"color:#000;\">Type</span></a></b>";
                    if (!html.Contains(typeStr))
                    {
                        typeStr = "<b><a href=\"/wiki/Type\" title=\"Type\"><span style=\"color:#000;\">Types</span></a></b>";
                    }
                    var typeIndex = html.IndexOf(typeStr) + typeStr.Length;

                    var subhtml = html.Remove(0, typeIndex);
                    subhtml = subhtml.Remove(subhtml.IndexOf("</table>"));

                    var matches = Regex.Matches(subhtml, "<b>.*?</b>");
                    processType(0, matches[0].Value);
                    processType(1, matches[1].Value);

                    void processType(int typeId, string matchStr)
                    {
                        var type = matchStr.Remove(0, 3);
                        type = type.Remove(type.IndexOf("<"));
                        if (type == "Unknown")
                        {
                            type = "none";
                        }
                        if (typeId == 0)
                        {
                            data.type1 = type.ToLower();
                        }
                        else
                        {
                            data.type2 = type.ToLower();
                        }

                        Console.Write(".");
                    }
                }

                // egg data
                {
                    var eggData = eggDatas.First(e => e.id == id);
                    data.eggCycles = eggData.eggCycles;
                    data.eggGroups = eggData.eggGroups.Select(g => EggData.FormatEggGroup(g)).ToArray();

                    Console.Write(".");
                }

                // moves
                {
                    var bulbapediaMovesUrl = $"https://bulbapedia.bulbagarden.net/wiki/{name}_(Pok%C3%A9mon)/Generation_II_learnset";
                    var movesHtml = client.DownloadString(bulbapediaMovesUrl);

                    Console.Write(".");

                    var moves = new List<MovesetEntryData>();

                    // level up
                    {
                        var tableSearchStr = "<table width=\"100%\" style=\"border-collapse:collapse;\" class=\"sortable\">";
                        var tableStr = movesHtml.Remove(0, movesHtml.IndexOf(tableSearchStr) + tableSearchStr.Length);
                        tableStr = tableStr.Remove(0, tableStr.IndexOf("<tr>"));
                        tableStr = tableStr.Remove(tableStr.IndexOf("</table>"));

                        var movesStrs = tableStr.Split(new[] { "<tr>" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var moveStr in movesStrs)
                        {
                            var levelStr = moveStr.Remove(0, moveStr.IndexOf("</span>") + 7);
                            levelStr = levelStr.Remove(levelStr.IndexOf("<")).Trim();
                            var level = Convert.ToInt32(levelStr);

                            var moveHtmlStr = "<span style=\"color:#000;\">";
                            var moveName = moveStr.Remove(0, moveStr.IndexOf(moveHtmlStr) + moveHtmlStr.Length);
                            moveName = moveName.Remove(moveName.IndexOf("<")).Trim().ToUpper();

                            moves.Add(new MovesetEntryData
                            {
                                level = level,
                                name = moveName,
                            });
                        }

                        Console.Write(".");
                    }

                    // tms
                    {
                        var tableSearchStr = "<span style=\"color:#000;\">TM</span>";
                        var tableStr = movesHtml.Remove(0, movesHtml.IndexOf(tableSearchStr) + tableSearchStr.Length);
                        tableStr = tableStr.Remove(0, tableStr.IndexOf("<tr>"));
                        tableStr = tableStr.Remove(tableStr.IndexOf("</table>"));

                        var movesStrs = tableStr.Split(new[] { "<tr>" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var moveStr in movesStrs)
                        {
                            var moveHtmlStr = "(move)\"><span style=\"color:#000;\">";
                            if (moveStr.IndexOf(moveHtmlStr) > -1)
                            {
                                var moveName = moveStr.Remove(0, moveStr.IndexOf(moveHtmlStr) + moveHtmlStr.Length);
                                moveName = moveName.Remove(moveName.IndexOf("<")).Trim().ToUpper();

                                moves.Add(new MovesetEntryData
                                {
                                    tm = true,
                                    name = moveName,
                                });
                            }
                        }

                        Console.Write(".");
                    }

                    // breeding
                    {
                        var tableSearchStr = "<span style=\"color:#000;\">Father</span></a>";
                        var tableStr = movesHtml.Remove(0, movesHtml.IndexOf(tableSearchStr) + tableSearchStr.Length);
                        tableStr = tableStr.Remove(0, tableStr.IndexOf("<tr>"));
                        tableStr = tableStr.Remove(tableStr.IndexOf("</table>"));

                        var movesStrs = tableStr.Split(new[] { "<tr>" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var moveStr in movesStrs)
                        {
                            var moveHtmlStr = "(move)\"><span style=\"color:#000;\">";
                            if (moveStr.IndexOf(moveHtmlStr) > -1)
                            {
                                var moveName = moveStr.Remove(0, moveStr.IndexOf(moveHtmlStr) + moveHtmlStr.Length);
                                moveName = moveName.Remove(moveName.IndexOf("<")).Trim().ToUpper();

                                moves.Add(new MovesetEntryData
                                {
                                    breeding = true,
                                    name = moveName,
                                });
                            }
                        }

                        Console.Write(".");
                    }

                    data.moves = moves.ToArray();
                }

                var json = JsonConvert.SerializeObject(data);
                File.WriteAllText(Path.Combine(outputFolder, id + ".json"), json);

                Console.Write(".");

                Console.WriteLine("DONE");

                // chill
                Thread.Sleep(100);
            }
        }
    }
}
