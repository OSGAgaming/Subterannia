using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Subterannia.Core.Mechanics.Interfaces;

namespace Subterannia.Core.Mechanics
{
	public class ModEntitySet
    {
        public IList<ILayeredDraw> Drawables = new List<ILayeredDraw>();
        public IList<IUpdate> Tickables = new List<IUpdate>();

        public void UpdateTickables()
        {
            foreach (IUpdate tickable in Tickables)
                tickable.Update();
        }

        public void Activate()
        {
            OnActivate();
        }

        public virtual void RegisterSystems() { }

        public virtual void Update() { }

        public virtual void OnActivate() { }

        public virtual void OnDeactivate() { }
    }
}
