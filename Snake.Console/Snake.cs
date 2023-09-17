using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame.ConsoleApp;
public class Snake
{
    List<(int, int)> segments;
    int dx = 1, dy = 0;
    int windowWidth, windowHeight;

    public void Initialize(int x, int y, int length, int windowWidth, int windowHeight)
    {
        this.windowWidth = windowWidth;
        this.windowHeight = windowHeight;
        segments = new List<(int, int)>();
        for (int i = 0; i < length; i++)
        {
            segments.Add((x - i, y));
        }
    }

    public async Task Run()
    {
        while (true)
        {
            Console.Clear();

            // Draw border
            for (int i = 0; i < windowWidth; i++)
            {
                Console.SetCursorPosition(i, 0);
                Console.Write("#");
                Console.SetCursorPosition(i, windowHeight - 1);
                Console.Write("#");
            }

            for (int i = 1; i < windowHeight - 1; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write("#");
                Console.SetCursorPosition(windowWidth - 1, i);
                Console.Write("#");
            }

            // Draw snake
            foreach (var (x, y) in segments)
            {
                Console.SetCursorPosition(x, y);
                Console.Write("O");
            }

            // Move snake's head
            var head = segments[0];
            head.Item1 += dx;
            head.Item2 += dy;
            segments.Insert(0, (head.Item1, head.Item2));

            // Check for collision with walls or itself
            if (head.Item1 == 0 || head.Item1 == windowWidth - 1 || head.Item2 == 0 || head.Item2 == windowHeight - 1
                || segments.Skip(1).Contains((head.Item1, head.Item2)))
            {
                //Console.SetCursorPosition(0, windowHeight);
                Console.WriteLine("Game Over!");
                break;
            }

            // Remove last segment (to simulate movement)
            segments.RemoveAt(segments.Count - 1);

            // Handle keyboard input
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        dx = 0;
                        dy = -1;
                        break;
                    case ConsoleKey.DownArrow:
                        dx = 0;
                        dy = 1;
                        break;
                    case ConsoleKey.LeftArrow:
                        dx = -1;
                        dy = 0;
                        break;
                    case ConsoleKey.RightArrow:
                        dx = 1;
                        dy = 0;
                        break;
                }
            }

            await Task.Delay(120);
        }
    }
}
