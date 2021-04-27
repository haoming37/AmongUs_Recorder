using HarmonyLib;
using System.Threading.Tasks;

namespace RecorderClient {
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Awake))]
    public class MeetingHudAwakePatch{
        public static void Postfix(){
            Recorder.LogInfo("MeetingHudAwakePatch");
            Task.Run(() => Recorder.EndDay());
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    public class MeetingHudClosePatch{
        public static void Postfix(){
            Recorder.LogInfo("MeetingHudClosePatch");
            Task.Run(() => Recorder.NewDay());
        }
    }
}