using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTyping
{
    public class GameLogic
    {
        // 計算英文輸入邏輯分數
        public void CaculateEnglishTypeScore(string inputText, string fullQuiz, out int score)
        {
            int lastCharIndex = inputText.Length - 1;
            char lastChar = inputText[lastCharIndex];

            if (fullQuiz[lastCharIndex] == lastChar)
                score = inputText.Length; // 對一個單字得1分
            else
                score = 0; // 錯一個單字就歸零
        }

        // 計算其他輸入邏輯分數
        public void CaculateOthersTypeScore(string inputText, string fullQuiz, out int score)
        {
            score = 0;
            for (int i = 0; i <= inputText.Length - 1; i++)
            {
                if (inputText[i] == fullQuiz[i]) // 對一個單字得1分
                    score += 1;
            }
        }

        // 計算搶單字邏輯分數
        public void CaculateGrabWordsScore(char inputText, int times, ref string quiz, ref int score)
        {
            var exist = quiz.Where(q => q != inputText); // 抓走剩下的

            var grab = quiz.Length - exist.Count();
            if (grab > 0)
                score += grab * times; // 抓到一個單字得1分 (重複次數代表分數加倍而已)
            else
                score -= 2; // 抓錯就扣2分

            quiz = string.Concat(exist); // 更新剩餘的單字
        }


    }
}