using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Subterannia.Core.Mechanics.Interfaces;
using Subterannia.Core.Mechanics;
using Terraria;
using Microsoft.Xna.Framework.Input;
using Terraria.ID;
using ReLogic.Content;
using Subterannia.Core.Utility;

namespace Subterannia.Core.Mechanics
{
	public class Debug3DScene : ModEntitySet
    {
        public ModelComponent Clouds;
        public ModelComponent Planets;

        public ModelComponent Doodle1;
        public ModelComponent Doodle2;

        public Asset<Texture2D> Noise;

        public override void OnActivate() 
        {
            Noise ??= Utilities.GetTexture("Maps/noise",AssetRequestMode.ImmediateLoad);

            Clouds = new ModelComponent(ModelRepository.Clouds, false, SubteranniaMod.ExampleModelShader);
            Clouds.Layer = "Default";
            Clouds.ShaderParameters = (effect) =>
            {
                effect.Parameters["Progress"]?.SetValue(Main.GameUpdateCount);
                effect.Parameters["noiseTexture"]?.SetValue(Noise.Value);
            };

            Planets = new ModelComponent(ModelRepository.Planet, false);
            Planets.Layer = "Default";
            Planets.ShaderParameters = (effect) =>
            {
                effect.Parameters["Progress"]?.SetValue(Main.GameUpdateCount);
                effect.Parameters["noiseTexture"]?.SetValue(Noise.Value);
            };

            Doodle1 = new ModelComponent(ModelRepository.Doodle, false);
            Doodle1.Layer = "Default";

            Doodle2 = new ModelComponent(ModelRepository.Doodle, false);
            Doodle2.Layer = "Default";

            //==================Drawables=====================
            Drawables.Add(Planets);
            Drawables.Add(Clouds);
            Drawables.Add(Doodle1);
            Drawables.Add(Doodle2);
        }

        public override void OnDeactivate() 
        { 

        }

        public override void Update()
        {
            KeyboardState k = Keyboard.GetState();

            Planets.Transform.Position.Z = 1000f;
            Planets.Transform.Scale = 0.4f;
            Planets.Transform.Position.X = Main.LocalPlayer.Center.X;
            Planets.Transform.Position.Y = Main.LocalPlayer.Center.Y + 100;
            Planets.Transform.Rotation.Y += 0.03f;

            Clouds.Transform.Position.Z = 1000f;
            Clouds.Transform.Position.X = Main.LocalPlayer.Center.X;
            Clouds.Transform.Position.Y = Main.LocalPlayer.Center.Y + 100;
            Clouds.Transform.Scale = 0.5f;
            Clouds.Transform.Rotation.Y += 0.01f;

            Doodle1.Transform.Position.Z = 1000f;
            Doodle2.Transform.Position.Z = 1000f;

            Doodle1.Transform.Position.X = Main.LocalPlayer.Center.X + (float)Math.Sin(Main.GameUpdateCount / 40f) * 200;
            Doodle1.Transform.Position.Y = Main.LocalPlayer.Center.Y - 100;

            Doodle1.Transform.Scale = 0.1f;
            Doodle1.Transform.Rotation.Z += 0.05f;
            Doodle1.Transform.Rotation.Y += 0.1f;
            Doodle1.Transform.Rotation.X = 0f;

            Doodle2.Transform.Position.X = Main.LocalPlayer.Center.X - (float)Math.Sin(Main.GameUpdateCount / 40f) * 200;
            Doodle2.Transform.Position.Y = Main.LocalPlayer.Center.Y - 100;

            Doodle2.Transform.Scale = 0.1f;
            Doodle2.Transform.Rotation.Z -= 0.05f;
            Doodle2.Transform.Rotation.Y -= 0.1f;
            Doodle2.Transform.Rotation.X = 0f;

        }
    }
}
