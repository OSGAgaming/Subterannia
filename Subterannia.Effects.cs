
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Subterannia.Core.Utility;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Subterannia
{
    public class ShaderPathAttribute : Attribute
    {
        public string target;
        public bool load;
        public string pass;

        public ShaderPathAttribute(string type, bool loadToDict = false, string pass = "P1")
        {
            target = type;
            load = loadToDict;
            this.pass = pass;
        }
    }

    public partial class SubteranniaMod : Mod
    {
        public static Asset<Effect> PrimitiveShader;
        [ShaderPath("ModelShaders/ExampleModelShader")]
        public static Asset<Effect> ExampleModelShader;
        [ShaderPath("PixelationShader", true)]
        public static Asset<Effect> PixelationShader;
        [ShaderPath("ModelShaders/NormalMapModelShader", true, "P1")]
        public static Asset<Effect> NormalMapModelShader;

        public static void UnloadShaders()
        {
            FieldInfo[] Models = typeof(SubteranniaMod).GetFields();
            for (int i = 0; i < Models.Length; i++)
            {
                FieldInfo fi = Models[i];

                if (fi.FieldType == typeof(Effect))
                {
                    fi.SetValue(null, null);
                }
            }
        }

        public static void LoadShaders()
        {
            FieldInfo[] Models = typeof(SubteranniaMod).GetFields();
            for (int i = 0; i < Models.Length; i++)
            {
                FieldInfo fi = Models[i];

                if (fi.FieldType == typeof(Asset<Effect>))
                {
                    ShaderPathAttribute spa;
                    if (fi.TryGetCustomAttribute(out spa))
                    {
                        Asset<Effect> effect = Utilities.GetEffect($"Effects/{spa.target}", AssetRequestMode.ImmediateLoad);

                        fi.SetValue(null, effect);
                        continue;
                    }
                    fi.SetValue(null, Utilities.GetEffect($"Effects/{fi.Name}", AssetRequestMode.ImmediateLoad));
                }
            }
        }

        static void QuickLoadScreenShader(string Path)
        {
            string EffectPath = "Effects/ScreenShaders/" + Path;
            string DictEntry = AssetDirectories.ModName + ":" + Path;
            Asset<Effect> effect = Utilities.GetEffect(EffectPath);

            Filters.Scene[DictEntry] = new Filter(new ScreenShaderData(effect, "P1"), EffectPriority.VeryHigh);
            Filters.Scene[DictEntry].Load();
        }

        static void LoadScreenShaders()
        {
            string[] Shaders = Directory.GetFiles($@"{Main.SavePath}\ModSources\{AssetDirectories.ModName}\Assets\Effects\ScreenShaders");
            for (int i = 0; i < Shaders.Length; i++)
            {
                string filePath = Shaders[i];

                if (filePath.Contains(".xnb") ||
                    filePath.Contains(".exe") ||
                    filePath.Contains(".dll")) continue;

                string charSeprator = @"ScreenShaders\";
                int Index = filePath.IndexOf(charSeprator) + charSeprator.Length;
                string AlteredPath = filePath.Substring(Index);

                QuickLoadScreenShader(AlteredPath.Replace(".fx", ""));
            }

            FieldInfo[] Models = typeof(SubteranniaMod).GetFields();
            for (int i = 0; i < Models.Length; i++)
            {
                FieldInfo fi = Models[i];

                if (fi.FieldType == typeof(Effect))
                {
                    Asset<Effect> effect;
                    ShaderPathAttribute spa;
                    if (fi.TryGetCustomAttribute(out spa))
                    {
                        effect = Utilities.GetEffect($"Effects/{spa.target}");
                        if (!spa.load) continue;

                        Filters.Scene[fi.Name] = new Filter(new ScreenShaderData(effect, spa.pass), EffectPriority.VeryHigh);
                        Filters.Scene[fi.Name].Load();
                    }                    
                }
            }
        }
        public void ShaderLoading()
        {
            LoadShaders();
            LoadScreenShaders();
        }
    }
}