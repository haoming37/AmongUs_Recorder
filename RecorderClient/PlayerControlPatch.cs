using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using SystemTypes = BCPJLGGNHBC;
using Palette = BLMBFIODBKL;
using Constants = LNCOKMACBKP;
using PhysicsHelpers = FJFJIDCFLDJ;
using DeathReason = EGHDCAKGMKI;
using GameOptionsData = CEIOGGEDKAN;
using Effects = AEOEPNHOJDP;

namespace RecorderClient {
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControlFixedUpdatePatch{
        public static void Prefix(PlayerControl __instance) {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GCDONLGCMIL.Started) return;
        }
        public static void Postfix(PlayerControl __instance) {
            return;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class PlayerControlExiledPatch
    {
        public static void Prefix(PlayerControl __instance) {
            Recorder.LogInfo("PlayerControlExiledPatch");
            Recorder.GetInstance().exiledPlayers.Add(__instance.PlayerId);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class MurderPlayerPatch
    {
        public static void Prefix(PlayerControl __instance, PlayerControl DGDGDKCCKHJ)
        {
            Recorder.LogInfo("PlayerControlMurderPlayerPatch");
            Recorder.GetInstance().deadPlayers.Add(DGDGDKCCKHJ.PlayerId);
        }
    }
}