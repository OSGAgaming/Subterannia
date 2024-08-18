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

namespace Subterannia.Core.Subworlds.LinuxSubworlds
{
    public class CutsceneSubworld : Subworld
    {
        public override Point Dimensions => new Point(500, 500);

        public override Point SpawnTile => new Point(10, 200);

        public override string Name => "Cutscene";

        public override void WorldGeneration()
        {
            AddGenerationPass(new SubworldGenerationPass(
            "Fill Region",
             0,
            () =>
            {
                Utilities.FillRegion(new Rectangle(0,0,200,200), TileID.BlueDungeonBrick);
            }));

            AddGenerationPass(new SubworldGenerationPass(
            "Fill Region 2",
            0.1f,
            () =>
            {
                Utilities.FillRegion(new Rectangle(200, 200, 200, 200), TileID.BlueDungeonBrick);
            }));

            AddGenerationPass(new SubworldGenerationPass(
            "Fill Region 3",
            0.2f,
            () =>
            {
                Utilities.FillRegion(new Rectangle(0, 200, 200, 200), TileID.BlueDungeonBrick);
            }));
        }

        public override void PlayerUpdate(Player player) { }

        public override void DrawLoadingUI(SpriteBatch sb)
        {
            Utilities.DrawBoxFill(new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);
            if (CurrentPass != null)
            {
                Utilities.UITextToCenter(CurrentPass?.PassName ?? "Fetching Information", Color.White, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 100), 0);
                //Utilities.UITextToCenter((int)(GenerationCompletion * 100) + "%", Color.White, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2), 0);
            }
        }
    }
}