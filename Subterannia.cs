using Microsoft.Xna.Framework;
using Subterannia.Core.Mechanics.Interfaces;
using Subterannia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Subterannia
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class Subterannia : Mod
    {
        private GameTime lastGameTime;

        public static List<ILoad> Loadables;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                Loadables = new List<ILoad>();

                Type[] loadables = Util.GetInheritedClasses(typeof(ILoad));
                foreach (Type type in loadables)
                {
                    ILoad loadable = Activator.CreateInstance(type) as ILoad;
                    loadable.Load();

                    Loadables.Add(loadable);
                }

                Debug.WriteLine("Loaded!");
            }
        }

        public static T GetLoadable<T>()
        {
            foreach (ILoad loadable in Loadables)
                if (loadable is T) return (T)loadable;

            throw new NullReferenceException("Loadable could not be found");
        }

        public override void Unload()
        {        
            if(Loadables != null) 
            {
                for (int i = 0; i < Loadables.Count; i++)
                    Loadables[i] = null;

                Loadables.Clear();

                Loadables = null;

                Debug.WriteLine("Unloaded!");
                Debug.WriteLine("");
            }
        }

    }
}
