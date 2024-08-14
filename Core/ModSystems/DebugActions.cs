using Terraria.ModLoader;
using Terraria;
using Humanizer;
using Microsoft.Xna.Framework;
using Subterannia.Core.Subworlds;
using Subterannia.Core.Subworlds.LinuxSubworlds;

public class DebugActions : ModSystem
{
    public override void Load()
    {
        On_Main.DoUpdate += Update;
    }

    private void Update(On_Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
    {
        orig(self, ref gameTime);

        if (!Main.gameMenu && Main.LocalPlayer.controlHook)
        {
            if(!Main.LocalPlayer.GetModPlayer<SubworldPlayer>().InSubworld)
            {
                SubworldManager.EnterSubworld<CutsceneSubworld>();
            }
            else
            {
                SubworldManager.ReturnToMainWorld();
            }
            Main.LocalPlayer.controlHook = false;
        }
    }
}