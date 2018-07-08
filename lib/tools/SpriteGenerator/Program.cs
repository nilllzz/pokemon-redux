using Newtonsoft.Json;
using PokemonRedux.Game.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using static PokemonRedux.Game.Data.PokemonData;

namespace SpriteGenerator
{
    class Program
    {
        public static void Main(string[] args)
        {
            var execFolder = AppDomain.CurrentDomain.BaseDirectory;
            var inFolder = Path.Combine(execFolder, "in");

            if (!Directory.Exists(inFolder))
            {
                return;
            }

            var number = 0;
            var file = Path.GetFileNameWithoutExtension(Directory.GetFiles(inFolder)[0]);
            var parts = file.Split('_');
            foreach (var part in parts)
            {
                int.TryParse(part, out number);
            }

            Bitmap inFront = null, inBack = null, inFrontS = null, inBackS = null;

            var numStr = number.ToString("D3");
            foreach (var c in new[] { 'g', 's', 'c' })
            {
                var fFile = Path.Combine(inFolder, $"Spr_2{c}_{numStr}.png");
                if (inFront == null && File.Exists(fFile))
                {
                    inFront = new Bitmap(fFile);
                }
                var bFile = Path.Combine(inFolder, $"Spr_b_2{c}_{numStr}.png");
                if (inBack == null && File.Exists(bFile))
                {
                    inBack = new Bitmap(bFile);
                }
                var fsFile = Path.Combine(inFolder, $"Spr_2{c}_{numStr}_s.png");
                if (inFrontS == null && File.Exists(fsFile))
                {
                    inFrontS = new Bitmap(fsFile);
                }
                var bsFile = Path.Combine(inFolder, $"Spr_b_2{c}_{numStr}_s.png");
                if (inBackS == null && File.Exists(bsFile))
                {
                    inBackS = new Bitmap(bsFile);
                }
            }

            var normalColors = new List<Color>();
            var shinyColors = new List<Color>();

            // find and order colors
            for (int y = 0; y < inFront.Height; y++)
            {
                for (int x = 0; x < inFront.Width; x++)
                {
                    void addColor(Bitmap img, List<Color> colors)
                    {
                        var pixel = img.GetPixel(x, y);
                        if (pixel.A == 255)
                        {
                            if (!colors.Contains(pixel))
                            {
                                colors.Add(pixel);
                                // order colors by combined channel brightness
                                if (colors == normalColors)
                                {
                                    normalColors = colors.OrderBy(c => c.R + c.G + c.B).ToList();
                                    colors = normalColors;
                                }
                                else
                                {
                                    shinyColors = colors.OrderBy(c => c.R + c.G + c.B).ToList();
                                    colors = shinyColors;
                                }
                            }
                        }
                    }

                    addColor(inFront, normalColors);
                    addColor(inFrontS, shinyColors);
                }
            }
            // set front colors
            for (int y = 0; y < inFront.Height; y++)
            {
                for (int x = 0; x < inFront.Width; x++)
                {
                    void setColor(Bitmap img, List<Color> colors)
                    {
                        var pixel = img.GetPixel(x, y);
                        if (pixel.A == 255)
                        {
                            img.SetPixel(x, y, GetIndexColor(colors.IndexOf(pixel)));
                        }
                    }

                    setColor(inFront, normalColors);
                    setColor(inFrontS, shinyColors);
                }
            }

            // set back colors
            for (int y = 0; y < inFront.Height; y++)
            {
                for (int x = 0; x < inFront.Width; x++)
                {
                    void setPixel(Bitmap img, List<Color> colors)
                    {
                        var pixel = img.GetPixel(x, y);
                        if (pixel.A == 255)
                        {
                            var index = -1;
                            var maxDiff = 0;
                            do
                            {
                                for (int i = 0; i < colors.Count; i++)
                                {
                                    var c = colors[i];
                                    var diffR = Math.Abs(pixel.R - c.R);
                                    var diffG = Math.Abs(pixel.G - c.G);
                                    var diffB = Math.Abs(pixel.B - c.B);

                                    if (diffR <= maxDiff && diffG <= maxDiff && diffB <= maxDiff)
                                    {
                                        index = i;
                                        break;
                                    }
                                }
                                maxDiff++;
                            } while (index == -1);
                            img.SetPixel(x, y, GetIndexColor(index));
                        }
                    }

                    setPixel(inBack, normalColors);
                    setPixel(inBackS, shinyColors);
                }
            }

            var outBmp = new Bitmap(56 * 2, 56);
            var g = Graphics.FromImage(outBmp);
            g.Clear(Color.Transparent);
            g.DrawImage(inFront, new Rectangle(0, 0, 56, 56));
            g.DrawImage(inBack, new Rectangle(56, 0, 56, 56));

            var solutionFolder = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.Parent.Parent.FullName;
            var imgFile = Path.Combine(solutionFolder, "PokemonRedux/Content/Textures/Pokemon/Main", number + ".png");
            outBmp.Save(imgFile, ImageFormat.Png);

            var data = new Palette
            {
                normal = normalColors.Select(c => new int[] { c.R, c.G, c.B }).ToArray(),
                shiny = shinyColors.Select(c => new int[] { c.R, c.G, c.B }).ToArray(),
            };

            var dataFile = Path.Combine(solutionFolder, "PokemonRedux/Content/Data/Pokemon", number + ".json");
            if (File.Exists(dataFile))
            {
                var existingData = JsonConvert.DeserializeObject<PokemonData>(File.ReadAllText(dataFile));
                existingData.colors = data;
                var json = JsonConvert.SerializeObject(existingData);
                File.WriteAllText(dataFile, json);
            }
        }

        private static Color GetIndexColor(int index)
        {
            switch (index)
            {
                case 0:
                    return Color.FromArgb(0, 0, 0);
                case 1:
                    return Color.FromArgb(255, 0, 0);
                case 2:
                    return Color.FromArgb(0, 255, 0);
                case 3:
                    return Color.FromArgb(0, 0, 255);
            }
            return default(Color);
        }
    }
}
