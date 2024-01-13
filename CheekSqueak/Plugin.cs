using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CheekSqueak.Patches;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CheekSqueak
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class CheekSqueakMod : BaseUnityPlugin
    {
        private const string modGUID = "CheekSqueak";
        private const string modName = "Cheek Squeak";
        private const string modVersion = "1.0.3";

        public static ConfigEntry<Key> fartKey;

        private readonly Harmony harmony = new Harmony(modGUID);

        private static CheekSqueakMod Instance;

        public static ManualLogSource Log;

        public static Key defaultKey = Key.F;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            Log = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            fartKey = Config.Bind("Bind", "fart", defaultKey, "");

            Log.LogInfo("Binded " + fartKey.Value + " key to fart");

            harmony.PatchAll(typeof(CheekSqueakMod));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(GameNetworkManagerPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));

            Log.LogInfo("Cheek Squeak ready!");

            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }

}
