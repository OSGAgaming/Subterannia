using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Subterannia.Core.Mechanics.Interfaces;
using Terraria;

namespace Subterannia.Core.Mechanics
{
    public class LocalRenderer : IMainThreadLoad
    {
        public static Point MaxResolution => new Point(2560, 1440);
        public static Rectangle MaxResolutionBounds => new Rectangle(0, 0, MaxResolution.X, MaxResolution.Y);

        public static GraphicsDeviceManager GraphicsDeviceManager => Main.graphics;

        public static GraphicsDevice Device => GraphicsDeviceManager.GraphicsDevice;

        public static Viewport Viewport => Device.Viewport;

        public static Point ViewportSize => new Point(Viewport.Width, Viewport.Height);

        public static PresentationParameters PresentationParameters => Device.PresentationParameters;

        public static Point BackBufferSize => new Point(PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

        public static FirstPersonCamera DefaultCamera { get; set; }

        public static CameraTransform UICamera { get; set; }

        public void Load()
        {
            LayerSet.Load();

            InitializeCameras();
            RegisterLayers();
        }

        public void Unload()
        {
            LayerSet.Unload();
            DefaultCamera = null;
            UICamera = null;
        }

        public static void InitializeCameras()
        {
            DefaultCamera = new FirstPersonCamera(null, Vector3.UnitZ, fieldOfView: MathHelper.PiOver2 * 1.2f, farPlane: 10000, frustrum: FrustrumType.Orthographic);
        }

        public static void RegisterLayers()
        {     
            LayerSet.RegisterLayer(new CenterScisorLayer(0, DefaultCamera, SubteranniaMod.PixelationShader), "Default");
            LayerSet.RegisterLayer(new Layer(2, new CameraTransform(Vector3.UnitZ)), "Logger");
        }

        public static void DrawSceneToTarget(ModEntitySet scene, SpriteBatch sb)
        {
            if (scene != null)
            {
                LayerSet.DrawLayersToTarget(scene, sb);
                LayerSet.DrawLayers(sb);
            }
        }
    }
}
