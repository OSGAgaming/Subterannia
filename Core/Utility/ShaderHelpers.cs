﻿
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
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace Subterannia.Core.Utility
{
    public static partial class Utilities
    {
        public static void ActivateScreenShader(string ShaderName, Vector2 vec = default)
        {
            if (Main.netMode != NetmodeID.Server && !Filters.Scene[ShaderName].IsActive())
            {
                Filters.Scene.Activate(ShaderName, vec);
            }
        }

        public static ScreenShaderData GetScreenShader(string ShaderName) => Filters.Scene[ShaderName].GetShader();

        public static Color GetWorldLighting(Vector2 v) => Lighting.GetColor(new Point((int)v.X / 16, (int)v.Y / 16));

        public static Vector3 ColorToVector(Color c) => new Vector3(c.R, c.G, c.B);


    }
}