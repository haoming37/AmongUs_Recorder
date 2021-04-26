using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
// using BepInEx;
// using BepInEx.Configuration;
// using TheOtherRoles;
// using SystemTypes = BCPJLGGNHBC;
// using SwitchSystem = ABIMJJMBJJM;
// using ISystemType = JBBCJFNFOBB;
// using GameOverReason = AMGMAKBHCMN;

namespace RecorderClient{
    public enum Sabotage{
        None,
        Communications,
        Reactor,
        O2,
        Electrical
    }
    public class Player{
        public string name{ get; set;}
        public string role{get; set;}
        public bool isDead{get; set;}
        public int colorId{get; set;}
        public int playerId{get; set;}
        public float x{get; set;}
        public float y{get; set;}
        public float z{get; set;}

        public Player(){
            name = "";
            role = "";
            isDead = false;
            colorId = 0;
            playerId = 0;
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;
        }
    }
    public class Frame{
        public int eventId{get; set;}
        public string players{get; set;}
        public DateTime time{get; set;}
        public Frame(){
            players = "[]";
            time = DateTime.Now;
            eventId = 0;
        }
    }
    public class Day{
        public string deadPlayers{get; set;}
        public string exiledPlayers{get; set;}
        public Day(){
            deadPlayers = "[]";
            exiledPlayers = "[]";
        }
    }
    public class Game{
        public string mapName{get; set;}
        public int gameOverReason{get; set;}
        public string players{get; set;}
        public DateTime time{get; set;}
        public Game(){
            mapName = "";
            gameOverReason = 0;
            players = "[]";
            time = DateTime.Now;
        }
    }
    // サーバーからゲームIDを付与されたデータを受け取るための拡張クラス
    public class ResGame:Game{
        public int numDays{get; set;}
        public int id{get; set;}

    }
    public class ResDay: Day{
        public string numFrames{get; set;}
        public int dayId{get; set;}
    }
    
    public class ResFrame: Frame{
        public int frameId{get; set;}
    }
}