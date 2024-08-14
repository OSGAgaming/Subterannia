using Terraria;
using Terraria.Utilities;

namespace Subterannia.Core.Utility
{
    public static class RandExtensions
    {
        public static T NextArray<T>(this UnifiedRandom rand, params T[] array) => rand.Next(array);
    }
}