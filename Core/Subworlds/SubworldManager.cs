
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Generation;
using Terraria.GameContent.UI.States;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Social;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using static tModPorter.ProgressUpdate;

namespace Subterannia.Core.Subworlds
{
    public enum SubworldServerState : byte
    {
        None,
        SinglePlayer,
        MultiPlayer
    }
    //TODO: This is awful
    public class SubworldManager
    {
        private static string Path;
        private static WorldGenerator _generator;
        public static SubworldServerState serverState = SubworldServerState.None;
        public static Subworld currentSubworld = null;
        private static void AddGenerationPass(string name, WorldGenLegacyMethod method) => _generator.Append(new PassLegacy(name, method));

        public static void Reset(int seed)
        {
            Logging.Terraria.InfoFormat("Generating World: {0}", Main.ActiveWorldFileData.Name);

            GenVars.configuration = WorldGenConfiguration.FromEmbeddedPath("Terraria.GameContent.WorldBuilding.Configuration.json");
            _generator = new WorldGenerator(seed, GenVars.configuration);

            GenVars.structures = new StructureMap();
            //MicroBiome.ResetAll();

            AddGenerationPass("Reset", delegate (GenerationProgress progress, GameConfiguration passConfig)
            {
                progress.Message = "Resetting";

                Liquid.ReInit();

                Main.cloudAlpha = 0f;
                Main.maxRaining = 0f;
                Main.raining = false;

                WorldGen.RandomizeTreeStyle();
                WorldGen.RandomizeCaveBackgrounds();

                Main.worldID = WorldGen.genRand.Next(int.MaxValue);
            });
        }

        public static void PostReset(GenerationProgress customProgressObject = null)
        {
            _generator.GenerateWorld(customProgressObject);

            Main.WorldFileMetadata = FileMetadata.FromCurrentSettings(FileType.World);
        }

        internal static void PreSaveAndQuit()
        {
            Mod[] mods = ModLoader.Mods;

            for (int i = 0; i < mods.Length; i++)
            {
               //mods[i].PreSaveAndQuit();
            }
        }

        public static void EnterSubworld<T>() where T : Subworld, new()
        {
            if (SubteranniaMod.GetLoadable<SubworldInstance>().IsSaving) 
                return;

            currentSubworld = null;

            SubteranniaMod.GetLoadable<SubworldInstance>().IsSaving = true;
            SoundEngine.PlaySound(SoundID.MenuClose);
            PreSaveAndQuit();
            ThreadPool.QueueUserWorkItem(SaveAndQuitCallBack, new T());
        }

        public static void ReturnToMainWorld()
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
            PreSaveAndQuit();
            ThreadPool.QueueUserWorkItem(SaveAndQuitCallBack, Main.LocalPlayer.GetModPlayer<SubworldPlayer>().PrimaryWorldName);
        }

        public static void SaveAndQuitCallBack(object threadContext)
        {
            int netMode = Main.netMode;
            try
            {
                //SoundEngine.PlaySound(34, -1, -1, 0);
                //SoundEngine.PlaySound(35, -1, -1, 0);
            }
            catch
            {
            }
            if (netMode == 0)
            {
                WorldFile.CacheSaveTime();
            }
            Main.invasionProgress = -1;
            Main.invasionProgressDisplayLeft = 0;
            Main.invasionProgressAlpha = 0f;
            Main.invasionProgressIcon = 0;
            Main.menuMode = 10;
            Main.gameMenu = true;
            SoundEngine.StopTrackedSounds();
            CaptureInterface.ResetFocus();
            Main.ActivePlayerFileData.StopPlayTimer();
            Player.SavePlayer(Main.ActivePlayerFileData);
            Player.ClearPlayerTempInfo();
            Rain.ClearRain();
            if (netMode == NetmodeID.SinglePlayer)
            {
                WorldFile.SaveWorld();
                serverState = SubworldServerState.SinglePlayer;

                //SoundEngine.PlaySound(10);
            }
            else
            {
                serverState = SubworldServerState.MultiPlayer;
                Netplay.Disconnect = true;
                Main.netMode = 0;
            }
            Main.fastForwardTimeToDawn = false;
            Main.fastForwardTimeToDusk = false;
            Main.UpdateTimeRate();
            Main.menuMode = 0;
            if (threadContext != null)
            {
                if (threadContext is Subworld sub)
                {
                    currentSubworld = sub;
                    EnterSub(sub);
                }

                if (threadContext is string subN)
                    EnterSub(subN);
                //((Action)threadContext)();
            }
        }

        public static void Do_worldGenCallBack(object threadContext)
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            WorldGen.clearWorld();
            GenerateWorld((Subworld)threadContext, Main.ActiveWorldFileData.Seed, null);
            WorldFile.SaveWorld(Main.ActiveWorldFileData.IsCloudSave, resetTime: true);

            if (Main.menuMode == 10 || Main.menuMode == 888) Main.menuMode = 6;
            WorldGen.generatingWorld = false;
        }

        public static void GenerateWorld(Subworld subworld, int seed, GenerationProgress customProgressObject = null)
        {
            subworld.Generate(seed, customProgressObject);
        }

        public static void WorldGenCallBack(object threadContext)
        {
            try
            {
                Do_worldGenCallBack(threadContext);
            }
            catch (Exception ex)
            {
                Logging.Terraria.Error(Language.GetTextValue("tModLoader.WorldGenError"), ex);
            }
        }

        public static void CreateNewWorld(Subworld subworld)
        {
            WorldGen.generatingWorld = true;
            Main.rand = new UnifiedRandom(Main.ActiveWorldFileData.Seed);
            WorldGen.gen = true;
            Main.menuMode = 888;
            try
            {
                Main.MenuUI.SetState(new UIWorldLoad());
            }
            catch
            {
            }
            ThreadPool.QueueUserWorkItem(WorldGenCallBack, subworld);
        }

        public static string ConvertToSafeArgument(string arg) => Uri.EscapeDataString(arg);
        public static string SubworldPath => $@"{Main.SavePath}\Worlds\SubterraniaMeta";
        private static void OnWorldNamed(object subworld)
        {
            string Name = "";

            if (subworld is Subworld sub) Name = sub.Name;
            if (subworld is string subN) Name = subN;

            if (Name != Main.LocalPlayer.GetModPlayer<SubworldPlayer>().PrimaryWorldName && subworld is Subworld Subworld)
            {
                Main.LocalPlayer.GetModPlayer<SubworldPlayer>().InSubworld = true;
                Main.LocalPlayer.GetModPlayer<SubworldPlayer>().CurrentSubworld = Subworld;

                Path = $@"{SubworldPath}\{Main.LocalPlayer.GetModPlayer<SubworldPlayer>().PrimaryWorldName}Subworlds";

                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }

                string FinalPath = $@"{Path}\{Name}.wld";

                if (!File.Exists(FinalPath) )
                {
                    WorldGen.clearWorld();
                    Main.autoGenFileLocation = FinalPath;
                    Main.ActiveWorldFileData = WorldFile.CreateMetadata(Name, SocialAPI.Cloud != null && SocialAPI.Cloud.EnabledByDefault,Main.GameMode);
                    Main.ActiveWorldFileData.GameMode = Main.GameMode;

                    CreateNewWorld(Subworld);

                    while (WorldGen.generatingWorld) continue;

                    Main.worldName = Name.Trim();
                    Main.ActivePlayerFileData.SetAsActive();

                    WorldGen.playWorld();
                    Main.menuMode = 10;

                    return;
                } 
                else
                {
                    Main.ActiveWorldFileData = WorldFile.GetAllMetadata(FinalPath, false);
                    Main.ActivePlayerFileData.SetAsActive();
                }
            }
            else
            {
                Main.LocalPlayer.GetModPlayer<SubworldPlayer>().InSubworld = false;
                Main.ActiveWorldFileData = WorldFile.GetAllMetadata($@"{Main.SavePath}\Worlds\{Name}.wld", false);
                Main.ActivePlayerFileData.SetAsActive();
            }

            if (serverState == SubworldServerState.SinglePlayer)
            {
                SoundEngine.PlaySound(SoundID.MenuOpen);
                WorldGen.playWorld();
                Main.menuMode = 10;
            }
            else
            {
                Main.clrInput();

                Netplay.ServerPassword = "";

                Main.GetInputText("");

                Main.autoPass = false;
                Main.menuMode = 30;

                SoundEngine.PlaySound(SoundID.MenuOpen);
            }
        }

        public static void StartClientGameplay()
        {
            Main.menuMode = 10;
            Netplay.StartTcpClient();
        }

        public static void EnterSub(string sub)
        {
            OnWorldNamed(sub);
        }
        public static void EnterSub(Subworld subworld)
        {
            OnWorldNamed(subworld);
        }
    }
}