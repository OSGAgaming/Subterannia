
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using System.Collections.Generic;
using System.Linq;
using System;
using static Terraria.ModLoader.ModContent;
using System.Reflection;
namespace Subterannia.Core.Mechanics.Primitives
{
    public class PrimitiveManager
    {
        public List<Primitive> _trails = new List<Primitive>();
        public void Draw(SpriteBatch sb)
        {
            foreach (Primitive trail in _trails.ToArray())
            {
                trail.Draw(sb);
            }
        }
        public void Update()
        {
            foreach (Primitive trail in _trails.ToArray())
            {
                trail.Update();
            }
        }
        public void CreateTrail(Primitive PT) => _trails.Add(PT);
    }
    public partial class Primitive
    {
        protected int MaxPoints;
        protected int NumberOfPoints;
        protected IPrimitiveShader _trailShader;
        protected List<Vector2> _points = new List<Vector2>();

        protected GraphicsDevice _device;
        protected Effect _effect;
        protected BasicEffect _basicEffect;
        protected VertexPositionColorTexture[] vertices;
        protected int currentIndex;

        public Primitive()
        {
            _trailShader = new DefaultShader();
            _device = Main.graphics.GraphicsDevice;
            _basicEffect = new BasicEffect(_device);
            _basicEffect.VertexColorEnabled = true;
            SetDefaults();
            vertices = new VertexPositionColorTexture[MaxPoints];
        }


        public void Dispose()
        {
            //PrimitivePass.Instance.Primitives._trails.Remove(this);
        }

        public void Update()
        {
            OnUpdate();
        }
        public void Draw(SpriteBatch sb)
        {
            vertices = new VertexPositionColorTexture[NumberOfPoints];
            currentIndex = 0;

            PrimStructure(sb);
            SetShaders();
            try
            {
                if (NumberOfPoints >= 1)
                    _device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, NumberOfPoints / 3);
            }
            catch
            {
                Main.NewText("Failed To Draw Primitives");
            }
        }
        public virtual void OnUpdate() { }
        public virtual void PrimStructure(SpriteBatch spriteBatch) { }
        public virtual void SetShaders() { }
        public virtual void SetDefaults() { }
        public virtual void OnDestroy() { }
        //Helper methods
    }
}