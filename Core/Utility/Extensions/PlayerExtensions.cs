using System.Linq;
using Terraria;

namespace Subterannia.Core.Utility
{
    public static class PlayerExtensions
    {
        public static bool IsAlive(this Player player) => player?.active is true && !(player.dead || player.ghost);

        public static bool IsUnderwater(this Player player) => Collision.DrownCollision(player.position, player.width, player.height, player.gravDir);

        public static bool InSpace(this Player player)
        {
            float x = Main.maxTilesX / 4200f;
            x *= x;
            float spaceGravityMult = (float)((player.position.Y / 16f - (60f + 10f * x)) / (Main.worldSurface / 6.0));
            return spaceGravityMult < 1f;
        }

        public static bool PillarZone(this Player player) => player.ZoneTowerStardust || player.ZoneTowerSolar || player.ZoneTowerVortex || player.ZoneTowerNebula;

        public static bool InventoryHas(this Player player, params int[] items) => items.Any(itemType => player.HasItem(itemType));//player.inventory.Any(item => items.Contains(item.type));
    }
}