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

namespace RecorderClient{
    public sealed class Recorder{
        public bool isServerActive = true;
        public BepInEx.Logging.ManualLogSource logger = null;
        private static Recorder _instance = new Recorder();
        public static Recorder GetInstance() { return _instance;}
        public string url = "http://localhost:8000/recorder/games/";
        private bool isRunning = false;
        public bool isGameEnd = false;
        private Timer timer;
        private int gameId = 0;
        private int dayId = 0;
        private int frameId = 0;
        private List<Frame> frames = new List<Frame>();
        public List<int> exiledPlayers = new List<int>();
        public List<int> deadPlayers = new List<int>();
        public CustomField cf = new CustomField();
        public List<Player> players = new List<Player>();
        public Game game = new Game();

        public static void LogInfo(string msg){
            if(_instance.logger != null){
                _instance.logger.LogInfo(msg);
            }
        }
        
        public static async Task NewGame(string map){
            if(!_instance.isServerActive) return;
            LogInfo("NewGame");
            _instance.isGameEnd = false;
            _instance.deadPlayers = new List<int>();
            _instance.exiledPlayers = new List<int>();
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
                else 
                {
                    _instance.isServerActive = false;
                }
            }
            await NewDay();
            return;
        }
        public static async Task NewDay(){
            if (!_instance.isServerActive) return;
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
                else 
                {
                    _instance.isServerActive = false;
                }
            }
            TimerCallback tc = new TimerCallback(OnTimedEvent);
            _instance.timer = new Timer(tc, null, 0, 100);
            _instance.isRunning = true;
            return;
        }
        private static void OnTimedEvent(System.Object o){
            NewFrame();
        }
        
        public static void NewFrame(bool force=false)
        {
            if (!_instance.isServerActive) return;
            //LogInfo("NewFrame");
            Frame frame = new Frame();

            // プレイヤー状態取得
            string playerJson = JsonConvert.SerializeObject(_instance.players);

            // サボタージュ状態取得
            string cfJson = JsonConvert.SerializeObject(_instance.cf);

            // フレームクラスに代入
            frame.customField = cfJson;
            frame.players = playerJson;
            frame.time = DateTime.Now;
            frame.eventId = 0;
            _instance.frames.Add(frame);

            // 50フレーム以上溜まっていたらPOSTする
            if(_instance.frames.Count >= 50 || force)
            {
                Task t = Task.Run(() => UploadFrame());

                // EndDayから予備出された強制出力の時は同期処理とする
                if(force)
                {
                    if(t.Wait(10000)){
                        LogInfo("Frame強制アップロード成功");
                    }else{
                        LogInfo("Frame強制アップロード失敗");
                    }
                }
            }
            return;
        }

        private static void UploadFrameThread(){
            Task t = Task.Run(() => UploadFrame());
            t.Wait();
        }

        private static async Task UploadFrame()
        {
            List<Frame> frames = new List<Frame>(_instance.frames);
            _instance.frames.Clear();
            string json =JsonConvert.SerializeObject(frames);
            using(var client = new HttpClient())
            {
                string url = _instance.url + _instance.gameId.ToString() + "/days/" + _instance.dayId.ToString() + "/frames/";
                var content = new StringContent(json, Encoding.UTF8);
                var response = await client.PostAsync(url, content);
                if(response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    ResFrame retFrame = JsonConvert.DeserializeObject<ResFrame>(data);
                    _instance.frameId = retFrame.frameId;
                }
                else 
                {
                    _instance.isServerActive = false;
                }
            }
        }

        public static async Task EndGame(GameOverReason gameOverReason)
        {
            if(!_instance.isServerActive) return;
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
                else 
                {
                    _instance.isServerActive = false;
                }
            }
        }

        public static async Task EndDay()
        {
            if(!_instance.isServerActive) return;
            LogInfo("EndDay");
            // フレームキャプチャの定期実行を停止
            if(_instance.isRunning){
                _instance.timer.Dispose();
                _instance.isRunning = false;
            }
            // 溜めていたフレームを強制アップロード
            NewFrame(true);

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
                    LogInfo("Update deadPlayers: " + retDay.deadPlayers );
                    LogInfo("Update exiledPlayers: " + retDay.exiledPlayers );
                    string json = JsonConvert.SerializeObject(retDay);
                    var content = new StringContent(json, Encoding.UTF8);
                    response = await client.PutAsync(url, content);
                    if(! response.IsSuccessStatusCode){
                        LogInfo("Error:Update Day");
                    }
                }
                else 
                {
                    _instance.isServerActive = false;
                }
            }
        }
    }
}