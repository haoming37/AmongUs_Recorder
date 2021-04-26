using HarmonyLib;
using System.Threading.Tasks;

namespace RecorderClient {
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    public static class ShipStatusStartPatch{
        public static void Prefix(){
            Recorder.LogInfo("ShipStatusStartPatch");
            Task t = Recorder.NewGame(ShipStatus.Instance.name);
            return;
        }

    }
}