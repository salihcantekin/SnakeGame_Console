using System.Drawing;

namespace SnakeGame.ConsoleApp;

public class Snake
{
    private const int DEFAULT_DELAY = 75;
    private readonly LinkedList<Point> segments;
    private Point food;
    private int dx = 1,
                dy = 0,
                foodEaten = 0;

    private Rectangle borderRec;

    private readonly double delay = DEFAULT_DELAY;
    private readonly double delayBoost = DEFAULT_DELAY / 2;

    private readonly char snakeChar = 'O',
                          borderChar = '#';

    private bool speedBoosted = false,
                 isPaused = false;

    private readonly HashSet<Point> emptyPoints;

    public Snake(char snakeChar,
                 Point snakePoint,
                 int snakeInitialLength,
                 Rectangle borderRec)
    {
        this.borderRec = borderRec;
        this.snakeChar = snakeChar;

        segments = new LinkedList<Point>();

        for (int i = 0; i < snakeInitialLength; i++)
        {
            segments.AddLast(new Point(borderRec.Left + snakePoint.X - i, borderRec.Top + snakePoint.Y));
        }

        // inital segment excluding
        emptyPoints = new HashSet<Point>(borderRec.Width * borderRec.Height);
        for (int x = borderRec.Left + 1; x < borderRec.Right - 1; x++)
        {
            for (int y = borderRec.Top + 1; y < borderRec.Bottom - 1; y++)
            {
                emptyPoints.Add(new Point(x, y));
            }
        }

        emptyPoints.ExceptWith(segments);

        CreateFood().GetAwaiter().GetResult();
    }

    public async Task CreateFood(CancellationToken token = default)
    {
        if (emptyPoints.Count == 0)
        {
            await PrintComplete(token);
        }

        var randomIndex = Random.Shared.Next(0, emptyPoints.Count);
        food = emptyPoints.ElementAt(randomIndex);
        emptyPoints.Remove(food);
    }

    private static void DrawSnakeHead(Point position, int dx, int dy)
    {
        ConsoleHelper.ResetCursorPosition(position.X, position.Y);

        if (dx == 0 && dy == -1)
        {
            Console.Write("^");
            //Console.Write("↑");
        }
        else if (dx == 0 && dy == 1)
        {
            Console.Write("v");
            //Console.Write("↓");
        }
        else if (dx == 1 && dy == 0)
        {
            Console.Write(">");
            //Console.Write("→");
        }
        else if (dx == -1 && dy == 0)
        {
            Console.Write("<");
            //Console.Write("←");
        }
    }


    public async Task Run(CancellationToken token = default)
    {
        PrintBorder();
        PrintFood(food.X, food.Y); // initial food

        while (!token.IsCancellationRequested)
        {
            WaitForKeyPressAndSetDirectionAndDelay();
            PrintStatusBar();

            // save old head and tail
            var oldTail = segments.Last.Value;
            var oldHead = segments.First.Value;

            // head to the new position
            var head = new Point(segments.First.Value.X + dx,
                                   segments.First.Value.Y + dy);

            segments.AddFirst(head);

            if (head == food) // Eat the food
            {
                await CreateFood(token);
                PrintFood(food.X, food.Y); // new food

                foodEaten++;
                PrintStatusBar();
            }
            else
            {
                segments.RemoveLast();
            }

            ConsoleHelper.ClearText(oldTail.X, oldTail.Y); // remove old tail


            DrawSnakeHead(head, dx, dy);
            // replace old head with snakeChar
            ConsoleHelper.ResetCursorPosition(oldHead.X, oldHead.Y);
            Console.Write(snakeChar);




            // Check for collision with walls or itself
            bool hasCollision = HasCollision(head.X, head.Y);
            if (hasCollision)
            {
                await PrintGameOver(token);
                break;
            }

            await Task.Delay((int)GetDelay(), token);
        }
    }



    private double GetDelay()
    {
        var defaultDelay = speedBoosted ? delayBoost : delay;
        return dx == 0 ? defaultDelay : defaultDelay * .8;   // slower by 20% when moving horizontally
    }

    private bool HasCollision(int x, int y)
    {
        return x == borderRec.X
                || x == borderRec.Right
                || y == borderRec.Y
                || y == borderRec.Bottom
                || segments.Skip(1).Any(i => i.X == x && i.Y == y);
    }

    private async Task PrintComplete(CancellationToken token = default)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        PrintBorder();
        PrintStatusBar();

        var message = "Completed!";

        var middle = GetCenterOfBorder(message.Length / 2);

        await ConsoleHelper.PrintBlinkingText(message, middle, 500, token);
    }

    private async Task PrintGameOver(CancellationToken token = default)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        PrintBorder();
        PrintStatusBar();

        var message = "Game Over!";

        var middle = GetCenterOfBorder(message.Length / 2);
        
        await ConsoleHelper.PrintBlinkingText(message, middle, delay: 500, token);
    }

    private void WaitForKeyPressAndSetDirectionAndDelay()
    {
        if (!Console.KeyAvailable)
            return;

        var key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.P)
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                string pauseMessage = "PAUSED. Press ANY key to continue";
                PrintStatusBarWithMessage(pauseMessage);
                Console.ReadKey(true);
                isPaused = false;
            }
        }
        else if (key == ConsoleKey.UpArrow && dy == 0)
        {
            dx = 0; dy = -1;
        }
        else if (key == ConsoleKey.DownArrow && dy == 0)
        {
            dx = 0; dy = 1;
        }
        else if (key == ConsoleKey.LeftArrow && dx == 0)
        {
            dx = -1; dy = 0;
        }
        else if (key == ConsoleKey.RightArrow && dx == 0)
        {
            dx = 1; dy = 0;
        }
        else if (key == ConsoleKey.Spacebar)
        {
            speedBoosted = !speedBoosted;
        }
    }

    private void PrintStatusBar()
    {
        var direction = dx == 0 ? (dy == -1 ? "UP" : "DOWN") : (dx == -1 ? "LEFT" : "RIGHT");
        var message = string.Format("Eaten: {0}, L: {1}, S: {2}, Dir: {3}, Delay: {4}, Food: {5}, Head: {6}",
                                foodEaten,
                                segments.Count,
                                borderRec.Width + "x" + borderRec.Height,
                                direction,
                                GetDelay().ToString("#"),
                                $"X:{borderRec.X + food.X}, Y: {borderRec.Y + food.Y}",
                                $"{segments.First.Value.X},{segments.First.Value.Y}");

        if (isPaused)
            message += " Status: PAUSED";

        if (speedBoosted)
            message += " SPEED BOOST";

        PrintStatusBarWithMessage(message);
    }

    private void PrintStatusBarWithMessage(string message)
    {
        ConsoleHelper.ClearLine(y: 0);
        ConsoleHelper.ResetCursorPosition(borderRec.Left);
        Console.Write(message);
    }

    private void PrintBorder()
    {
        // Draw border
        for (int i = borderRec.X; i < borderRec.Right; i++)
        {
            ConsoleHelper.ResetCursorPosition(i, borderRec.Y); // first line
            Console.Write(borderChar);
            ConsoleHelper.ResetCursorPosition(i, borderRec.Bottom); // last line
            Console.Write(borderChar);
        }

        for (int i = borderRec.Y; i < borderRec.Y + borderRec.Height; i++)
        {
            ConsoleHelper.ResetCursorPosition(borderRec.X, i); // first column
            Console.Write(borderChar);
            ConsoleHelper.ResetCursorPosition(borderRec.Right, i); // last column
            Console.Write(borderChar);
        }
    }

    private static void PrintFood(int x, int y)
    {
        var currentColor = Console.ForegroundColor;
        ConsoleHelper.ResetCursorPosition(x, y);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("*");
        Console.ForegroundColor = currentColor;
    }

    #region Helper Methods

    private Point GetCenterOfBorder(int xOffSet = 0, int yOffSet = 0)
    {
        return new Point((borderRec.Left + borderRec.Right) / 2 - xOffSet,
                         (borderRec.Top + borderRec.Bottom) / 2 - yOffSet);
    }
    
    #endregion
}