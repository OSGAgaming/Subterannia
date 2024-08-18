
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Social;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Subterannia.Core.Subworlds
{
    //Webhook
    public abstract class Subworld
    {
        public virtual string Name => "Subworld";

        public virtual Point Dimensions => Point.Zero;

        public virtual Point SpawnTile => Point.Zero;

        protected List<SubworldGenerationPass> Generation = new List<SubworldGenerationPass>();
        public SubworldGenerationPass CurrentPass = null;

        protected float TotalWeight = 0;
        protected float GenerationWeight = 0;
        protected float GenerationCompletion
        { 
            get
            {
                if (TotalWeight != 0) return GenerationCompletion / TotalWeight;
                else return 0;
            }
        }

        internal void Generate(int seed, GenerationProgress customProgressObject = null) 
        {
            Main.maxTilesX = Dimensions.X;
            Main.maxTilesY = Dimensions.Y;
            Main.spawnTileX = SpawnTile.X;
            Main.spawnTileY = SpawnTile.Y;

            SubworldManager.Reset(seed);
            SubworldManager.PostReset(customProgressObject);

            TotalWeight = 0;
            GenerationWeight = 0;

            WorldGeneration();
            Generate();

            Subterannia.GetLoadable<SubworldInstance>().IsSaving = false;
        }

        public void AddGenerationPass(SubworldGenerationPass Pass) 
        {
            Generation.Add(Pass);
            Generation = Generation.OrderBy(n => n.Order).ToList();
            TotalWeight += Pass.Weight;
        }

        public void Generate()
        {
            foreach(SubworldGenerationPass pass in Generation)
            {
                GenerationWeight += pass.Weight;

                CurrentPass = pass;
                pass.Generation.Invoke();
            }
        }

        public virtual void WorldGeneration() { }

        public virtual void PlayerUpdate(Player player) { }

        public virtual void DrawLoadingUI(SpriteBatch sb) { }
    }

    public class SubworldGenerationPass
    {
        public float Order;
        public string PassName;
        public Action Generation;
        public float Weight;

        public SubworldGenerationPass(string PassName, float Order, Action Generation = null, float Weight = 1) 
        {
            this.PassName = PassName;
            this.Order = Order;
            this.Generation = Generation;
            this.Weight = Weight;
        }
    }
}