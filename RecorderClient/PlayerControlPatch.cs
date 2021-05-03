using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnityEngine;
using Hazel;
using TheOtherRoles;

// using SystemTypes = BCPJLGGNHBC;
// using Palette = BLMBFIODBKL;
// using Constants = LNCOKMACBKP;
// using PhysicsHelpers = FJFJIDCFLDJ;
// using DeathReason = EGHDCAKGMKI;
// using GameOptionsData = CEIOGGEDKAN;
// using Effects = AEOEPNHOJDP;

namespace RecorderClient {
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControlFixedUpdatePatch{
        public static void Prefix(PlayerControl __instance) {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            //Recorder.LogInfo("PlayerControlFixedUpdatePatch");

            // プレイヤー一覧取得
            List<Player> players = new List<Player>();
            foreach(PlayerControl player in PlayerControl.AllPlayerControls)
            {
                Player p = new Player();
                p.x = player.transform.position.x;
                p.y = player.transform.position.y;
                p.z = player.transform.position.z;
                p.isDead = player.Data.IsDead;
                p.colorId = player.Data.ColorId;
                p.playerId = player.Data.PlayerId;
                p.name = player.name;
                List<RoleInfo> roles = RoleInfo.getRoleInfoForPlayer(player);
                foreach(RoleInfo rol in roles){
                    if(p.role.Length != 0){
                        p.role +=", ";
                    }
                    p.role += rol.name;
                }
                players.Add(p);
            }
            Recorder.GetInstance().players = players;

            // サボタージュ状況取得
            CustomField cf = new CustomField();
            foreach(PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if(player.Data.IsDead == false)
                {
                    foreach(PlayerTask task in player.myTasks)
                    {
                        if(task.TaskType == TaskTypes.FixComms)
                        {
                            cf.commsActive = true; 
                        }
                        else if(task.TaskType == TaskTypes.RestoreOxy)
                        {
                            cf.oxyActive = true;
                        }
                        else if(task.TaskType == TaskTypes.ResetSeismic)
                        {
                            cf.reactorActive = true;
                        }
                        else if(task.TaskType == TaskTypes.ResetReactor)
                        {
                            cf.reactorActive = true;
                        }
                        else if(task.TaskType == TaskTypes.StopCharles)
                        {
                            cf.reactorActive = true;
                        }
                        else if(task.TaskType == TaskTypes.FixLights)
                        {
                            cf.lightsActive = true;
                        }
                    }
                }
            }
            Recorder.GetInstance().cf = cf;
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
            if(Recorder.GetInstance().isGameEnd)
            {
                Task.Run(() => Recorder.EndDay());
            }
        }
    }
}