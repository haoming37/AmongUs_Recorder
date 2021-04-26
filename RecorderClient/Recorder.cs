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

namespace RecorderClient{
    public sealed class Recorder{
        public bool isServerActive = true;
        public BepInEx.Logging.ManualLogSource logger = null;
        private static Recorder _instance = new Recorder();
        public static Recorder GetInstance() { return _instance;}
        public string url = "http://localhost:8000/recorder/games/";
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
            if (!_instance.isServerActive) return;
            LogInfo("NewGame");
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
            _instance.timer = new Timer(tc, null, 0, 250);
            _instance.isRunning = true;
            return;
        }
        private static void OnTimedEvent(Object o){
            Task t = NewFrame();
            t.Wait();
        }
        
        public static async Task NewFrame(bool force=false)
        {
            if (!_instance.isServerActive) return;
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
            string playerJson = JsonConvert.SerializeObject(players);
            frame.players = playerJson;
            frame.time = DateTime.Now;
            frame.eventId = 0;
            _instance.frames.Add(frame);

            // 10フレーム以上溜まっていたらPOSTする
            if(_instance.frames.Count >= 30 || force)
            {
                List<Frame> frames = new List<Frame>(_instance.frames);
                _instance.frames.Clear();
                string json =JsonConvert.SerializeObject(frames);
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
                    else 
                    {
                        _instance.isServerActive = false;
                    }
                }
                _instance.frames.Clear();
            }
            return;
        }

        public static async Task EndGame(GameOverReason gameOverReason)
        {
            if (!_instance.isServerActive) return;
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
            if (!_instance.isServerActive) return;
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
                else 
                {
                    _instance.isServerActive = false;
                }
            }
        }
    }
}