using Terraria.ModLoader;
using Terraria;
using Humanizer;
using Microsoft.Xna.Framework;
using Subterannia.Core.Subworlds;
using Subterannia.Core.Subworlds.LinuxSubworlds;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Subterannia.Core.Utility;

namespace Subterannia.Core.Subworlds
{
    public class SubworldLoadingUI : ModSystem
    {
        public override void Load()
        {
            On_Main.DrawMenu += DrawOver;
        }

        private void DrawOver(On_Main.orig_DrawMenu orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);

            if (!Main.gameMenu) return;

            if (Subterannia.GetLoadable<SubworldInstance>().IsSaving)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
                    Main.SamplerStateForCursor, DepthStencilState.None, 
                    RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                if (SubworldManager.currentSubworld == null) DefaultLoadingUI(Main.spriteBatch);
                else SubworldManager.currentSubworld.DrawLoadingUI(Main.spriteBatch);

                Main.spriteBatch.End();
            }
        }

        public void DefaultLoadingUI(SpriteBatch sb)
        {
            Utilities.DrawBoxFill(new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);
        }
    }
}