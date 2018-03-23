using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
using System.Text.RegularExpressions;

namespace SpeedTyping
{
    [HubName("gameHub")]
    public class GameHub : Hub
    {



        public void OnConnected(string name)
        {
            var id = Context.ConnectionId;

            if (UserHandler.Users.Count(x => x.Id == id) == 0)
            {
                UserHandler.Users.Add(new User()
                {
                    Id = id,
                    Name = name,
                });

                Clients.Others.join(name, GameHandler.IsGameStarting);
            }

            //初始化、更新所有的上線清單
            Clients.All.getList(
                UserHandler.Users
                .OrderByDescending(p => p.Game.Score)
                .ToList()
                );
        }


        /// <summary>
        /// 初始化、更新題目
        /// </summary>
        /// <param name="quiz">題目</param>
        /// <param name="times">重複次數</param>
        public void RefreshQuiz(string quiz, int times)
        {
            if (!string.IsNullOrEmpty(quiz))
            {
                GameHandler.Quiz = quiz;

                if (GameHandler.GameType == GameType.GrabWords)
                    GameHandler.GameType = GameType.GrabWords;
                else if (HasChinese(quiz))
                    GameHandler.GameType = GameType.ChineseType;
                else
                    GameHandler.GameType = GameType.EnglishType;
            }

            GameHandler.Times = times;
            Clients.All.refreshQuiz(GameHandler.Quiz, times);
        }

        bool HasChinese(string input)
        {
            //判斷是不是有中文字
            for (int i = 0; i < input.Length; i++)
            {
                Regex rx = new Regex("^[\u4e00-\u9fa5]$");
                if (rx.IsMatch(input[i].ToString()))
                    return true;
            }

            return false;
        }


        public void GameCheck(string inputText)
        {
            var id = Context.ConnectionId;
            var find = UserHandler.Users.FirstOrDefault(x => x.Id == Context.ConnectionId);

            int score = find.Game.Score;

            if (GameHandler.IsGameStarting) // 遊戲開始才進行判斷
            {
                score = 0; // 分數初始計算

                var gameLogic = new GameLogic();
                var fullQuiz = GameHandler.FullQuiz;

                switch (GameHandler.GameType)
                {
                    case GameType.EnglishType:
                        gameLogic.CaculateEnglishTypeScore(inputText, fullQuiz, out score);
                        if (score == 0)
                            inputText = ""; // 輸錯就從打

                        CheckWinner(find.Name, () => inputText.Length == fullQuiz.Length);
                        break;
                    case GameType.ChineseType:
                        gameLogic.CaculateOthersTypeScore(inputText, fullQuiz, out score);

                        CheckWinner(find.Name, () => inputText == fullQuiz);
                        break;
                    case GameType.GrabWords:
                        string quiz = GameHandler.TempQuiz; // 目前的題目
                        score = find.Game.Score; // 之前的成績
                        gameLogic.CaculateGrabWordsScore(Convert.ToChar(inputText), GameHandler.Times, ref quiz, ref score);
                        // 更新資料
                        GameHandler.TempQuiz = quiz;
                        find.Game.Score = score;

                        //告知其他人單字被抓走
                        Clients.All.removeWord(inputText , find.Id);

                        // 確認遊戲是否結束 (單字被抓完)
                        if (GameHandler.TempQuiz.Length == 0)
                        {
                            // 分數最高者為勝利者
                            var winner = UserHandler.Users.OrderByDescending(u => u.Game.Score).First().Name;
                            CheckWinner(winner, () => true);
                        }
                        inputText = ""; // 輸過的單字就刪掉
                        break;
                }
            }

            find.Game.InputText = inputText;
            find.Game.Score = score; // 分數更新

            Clients.All.getList(
                UserHandler.Users
                .OrderByDescending(p => p.Game.Score)
                .ToList()
                );
        }



        public void CheckWinner(string winnername, Func<bool> checkWin)
        {
            // check if win the game
            if (checkWin())
            {
                GameHandler.IsGameStarting = false;
                Clients.All.gameInfo("遊戲結束! 獲勝者是 " + winnername);
            }
        }


        //更換遊戲模式
        public void ChangeGameMode(GameType type)
        {
            if (type == GameType.Null) // 登入取得遊戲模式而已
            {
                Clients.Caller.changeGameMode("已取得遊戲模式", GameHandler.GameType == GameType.GrabWords ? true : false);
                return;
            }

            GameHandler.GameType = type;
            Clients.All.changeGameMode("已更新遊戲模式", type == GameType.GrabWords ? true:false);

            if (type == GameType.ChineseType || type == GameType.EnglishType)
            {
                RefreshQuiz(GameHandler.Quiz, GameHandler.Times);
            }
        }

        //遊戲開始
        public void StartGame()
        {
            GameHandler.IsGameStarting = true;
            UserHandler.Users.ForEach(u => u.Game.Score = 0); // 分數重算

            if (GameHandler.GameType == GameType.GrabWords)
                GameHandler.TempQuiz = GameHandler.Quiz;

            Clients.All.gameStart();
        }

        //遊戲結束
        public void StopGame()
        {
            GameHandler.IsGameStarting = false;

            var id = Context.ConnectionId;
            var find = UserHandler.Users.FirstOrDefault(x => x.Id == Context.ConnectionId);

            Clients.All.gameInfo("遊戲被" + find?.Name + "終止了!");
        }

        //當使用者斷線時呼叫
        public override Task OnDisconnected(bool stopCalled)
        {
            if (stopCalled)
            {
                var find = UserHandler.Users.FirstOrDefault(x => x.Id == Context.ConnectionId);

                UserHandler.Users.Remove(find);

                Clients.Others.leaves(find.Name);


                if (UserHandler.Users.Count >= 0)
                {
                    //更新所有的上線清單
                    Clients.All.getList(
                        UserHandler.Users
                        .OrderByDescending(p => p.Game.Score)
                        .ToList());
                }
                else
                {
                    // 沒人在房間內就中斷遊戲
                    StopGame();
                }
            }

            return base.OnDisconnected(stopCalled);
        }

    }
}