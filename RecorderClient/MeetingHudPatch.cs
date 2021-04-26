using HarmonyLib;
using System.Threading.Tasks;

namespace RecorderClient {
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Awake))]
    public class MeetingHudAwakePatch{
        public static void Postfix(){
            Recorder.LogInfo("MeetingHudAwakePatch");
            Task t = Recorder.EndDay();
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    public class MeetingHudClosePatch{
        public static void Postfix(){
            Recorder.LogInfo("MeetingHudClosePatch");
            Task t =  Recorder.NewDay();
        }
    }

    //FOLNJDKNFOF⇨CoSendSceneChange
    [HarmonyPatch(typeof(InnerNet.InnerNetClient), nameof(InnerNet.InnerNetClient.FOLNJDKNFOF))]
    public class CoSendSceneChangePatch{
        public static void Postfix(string JLNFHGBCMCG){
            Recorder.LogInfo("CoSendSceneChange: " + JLNFHGBCMCG);

        }
    }
}