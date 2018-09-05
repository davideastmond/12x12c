using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace _12x12console
{
    class Program
    {
        static int episode_count = 200;
        static int AIGameSpeed = 0;
        static bool StatsOn = true;
        static List<GameEndedEventArgs> GameEndedStats = new List<GameEndedEventArgs>();
        
        static void Main(string[] args)
        {
            // Create a new game
            Gamesize gameDimension = new Gamesize(6, 6);

            Game gGame = new Game(new Tuple<int, int>(gameDimension.X, gameDimension.Y), Game.GameMode.AIvAI, false);
            gGame.SupressPrinting = true;

            gGame.OnGameFinished += GGame_OnGameFinished;
            gGame.OnGameStarted += GGame_OnGameStarted;

            // Get a command line
            while (true)
            {
                Console.WriteLine("Press the appropriate number:");
                Console.WriteLine("(1) Run Game.   (2) Diagnostics");
                System.ConsoleKeyInfo initialInput = Console.ReadKey();

                if (initialInput.Key == ConsoleKey.D1 || initialInput.Key == ConsoleKey.NumPad1)
                {
                    Console.WriteLine("");
                    break;
                } else
                {
                    // Get the system diagnostics
                    // Need to open the "latest.dat" file that contains the path for the latest stats file
                    // Get the latest stats data file path
                    StreamReader sReader;
                    if (File.Exists(Environment.CurrentDirectory + "\\" + "latest.dat"))
                    {
                        sReader = new StreamReader(Environment.CurrentDirectory + "\\" + "latest.dat"); // Get the stream
                        string rStringToend = "";
                        while (!sReader.EndOfStream)
                        {
                            rStringToend = sReader.ReadLine();
                        }

                        // We need to open the file
                        if (File.Exists(rStringToend))
                        {
                            // We need to open the file and get the stats via deserialization
                            using (FileStream s_stream = new FileStream(rStringToend, FileMode.Open, FileAccess.Read))
                            {
                                // Open the file and create a new list
                                BinaryFormatter myFormatter = new BinaryFormatter();
                                List<GameEndedEventArgs> list_args = myFormatter.Deserialize(s_stream) as List<GameEndedEventArgs>;
                            }
                        } else
                        {

                            Debug.Print("There was an issue opening the file {0}", rStringToend);
                            Application.Exit();
                        }
                        
                    } else
                    {
                        Debug.Print("No data file exists. Running game.");
                        break;
                    }
                }
            }
            // Diagnostics: create a stopwatch timer
            Stopwatch exWatch = new Stopwatch();
            Console.Write("Game is running...");
            exWatch.Start();
            for (int GameRunCount = 0; GameRunCount < episode_count; GameRunCount++)
            {
                gGame.Start();
                RunGame(gGame);
            }
            // Write stats to file
            if (StatsOn)
            {
                // Serialize and write to a file
                WriteStatsToFile(GameEndedStats);
            }
            exWatch.Stop();

            // We need to display the stats data
            Debug.Print("Elapsed time in milliseconds {0}", exWatch.ElapsedMilliseconds);

            
            
        }

        private static void GGame_OnGameStarted(object sender, EventArgs e)
        {
            Debug.Print("Game has started");
        }

        private static void GGame_OnGameFinished(object sender, GameEndedEventArgs e)
        {
            // Get some stats
            Debug.Print("Winner is " + e.Winner.PieceColor);
            Debug.Print("Final Score " + e.FinalScore);
            GameEndedStats.Add(e);
        }

        private static void RunGame(Game gGame)
        {
            int signalAIPlayPrompt = 0;
            bool TestMode = false; // For debugging

            // Create a stop watch
            
            if (TestMode == false)
            {
                while (true)
                {
                    if (gGame.IsComplete())
                    {
                        //Console.WriteLine("Game is complete.");
                        // Post score
                        break;
                    }
                    if (signalAIPlayPrompt == 1 && gGame.GameType == Game.GameMode.PlayerVAI)
                    {
                        signalAIPlayPrompt = 0;

                        // **************** AI CAN MAKE MOVE HERE
                        gGame.MakeAIMove();
                        gGame.Print(); // Print the game board with an updated AIMove
                    }
                    else
                    {
                        gGame.Print(); // Print the game board
                    }
                    string[] move_data;

                    // If the Game mode is PlayerVAi, we need to prompt the human user for input for a game move
                    if (gGame.GameType == Game.GameMode.PlayerVAI && gGame.GameIsOn)
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
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else if (input == "pb")
                        {
                            // Print the board
                            gGame.Print();
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
                            gGame.Print();
                            Console.WriteLine("Invalid move. " + ex.Message);
                        }
                    }
                    else if(gGame.GameType == Game.GameMode.AIvAI && gGame.GameIsOn)
                    {
                        // AI v AI move
                        AIPlayer AIPlayer1 = (AIPlayer)gGame.Player1;
                        AIPlayer AIPlayer2 = (AIPlayer)gGame.Player2;

                        // Testing purposes, make one AI use the white space strategy
                        AIPlayer1.EnableWhiteSpaceStrategy = false;
                        AIPlayer2.EnableWhiteSpaceStrategy = true;

                        AIPlayer1.isDumbPlayer = true;

                        while (gGame.GameIsOn)
                        {
                            // Player 1 moves

                            Tuple<int, int> P1MoveTuple = AIPlayer1.DoAIMove(gGame.Board);
                            gGame.MakeMove(AIPlayer1.PieceColor, P1MoveTuple);
                            //Console.WriteLine("Blue plays (" + P1MoveTuple.Item1 + ", " + P1MoveTuple.Item2 + ")");
                            gGame.Print();
                            Thread.Sleep(AIGameSpeed);

                            // Player 2 moves
                            Tuple<int, int> P2MoveTuple = AIPlayer2.DoAIMove(gGame.Board);
                            gGame.MakeMove(AIPlayer2.PieceColor, P2MoveTuple);
                            //Console.WriteLine("Red plays (" + P2MoveTuple.Item1 + ", " + P2MoveTuple.Item2 + ")");
                            gGame.Print();
                            Thread.Sleep(AIGameSpeed);
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
                    }
                    else if (input == "pb")
                    {
                        // print the board
                        gGame.Print();
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
                            gGame.Print();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Input Error {0}", e.Message);
                            continue;
                        }

                    }
                    else
                    {
                        Console.WriteLine("Invalid input.");
                        continue;
                    }
                }
            
            }
          
            
        }

        private static void WriteStatsToFile (List<GameEndedEventArgs> rawData)
        {
            string statsFilePath = Environment.CurrentDirectory + "\\" + DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + "_" + "stats.dat";
            string latestdata = Environment.CurrentDirectory + "\\" + "latest.dat";
            if (!File.Exists(statsFilePath))
            {
                using (FileStream stream = new FileStream(statsFilePath, FileMode.OpenOrCreate))
                {
                    if (!File.Exists(latestdata))
                    {
                        File.Create(latestdata); // Create a blank file
                    }
                    BinaryFormatter mFormatter = new BinaryFormatter();
                    mFormatter.Serialize(stream, rawData); // Save to file
                    Debug.Print("Stats data saved to file.");

                }
                // Create a latest.dat file
                if (File.Exists(latestdata))
                {
                    File.AppendAllText(latestdata, statsFilePath + System.Environment.NewLine); // Append a file path to the data file
                } 
            } else
            {
                // File exists
            }
        }
        
    }
}
