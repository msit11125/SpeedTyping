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

                if (HasChinese(quiz))
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

            int score = 0;

            if (GameHandler.IsGameStarting) // 遊戲開始才進行判斷
            {
                var fullQuiz = GameHandler.FullQuiz;

                switch (GameHandler.GameType)
                {
                    case GameType.EnglishType:
                        CaculateEnglishTypeScore(inputText, fullQuiz, out score);
                        if (score == 0)
                            inputText = ""; // 輸錯就從打

                        CheckWinner(find.Name, () => inputText.Length == fullQuiz.Length);
                        break;
                    case GameType.ChineseType:
                        CaculateOthersTypeScore(inputText, fullQuiz, out score);

                        CheckWinner(find.Name, () => inputText == fullQuiz);
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


        // 計算英文輸入邏輯分數
        void CaculateEnglishTypeScore(string inputText, string fullQuiz, out int score)
        {
            int lastCharIndex = inputText.Length - 1;
            char lastChar = inputText[lastCharIndex];

            if (fullQuiz[lastCharIndex] == lastChar)
                score = inputText.Length; // 對一個單字得1分
            else
                score = 0; // 錯一個單字就歸零
        }

        // 計算其他輸入邏輯分數
        void CaculateOthersTypeScore(string inputText, string fullQuiz, out int score)
        {
            score = 0;
            for(int i = 0; i <= inputText.Length - 1; i++)
            {
                if (inputText[i] == fullQuiz[i]) // 對一個單字得1分
                    score+=1;
            }
        }


        void CheckWinner(string winnername, Func<bool> checkWin)
        {
            // check if win the game
            if (checkWin())
            {
                GameHandler.IsGameStarting = false;
                Clients.All.gameInfo("遊戲結束! 獲勝者是 " + winnername);
            }
        }


        //遊戲開始
        public void StartGame()
        {
            GameHandler.IsGameStarting = true;
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
                }else
                {
                    // 沒人在房間內就中斷遊戲
                    StopGame();
                }
            }

            return base.OnDisconnected(stopCalled);
        }

    }
}