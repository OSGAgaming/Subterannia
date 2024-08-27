using Subterannia.Core.Mechanics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework.Input;
using Subterannia.Core.Utility;
using ReLogic.Content;
using CppNet;
using System.Drawing;

namespace Subterannia.Core.Subworlds.LinuxSubworlds
{
    public partial class CutsceneSubworld : Subworld
    {
        public void GenerateFang(int xStart, int yStart, int strength, int steps, int type, float angle, int iterations, float curve = 0.01f)
        {
            Vector2 direction = Utilities.VectorFromRotation(angle, strength / 5);
            Vector2 source = new Vector2(xStart, yStart);
            float sizeMutation = 0.8f;

            for (int i = 0; i < iterations; i++)
            {
                float size = 1 - (i / (float)iterations) * sizeMutation;

                WorldGen.TileRunner((int)source.X, (int)source.Y, strength * size, steps, type, true);
                source += direction;
                direction = direction.RotatedBy(curve);
            }
        }

        public void GenerateBranch(int x, int y, int size, int side, int iters = 0, int maxHeight = -1, int flip = 1)
        {
            if (iters >= 3 || size <= 0)
            {
                WorldGen.TileRunner(x, y, 5, 2, TileID.LivingMahoganyLeaves, true, 0, 0, false, false);
                return;
            }

            if (iters > 0)
            {
                if (WorldGen.genRand.NextBool(2))
                    WorldGen.TileRunner(x, y, 5, 2, TileID.LivingMahoganyLeaves, true, 0, 0, false, false);
            }

            int XSize = WorldGen.genRand.Next(size - 1, size + 2);
            int YSize = WorldGen.genRand.Next(size - 1, size + 2);

            for (int i = 0; i <= XSize; i++)
            {
                if (i == 0) continue;

                Tile t = Framing.GetTileSafely(i * side + x, y);
                Tile tU = Framing.GetTileSafely(i * side + x, y - 1);
                Tile tD = Framing.GetTileSafely(i * side + x, y + 1);

                if (t.HasTile || tU.HasTile || tD.HasTile) return;
                if (i == XSize)
                {
                    Tile t1 = Framing.GetTileSafely((i + 1) * side + x, y);
                    if (t1.HasTile) return;
                }
            }

            for (int i = 0; i <= YSize; i++)
            {
                if (i == 0) continue;

                Tile t = Framing.GetTileSafely(x + XSize * side, y - i);
                Tile tL = Framing.GetTileSafely(x + XSize * side - 1, y - i * flip);
                Tile tR = Framing.GetTileSafely(x + XSize * side + 1, y - i * flip);

                if (t.HasTile || tL.HasTile || tR.HasTile || (maxHeight != -1 && y - i < maxHeight)) return;
                if (i == YSize)
                {
                    Tile t1 = Framing.GetTileSafely(x + XSize * side, y - (i - 1) * flip);
                    if (t1.HasTile) return;
                }
            }

            for (int i = 0; i <= XSize; i++)
            {
                if (WorldGen.InWorld(i * side + x, y))
                    WorldGen.PlaceTile(i * side + x, y, TileID.LivingWood, false, true);
            }

            for (int i = 0; i <= YSize; i++)
            {
                if (WorldGen.InWorld(x + XSize * side, y - i * flip))
                    WorldGen.PlaceTile(x + XSize * side, y - i * flip, TileID.LivingWood, false, true);
            }

            int EndsX = x + XSize * side;
            int EndsY = y - YSize * flip;

            GenerateBranch(EndsX, EndsY, XSize - WorldGen.genRand.Next(2, XSize / 2 + 2), 1, iters + 1);
            GenerateBranch(EndsX, EndsY, XSize - WorldGen.genRand.Next(2, XSize / 2 + 2), -1, iters + 1);
        }

        public void GenerateTree(int xStart, int yStart, int flip = 1)
        {
            int height = 40;
            int branchSize = 4;

            float lTrunk = WorldGen.genRand.Next(-3, 1);
            float rTrunk = WorldGen.genRand.Next(0, 4);

            float origLTrunk = lTrunk;
            float origRTrunk = rTrunk;

            int baseWidthL = WorldGen.genRand.Next(0, 2);
            int baseWidthR = WorldGen.genRand.Next(0, 2);

            float baseDecay = 1.2f;

            for (int i = 0; i < height; i++)
            {
                for (int a = (int)lTrunk - baseWidthL; a <= rTrunk + baseWidthR; a++)
                {
                    int x = xStart + a;
                    int y = yStart - i * flip;
                    if (WorldGen.InWorld(x, y))
                    {
                        WorldGen.PlaceTile(x, y, TileID.LivingWood);
                    }
                }


                lTrunk /= baseDecay;
                rTrunk /= baseDecay;
            }

            lTrunk = origLTrunk;
            rTrunk = origRTrunk;

            for (int i = 0; i < height; i++)
            {
                for (int a = (int)lTrunk - baseWidthL; a <= rTrunk + baseWidthR; a++)
                {
                    int x = xStart + a;
                    int y = yStart - i * flip;
                    if (WorldGen.InWorld(x, y))
                    {
                        if (i > 5)
                        {
                            if (WorldGen.genRand.NextBool(4) && a == (int)lTrunk - baseWidthL) GenerateBranch(x, y,
                                branchSize - WorldGen.genRand.Next(0, branchSize / 2), -1, 0, yStart - height, flip);
                            if (WorldGen.genRand.NextBool(4) && a == (int)rTrunk + baseWidthR) GenerateBranch(x, y,
                                branchSize - WorldGen.genRand.Next(0, branchSize / 2), 1, 0, yStart - height, flip);
                        }
                    }
                }


                lTrunk /= baseDecay;
                rTrunk /= baseDecay;

                if (i == height - 1)
                {
                    int x = xStart;
                    int y = yStart - i * flip;
                    WorldGen.TileRunner(x, y, 15, 4, TileID.LivingMahoganyLeaves, true, 0, 0, false, false);
                }
            }
        }
        public override void WorldGeneration()
        {
            SmoothedGenerationPercentage = 0;

            AddGenerationPass(new SubworldGenerationPass(
            "Fangs",
             0,
            () =>
            {
                int noOfTendrils = 25;
                for (int i = 0; i < noOfTendrils; i++)
                {
                    int X = WorldGen.genRand.Next(0, 499);
                    int Y = WorldGen.genRand.Next(30, 100);

                    float perc = X / 500f;
                    float leftAngle = -0.3f;
                    float rightAngle = 0.3f;

                    float curve = MathHelper.Lerp(leftAngle, rightAngle, perc);

                    GenerateFang(X, Y, 18, 9, TileID.Dirt, 0, 10, curve);
                }
            }));

            AddGenerationPass(new SubworldGenerationPass(
            "Fang Shrubs", 0,
            () =>
            {
                for (int i = 0; i < Dimensions.X; i++)
                {
                    for (int j = 0; j < Dimensions.Y; j++)
                    {
                        Tile t = Framing.GetTileSafely(i, j);
                        Tile tB = Framing.GetTileSafely(i, j + 1);
                        Tile tU = Framing.GetTileSafely(i, j - 1);
 

                        Tile tL = Framing.GetTileSafely(i - 1, j);
                        Tile tR = Framing.GetTileSafely(i + 1, j);

                        if (t.HasTile && (!tB.HasTile) && tU.HasTile)
                        {
                            WorldGen.PlaceTile(i, j, TileID.TeamBlockGreen, false, true);
                        }
                    }
                }
            }));

            AddGenerationPass(new SubworldGenerationPass(
            "Top Ledge", 0,
            () =>
            {
                FastNoiseLite noise = new FastNoiseLite();
                noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
                noise.SetFrequency(0.005f);

                int accuracy = 50;
                for (int i = 0; i < accuracy; i++)
                {
                    float perc = i / (float)accuracy;
                    int X = (int)MathHelper.Lerp(0, 500, perc);

                    for (int j = 0; j < 3; j++)
                    {
                        WorldGen.TileRunner(X, 50 - (int)(noise.GetNoise(X, 0) * 10) - 20, 40, 40, TileID.Dirt, true);
                    }
                }
            }));

            AddGenerationPass(new SubworldGenerationPass(
            "Generate Trees", 0,
            () =>
            {
                for (int i = 0; i < Dimensions.X; i++)
                {
                    for (int j = 0; j < Dimensions.Y; j++)
                    {
                        int height = 40;
                        bool hasSpace = true;
                        for (int a = 1; a < height; a++)
                        {
                            Tile t = Framing.GetTileSafely(i, j + a);
                            if (t.HasTile)
                            {
                                hasSpace = false;
                                break;
                            }
                        }

                        Tile tile = Framing.GetTileSafely(i, j);
                        if (hasSpace && tile.HasTile && (tile.TileType == TileID.Dirt || tile.TileType == TileID.TeamBlockGreen) && WorldGen.genRand.NextBool(25))
                        {
                            GenerateTree(i, j, -1);
                        }
                    }
                }
            }));
        }
    }
}