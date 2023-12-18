using System;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace friendly_tp.patches
{
    [HarmonyPatch(typeof(ShipTeleporter))]
    [HarmonyPatch("TeleportPlayerOutWithInverseTeleporter")]
    class TPPlayerOutPatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            return false;
        }
        [HarmonyPostfix]
		public static void TeleportPlayerOutWithInverseTeleporter(ShipTeleporter __instance, AudioSource ___shipTeleporterAudio, AudioClip ___teleporterBeamUpSFX, int playerObj, Vector3 teleportPos)
		{
			if (StartOfRound.Instance.allPlayerScripts[playerObj].isPlayerDead)
			{
				var StartCoroutine = AccessTools.Method(typeof(Coroutine), "StartCoroutine");
				var teleportBodyOut = AccessTools.Method(typeof(ShipTeleporter), "teleportBodyOut");
				StartCoroutine.Invoke(__instance, new object[] {teleportBodyOut.Invoke(__instance, new object[] { playerObj, playerObj })});
				//StartCoroutine(teleportBodyOut(___playerObj, ___playerObj));
				return;
			}
			PlayerControllerB playerControllerB = StartOfRound.Instance.allPlayerScripts[playerObj];
			var SetPlayerTeleporterId = AccessTools.Method(typeof(ShipTeleporter), "SetPlayerTeleporterId");
			SetPlayerTeleporterId.Invoke(__instance, new object[] { playerControllerB, -1 });
			if ((bool)UnityEngine.Object.FindObjectOfType<AudioReverbPresets>())
			{
				UnityEngine.Object.FindObjectOfType<AudioReverbPresets>().audioPresets[2].ChangeAudioReverbForPlayer(playerControllerB);
			}
			playerControllerB.isInElevator = false;
			playerControllerB.isInHangarShipRoom = false;
			playerControllerB.isInsideFactory = true;
			playerControllerB.averageVelocity = 0f;
			playerControllerB.velocityLastFrame = Vector3.zero;
			StartOfRound.Instance.allPlayerScripts[playerObj].TeleportPlayer(teleportPos);
			StartOfRound.Instance.allPlayerScripts[playerObj].beamOutParticle.Play();
			___shipTeleporterAudio.PlayOneShot(___teleporterBeamUpSFX);
			StartOfRound.Instance.allPlayerScripts[playerObj].movementAudio.PlayOneShot(___teleporterBeamUpSFX);
			if (playerControllerB == GameNetworkManager.Instance.localPlayerController)
			{
				Debug.Log("Teleporter shaking camera");
				HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
			}
		}
	}
}
