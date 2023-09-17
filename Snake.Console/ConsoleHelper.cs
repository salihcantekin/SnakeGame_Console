using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame.ConsoleApp;
public static class ConsoleHelper
{
    public static void ClearText(int x, int y)
    {
        ResetCursorPosition(x, y);
        Console.Write(' ');
    }

    public static void ClearLine(int y)
    {
        ResetCursorPosition(y: y);
        Console.Write(new string(' ', Console.WindowWidth));
    }

    public static void ResetCursorPosition(int x = 0, int y = 0)
    {
        Console.SetCursorPosition(x, y);
    }

    public static async Task PrintBlinkingText(string message, Point point, int delay = 500, CancellationToken token = default)
    {
        while (true)
        {
            ResetCursorPosition(point.X, point.Y);
            Console.Write(message);
            await Task.Delay(delay, token);

            ResetCursorPosition(point.X, point.Y);
            Console.Write(new string(' ', message.Length));

            await Task.Delay(delay, token);
        }
    }

}
