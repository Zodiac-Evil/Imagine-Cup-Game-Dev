using System;

namespace CatchingFish
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
            using (CatchingFish game = new CatchingFish())
            {
                game.Run();
            }
        }
    }
#endif
}

