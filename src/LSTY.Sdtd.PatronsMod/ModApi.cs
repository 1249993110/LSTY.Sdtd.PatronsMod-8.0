using HarmonyLib;
using IceCoffee.Common.Timers;
using LSTY.Sdtd.PatronsMod.Commands;
using LSTY.Sdtd.PatronsMod.Hooks;
using LSTY.Sdtd.PatronsMod.SignalR;
using MapRendering;
using Microsoft.Owin.Hosting;
using System.Text;

namespace LSTY.Sdtd.PatronsMod
{
    public class ModApi : IModApi
    {
        public readonly static string ModIdentity = typeof(ModApi).Namespace;

        private static AppSettings _appSettings;
        public static AppSettings AppSettings => _appSettings;

        private static Harmony _harmony;
        private static Mod _modInstance;

        public static Harmony Harmony => _harmony;
        public static SynchronizationContext MainThreadSyncContext { get; private set; }

        public static string ModDirectory => _modInstance.Path;

        public static bool IsGameStartDone { get; private set; }
        public static MapTileCache MapTileCache { get; private set; }

        public void InitMod(Mod modInstance)
        {
            try
            {
                _modInstance = modInstance;
                MainThreadSyncContext = SynchronizationContext.Current;

                LoadAppSettings();

                StartupSignalR();

                PatchByHarmony();

                RegisterModEventHandlers();
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Initialize mod: " + ModIdentity + " failed.");
            }
        }

        private static void LoadAppSettings()
        {
            string path = Path.Combine(ModDirectory, "appsettings.json");
            string json = File.ReadAllText(path, Encoding.UTF8);
            
            var appSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<AppSettings>(json);
            if (appSettings == null)
            {
                CustomLogger.Error("Load appsettings failed.");
            }
            else
            {
                _appSettings = appSettings;
            }
        }

        private static void StartupSignalR()
        {
            try
            {
                string url = AppSettings.SignalrUrl;
                WebApp.Start<SignalrStartup>(url);
                CustomLogger.Info($"SignalR Server running on {url}");
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Startup signalR server failed.");
                throw;
            }
        }

        private static void PatchByHarmony()
        {
            try
            {
                _harmony = new Harmony(ModIdentity);
                _harmony.PatchAll();

                CustomLogger.Info("Successfully patch all by harmony.");
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Patch by harmony failed.");
                throw;
            }
        }

        private static void RegisterModEventHandlers()
        {
            try
            {
                Log.LogCallbacks += ModEventHook.LogCallback;
                ModEvents.GameAwake.RegisterHandler(ModEventHook.GameAwake);
                ModEvents.GameStartDone.RegisterHandler(()=> 
                {
                    WorldStaticDataHook.ReplaceXmlsToImplRemovePlayerItems();
                    MapTileCache = (MapTileCache)MapRenderer.GetTileCache();
                    ModEventHook.GameStartDone();
                    IsGameStartDone = true;
                });
                ModEvents.GameShutdown.RegisterHandler(() =>
                {
                    ModEventHook.GameShutdown();
                    RestartServer.GameShutdown();
                });
                ModEvents.PlayerSpawnedInWorld.RegisterHandler(ModEventHook.PlayerSpawnedInWorld);
                ModEvents.EntityKilled.RegisterHandler(ModEventHook.EntityKilled);
                ModEvents.PlayerDisconnected.RegisterHandler(ModEventHook.PlayerDisconnected);
                ModEvents.SavePlayerData.RegisterHandler(ModEventHook.SavePlayerData);
                ModEvents.ChatMessage.RegisterHandler(ModEventHook.ChatMessage);
                ModEvents.PlayerSpawning.RegisterHandler(ModEventHook.PlayerSpawning);

                GlobalTimer.RegisterSubTimer(new SubTimer(SkyChangeTrigger.Callback, 1) { IsEnabled = true });

                CustomLogger.Info("Successfully registered mod event handlers.");
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Register mod event handlers failed.");
                throw;
            }
        }
    }
}