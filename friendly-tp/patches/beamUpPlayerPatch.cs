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
	[HarmonyPatch("beamUpPlayer")]
	class beamUpPlayerPatch
	{
		[HarmonyPrefix]
		public static bool Prefix()
		{
			return false;
		}
		[HarmonyPostfix]
		public static IEnumerator teleportPatch(IEnumerator t, AudioSource ___shipTeleporterAudio, AudioClip ___teleporterSpinSFX, AudioClip ___beamUpPlayerBodySFX,
			Transform ___teleporterPosition, AudioClip ___teleporterBeamUpSFX, ShipTeleporter __instance)
		{
			___shipTeleporterAudio.PlayOneShot(___teleporterSpinSFX);
			PlayerControllerB playerToBeamUp = StartOfRound.Instance.mapScreen.targetedPlayer;
			if (playerToBeamUp == null)
			{
				yield break;
			}
			var SetPlayerTeleporterId = AccessTools.Method(typeof(ShipTeleporter), "SetPlayerTeleporterId");
			SetPlayerTeleporterId.Invoke(__instance, new object[] { playerToBeamUp, 1 });
			if (playerToBeamUp.deadBody != null)
			{
				if (playerToBeamUp.deadBody.beamUpParticle == null)
				{
					yield break;
				}
				playerToBeamUp.deadBody.beamUpParticle.Play();
				playerToBeamUp.deadBody.bodyAudio.PlayOneShot(___beamUpPlayerBodySFX);
			}
			else
			{
				playerToBeamUp.beamUpParticle.Play();
				playerToBeamUp.movementAudio.PlayOneShot(___beamUpPlayerBodySFX);
			}

			yield return new WaitForSeconds(3f);
			bool flag = false;
			if (playerToBeamUp.deadBody != null)
			{
				if (playerToBeamUp.deadBody.grabBodyObject == null || !playerToBeamUp.deadBody.grabBodyObject.isHeldByEnemy)
				{
					flag = true;
					playerToBeamUp.deadBody.attachedTo = null;
					playerToBeamUp.deadBody.attachedLimb = null;
					playerToBeamUp.deadBody.secondaryAttachedLimb = null;
					playerToBeamUp.deadBody.secondaryAttachedTo = null;
					playerToBeamUp.deadBody.SetRagdollPositionSafely(___teleporterPosition.position, disableSpecialEffects: true);
					playerToBeamUp.deadBody.transform.SetParent(StartOfRound.Instance.elevatorTransform, worldPositionStays: true);
				}
			}
			else
			{
				flag = true;
				if ((bool)UnityEngine.Object.FindObjectOfType<AudioReverbPresets>())
				{
					UnityEngine.Object.FindObjectOfType<AudioReverbPresets>().audioPresets[3].ChangeAudioReverbForPlayer(playerToBeamUp);
				}
				playerToBeamUp.isInElevator = true;
				playerToBeamUp.isInHangarShipRoom = true;
				playerToBeamUp.isInsideFactory = false;
				playerToBeamUp.averageVelocity = 0f;
				playerToBeamUp.velocityLastFrame = Vector3.zero;
				playerToBeamUp.TeleportPlayer(___teleporterPosition.position, withRotation: true, 160f);
			}
			SetPlayerTeleporterId.Invoke(__instance, new object[] { playerToBeamUp, -1 });
			if (flag)
			{
				___shipTeleporterAudio.PlayOneShot(___teleporterBeamUpSFX);
				if (GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
				{
					HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
				}
			}
		}
	}
}
