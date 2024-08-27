using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using SkinnedModel;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Reflection;
using System.Diagnostics;
using Subterannia.Core.Utility;

namespace Subterannia.Core.Mechanics
{
    public static class ModelRepository
    {
        [ModelPath("Planet")]
        public static Model Clouds;
        [ModelPath("Planet")]
        public static Model Planet;
        [ModelPath("earthen_greatsword")]
        public static Model Doodle;
        [ModelPath("statue_with_texture")]
        public static Model Statue;
        [ModelPath("rat")]
        public static Model Rat;
    }
}