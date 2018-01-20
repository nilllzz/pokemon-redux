using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using static Core;

namespace PTCG.Content
{
    public static class ContentLoader
    {
        private static Dictionary<string, object> _resourceBuffer = new Dictionary<string, object>();

        public static void Clear()
        {
            _resourceBuffer.Clear();
        }

        public static Texture2D LoadFromImage(this ContentManager content, string file)
        {
            var key = file.ToLower();
            if (!_resourceBuffer.TryGetValue(key, out var texture)) {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, content.RootDirectory, file);

                using (var stream = new FileStream(path, FileMode.Open)) {
                    texture = Texture2D.FromStream(Controller.GraphicsDevice, stream);
                }

                _resourceBuffer.Add(key, texture);
            }

            return (Texture2D)texture;
        }
    }
}
