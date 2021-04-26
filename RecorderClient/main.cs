using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hazel;
using Logger = BepInEx.Logging.Logger;


namespace RecorderClient
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency("me.eisbison.theotherroles")]
    public class ReplayPlugin : BasePlugin
    {
        public const string Id = "jp.haoming.recorder";

        public Harmony Harmony { get; } = new Harmony(Id);

        // カスタムサーバー接続先設定
        public static ConfigEntry<string> Ip { get; set; }
        public static ConfigEntry<ushort> Port { get; set; }

        public override void Load()
        {
            // カスタムサーバーへの接続先を追加
            Ip = Config.Bind("Custom", "Custom Server IP", "18.177.110.86");
            Port = Config.Bind("Custom", "Custom Server Port", (ushort)22023);

            IRegionInfo customRegion = new DnsRegionInfo(Ip.Value, "HaomingAWS", StringNames.NoTranslation, Ip.Value, Port.Value).Cast<IRegionInfo>();
            ServerManager serverManager = DestroyableSingleton<ServerManager>.CHNDKKBEIDG;
            IRegionInfo[] regions = ServerManager.DefaultRegions;

            regions = regions.Concat(new IRegionInfo[] { customRegion }).ToArray();
            ServerManager.DefaultRegions = regions;
            serverManager.AGFAPIKFOFF = regions;
            serverManager.SaveServers();

            // リプレイ保存機能スタート
            Log.LogInfo("Start Haoming.Replay");
            Recorder.GetInstance().logger = Log;
            // Recorder.Start();
            
            Harmony.PatchAll();
        }
    }

    // 切断時にBANされるのを無効化する（デバッグ時によく実施するので）
    [HarmonyPatch(typeof(IAJICOPDKHA), nameof(IAJICOPDKHA.LEHPLHFNDLM), MethodType.Getter)]
    public static class AmBannedPatch
    {
        public static void Postfix(out bool __result)
        {
            __result = false;
        }
    }
}