using System.Drawing;

Console.BackgroundColor = ConsoleColor.Black;
//Console.SetWindowSize(500, 500);
Console.WindowWidth = 100;
Console.WindowHeight = 100;
Console.CursorVisible = false;

int borderWidth = 50;
int borderHeigth = 20;

int borderX = 5;
int borderY = 5;

Snake snake = new(snakeChar: 'O',
                  snakePoint: new Point(5, 5),
                  snakeInitialLength: 2,
                  new Rectangle(borderX, borderY, borderWidth, borderHeigth));

Console.ReadKey();

await snake.Run();

class Snake
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
        emptyPoints = new HashSet<Point>((borderRec.Width) * (borderRec.Height));
        for (int x = borderRec.Left + 1; x < borderRec.Right - 1; x++)
        {
            for (int y = borderRec.Top + 1; y < borderRec.Bottom - 1; y++)
            {
                emptyPoints.Add(new Point(x, y));
            }
        }

        emptyPoints.ExceptWith(segments);

        CreateFood();
    }

    public async Task CreateFood()
    {
        if (emptyPoints.Count == 0)
        {
            await PrintComplete();
        }

        var randomIndex = Random.Shared.Next(0, emptyPoints.Count);
        food = emptyPoints.ElementAt(randomIndex);
        emptyPoints.Remove(food);
    }

    private void DrawSnake(LinkedList<Point> segments)
    {
        foreach (var segment in segments.Skip(1))
        {
            ResetCursorPosition(segment.X, segment.Y);
            Console.Write(snakeChar);
        }
    }

    private static void DrawSnakeHead(Point position, int dx, int dy)
    {
        ResetCursorPosition(position.X, position.Y);

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


    public async Task Run()
    {
        PrintBorder();
        DrawSnake(segments); // initial snake
        PrintFood(food.X, food.Y); // initial food

        while (true)
        {
            WaitForKeyPressAndSetDirectionAndDelay();
            PrintStatusBar();

            // save old tail
            var oldTail = segments.Last.Value;

            // head to the new position
            Point head = new Point(segments.First.Value.X + dx,
                                   segments.First.Value.Y + dy);

            segments.AddFirst(head);

            if (head == food) // Eat the food
            {
                await CreateFood();
                PrintFood(food.X, food.Y); // new food

                foodEaten++;
                PrintStatusBar();
            }
            else
            {
                segments.RemoveLast();
            }

            ClearText(oldTail.X, oldTail.Y); // remove old tail

            DrawSnake(segments);
            DrawSnakeHead(head, dx, dy);




            // Check for collision with walls or itself
            bool hasCollision = HasCollision(head.X, head.Y);
            if (hasCollision)
            {
                await PrintGameOver();
                break;
            }

            await Task.Delay((int)GetDelay());
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

    private async Task PrintComplete()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        PrintBorder();
        PrintStatusBar();

        var message = "Completed!";

        var middle = GetCenterOfBorder(message.Length / 2);

        await PrintBlinkingText(message, middle, 500);
    }

    private async Task PrintGameOver()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        PrintBorder();
        PrintStatusBar();

        var message = "Game Over!";

        var middle = GetCenterOfBorder(message.Length / 2);

        await PrintBlinkingText(message, middle, 500);
    }

    private void WaitForKeyPressAndSetDirectionAndDelay()
    {
        if (!Console.KeyAvailable)
            return;

        var key = Console.ReadKey(true).Key;

        switch (key)
        {
            case ConsoleKey.P:
                isPaused = !isPaused;
                if (isPaused)
                {
                    string pauseMessage = "PAUSED. Press ANY key to continue";
                    PrintStatusBarWithMessage(pauseMessage);
                    Console.ReadKey(true);
                    isPaused = false;
                }
                break;

            case ConsoleKey.UpArrow when dy == 0:
                dx = 0; dy = -1;
                break;

            case ConsoleKey.DownArrow when dy == 0:
                dx = 0; dy = 1;
                break;

            case ConsoleKey.LeftArrow when dx == 0:
                dx = -1; dy = 0;
                break;

            case ConsoleKey.RightArrow when dx == 0:
                dx = 1; dy = 0;
                break;

            case ConsoleKey.Spacebar:
                speedBoosted = !speedBoosted;
                break;

            default:
                break;
        }
    }

    private void PrintStatusBar()
    {
        var message = string.Format("Eaten: {0}, Length: {1}, Size: {2}, Direction: {3}, Delay: {4}, Food: {5}, Head: {6}",
                                foodEaten,
                                segments.Count,
                                borderRec.Width + "x" + borderRec.Height,
                                $"{dx},{dy}",
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
        ClearLine(y: 0);
        ResetCursorPosition(borderRec.Left);
        Console.Write(message);
    }

    private void PrintBorder()
    {
        // Draw border
        for (int i = borderRec.X; i < borderRec.Right; i++)
        {
            ResetCursorPosition(i, borderRec.Y); // first line
            Console.Write(borderChar);
            ResetCursorPosition(i, borderRec.Bottom); // last line
            Console.Write(borderChar);
        }

        for (int i = borderRec.Y; i < borderRec.Y + borderRec.Height; i++)
        {
            ResetCursorPosition(borderRec.X, i); // first column
            Console.Write(borderChar);
            ResetCursorPosition(borderRec.Right, i); // last column
            Console.Write(borderChar);
        }
    }

    private static void PrintFood(int x, int y)
    {
        var currentColor = Console.ForegroundColor;
        ResetCursorPosition(x, y);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("*");
        Console.ForegroundColor = currentColor;
    }

    #region Helper Methods
    private static void ClearText(int x, int y)
    {
        ResetCursorPosition(x, y);
        Console.Write(' ');
    }

    private static void ClearLine(int y)
    {
        ResetCursorPosition(y: y);
        Console.Write(new string(' ', Console.WindowWidth));
    }

    private static void ResetCursorPosition(int x = 0, int y = 0)
    {
        Console.SetCursorPosition(x, y);
    }

    private Point GetCenterOfBorder(int xOffSet = 0, int yOffSet = 0)
    {
        return new Point((borderRec.Left + borderRec.Right) / 2 - xOffSet,
                         (borderRec.Top + borderRec.Bottom) / 2 - yOffSet);
    }


    private async Task PrintBlinkingText(string message, Point point, int delay = 500)
    {
        while (true)
        {
            ResetCursorPosition(point.X, point.Y);
            Console.Write(message);
            await Task.Delay(delay);

            ResetCursorPosition(point.X, point.Y);
            Console.Write(new string(' ', message.Length));

            await Task.Delay(delay);
        }
    }

    #endregion
}
