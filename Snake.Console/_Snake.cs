using System;

namespace SnakeGame.ConsoleApp;
public class _Snake
{
    private readonly LinkedList<char> snake = new();
    private int x;
    private int y;

    public void SetSnakePosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void AddSnakePart()
    {
        snake.AddFirst('#');
    }

    public void RemoveSnakePart()
    {
        snake.RemoveLast();
    }
    
    public void Print(int x, int y)
    {
        Console.SetCursorPosition(x, y);
        foreach (var item in snake)
        {
            Console.Write(item);
            x++;
        }
        Console.WriteLine();
    }

}
