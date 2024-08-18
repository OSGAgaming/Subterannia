
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

namespace Subterannia.Core.Utility
{
    public enum TileSpacing
    {
        None,
        Bottom,
        Top,
        Right,
        Left
    }
    public static partial class Utilities
    {
        public static void FillRegion(Point startingPoint, int width, int height, int type)
        {
            for (int i = startingPoint.X; i < width; i++)
            {
                for (int j = startingPoint.Y; j < height; j++)
                {
                    WorldGen.PlaceTile(i,j,type);
                }
            }
        }

        public static void FillRegion(Rectangle r, int type)
        {
            for (int i = r.X; i < r.Width; i++)
            {
                for (int j = r.Y; j < r.Height; j++)
                {
                    WorldGen.PlaceTile(i, j, type);
                }
            }
        }
    }
}