﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _12x12console
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a new game
            Game gGame = new Game(new Tuple<int, int>(6, 6), Game.GameMode.PlayerVAI);
            int signalAIPlayPrompt = 0;
            bool TestMode = false; // For debugging

            gGame.MakeMove(2, new Tuple<int, int>(1, 1));
            gGame.MakeMove(1, new Tuple<int, int>(1, 0));
            gGame.MakeMove(1, new Tuple<int, int>(0, 1));

            if (TestMode == false)
            {
                while (true)
                {
                    if (gGame.IsComplete())
                    {
                        Console.WriteLine("Game is complete.");
                        // Post score
                        break;
                    }
                    if (signalAIPlayPrompt == 1)
                    {
                        signalAIPlayPrompt = 0;
                        Console.WriteLine("AI makes move");
                        // **************** AI CAN MAKE MOVE HERE
                        gGame.MakeAIMove();
                        gGame.Board.Print(); // Print the game board with an updated AIMove
                    }
                    else
                    {
                        gGame.Board.Print(); // Print the game board
                    }
                    string[] move_data;

                    // If the Game mode is PlayerVAi, we need to prompt the human user for input for a game move
                    if (gGame.GameType == Game.GameMode.PlayerVAI)
                    {
                        Console.Write("Your move? [r,c]:");
                        string input = Console.ReadLine(); // Get input from console. input should be informat x,y
                        if (input == "t")
                        {
                            Console.Write("Are you sure you want to enter test mode? Y/N");
                            ConsoleKeyInfo conf = Console.ReadKey();
                            if (conf.Key == ConsoleKey.Y)
                            {
                                TestMode = true; // test as test mode
                                break; // Exit the loop
                            } else
                            {
                                continue;
                            }
                        } else if (input == "pb")
                        {
                            // Print the board
                            gGame.Board.Print();
                            continue;
                        }
                        move_data = input.Split(',');
                        try
                        {
                            move_data[1] = move_data[1].Trim(' ');
                            Tuple<int, int> moveinfo = new Tuple<int, int>(int.Parse(move_data[0]), int.Parse(move_data[1]));

                            int result = gGame.MakeMove(1, moveinfo);
                            if (result == 0)
                            {
                                // gGame.Board.Print();
                                // Game board is updated, AI needs to make the next move
                                if (gGame.GameType == Game.GameMode.PlayerVAI)
                                {
                                    // Do an AIMove
                                    signalAIPlayPrompt = 1;
                                }
                            }
                            else
                            {
                                switch (result)
                                {
                                    case (int)PlacementErrors.TileOccupied:
                                        Console.WriteLine("Invalid move: tile position occupied.");
                                        break;
                                    case (int)PlacementErrors.OutOfBounds:
                                        Console.WriteLine("Invalid move: placement out of bounds.");
                                        break;
                                    default:
                                        Console.WriteLine("Invalid move: unknown error.");
                                        break;
                                }
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            gGame.Board.Print();
                            Console.WriteLine("Invalid move. " + ex.Message);
                        }
                    }
                }
            }
            if (TestMode)
            {
                // Give instructions, then start a debugging loop
                Console.WriteLine("");
                Console.WriteLine("You are now in debug mode. To enter commands, use the following format:");
                Console.WriteLine("[p, r, c] Where p is the player 1 or 2, r = row, c = column");
                while (true)
                {
                    // For debugging, specify the player
                    
                    Console.Write("Debug move? [p,r,c]:");
                    string input = Console.ReadLine(); // Get input from console. input should be in format x,y
                    if (input == "exit")
                    {
                        Console.Write("Are you sure you want to exit debug mode? y/n");
                        ConsoleKeyInfo secondary_conf_input = Console.ReadKey();
                        if (secondary_conf_input.Key == ConsoleKey.Y)
                        {
                            string[] _args = new string[2];
                            Main(_args);
                        }
                    } else if (input == "pb")
                    {
                        // print the board
                        gGame.Board.Print();
                    }
                    string[] move_data;
                    move_data = input.Split(',');
                    if (move_data.Length == 3)
                    {
                        // Valid input
                        move_data[0] = move_data[0].Trim();
                        move_data[1] = move_data[1].Trim();
                        move_data[2] = move_data[2].Trim();
                        try
                        {
                            gGame.MakeMove(int.Parse(move_data[0]), new Tuple<int, int>(int.Parse(move_data[1]), int.Parse(move_data[2])));
                            gGame.Board.Print();
                        } catch (Exception e)
                        {
                            Console.WriteLine("Input Error {0}", e.Message);
                            continue;
                        }

                    } else
                    {
                        Console.WriteLine("Invalid input.");
                        continue;
                    }
                }
            }
        }
    }
}
