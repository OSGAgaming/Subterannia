
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;

namespace Subterannia.Core.Subworlds
{
    public class SubworldPlayer : ModPlayer
    {
        internal string PrimaryWorldName = "";

        public bool InSubworld;

        public Subworld CurrentSubworld = null;

        public override void PreUpdate()
        {
            if (!InSubworld && !Main.gameMenu)
            {
                PrimaryWorldName = Main.ActiveWorldFileData.Name;
                if (SubteranniaMod.GetLoadable<SubworldInstance>().IsSaving)
                {
                    SubteranniaMod.GetLoadable<SubworldInstance>().IsSaving = false;
                }
            }

            CurrentSubworld?.PlayerUpdate(Player);
        }
    }
}
