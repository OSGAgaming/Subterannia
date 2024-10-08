﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Globalization;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Subterannia.Core.Utility
{
    // public delegate void ExtraAction();

    // todo: note to self, migrate stuff to utils
    public static partial class Utilities
    {
        #region Spawn helpers

        public static bool NoInvasion(NPCSpawnInfo spawnInfo)
        {
            return !spawnInfo.Invasion && (!Main.pumpkinMoon && !Main.snowMoon || spawnInfo.SpawnTileY > Main.worldSurface || Main.dayTime) && (!Main.eclipse || spawnInfo.SpawnTileY > Main.worldSurface || !Main.dayTime);
        }

        public static bool NoBiome(NPCSpawnInfo spawnInfo)
        {
            Player player = spawnInfo.Player;
            return !player.ZoneJungle && !player.ZoneDungeon && !player.ZoneCorrupt && !player.ZoneCrimson && !player.ZoneHallow && !player.ZoneSnow && !player.ZoneUndergroundDesert;
        }

        public static bool NoZoneAllowWater(NPCSpawnInfo spawnInfo)
        {
            return !spawnInfo.Sky && !spawnInfo.Player.ZoneMeteor && !spawnInfo.SpiderCave;
        }

        public static bool NoZone(NPCSpawnInfo spawnInfo)
        {
            return NoZoneAllowWater(spawnInfo) && !spawnInfo.Water;
        }

        public static bool NormalSpawn(NPCSpawnInfo spawnInfo)
        {
            return !spawnInfo.PlayerInTown && NoInvasion(spawnInfo);
        }

        public static bool NoZoneNormalSpawn(NPCSpawnInfo spawnInfo)
        {
            return NormalSpawn(spawnInfo) && NoZone(spawnInfo);
        }

        public static bool NoZoneNormalSpawnAllowWater(NPCSpawnInfo spawnInfo)
        {
            return NormalSpawn(spawnInfo) && NoZoneAllowWater(spawnInfo);
        }

        public static bool NoBiomeNormalSpawn(NPCSpawnInfo spawnInfo)
        {
            return NormalSpawn(spawnInfo) && NoBiome(spawnInfo) && NoZone(spawnInfo);
        }

        #endregion Spawn helpers

        public static Vector2 RandomPosition(Vector2 pos1, Vector2 pos2)
        {
            return new Vector2(Main.rand.Next((int)pos1.X, (int)pos2.X) + 1, Main.rand.Next((int)pos1.Y, (int)pos2.Y) + 1);
        }

        public static int GetNearestAlivePlayer(NPC npc)
        {
            float NearestPlayerDistSQ = 4815162342f;
            int NearestPlayer = -1;
            for (int i = 0; i < Main.player.Length; i++)
            {
                Player player = Main.player[i];
                float distSQ = player.DistanceSQ(npc.Center);
                if (!(!player.active || player.dead || player.ghost) && distSQ < NearestPlayerDistSQ)
                {
                    NearestPlayerDistSQ = distSQ;
                    NearestPlayer = player.whoAmI;
                }
            }
            return NearestPlayer;
        }

        public static Vector2 VelocityFPTP(Vector2 pos1, Vector2 pos2, float speed)
        {
            Vector2 move = pos2 - pos1;
            //speed2 = speed * 0.5;
            return move * (speed / move.Length());
        }

        /// <summary>
        /// *Используется для нахождения ID ближайшего NPC к точке*
        /// </summary>
        /// <param name="Point">Точка</param>
        /// <param name="Friendly">Считаются ли мирные NPC?</param>
        /// <param name="NoBoss">Не возвращять боссов</param>
        /// <returns>Возвращяет ID ближайшего NPC к точке с задаными параметрами</returns>
        public static int GetNearestNPC(Vector2 Point, bool Friendly = false, bool NoBoss = false)
        {
            float NearestNPCDistSQ = -1;
            int NearestNPC = -1;
            int npcs = Main.npc.Length - 1;
            for (int i = 0; i < npcs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || (NoBoss && npc.boss) || (!Friendly && (npc.friendly || npc.lifeMax <= 5)))
                {
                    continue;
                }

                float distSQ = npc.DistanceSQ(Point);
                if (NearestNPCDistSQ == -1 || distSQ < NearestNPCDistSQ)
                {
                    NearestNPCDistSQ = distSQ;
                    NearestNPC = npc.whoAmI;
                }
            }
            return NearestNPC;
        }

        /// <summary>
        /// *Используется для нахождения ID ближайшего игрока к точке*
        /// </summary>
        /// <param name="Point">Точка</param>
        /// <param name="Alive">Нужны ли только живые игроки</param>
        /// <returns>Возвращяет ID ближайшего игрока к точке с задаными параметрами</returns>
        public static int GetNearestPlayer(Vector2 Point, bool Alive = false)
        {
            float NearestPlayerDistSQ = -1;
            int NearestPlayer = -1;
            for (int i = 0; i < Main.player.Length; i++)
            {
                Player player = Main.player[i];
                if (Alive && (!player.active || player.dead))
                {
                    continue;
                }

                float distSQ = player.DistanceSQ(Point);
                if (NearestPlayerDistSQ == -1 || distSQ < NearestPlayerDistSQ)
                {
                    NearestPlayerDistSQ = distSQ;
                    NearestPlayer = player.whoAmI;
                }
            }
            return NearestPlayer;
        }

        public static int GetNearestPlayer(this NPC npc)
        {
            float NearestPlayerDistSQ = 4815162342f;
            int NearestPlayer = -1;
            for (int i = 0; i < Main.player.Length; i++)
            {
                Player player = Main.player[i];
                float distSQ = player.DistanceSQ(npc.Center);
                if (distSQ < NearestPlayerDistSQ)
                {
                    NearestPlayerDistSQ = distSQ;
                    NearestPlayer = player.whoAmI;
                }
            }
            return NearestPlayer;
        }

        /// <summary>
        /// *Используется для вычислиения инерции от точки до точки с заданой скоростью*
        /// </summary>
        /// <param name="A">Точка А</param>
        /// <param name="B">Точка В</param>
        /// <param name="Speed">Скорость</param>
        /// <returns>Возвращяет инерцию</returns>
        public static Vector2 VelocityToPoint(Vector2 A, Vector2 B, float Speed)
        {
            Vector2 Move = B - A;
            return Move * (Speed / Move.Length());
        }

        /// <summary>
        /// *Вычесление случайной точки в прямоугольнике*
        /// </summary>
        /// <param name="A">Точка прямоугольника A</param>
        /// <param name="B">Точка прямоугольника B</param>
        /// <returns>Случайную точку в прямоугольнике из заданых точек</returns>
        public static Vector2 RandomPointInArea(Vector2 A, Vector2 B)
        {
            return new Vector2(Main.rand.Next((int)A.X, (int)B.X) + 1, Main.rand.Next((int)A.Y, (int)B.Y) + 1);
        }

        /// <summary>
        /// *Вычесление случайной точки в прямоугольнике*
        /// </summary>
        /// <param name="Area">Прямоугольник</param>
        /// <returns>Случайную точку в заданом прямоугольнике</returns>
        public static Vector2 RandomPointInArea(Rectangle Area)
        {
            return new Vector2(Main.rand.Next(Area.X, Area.X + Area.Width), Main.rand.Next(Area.Y, Area.Y + Area.Height));
        }

        /// <summary>
        /// *Используется для поворота объекта между двумя точками*
        /// </summary>
        /// <param name="A">Точка А</param>
        /// <param name="B">Точка B</param>
        /// <returns>Угол поворота между данными точками в раданах</returns>
        public static float rotateBetween2Points(Vector2 A, Vector2 B)
        {
            return (float)Math.Atan2(A.Y - B.Y, A.X - B.X);
        }

        /// <summary>
        /// *Вычисления позиции между двумя точками*
        /// </summary>
        /// <param name="A">Точка A</param>
        /// <param name="B">Точка B</param>
        /// <returns>Точку между точками A и B</returns>
        public static Vector2 CenterPoint(Vector2 A, Vector2 B)
        {
            return new Vector2((A.X + B.X) / 2.0f, (A.Y + B.Y) / 2.0f);
        }

        /// <summary>
        /// *Вычисление позици точки с полярным смещением от другой*
        /// </summary>
        /// <param name="Point">Начальная точка</param>
        /// <param name="Distance">Дистанция смещения</param>
        /// <param name="Angle">Угол смещения в радианах</param>
        /// <param name="XOffset">Смещение по X</param>
        /// <param name="YOffset">Смещение по Y</param>
        /// <returns>Возвращяет точку смещённую по заданым параметрам</returns>
        public static Vector2 PolarPos(Vector2 Point, float Distance, float Angle, int XOffset = 0, int YOffset = 0)
        {
            Vector2 returnedValue = new Vector2
            {
                X = Distance * (float)Math.Sin(MathHelper.ToDegrees(Angle)) + Point.X + XOffset,
                Y = Distance * (float)Math.Cos(MathHelper.ToDegrees(Angle)) + Point.Y + YOffset
            };
            return returnedValue;
        }

        /// <summary>
        /// *Используется для плавного перехода одного вектора в другой*
        /// Добавляет во входящий вектор From разницу To и From делённую на Smooth
        /// </summary>
        /// <param name="From">Исходный вектор</param>
        /// <param name="To">Конечный вектор</param>
        /// <param name="Smooth">Плавность перехода</param>
        /// <returns>Возвращяет вектор From который был приближен к вектору To с плавностью Smooth</returns>
        public static Vector2 SmoothFromTo(Vector2 From, Vector2 To, float Smooth = 60f)
        {
            return From + (To - From) / Smooth;
        }

        // @todo this shit weird af
        public static float DistortFloat(float Float, float Percent)
        {
            float DistortNumber = Float * Percent;
            int Counter = 0;
            while (DistortNumber.ToString(CultureInfo.InvariantCulture).Split(',').Length > 1)
            {
                DistortNumber *= 10;
                Counter++;
            }
            return Float + Main.rand.Next(0, (int)DistortNumber + 1) / (float)Math.Pow(10, Counter) * (Main.rand.NextBool(2) ? -1 : 1);
        }


        public static void DrawAroundOrigin(int index, Color lightColor)
        {
            Projectile projectile = Main.projectile[index];
            Texture2D texture2D = TextureAssets.Projectile[projectile.type].Value;
            Vector2 origin = new Vector2(texture2D.Width * 0.5f, texture2D.Height / Main.projFrames[projectile.type] * 0.5f);
            SpriteEffects effects = projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.spriteBatch.Draw(texture2D, projectile.Center - Main.screenPosition, texture2D.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame), lightColor, projectile.rotation, origin, projectile.scale, effects, 0f);
        }

        public static bool OnGround(NPC npc)
        {
            Vector2 tilePos;
            int minTilePosX = (int)(npc.Center.X / 16.0) - 1;
            int maxTilePosX = (int)(npc.Center.X / 16.0) + 1;
            int minTilePosY = (int)(npc.position.Y / 16.0) - 5;
            int maxTilePosY = (int)((npc.position.Y + npc.height) / 16.0);
            if (minTilePosX < 0)
            {
                minTilePosX = 0;
            }

            if (maxTilePosX > Main.maxTilesX)
            {
                maxTilePosX = Main.maxTilesX;
            }

            if (minTilePosY < 0)
            {
                minTilePosY = 0;
            }

            if (maxTilePosY > Main.maxTilesY)
            {
                maxTilePosY = Main.maxTilesY;
            }
            for (int i = minTilePosX; i < maxTilePosX; ++i)
            {
                for (int j = minTilePosY; j < maxTilePosY + 5; ++j)
                {
                    Tile tile = Framing.GetTileSafely(i, j);
                    if (tile.HasTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType] && tile.TileFrameY == 0))
                    {
                        tilePos.X = i * 16;
                        tilePos.Y = j * 16;

                        if (Math.Abs(npc.Center.Y - tilePos.Y) <= 16 + (npc.height / 2))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool isCollidingWithWall(NPC npc)
        {
            int minTilePosX = (int)(npc.Center.X / 16.0) - 1;
            int maxTilePosX = (int)(npc.Center.X / 16.0) + 1;
            int minTilePosY = (int)(npc.position.Y / 16.0) - 5;
            int maxTilePosY = (int)((npc.position.Y + npc.height) / 16.0);
            if (minTilePosX < 0)
            {
                minTilePosX = 0;
            }

            if (maxTilePosX > Main.maxTilesX)
            {
                maxTilePosX = Main.maxTilesX;
            }

            if (minTilePosY < 0)
            {
                minTilePosY = 0;
            }

            if (maxTilePosY > Main.maxTilesY)
            {
                maxTilePosY = Main.maxTilesY;
            }
            for (int i = minTilePosX; i < maxTilePosX; ++i)
            {
                for (int j = minTilePosY; j < maxTilePosY; ++j)
                {
                    Tile tile = Framing.GetTileSafely(i, j);
                    if (tile.HasTile is true && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType] && tile.TileFrameY == 0))
                    {
                        Vector2 vector2;
                        vector2.X = i * 16;
                        vector2.Y = j * 16;

                        if (Math.Abs(npc.Center.X - vector2.X) <= 16 + (npc.width / 2))
                        {
                            return true;

                        }
                    }
                }
            }
            return false;
        }
    }

    public struct Drop
    {
        public int ItemType { get; }
        public int StackSize { get; }
        public int DropChance { get; }

        public Drop(int itemType, int stackSize, int dropChance)
        {
            ItemType = itemType;
            StackSize = stackSize;
            DropChance = dropChance;
        }
    }
}