
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.Diagnostics;
//using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using Terraria.DataStructures;

using Terraria.ObjectData;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Subterannia.Core.Utility
{
    public class AssetDirectories
    {
        public static readonly string ModName = "Subterannia/";

        public static readonly string Models = "Models/";

    }

    public static partial class Utilities
    {
        private static string AssetPath = "Assets";

        public static Asset<Texture2D> GetTexture(string path, AssetRequestMode requestMode = AssetRequestMode.AsyncLoad)
        {
            return ModContent.Request<Texture2D>($"Subterannia/{AssetPath}/{path}", requestMode);
        }

        public static Asset<Effect> GetEffect(string path, AssetRequestMode requestMode = AssetRequestMode.AsyncLoad)
        {
            return ModContent.Request<Effect>($"Subterannia/{AssetPath}/{path}", requestMode);
        }

        public static Asset<T> GetAsset<T>(string path, AssetRequestMode requestMode = AssetRequestMode.AsyncLoad) where T : class
        {
            return ModContent.Request<T>($"Subterannia/{AssetPath}/{path}", requestMode);
        }
    }
}