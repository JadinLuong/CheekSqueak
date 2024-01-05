using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CheekSqueak.Patches
{

    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource cheekSqueakLog = CheekSqueakMod.Log;

        public static FartAction fartAction;

        public static float timer = 1.0f;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void initializePlayerWhoopie(PlayerControllerB __instance)
        {
            if (NetworkManager.Singleton.LocalClientId == __instance.playerClientId)
            {
                cheekSqueakLog.LogInfo("Initializing");

                fartAction = new FartAction();

                fartAction.Start(__instance);
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void PlayerControllerB_Update(PlayerControllerB __instance)
        {
            if (!fartAction.player.isPlayerDead)
            {
                bool keyPressed = Keyboard.current[CheekSqueakMod.fartKey.Value].wasPressedThisFrame;

                if (timer > 0.0f)
                {
                    timer -= Time.deltaTime;
                }

                if (keyPressed == true && timer < 0.0f)
                {
                    Debug.Log(fartAction.player.playerUsername + " farted");
                    fartAction.PlayFartSound();
                    timer = 1.0f;
                }
            }
        }
    }

    public class FartAction
    {

        public WhoopieCushionItem whoopieCushionObject;

        public PlayerControllerB player;

        public void Start(PlayerControllerB playerInstance)
        {
            player = playerInstance;
            whoopieCushionObject = Resources.FindObjectsOfTypeAll<WhoopieCushionItem>()[0];
            SoundManager.Instance.tempAudio1 = player.itemAudio;
            SoundManager.Instance.syncedAudioClips = whoopieCushionObject.fartAudios;
        }
        public void PlayFartSound()
        {
            SoundManager.Instance.tempAudio1.pitch = Random.Range(0.5f, 2.0f);
            SoundManager.Instance.PlayAudio1AtPositionServerRpc(player.transform.position, UnityEngine.Random.Range(0, SoundManager.Instance.syncedAudioClips.Length));
        }
    }
}
