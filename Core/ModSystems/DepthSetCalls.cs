using Subterannia.Core.Mechanics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Subterannia.Core.Mechanics
{
    public class DepthSetCalls : ModSystem
    {
        public static DepthSetCalls Instance;
        public static Action<SpriteBatch> OnPreDraw;
        public static Action OnPreUpdate;
        public static ModEntitySet ModEntitySet;

        public static void ChangeScene(ModEntitySet modEntitySet)
        {
            modEntitySet.Activate();
            ModEntitySet = modEntitySet;
        }

        public override void Load()
        {
            Instance = this;
            On_Main.DrawWoF += Main_DrawWoF;
            On_Main.DrawCachedNPCs += On_Main_DrawCachedNPCs;

            Main.OnPreDraw += Main_OnPreDraw;
        }

        private void On_Main_DrawCachedNPCs(On_Main.orig_DrawCachedNPCs orig, Main self, List<int> npcCache, bool behindTiles)
        {
            orig(self, npcCache, behindTiles);
            if (SubteranniaMod.Loaded)
                LayerSet.DrawLayers(Main.spriteBatch);
        }

        private void Main_OnPreDraw(GameTime obj)
        {
            RenderTargetBinding[] oldtargets = Main.graphics.GraphicsDevice.GetRenderTargets();

            if (Main.gameMenu)
                return;

            if (ModEntitySet != null)
                LayerSet.DrawLayersToTarget(ModEntitySet, Main.spriteBatch);

            OnPreDraw?.Invoke(Main.spriteBatch);
            OnPreUpdate?.Invoke();

            LayerSet.ClearCalls();

            Main.graphics.GraphicsDevice.SetRenderTargets(oldtargets);

        }

        private void Main_DrawWoF(On_Main.orig_DrawWoF orig, Main self)
        {
            ModEntitySet?.Update();

            orig(self);
        }
    }
}