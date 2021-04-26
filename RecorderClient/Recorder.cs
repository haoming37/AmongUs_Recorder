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
using TheOtherRoles;
using SystemTypes = BCPJLGGNHBC;
using SwitchSystem = ABIMJJMBJJM;
using ISystemType = JBBCJFNFOBB;
using GameOverReason = AMGMAKBHCMN;

namespace RecorderClient{
    public sealed class Recorder{
        public BepInEx.Logging.ManualLogSource logger = null;
        private static Recorder _instance = new Recorder();
        // public BepInEx.Logging.ManualLogSource logger = null;
        public static Recorder GetInstance() { return _instance;}
        public string url = "http://localhost:8000/recorder/games/";
        
        private int counter = 0;
        private bool isRunning = false;
        private Timer timer;
        private int gameId = 0;
        private int dayId = 0;
        private int frameId = 0;
        private List<Frame> frames = new List<Frame>();
        public List<int> exiledPlayers = new List<int>();
        public List<int> deadPlayers = new List<int>();

        public Game game = new Game();
        public static void LogInfo(string msg){
            if(_instance.logger != null){
                _instance.logger.LogInfo(msg);
            }
        }
        
        public static async Task NewGame(string map){
            LogInfo("NewGame");
            Game game = new Game();
            game.mapName = map;
            // プレイヤー一覧取得
            List<Player> players = new List<Player>();
            foreach(PlayerControl player in PlayerControl.AllPlayerControls)
            {
                Player p = new Player();
                p.x = player.transform.position.x;
                p.y = player.transform.position.y;
                p.z = player.transform.position.z;
                p.isDead = player.PPMOEEPBHJO.IAGJEKLJCCI;
                p.colorId = player.PPMOEEPBHJO.IMMNCAGJJJC;
                p.playerId = player.PPMOEEPBHJO.FNPNJHNKEBK;
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
            game.players = JsonConvert.SerializeObject(players);
            string json =JsonConvert.SerializeObject(game);
            using(var client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8);
                var response = await client.PostAsync(_instance.url, content);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    ResGame retGame = JsonConvert.DeserializeObject<ResGame>(data);
                    _instance.gameId = retGame.id;
                }
            }
            await NewDay();
            return;
        }
        public static async Task NewDay(){
            LogInfo("NewDay");
            Day day = new Day();
            string json =JsonConvert.SerializeObject(day);
            using(var client = new HttpClient())
            {
                string url = _instance.url + _instance.gameId.ToString() + "/days/";
                var content = new StringContent(json, Encoding.UTF8);
                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    ResDay retDay = JsonConvert.DeserializeObject<ResDay>(data);
                    _instance.dayId = retDay.dayId;
                }
            }
            TimerCallback tc = new TimerCallback(OnTimedEvent);
            _instance.timer = new Timer(tc, null, 0, 1000);
            _instance.isRunning = true;
            return;
        }
        private static void OnTimedEvent(Object o){
            Task t = NewFrame();
            t.Wait();
        }
        
        public static async Task NewFrame(bool force=false)
        {
            LogInfo("NewFrame");
            Frame frame = new Frame();

            // プレイヤー一覧取得
            List<Player> players = new List<Player>();
            foreach(PlayerControl player in PlayerControl.AllPlayerControls)
            {
                Player p = new Player();
                p.x = player.transform.position.x;
                p.y = player.transform.position.y;
                p.z = player.transform.position.z;
                p.isDead = player.PPMOEEPBHJO.IAGJEKLJCCI;
                p.colorId = player.PPMOEEPBHJO.IMMNCAGJJJC;
                p.playerId = player.PPMOEEPBHJO.FNPNJHNKEBK;
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
            string playerJson = JsonConvert.SerializeObject(players);
            frame.players = playerJson;
            frame.time = DateTime.Now;
            frame.eventId = 0;
            _instance.frames.Add(frame);

            // 10フレーム以上溜まっていたらPOSTする
            if(_instance.frames.Count >= 10 || force)
            {
                string json =JsonConvert.SerializeObject(_instance.frames);
                using(var client = new HttpClient())
                {
                    string url = _instance.url + _instance.gameId.ToString() + "/days/" + _instance.dayId.ToString() + "/frames/";
                    var content = new StringContent(json, Encoding.UTF8);
                    var response = await client.PostAsync(url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        ResFrame retFrame = JsonConvert.DeserializeObject<ResFrame>(data);
                        _instance.frameId = retFrame.frameId;
                    }
                }
                _instance.frames.Clear();
            }
            return;
        }

        public static async Task EndGame(GameOverReason gameOverReason)
        {
            LogInfo("EndGame");
            // Dayを終了
            await EndDay();

            // ゲーム終了理由を更新
            using(var client = new HttpClient())
            {
                string url = _instance.url + _instance.gameId .ToString() + "/";
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    ResGame retGame = JsonConvert.DeserializeObject<ResGame>(data);
                    retGame.gameOverReason = (int)gameOverReason;
                    string json = JsonConvert.SerializeObject(retGame);
                    var content = new StringContent(json, Encoding.UTF8);
                    response = await client.PutAsync(url, content);
                }
            }
            // TODO ゲーム終了理由が投票の場合は最終日を削除する

        }
        public static async Task EndDay()
        {
            LogInfo("EndDay");
            // フレームキャプチャの定期実行を停止
            if(_instance.isRunning){
                _instance.timer.Dispose();
                _instance.isRunning = false;
            }
            // 溜めていたフレームを強制アップロード
            await NewFrame(true);

            // 死んだプレイヤーと追放されたプレイヤーを更新
            using(var client = new HttpClient())
            {
                string url = _instance.url + _instance.gameId .ToString() + "/days/" + _instance.dayId.ToString() + "/";
                var response = await client.GetAsync(url);
                if(response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    ResDay  retDay = JsonConvert.DeserializeObject<ResDay>(data);
                    retDay.deadPlayers = JsonConvert.SerializeObject(_instance.deadPlayers);
                    retDay.exiledPlayers = JsonConvert.SerializeObject(_instance.exiledPlayers);
                    string json = JsonConvert.SerializeObject(retDay);
                    var content = new StringContent(json, Encoding.UTF8);
                    response = await client.PutAsync(url, content);
                }
            }
        }
    }
}