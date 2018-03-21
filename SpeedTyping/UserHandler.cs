using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTyping
{
    //宣告靜態類別，來儲存上線清單
    public static class UserHandler
    {
        public static List<User> Users = new List<User>(); //玩家
    }

    public static class GameHandler
    {
        public static bool IsGameStarting = false;

        public static string Quiz = "abcdefghijklmnopqrstuvwxyz"; // 預設題目
        public static GameType GameType = GameType.EnglishType; // 預設打字邏輯
        public static int Times = 1; // 預設重複次數

        public static string FullQuiz
        {
            get
            {
                return GameHandler.Quiz.Multiply(GameHandler.Times);
            }
        }
    }

    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public Game Game { get; set; }
        public User()
        {
            Game = new Game();
        }
    }
    public class Game
    {
        public string InputText { get; set; }
        public int LastCharIndex { get; set; }
        public int Score { get; set; }

        public Game()
        {
            InputText = "";
            LastCharIndex = 0;
            Score = 0;
        }
    }

    public enum GameType
    {
        EnglishType,
        ChineseType
    }
}