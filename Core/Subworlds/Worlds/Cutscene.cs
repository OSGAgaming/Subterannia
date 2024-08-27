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

namespace Subterannia.Core.Subworlds.LinuxSubworlds
{
    public partial class CutsceneSubworld : Subworld
    {
        public Asset<Texture2D> LoadingBar;
        public Asset<Texture2D> CenterPiece;
        public Asset<Texture2D> LoadingDrill;
        public Asset<Texture2D> LoadingBarFill;

        private float SmoothedGenerationPercentage;

        public override Point Dimensions => new Point(500, 500);

        public override Point SpawnTile => new Point(10, 200);

        public override string Name => "Cutscene";

        public override void PlayerUpdate(Player player) { }

        public override void DrawLoadingUI(SpriteBatch sb)
        {
            Utilities.DrawBoxFill(new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);
            if (CurrentPass != null)
            {
                SmoothedGenerationPercentage += (GenerationCompletion - SmoothedGenerationPercentage) / 16f;

                Point center = new Point(Main.screenWidth / 2, Main.screenHeight / 2);
                Utilities.UITextToCenter(
                    CurrentPass?.PassName ?? "Fetching Information",
                    Color.White,
                    new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 100), 0);

                Utilities.UITextToCenter(
                    (int)(SmoothedGenerationPercentage * 100) + "%",
                    Color.White,
                    new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 + 100), 0);

                LoadingBar ??= Utilities.GetTexture("SubworldAssets/LoadingBar");
                CenterPiece ??= Utilities.GetTexture("SubworldAssets/LoadingBarMiddlePiece");
                LoadingDrill ??= Utilities.GetTexture("SubworldAssets/LoadingBarDrill");
                LoadingBarFill ??= Utilities.GetTexture("SubworldAssets/LoadingBarFill");

                Texture2D bar = LoadingBar.Value;

                sb.Draw(bar, new Vector2(center.X, center.Y), bar.Bounds,
                    Color.White, 0, bar.TextureCenter(), 1f, SpriteEffects.None, 0f);

                Texture2D drill = LoadingDrill.Value;
                int speed = 10;
                int frameCount = 5;
                int frame = ((int)Main.GameUpdateCount / speed) % frameCount;


                Texture2D centerPiece = CenterPiece.Value;
                Texture2D loadingBarFill = LoadingBarFill.Value;

                sb.Draw(loadingBarFill, new Vector2(center.X - bar.Width / 2, center.Y - bar.Height / 2),
                    new Rectangle(0, 0, (int)(loadingBarFill.Width * SmoothedGenerationPercentage), loadingBarFill.Height),
                    Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                sb.Draw(drill, new Vector2(center.X - bar.Width / 2 + SmoothedGenerationPercentage * bar.Width, center.Y),
                    new Rectangle(0, (drill.Height / frameCount) * frame, drill.Width, drill.Height / frameCount),
                    Color.White, 0, new Vector2(drill.Width / 2, drill.Height / (2 * frameCount)), 1f, SpriteEffects.None, 0f);

                sb.Draw(centerPiece, new Vector2(center.X, center.Y), centerPiece.Bounds,
                    Color.White, 0, centerPiece.TextureCenter(), 1f, SpriteEffects.None, 0f);
            }
        }
    }
}