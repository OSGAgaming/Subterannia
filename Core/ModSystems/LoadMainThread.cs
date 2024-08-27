using Terraria.ModLoader;
using Terraria;
using Humanizer;
using Microsoft.Xna.Framework;
using Subterannia.Core.Subworlds;
using Subterannia.Core.Subworlds.LinuxSubworlds;
using Microsoft.Xna.Framework.Graphics;
using Subterannia.Core.Utility;
using ReLogic.Content;
using Subterannia.Core.Mechanics;
using Subterannia.Core.Mechanics.Interfaces;
using System;
using Subterannia;

public class LoadMainThread : ModSystem
{
    private bool Loaded = false;

    public override void Load()
    {
        On_Main.DoUpdate += Update;
    }

    public override void Unload()
    {
        Loaded = false;
    }

    private void Update(On_Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
    {
        orig(self, ref gameTime);

        if (!Loaded && SubteranniaMod.Loaded)
        {
            Type[] loadables = Utilities.GetInheritedClasses(typeof(IMainThreadLoad));
            foreach (Type type in loadables)
            {
                IMainThreadLoad loadable = Activator.CreateInstance(type) as IMainThreadLoad;
                loadable.Load();
            }
            Loaded = true;
            DepthSetCalls.ChangeScene(new Debug3DScene());
        }
    }
}