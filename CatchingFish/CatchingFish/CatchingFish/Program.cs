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
            using (fishCatching game = new fishCatching())
            {
                game.Run();
            }
        }
    }
#endif
}

