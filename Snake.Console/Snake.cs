using System.Drawing;

namespace SnakeGame.ConsoleApp;

public class Snake
{
    #region Constants
    private const int DEFAULT_DELAY = 75;
    private const int DEFAULT_SNAKE_LENGTH = 5;
    private const double DEFAULT_VERTICAL_DELAY_RATIO = 0.6;
    private const double delay = DEFAULT_DELAY;
    private const double delayBoost = DEFAULT_DELAY / 2;

    private const ConsoleColor FOOD_COLOR = ConsoleColor.Green;
    private const ConsoleColor SNAKE_COLOR = ConsoleColor.Yellow;
    private const char DEFAULT_SNAKE_CHAR = 'O';
    private const char DEFAULT_BORDER_CHAR = '#';
    private const char DEFAULT_FOOD_CHAR = '*';

    private const Direction DEFAULT_DIRECTION = Direction.Right;

    private static Point DEFAULT_SNAKE_POINT = Point.Empty;

    private static Rectangle DEFAULT_BORDER_REC = new(1, 1, 50, 20);

    #endregion

    #region Private Variables

    private LinkedList<Point> segments;
    private char snakeChar;
    private HashSet<Point> emptyPoints;

    private Point food, snakeStartingPoint;
    private int foodEaten = 0, initialSnakeLength;
    private Direction currentDirection;
    private Rectangle borderRec;

    private char borderChar, foodChar;

    private bool speedBoosted = false,
                 isPaused = false;

    #endregion

    #region Constructors

    public Snake()
    {
        SetDefaults();
    }

    public Snake(char snakeChar,
               Point snakePoint,
               int snakeInitialLength,
               Rectangle borderRec)
    {
        this.borderRec = borderRec;
        this.snakeChar = snakeChar;
        snakeStartingPoint = snakePoint;
        initialSnakeLength = snakeInitialLength;
    }

    #endregion


    public async Task Run(CancellationToken token = default)
    {
        Adjust();
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
            var x = currentDirection == Direction.Right ? 1 : currentDirection == Direction.Left ? -1 : 0;
            var y = currentDirection == Direction.Down ? 1 : currentDirection == Direction.Up ? -1 : 0;
            var head = new Point(oldHead.X + x,
                                 oldHead.Y + y);

            segments.AddFirst(head);

            if (head == food) // Eat the food
            {
                await CreateFood(token);
                PrintFood(food.X, food.Y);

                foodEaten++;
                PrintStatusBar();
                emptyPoints.Add(food); // add the food point to empty points
            }
            else
            {
                segments.RemoveLast();
                ConsoleHelper.ClearText(oldTail.X, oldTail.Y); // remove old tail
            }



            DrawSnakeHead(head, currentDirection);
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

    private void Adjust()
    {
        segments = new LinkedList<Point>();

        for (int i = 0; i < initialSnakeLength; i++)
        {
            segments.AddLast(new Point(borderRec.Left + snakeStartingPoint.X - i,
                                       borderRec.Top + snakeStartingPoint.Y));
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
    private void SetDefaults()
    {
        borderRec = DEFAULT_BORDER_REC;
        snakeChar = DEFAULT_SNAKE_CHAR;
        initialSnakeLength = DEFAULT_SNAKE_LENGTH;
        snakeStartingPoint = new Point(1, 1);
        currentDirection = DEFAULT_DIRECTION;
        foodChar = DEFAULT_FOOD_CHAR;
        borderChar = DEFAULT_BORDER_CHAR;
    }



    private static void DrawSnakeHead(Point position, Direction currentPosition)
    {
        ConsoleHelper.ResetCursorPosition(position.X, position.Y);

        var headChar = currentPosition switch
        {
            Direction.Up => '^',
            Direction.Down => 'v',
            Direction.Left => '<',
            Direction.Right => '>',
            _ => throw new NotImplementedException()
        };

        Console.Write(headChar);
    }

    private double GetDelay()
    {
        var defaultDelay = speedBoosted ? delayBoost : delay;
        return currentDirection == Direction.Left || currentDirection == Direction.Right
            ? defaultDelay
            : defaultDelay / DEFAULT_VERTICAL_DELAY_RATIO;
    }

    private bool HasCollision(int x, int y)
    {
        return x == borderRec.X
                || x == borderRec.Right
                || y == borderRec.Y
                || y == borderRec.Bottom
                || segments.Skip(1).Any(i => i.X == x && i.Y == y);
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
        else if (key == ConsoleKey.UpArrow)
        {
            currentDirection = Direction.Up;
        }
        else if (key == ConsoleKey.DownArrow)
        {
            currentDirection = Direction.Down;
        }
        else if (key == ConsoleKey.LeftArrow)
        {
            currentDirection = Direction.Left;
        }
        else if (key == ConsoleKey.RightArrow)
        {
            currentDirection = Direction.Right;
        }
        else if (key == ConsoleKey.Spacebar)
        {
            speedBoosted = !speedBoosted;
        }
    }

    private void PrintBorder()
    {
        // Draw border
        for (int i = borderRec.X; i < borderRec.Right; i++)
        {
            ConsoleHelper.ResetCursorPosition(i, borderRec.Y); // first line
            Console.Write(DEFAULT_BORDER_CHAR);
            ConsoleHelper.ResetCursorPosition(i, borderRec.Bottom); // last line
            Console.Write(DEFAULT_BORDER_CHAR);
        }

        for (int i = borderRec.Y; i < borderRec.Y + borderRec.Height; i++)
        {
            ConsoleHelper.ResetCursorPosition(borderRec.X, i); // first column
            Console.Write(DEFAULT_BORDER_CHAR);
            ConsoleHelper.ResetCursorPosition(borderRec.Right, i); // last column
            Console.Write(DEFAULT_BORDER_CHAR);
        }
    }

    #region Game Over/Completed Methods

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

    #endregion

    #region StatusBar Methods

    private void PrintStatusBar()
    {
        var message = string.Format("Eaten: {0}, L: {1}, S: {2}, Dir: {3}, Delay: {4}, Food: {5}, Head: {6}",
                                foodEaten,
                                segments.Count,
                                borderRec.Width + "x" + borderRec.Height,
                                currentDirection.ToString(),
                                GetDelay().ToString("#"),
                                $"{borderRec.X + food.X},{borderRec.Y + food.Y}",
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

    #endregion

    #region Food Methods

    private async Task CreateFood(CancellationToken token = default)
    {
        if (emptyPoints.Count == 0)
        {
            await PrintComplete(token);
            return;
        }

        var randomIndex = Random.Shared.Next(0, emptyPoints.Count);
        food = emptyPoints.ElementAt(randomIndex);
        emptyPoints.Remove(food);
    }

    private void PrintFood(int x, int y)
    {
        var currentColor = Console.ForegroundColor;
        ConsoleHelper.ResetCursorPosition(x, y);
        Console.ForegroundColor = FOOD_COLOR;
        Console.Write(foodChar);
        Console.ForegroundColor = currentColor;
    }

    #endregion

    #region Helper Methods

    private Point GetCenterOfBorder(int xOffSet = 0, int yOffSet = 0)
    {
        return new Point((borderRec.Left + borderRec.Right) / 2 - xOffSet,
                         (borderRec.Top + borderRec.Bottom) / 2 - yOffSet);
    }

    #endregion

    #region Property Set Methods

    public void SetSnakeChar(char snakeChar)
    {
        this.snakeChar = snakeChar;
    }

    public void SetSnakeStartingPoint(Point snakeStartingPoint)
    {
        this.snakeStartingPoint = snakeStartingPoint;
    }

    public void SetInitialSnakeLength(int initialSnakeLength)
    {
        this.initialSnakeLength = initialSnakeLength;
    }

    public void SetCurrentPosition(Direction currentPosition)
    {
        this.currentDirection = currentPosition;
    }

    public void SetBorderRec(Rectangle borderRec)
    {
        this.borderRec = borderRec;
    }

    public void SetBorderChar(char borderChar)
    {
        this.borderChar = borderChar;
    }

    public void SetFoodChar(char foodChar)
    {
        this.foodChar = foodChar;
    }

    #endregion
}