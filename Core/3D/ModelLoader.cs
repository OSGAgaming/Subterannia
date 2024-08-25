using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using SkinnedModel;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Reflection;
using System.Diagnostics;
using Subterannia.Core.Utility;
using Subterannia.Core.Mechanics.Interfaces;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading.Tasks;
using Subterannia.Core.Mechanics;

namespace Subterannia.Core.Mechanics
{
    public class ModelPathAttribute : Attribute
    {
        public string target;
        public ModelPathAttribute(string type)
        {
            target = type;
        }
    }

    public class ModelLoader : IMainThreadLoad
    {
        public static ContentManager contentManager;
        public static MethodInfo create_ContentReader;
        public static MethodInfo readAsset;

        public void Load()
        {
            InitializeContentReader();

            FieldInfo[] Models = typeof(ModelRepository).GetFields();
            for(int i = 0; i < Models.Length; i++)
            {
                FieldInfo fi = Models[i];

                if (fi.FieldType == typeof(Model))
                {
                    ModelPathAttribute mpa;
                    if (fi.TryGetCustomAttribute(out mpa))
                    {
                        fi.SetValue(null, LoadModel(out _, mpa.target));
                        continue;
                    }
                    fi.SetValue(null, LoadModel(out _, fi.Name));
                }
            }

        }

        public void Unload()
        {
            contentManager = null;
            create_ContentReader = null;
            readAsset = null;

            FieldInfo[] Models = typeof(ModelRepository).GetFields();
            foreach (FieldInfo fi in Models)
            {
                if (fi.FieldType == typeof(Model))
                {
                    fi.SetValue(null, null);
                }
            }
        }

        public static void InitializeContentReader() 
        {
            contentManager = new ContentManager(Main.ShaderContentManager.ServiceProvider);
            create_ContentReader = typeof(ContentManager).GetMethod("GetContentReaderFromXnb", BindingFlags.NonPublic | BindingFlags.Instance);

            readAsset = typeof(ContentReader).GetMethod("ReadAsset", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(typeof(object));
            //Check
        }

        public static Model LoadModel(out Model model, string Path)
        {
            string FullPath = $"{AssetDirectories.ModName}Assets/{AssetDirectories.Models}{Path}.urmom";
            byte[] file = ModContent.GetFileBytes(FullPath);
            model = LoadAsset<Model>(new MemoryStream(file));

            return model;
        }

        public static T LoadAsset<T>(Stream stream)
        {
            using BinaryReader xnbReader = new BinaryReader(stream);
            using (ContentReader contentReader = GetContentReaderFromXnb("UntexturedSphere", stream, xnbReader, null))
            {
                var thi = readAsset.Invoke(contentReader, null);
                return (T)thi;
            }
        }

        public static ContentReader GetContentReaderFromXnb(string originalAssetName, Stream stream, BinaryReader xnbReader, Action<IDisposable> recordDisposableObject)
        {
            List<char> targetPlatformIdentifiers = new List<char>
            {
               'w', 'x', 'i', 'a', 'd', 'X', 'n', 'r', 'P', '5',
               'O', 'S', 'G', 'b', 'W', 'M', 'm', 'p', 'v', 'g',
               'l'
            };

            byte num = xnbReader.ReadByte();
            byte b = xnbReader.ReadByte();
            byte b2 = xnbReader.ReadByte();
            byte item = xnbReader.ReadByte();
            if (num != 88 || b != 78 || b2 != 66 || !targetPlatformIdentifiers.Contains((char)item))
            {
                throw new ContentLoadException("Asset does not appear to be a valid XNB file. Did you process your content for Windows?");
            }
            byte b3 = xnbReader.ReadByte();
            byte num2 = xnbReader.ReadByte();
            bool flag = (num2 & 0x80) != 0;
            bool flag2 = (num2 & 0x40) != 0;
            if (b3 != 5 && b3 != 4)
            {
                throw new ContentLoadException("Invalid XNB version");
            }
            int num3 = xnbReader.ReadInt32();
            Stream stream2 = null;
            if (flag || flag2)
            {
                int decompressedSize = xnbReader.ReadInt32();
                if (flag)
                {
                    int compressedSize = num3 - 14;
                    stream2 = new LzxDecoderStream(stream, decompressedSize, compressedSize);
                }
                else if (flag2)
                {
                    stream2 = new Lz4DecoderStream(stream);
                }
            }
            else
            {
                stream2 = stream;
            }

            Type type = typeof(ContentReader);
            ContentReader c = type.Assembly.CreateInstance(type.FullName, false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, [contentManager, stream2, originalAssetName, b3, null, recordDisposableObject], null, null) as ContentReader;

            return c;
        }
    }
}

