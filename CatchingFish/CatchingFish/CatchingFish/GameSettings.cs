using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CatchingFish
{
    public class GameSettings
    {
        public static bool GameOver, GameWin, musicOn, GamePause, soundOn;
        public static int totalFish;

        //捕获到的鱼数目
        public int TotalFish { get; set; }

        //是否播放背景音乐
        public bool MusicOn { get; set; }

        public GameSettings()
        {

        }
    }
}
