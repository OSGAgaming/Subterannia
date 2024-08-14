using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Subterannia.Core.Mechanics.Interfaces
{
    public interface IDrawAdditive
    {
        void AdditiveCall(SpriteBatch spriteBatch);
    }
}