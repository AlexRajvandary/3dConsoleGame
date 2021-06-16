using PlayerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dFps
{

    class Program
    {
        private static int ScreenWidth = Console.LargestWindowWidth;
        private static int ScreenHeight = Console.LargestWindowHeight;

        private const int MapHeight = 32;
        private const int MapWidth = 32;

        private const double Depth = 20;
        private const double Fov = Math.PI / 3.5;

        private static (double, double) StartPosition = (3.0, 3.0);
        private static Player player;

        private static readonly StringBuilder Map = new StringBuilder();

        static async Task Main()
        {
            do
            {
                //ConsoleHelper.SetCurrentFont("Consolas", 10);

                ChangeConsoleColor(ConsoleColor.Black, ConsoleColor.White);

                await StartGame();

                RestartAndExitInfo();

            } while (Console.ReadKey().Key != ConsoleKey.Escape);
        }

        private static void RestartAndExitInfo()
        {
            Console.Clear();

            //ConsoleHelper.SetCurrentFont("Consolas", 40);

            ChangeConsoleColor(ConsoleColor.Red, ConsoleColor.White);

            Console.SetCursorPosition(25, 5);

            Console.Write(" Exit:              Restart:      ");

            Console.SetCursorPosition(25, 7);

            ChangeConsoleColor(ConsoleColor.White, ConsoleColor.Black);

            Console.Write(" Escape ");

            Console.SetCursorPosition(49, 7);
            Console.Write(" Any other key ");
        }

        private static void ChangeConsoleColor(ConsoleColor Background, ConsoleColor ForeGroung)
        {
            Console.BackgroundColor = Background;

            Console.ForegroundColor = ForeGroung;
        }

        private static async Task StartGame()
        {
            player = new Player(new Position(StartPosition));

            Console.SetWindowSize(ScreenWidth, ScreenHeight);
            Console.SetBufferSize(ScreenWidth, ScreenHeight);
            Console.CursorVisible = false;

            InitMap();

            var screen = new char[ScreenWidth * ScreenHeight];

            DateTime dateTimeFrom = DateTime.Now;

            while (true)
            {
                //elapsed time
                var dateTimeTo = DateTime.Now;
                double elapsedTime = (dateTimeTo - dateTimeFrom).TotalSeconds;
                dateTimeFrom = dateTimeTo;
                ConsoleKey InputedKey = new ConsoleKey();
                if (Console.KeyAvailable)
                {

                    InitMap();

                    InputedKey = Console.ReadKey(true).Key;

                    ControlPlayerMovement(elapsedTime, InputedKey);

                  

                    if (IsUserWantsToExit(InputedKey))
                    {
                        break;
                    }
                }

                //Ray Casting
                var rayCastingTasks = new List<Task<Dictionary<int, char>>>();
                for (int x = 0; x < ScreenWidth; x++)
                {
                    var x1 = x;
                    rayCastingTasks.Add(Task.Run(() => CastRay(x1)));
                }
                foreach (Dictionary<int, char> dictionary in await Task.WhenAll(rayCastingTasks))
                {
                    foreach (var key in dictionary.Keys)
                    {
                        screen[key] = dictionary[key];
                    }
                }

                //Stats
                GameStatistics(screen, elapsedTime, InputedKey);

                //Map
                ScreenUpdate(screen);

                Console.SetCursorPosition(0, 0);
                Console.Write(screen, 0, ScreenWidth * ScreenHeight);
            }
        }

        private static void ScreenUpdate(char[] screen)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    screen[(y + 1) * ScreenWidth + x] = Map[y * MapWidth + x];
                }
            }

            screen[(int)(player.Position.Y + 1) * ScreenWidth + (int)player.Position.X] = 'P';
        }

        private static void GameStatistics(char[] screen, double elapsedTime, ConsoleKey Pressedkey)
        {
            char[] stats = $"{player.Position}, FPS: {(int)(1 / elapsedTime)}, Pressed Key: {Pressedkey}"
                .ToCharArray();
            stats.CopyTo(screen, 0);
        }

        private static bool IsUserWantsToExit(ConsoleKey consoleKey)
        {
            return consoleKey == ConsoleKey.Escape;
        }

        private static void ControlPlayerMovement(double elapsedTime, ConsoleKey consoleKey)
        {
            switch (consoleKey)
            {
                case ConsoleKey.A:
                    player.Position.Angle += elapsedTime * 2;
                    break;
                case ConsoleKey.D:
                    player.Position.Angle -= elapsedTime * 2;
                    break;
                case ConsoleKey.W:
                    {
                        player.Position.X += Math.Sin(player.Position.Angle) * 3 * elapsedTime;
                        player.Position.Y += Math.Cos(player.Position.Angle) * 3 * elapsedTime;

                        if (Map[(int)player.Position.Y * MapWidth + (int)player.Position.X] == '#')
                        {
                            player.Position.X -= Math.Sin(player.Position.Angle) * 3 * elapsedTime;
                            player.Position.Y -= Math.Cos(player.Position.Angle) * 3 * elapsedTime;
                        }

                        break;
                    }

                case ConsoleKey.S:
                    {
                        player.Position.X -= Math.Sin(player.Position.Angle) * 5 * elapsedTime;
                        player.Position.Y -= Math.Cos(player.Position.Angle) * 5 * elapsedTime;

                        if (Map[(int)player.Position.Y * MapWidth + (int)player.Position.X] == '#')
                        {
                            player.Position.X += Math.Sin(player.Position.Angle) * 5 * elapsedTime;
                            player.Position.Y += Math.Cos(player.Position.Angle) * 5 * elapsedTime;
                        }

                        break;
                    }
            }
        }

        public static Dictionary<int, char> CastRay(int x)
        {
            var result = new Dictionary<int, char>();

            double rayAngle = (player.Position.Angle + Fov / 2) - x * Fov / ScreenWidth;

            double distanceToWall = 0;
            bool hitWall = false;
            bool isBound = false;
            double wallSize = 1;

            double rayY = Math.Cos(rayAngle);
            double rayX = Math.Sin(rayAngle);

            while (!hitWall && distanceToWall < Depth)
            {
                distanceToWall += 0.1;

                int testX = (int)(player.Position.X + rayX * distanceToWall);
                int testY = (int)(player.Position.Y + rayY * distanceToWall);

                if (testX < 0 || testX >= Depth + player.Position.X || testY < 0 || testY >= Depth + player.Position.Y)
                {
                    hitWall = true;
                    distanceToWall = Depth;
                }
                else
                {
                    char testCell = Map[testY * MapWidth + testX];

                    if (testCell == '#' || testCell == 'B')
                    {
                        hitWall = true;

                        wallSize = testCell == '#' ? 1 : testCell == 'B' ? 1.2 : wallSize;

                        var boundsVectorsList = new List<(double X, double Y)>();

                        for (int tx = 0; tx < 2; tx++)
                        {
                            for (int ty = 0; ty < 2; ty++)
                            {
                                double vx = testX + tx - player.Position.X;
                                double vy = testY + ty - player.Position.Y;

                                double vectorModule = Math.Sqrt(vx * vx + vy * vy);
                                double cosAngle = (rayX * vx / vectorModule) + (rayY * vy / vectorModule);
                                boundsVectorsList.Add((vectorModule, cosAngle));
                            }
                        }

                        boundsVectorsList = boundsVectorsList.OrderBy(v => v.X).ToList();

                        double boundAngle = 0.03 / distanceToWall;

                        if (Math.Acos(boundsVectorsList[0].Y) < boundAngle ||
                            Math.Acos(boundsVectorsList[1].Y) < boundAngle)
                            isBound = true;
                    }
                    else
                    {
                        Map[testY * MapWidth + testX] = '*';
                    }
                }
            }

            int ceiling = (int)(ScreenHeight / 2.0 - ScreenHeight * Fov / distanceToWall);
            int floor = ScreenHeight - ceiling;

            ceiling += (int)(ScreenHeight - ScreenHeight * wallSize);

            char wallShade;

            if (isBound)
                wallShade = '|';
            else if (distanceToWall <= Depth / 5)
                wallShade = '\u2588';
            else if (distanceToWall < Depth / 4)
                wallShade = '#';
            else if (distanceToWall < Depth / 3)
                wallShade = '=';
            else if (distanceToWall < Depth / 2)
                wallShade = '+';
            else if (distanceToWall < Depth)
                wallShade = '_';
            else
                wallShade = ' ';

            for (int y = 0; y < ScreenHeight; y++)
            {
                if (y < ceiling)
                    result[y * ScreenWidth + x] = ' ';
                else if (y > ceiling && y <= floor)
                    result[y * ScreenWidth + x] = wallShade;
                else
                {
                    char floorShade;
                    double b = 1.0 - (y - ScreenHeight / 2.0) / (ScreenHeight / 2.0);

                    if (b < 0.25)
                        floorShade = '#';
                    else if (b < 0.5)
                        floorShade = '.';
                    else if (b < 0.75)
                        floorShade = '.';
                    else if (b < 0.9)
                        floorShade = '.';
                    else
                        floorShade = ' ';

                    result[y * ScreenWidth + x] = floorShade;
                }
            }

            return result;
        }

        public static void InitMap()
        {
            Map.Clear();
            Map.Append("################################");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#            #                 #");
            Map.Append("#            #                 #");
            Map.Append("#            #                 #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#            #                 #");
            Map.Append("#            #                 #");
            Map.Append("#            #                 #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("#                              #");
            Map.Append("################################");
        }
    }
}
