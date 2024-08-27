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
        public ModelComponent Statue;
        public ModelComponent Rat;

        public Asset<Texture2D> Noise;

        public override void OnActivate() 
        {
            Noise ??= Utilities.GetTexture("Maps/noise",AssetRequestMode.ImmediateLoad);

            Clouds = new ModelComponent(ModelRepository.Clouds, false, SubteranniaMod.ExampleModelShader);


            Planets = new ModelComponent(ModelRepository.Planet, false);
            Planets.ShaderParameters = (effect) =>
            {
                effect.Parameters["Progress"]?.SetValue(Main.GameUpdateCount);
                effect.Parameters["noiseTexture"]?.SetValue(Noise.Value);
            };

            Doodle1 = new ModelComponent(ModelRepository.Doodle, false);

            Doodle2 = new ModelComponent(ModelRepository.Doodle, false);

            Statue = new ModelComponent(ModelRepository.Statue, false);
            Rat = new ModelComponent(ModelRepository.Rat, false);

            //==================Drawables=====================
            Drawables.Add(Planets);
            Drawables.Add(Clouds);
            //Drawables.Add(Doodle1);
            //Drawables.Add(Doodle2);
            Drawables.Add(Statue);
            Drawables.Add(Rat);
        }

        public override void OnDeactivate() 
        { 

        }

        public override void Update()
        {
            KeyboardState k = Keyboard.GetState();

            Planets.Transform.Position.Z = 1000f;
            Planets.Transform.Scale = 0.4f;
            Planets.Transform.Position.X = Main.LocalPlayer.Center.X + 70;
            Planets.Transform.Position.Y = Main.LocalPlayer.Center.Y;
            Planets.Transform.Rotation.Y += 0.03f;

            Clouds.Transform.Position.Z = 1000f;
            Clouds.Transform.Position.X = Main.LocalPlayer.Center.X + 70;
            Clouds.Transform.Position.Y = Main.LocalPlayer.Center.Y;
            Clouds.Transform.Scale = 0.5f;
            Clouds.Transform.Rotation.Y += 0.01f;

            //Doodle1.Transform.Position.Z = 1000f;
            //Doodle2.Transform.Position.Z = 1000f;

            //Doodle1.Transform.Position.X = Main.LocalPlayer.Center.X + (float)Math.Sin(Main.GameUpdateCount / 40f) * 200;
            //Doodle1.Transform.Position.Y = Main.LocalPlayer.Center.Y - 100;

            //Doodle1.Transform.Scale = 0.1f;
            //Doodle1.Transform.Rotation.Z += 0.05f;
            //Doodle1.Transform.Rotation.Y += 0.1f;
            //Doodle1.Transform.Rotation.X = 0f;

            //Doodle2.Transform.Position.X = Main.LocalPlayer.Center.X - (float)Math.Sin(Main.GameUpdateCount / 40f) * 200;
            //Doodle2.Transform.Position.Y = Main.LocalPlayer.Center.Y - 100;

            //Statue.Transform.Position.X = Main.LocalPlayer.Center.X - (float)Math.Sin(Main.GameUpdateCount / 40f) * 200;
            //Statue.Transform.Position.Y = Main.LocalPlayer.Center.Y - 100;
            //Statue.Transform.Scale = 4f;
            //Statue.Transform.Position.Z = 1000;
            //Statue.Transform.Rotation.Y -= 0.05f;
            //Statue.Transform.Rotation.Z = 0f;

            //Doodle2.Transform.Scale = 0.1f;
            //Doodle2.Transform.Rotation.Z -= 0.05f;
            //Doodle2.Transform.Rotation.Y -= 0.1f;
            //Doodle2.Transform.Rotation.X = 0f;

            //Rat.Transform.Position.X = Main.LocalPlayer.Center.X - (float)Math.Sin(Main.GameUpdateCount / 40f) * 200;
            //Rat.Transform.Position.Y = Main.LocalPlayer.Center.Y - 300;
            //Rat.Transform.Position.Z = 1000f;
            //Rat.Transform.Rotation.Y -= 0.05f;

            Clouds.ShaderParameters = (effect) =>
            {
                effect.Parameters["Progress"]?.SetValue(Main.GameUpdateCount);
                effect.Parameters["noiseTexture"]?.SetValue(Noise.Value);
                effect.Parameters["worldColor"]?.SetValue(Utilities.GetWorldLighting(Clouds.Transform.Position.XY()).ToVector3());
            };
        }
    }
}
