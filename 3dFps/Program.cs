using System;

namespace _3dFps
{
    class Program
    {

        private const int ScreenWidth = 100;
        private const int ScreenHeight = 50;

        private static readonly char[] Screen = new char[ScreenWidth * ScreenHeight];
        static void Main(string[] args)
        {
            Console.SetWindowSize(ScreenHeight,ScreenWidth);
            Console.SetBufferSize(ScreenHeight, ScreenWidth);
            Console.CursorVisible = false;

            char ch = ' ';
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ch = Console.ReadKey().KeyChar;
                }

                Array.Fill(Screen, ch);

                Console.SetCursorPosition(0, 0);
                Console.Write(Screen);
            }
        }
    }
}
