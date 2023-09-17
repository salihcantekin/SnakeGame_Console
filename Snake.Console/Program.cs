﻿using SnakeGame.ConsoleApp;
using System.Drawing;

//Console.BackgroundColor = ConsoleColor.Black;
//Console.WindowWidth = 100;
//Console.WindowHeight = 100;
Console.CursorVisible = false;

int borderWidth = 50;
int borderHeigth = 20;

int borderX = 5;
int borderY = 5;

var message = "Press any key to start the game";

Console.SetCursorPosition((Console.WindowWidth - message.Length) / 2, Console.CursorTop + 5);
Console.WriteLine(message);

Console.ReadKey();
Console.Clear();

Snake snake = new(snakeChar: 'O',
                  snakePoint: new Point(5, 5),
                  snakeInitialLength: 2,
                  new Rectangle(borderX, borderY, borderWidth, borderHeigth));

await snake.Run();
