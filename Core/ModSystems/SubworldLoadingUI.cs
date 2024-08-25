using Terraria.ModLoader;
using Terraria;
using Humanizer;
using Microsoft.Xna.Framework;
using Subterannia.Core.Subworlds;
using Subterannia.Core.Subworlds.LinuxSubworlds;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Subterannia.Core.Utility;
using Subterannia.Core.Mechanics;

namespace Subterannia.Core.Subworlds
{
    public class SubworldLoadingUI : ModSystem
    {
        private bool Loaded = false;
        public override void Load()
        {
            On_Main.DrawMenu += DrawOver;
        }

        private void DrawOver(On_Main.orig_DrawMenu orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);
        }
    }
}