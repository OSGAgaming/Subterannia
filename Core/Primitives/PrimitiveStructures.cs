
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using System.Collections.Generic;
using System.Linq;
using System;
using Subterannia.Core.Utility;

namespace Subterannia.Core.Mechanics.Primitives
{
    public partial class Primitive
    {
        public interface IPrimitiveShader
        {
            string ShaderPass { get; }
            void ApplyShader<T>(Effect effect, T trail, List<Vector2> positions, string ESP, float progressParam);
        }
        public class DefaultShader : IPrimitiveShader
        {
            public string ShaderPass => "DefaultPass";
            public void ApplyShader<T>(Effect effect, T trail, List<Vector2> positions, string ESP, float progressParam)
            {
                try
                {
                    effect.Parameters["progress"]?.SetValue(progressParam);
                    effect.CurrentTechnique.Passes[ESP]?.Apply();
                    effect.CurrentTechnique.Passes[ShaderPass]?.Apply();
                }
                catch { }

            }
        }
        protected static Vector2 CurveNormal(List<Vector2> points, int index)
        {
            if (points.Count == 1) return points[0];

            if (index == 0)
            {
                return Clockwise90(Vector2.Normalize(points[1] - points[0]));
            }
            if (index == points.Count - 1)
            {
                return Clockwise90(Vector2.Normalize(points[index] - points[index - 1]));
            }
            return Clockwise90(Vector2.Normalize(points[index + 1] - points[index - 1]));
        }
        protected static Vector2 Clockwise90(Vector2 vector)
        {
            return new Vector2(-vector.Y, vector.X);
        }
        public void PrepareShader(Effect effects, string PassName, float progress = 0)
        {
            int width = _device.Viewport.Width;
            int height = _device.Viewport.Height;
            Vector2 zoom = Main.GameViewMatrix.Zoom;
            Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up) * Matrix.CreateTranslation(width / 2, height / -2, 0) * Matrix.CreateRotationZ(MathHelper.Pi) * Matrix.CreateScale(zoom.X, zoom.Y, 1f);
            Matrix projection = Matrix.CreateOrthographic(width, height, 0, 1000);

            effects.Parameters["WorldViewProjection"].SetValue(view * projection);
            effects.Parameters["noiseTexture"].SetValue(Utilities.GetTexture("Noise/noise").Value);
            effects.Parameters["spotTexture"].SetValue(Utilities.GetTexture("Noise/Spot").Value);
            effects.Parameters["polkaTexture"].SetValue(Utilities.GetTexture("Noise/RandomPolkaDots").Value);
            effects.Parameters["Voronoi"].SetValue(Utilities.GetTexture("Noise/VoronoiNoise").Value);
            effects.Parameters["waterMask"].SetValue(Utilities.GetTexture("Noise/WaterShaderLightMap").Value);

            _trailShader.ApplyShader(effects, this, _points, PassName, progress);
        }

        protected void PrepareBasicShader()
        {
            int width = _device.Viewport.Width;
            int height = _device.Viewport.Height;
            Vector2 zoom = Main.GameViewMatrix.Zoom;
            Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up) * Matrix.CreateTranslation(width / 2, height / -2, 0) * Matrix.CreateRotationZ(MathHelper.Pi) * Matrix.CreateScale(zoom.X, zoom.Y, 1f);
            Matrix projection = Matrix.CreateOrthographic(width, height, 0, 1000);
            _basicEffect.View = view;
            _basicEffect.Projection = projection;
            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }
        }
        protected void AddVertex(Vector2 position, Color color, Vector2 uv)
        {
            if (currentIndex < vertices.Length)
                vertices[currentIndex++] = new VertexPositionColorTexture(new Vector3(position.ForDraw(), 0f), color, uv);
        }
    }
}
