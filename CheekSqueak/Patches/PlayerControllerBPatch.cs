using BepInEx.Logging;
using CheekSqueak.Patches;
using CheekSqueak;
using GameNetcodeStuff;
using HarmonyLib;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CheekSqueak.Patches
{

    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        public static ManualLogSource cheekSqueakLog = CheekSqueakMod.Log;

        public static PlayerControllerB player;

        public static bool isPlayerReady = false;

        public static float timer = 1.0f;

        [HarmonyPatch("ConnectClientToPlayerObject")]
        [HarmonyPostfix]
        public static void initializeClienttoPlayerObject()
        {
            cheekSqueakLog.LogInfo("Connected Client to Player object");

            player = GameNetworkManager.Instance.localPlayerController;

            CheekSqueakNetworkHandler.Instance.audioSource = player.itemAudio;
            CheekSqueakNetworkHandler.Instance.audioClip = Resources.FindObjectsOfTypeAll<WhoopieCushionItem>()[0].fartAudios;

            isPlayerReady = true;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void PlayerControllerB_Update(PlayerControllerB __instance)
        {
            if (isPlayerReady && !player.isPlayerDead && player == __instance)
            {
                bool keyPressed = Keyboard.current[CheekSqueakMod.fartKey.Value].wasPressedThisFrame;

                if (timer > 0.0f)
                {
                    timer -= Time.deltaTime;
                }

                if (keyPressed == true && timer < 0.0f)
                {
                    float pitch = Random.Range(0.5f, 2.0f);
                    int clipIndex = Random.Range(0, CheekSqueakNetworkHandler.Instance.audioClip.Length);
                    CheekSqueakNetworkHandler.Instance.PlayerFartServerRpc(player.playerUsername, player.transform.position, clipIndex, pitch);
                    timer = 1.0f;

                    bool noiseIsInsideClosedShip = player.isInHangarShipRoom && player.playersManager.hangarDoorsClosed;
                    RoundManager.Instance.PlayAudibleNoise(player.transform.position, 22f, 0.6f, 0, noiseIsInsideClosedShip, 6);
                }
            }
        }
    }

    public class CheekSqueakNetworkHandler : NetworkBehaviour
    {
        public static CheekSqueakNetworkHandler Instance;

        public AudioSource audioSource;

        public AudioClip[] audioClip;

        public bool isPitchNormal = true;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (audioSource != null && !audioSource.isPlaying && !isPitchNormal)
            {
                audioSource.pitch = 1.0f;
                isPitchNormal = true;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayerFartServerRpc(string username, Vector3 position, int clipIndex, float pitch)
        {
            PlayerFartClientRpc(username, position, clipIndex, pitch);
        }

        [ClientRpc]
        public void PlayerFartClientRpc(string username, Vector3 position, int clipIndex, float pitch)
        {
            CheekSqueakMod.Log.LogInfo(username + " farted!");
            audioSource.pitch = pitch;
            audioSource.transform.position = position;
            audioSource.PlayOneShot(audioClip[clipIndex]);
            isPitchNormal = false;
        }
    }

    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        public static AssetBundle CheekSqueakAssetBundle;

        public static GameObject CheekSqueakNetworkPrefab;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Init()
        {
            CheekSqueakMod.Log.LogInfo("GameNetworkManager Start");

            CheekSqueakAssetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("CheekSqueak.cheeksqueak"));
            CheekSqueakNetworkPrefab = CheekSqueakAssetBundle.LoadAsset<GameObject>("assets/networking/cheeksqueaknetwork.prefab");
            CheekSqueakNetworkPrefab.AddComponent<CheekSqueakNetworkHandler>();

            NetworkManager.Singleton.AddNetworkPrefab(CheekSqueakNetworkPrefab);
        }
    }

    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void SpawnNetworkHandler()
        {
            CheekSqueakMod.Log.LogInfo("StartOfRound Start");
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                var networkHandlerHost = Object.Instantiate(GameNetworkManagerPatch.CheekSqueakNetworkPrefab, Vector3.zero, Quaternion.identity);
                networkHandlerHost.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
