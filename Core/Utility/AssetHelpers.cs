
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
    public static partial class Utilities
    {
        private static string AssetPath = "Assets";

        public static Asset<Texture2D> GetTexture(string path, AssetRequestMode requestMode = AssetRequestMode.AsyncLoad)
        {
            return ModContent.Request<Texture2D>($"Subterannia/{AssetPath}/{path}", requestMode);
        }
    }
}