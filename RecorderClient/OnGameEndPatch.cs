using HarmonyLib;
using GameOverReason = AMGMAKBHCMN;
using System.Threading.Tasks;


namespace RecorderClient{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.DDIEDPFFHOG))]
    public class OnGameEndPatch {
        public static void Prefix(AmongUsClient __instance, ref GameOverReason NEPMFBMGGLF, bool FBEKDLNKNLL) {
            return;
        }
        public static void Postfix(AmongUsClient __instance, ref GameOverReason NEPMFBMGGLF, bool FBEKDLNKNLL) {
            Recorder.LogInfo("OnGameEndPatch");
            GameOverReason gameOverReason = NEPMFBMGGLF;
            Task t = Recorder.EndGame(gameOverReason);
            return;
        }
    }
}