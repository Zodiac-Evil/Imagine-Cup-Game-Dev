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

        //���񵽵�����Ŀ
        public int TotalFish { get; set; }

        //�Ƿ񲥷ű�������
        public bool MusicOn { get; set; }

        public GameSettings()
        {

        }
    }
}
