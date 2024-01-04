using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CheekSqueak.Patches;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace CheekSqueak
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class CheekSqueakMod : BaseUnityPlugin
    {
        private const string modGUID = "CheekSqueak.LCMod";
        private const string modName = "Cheek Squeak";
        private const string modVersion = "1.0.0.0";

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

            Log.LogInfo("Binded F key to fart");

            harmony.PatchAll(typeof(CheekSqueakMod));
            harmony.PatchAll(typeof(PlayerControllerBPatch));

            Log.LogInfo("Cheek Squeak ready!");
        }

    }

}
