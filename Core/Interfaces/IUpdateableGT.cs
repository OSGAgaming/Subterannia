using Microsoft.Xna.Framework;
using System.Drawing;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Subterannia.Core.Mechanics.Interfaces
{
    public interface IUpdateableGT
    {
        void Update(GameTime gameTime);
    }
}