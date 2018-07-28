using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Windows;

namespace _12x12console
{
    public enum PlacementErrors {TileOccupied = -1, OutOfBounds = -2}
    public static class RandomNumberGenerator
    {
        public static Random RndInt = new Random(DateTime.Now.Millisecond);
    }
    public class Game
    {
        public enum GameMode {PlayerVAI = 0, AIvAI = 1}
        
        public const int Empty = 0;
        public const int Blue = 1;
        public const int Red = 2;

        public GameBoard Board;
        public Player Player1;
        public Player Player2;

        private bool Scroll_Update;

        public bool GameIsOn = false;
        private Tuple<int, int> recentMove = new Tuple<int, int>(-1, -1);

        private int _moveCount = 0;
        public int MoveCount
        {
            get
            {
                return _moveCount;
            }
        }

        private int _Player1_Score;
        public int Player1_Score
        {
            get
            {
                return _Player1_Score;
            }
        }

        private int _Player2_Score;
        public int Player2_Score
        {
            get
            {
                return _Player2_Score;
            }
        }

        public GameMode GameType;
       
        public Game(Tuple<int, int> size, GameMode mode, bool ScrollUpdate = false)
        {
            // Initialize the game. Set the colors, players and create a gameboard
            Board = new GameBoard(size); // Initialize
            this._moveCount = 0; // Reset the move count to zero
            Scroll_Update = ScrollUpdate;

            if (mode == GameMode.AIvAI)
            {
                Player1 = new AIPlayer();
                Player1.PieceColor = Blue;
                Player2 = new AIPlayer();
                Player2.PieceColor = Red;
                this.GameType = mode;
            } else
            {
                Player1 = new HumanPlayer();
                Player1.PieceColor = Blue;
                Player2 = new AIPlayer();
                Player2.PieceColor = Red;
                this.GameType = mode;
            }
        }
        public void Reset()
        {
            this.recentMove = new Tuple<int, int>(-1, -1);
            this.Board.Clear();
            
        }
        public void Start()
        {
            this.Board.Clear();
            this.GameIsOn = true;

            // Here we can randomly have the AI be first to go
            int first = RandomNumberGenerator.RndInt.Next(0, 2);
            if (first == 1)
            {
                MakeAIMove(); // Do an AI move
            }
            

        }
        public void Stop()
        {
            this.GameIsOn = false;
        }
        public void Print()
        {
            // Print out a helper guide

            if (Scroll_Update == true)
            {
                Console.Clear();
            }
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Green;
            for (int colCount = 0; colCount < Board.Grid.GetLength(1); colCount++)
            {
                Console.Write(colCount);
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("   Blue:" + Player1_Score);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("   Red:" + Player2_Score);

            // New line
            Console.WriteLine("");
            Console.ResetColor();
            // Prints the game board to the console
            if (Board.Grid != null)
            {
                for (int numRows = 0; numRows < Board.Grid.GetLength(0); numRows++)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(numRows + " "); // Row guide
                    Console.ResetColor();
                    for (int numCols = 0; numCols < Board.Grid.GetLength(1); numCols++)
                    {
                        if (Board.Grid[numRows, numCols] == Game.Blue)
                        {
                            if (IsPieceCaptured(new Tuple<int, int>(numRows, numCols)))
                            {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                            }
                        }
                        else if (Board.Grid[numRows, numCols] == Game.Red)
                        {
                            if (IsPieceCaptured(new Tuple<int, int>(numRows, numCols)))
                            {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                            }
                        }
                        else
                        {
                            Console.ResetColor();
                        }
                        Console.Write(Board.Grid[numRows, numCols]);
                        Console.ResetColor();
                    }
                    if (recentMove.Item1 == numRows)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(" <");
                        Console.ResetColor();
                    } else
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.ResetColor();
                    }
                    Console.WriteLine("");
                }
            }
            else
            {
                Console.WriteLine("Grid value is null.");
            }
        }
        public int MakeMove(int P, Tuple<int, int>loc)
        {
            if (this.GameIsOn)
            {
                int p_color;
                if (P == 1)
                {
                    p_color = Blue;
                }
                else
                {
                    p_color = Red;
                }

                // Make a move on a tile
                if (loc.IsInBounds(this.Board.Grid))
                {
                    if (Board.Grid[loc.Item1, loc.Item2] == Empty)
                    {
                        Board.Grid[loc.Item1, loc.Item2] = p_color;
                        SweepForScore();
                        Board.Space_Tracker.Remove(new Tuple<int, int>(loc.Item1, loc.Item2));
                        recentMove = loc;
                        
                        if (IsComplete())
                        {
                            this.Stop(); // Stop the game
                            Console.WriteLine("Game has ended.");
                            Console.WriteLine("Final score Blue: " + this.Player1_Score + " Red: " + this.Player2_Score);
                            int[,] transformed_board = this.Board.Grid;
                           
                        }
                        return 0;
                    }
                }
                else
                {
                    return (int)PlacementErrors.OutOfBounds; //
                }
                return (int)PlacementErrors.TileOccupied; // Move is invalid
            }
            else
            {
                Console.WriteLine("Game is not started.");
                return -1;
            }
        }
        public void MakeAIMove()
        {
            // The AI will never make a wrong move, so it will be a void function
            if (Player2 is AIPlayer == false)
            {
                // Throw an exception
                throw new Exception("No AIPlayers in this game");
            } else
            {
                // Do an AIMove. Call a function that will return a Tuple<int, int> indicating next move to play
                AIPlayer AI_Player = (AIPlayer)Player2; // Cast
                Tuple<int, int> finalmove = AI_Player.DoAIMove(this.Board);
                MakeMove(AI_Player.PieceColor, finalmove);
                Console.WriteLine("CPU plays: " + finalmove.Item1 + "," + finalmove.Item2);
                
            }
        }
        public bool IsComplete()
        {
            // Returns true if all spaces are occupied with tiles
           if (Board.Space_Tracker.Count > 0)
           {
               return false;
           }
           return true;
        }
        public void SweepForScore()
        {
            // Scans the board and tabulates the score
            // Reset the score
            _Player1_Score = 0;
            _Player2_Score = 0;
            for (int rows = 0; rows < this.Board.Grid.GetLength(0); rows++)
            {
                for (int cols = 0; cols < this.Board.Grid.GetLength(1); cols++)
                {
                    if (IsPieceCaptured(new Tuple<int, int>(rows, cols)))
                    {
                        if (Board.Grid[rows, cols] == Game.Blue)
                        {
                            _Player2_Score++;
                        } else if (Board.Grid[rows, cols] == Game.Red)
                        {
                            _Player1_Score++;
                        }
                    }
                }
            }
        }
        private int GetOppColor(int homePColor)
        {
            if (homePColor == 1)
            {
                return 2;
            } else if (homePColor == 2)
            {
                return 1;
            } else
            {
                throw new ArgumentException("Invalid Piece Color.");
            }
        }
        private bool IsPieceCaptured(Tuple<int, int> atCenter)
        {
            // Should return true if the piece atCenter is captured
            // What is the color of piece atCenter?

            int pColorAtCenter = this.Board.Grid[atCenter.Item1, atCenter.Item2];

            if (pColorAtCenter == 0)
            {
                return false;
            }
            int OpposingPiece = GetOppColor(pColorAtCenter);
            List<Tuple<int, int>> s_pieces = Board.GetSurroundingPieces(atCenter);
            int s_count = 0;
            foreach (Tuple<int, int> spot in s_pieces)
            {
                if (this.Board.Grid[spot.Item1, spot.Item2] == OpposingPiece)
                {
                    s_count++;
                }
            }
            if (s_count == s_pieces.Count)
            {
                return true;
            }
            return false;
        }
    }

    [Serializable]
    public class GameBoard : ISerializable
    {
        public int[,] Grid;
        
        public GameBoard(Tuple<int, int> size)
        {
            
            // Initialize a new maxtrix
            Grid = new int[size.Item1, size.Item2];
            bSize_ = Grid.GetLength(0) * Grid.GetLength(1); // Set the size property which is row * col
            PopulateSpaceTrackerList();
            
        }
        public GameBoard(SerializationInfo info, StreamingContext context)
        {
            // Serializable constructor
            Grid = (int[,])info.GetValue("grid", typeof(int[,]));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // The only property that gets serialized is the game's grid.
            info.AddValue("grid", Grid, typeof(int[,]));
        }
        public List<Tuple<int, int>> Space_Tracker = new List<Tuple<int, int>>();
       
        private int bSize_;
        public int BoardSize
        {
            get
            {
                return bSize_;
            }
        }
        private void PopulateSpaceTrackerList()
        {
            for (int rows = 0; rows < Grid.GetLength(0); rows++)
            {
                for (int cols = 0; cols < Grid.GetLength(1); cols++)
                {
                    Space_Tracker.Add(new Tuple<int, int>(rows, cols));
                }
            }
        }
       
        public int EmptySpaceCount
        {
            // Property that keeps track of the amount of empty spaces left
            get
            {
                return Space_Tracker.Count;
            }
        }
        public void Clear()
        {
            Space_Tracker.Clear(); // Clear the space tracker array
            // Clears the board
            for (int rows = 0; rows < this.Grid.GetLength(0); rows++)
            {
                for (int cols = 0; cols < this.Grid.GetLength(1); cols++)
                {
                    Space_Tracker.Add(new Tuple<int, int>(rows, cols));
                    Grid[rows, cols] = Game.Empty;
                }
            }
        }
        
        public static int GetOppColor(int homePColor)
        {
            if (homePColor == 1)
            {
                return 2;
            }
            else if (homePColor == 2)
            {
                return 1;
            }
            else
            {
                return 0; // Empty...there's no opposite color to empty
            }
        }
        
        public List<Tuple<int, int>> GetSurroundingPieces(Tuple<int, int> atCenter, bool diagonals=false)
        {
            // Gets the 2 to 4 surrounding pieces at a given centerPoint
            List<Tuple<int, int>> output = new List<Tuple<int, int>>();

            if (!diagonals)
            {
                // Default output, top, bottom, right and left
                output.Add(new Tuple<int, int>(atCenter.Item1 - 1, atCenter.Item2));
                output.Add(new Tuple<int, int>(atCenter.Item1 + 1, atCenter.Item2));
                output.Add(new Tuple<int, int>(atCenter.Item1, atCenter.Item2 + 1));
                output.Add(new Tuple<int, int>(atCenter.Item1, atCenter.Item2 - 1));
            } else
            {
                // Diagonals
                output.Add(new Tuple<int, int>(atCenter.Item1 - 1, atCenter.Item2 - 1)); // Diagonal up-left
                output.Add(new Tuple<int, int>(atCenter.Item1 - 1, atCenter.Item2 + 1)); // Diagnonal up right
                output.Add(new Tuple<int, int>(atCenter.Item1 + 1, atCenter.Item2 - 1)); // Diagonal bottom left
                output.Add(new Tuple<int, int>(atCenter.Item1 + 1, atCenter.Item2 + 1)); // Diagonal bottom right
            }

            if (!diagonals)
            {
                int row_checker = 0;
                if (atCenter.Item1 == 0)
                {
                    output.RemoveAt(0);
                   
                    row_checker++;
                }
                else if (atCenter.Item1 == this.Grid.GetLength(0) - 1)
                {
                    output.RemoveAt(1);
                    row_checker++;
                }

                if (atCenter.Item2 == 0)
                {
                    output.RemoveAt(3 - row_checker);
                }
                else if (atCenter.Item2 == this.Grid.GetLength(1) - 1)
                {
                    output.RemoveAt(2 - row_checker);
                }
            } else
            {
                // For diagonals, we'll simply remove any Tuple whose <item1, item2> is out of bounds/
                output = output.RemoveOutOfRangeElements(this);
            }

            return output;
        }     
    }
    public abstract class Player
    {
        private int _pColor;
        public int PieceColor
        {
            get
            {
                return _pColor;
            }
            set
            {
                _pColor = value;
            }
        }
        private string _tag;
        public string Tag
        {
            get
            {
                return _tag;
            }
            set
            {
                _tag = value;
            }
        }
       
    }
    public class HumanPlayer : Player
    {
        public HumanPlayer()
        {

        }
    }
    public class AIPlayer : Player
    {
        public bool EnableWhiteSpaceStrategy = true;
        public AIPlayer()
        {
            
        }
        
        public Tuple<int, int> DoAIMove(GameBoard boardstate)
        {
            /* AI Logic! */
            // Executes an AI Move based on the current game board state. 
            Tuple<int, int> return_move = new Tuple<int, int>(-1,-1); // Default value for an invalid move
            //***************************************
            List<Strategy> masterStrategyList = Strategy.GetAllStrategies(boardstate, PieceColor); // Get a list of all the master strategies and plays
            //***************************************

            // First let's find all winning strategies, which is defined as the next move shall score a point for the  AI
            // 1.
            List<Strategy> PointScoringStrategies = GetAIPointScoringStrategies(masterStrategyList);

            // Get the point-blocking strategies - prevent the opponent from scoring a point
            List<Strategy> PointBlockStrategies = GetPointBlockStrategies(masterStrategyList);

            // Get offensive whitespace strategies

            // Get White space defensive blocking opportunities
            List<Strategy> WhiteSpaceBlockStrategies = PrepareWhiteSpaceDefensiveBlockStrategies(masterStrategyList);

            

            // Get all strategies where there is a potential to build a score
            List<Strategy> PointBuildingStrategies = GetPointBuildingStrategies(masterStrategyList);

            // White space offense
            
            List<Strategy> WhiteSpaceStrategies = FilterWhiteSpaceBuildingStrategies(masterStrategyList, boardstate);

            List<Strategy> RemainingStrategies = GetRemainingStrategies(masterStrategyList);

            /* What we can do is get all possible moves of each strategy and feed it into a final decision algorithm*/

            // Next we'll
            // Console.WriteLine("There are {0} point scoring strategies", PointScoringStrategies.Count);

            if (PointScoringStrategies.Count > 0)
            {
                // Point scoring. Pick a random point scoring move
                int tryCount = 0;
                while (true)
                {
                    if (tryCount >= boardstate.BoardSize)
                    {
                        break;
                    }
                    int pick = RandomNumberGenerator.RndInt.Next(0, PointScoringStrategies.Count);
                    return_move = PointScoringStrategies[pick].ScoringMove;
                    if (WillMoveEndangerAIPlayer(return_move, boardstate) == false)
                    {
                        Console.WriteLine("-- Point Scoring strategy taken");
                        return return_move;
                    } else
                    {
                        tryCount++;
                    }
                }
            }

            if (PointBlockStrategies.Count > 0)
            {
                // point blocking - we need to gauge priority
                Tuple<List<Strategy>, List<Strategy>, List<Strategy>> getExtracted = PrepareBlockingStrategies(PointBlockStrategies, this.PieceColor);
                if (getExtracted.Item1.Count > 0)
                {
                    // Level 4 block
                    List<Strategy> l4_ = getExtracted.Item1;
                    List<Strategy> l2_ = getExtracted.Item2;
                    List<Strategy> l3_ = getExtracted.Item3;

                    if (l4_.Count > 0 )
                    {
                        int pick = RandomNumberGenerator.RndInt.Next(0, l4_.Count);
                        int r_pick = RandomNumberGenerator.RndInt.Next(0, l4_[pick].possible_moves.Count);
                        return_move = l4_[pick].possible_moves[r_pick];
                        Console.WriteLine("-- Point Block strategy ");
                        return return_move;
                        
                    }
                    if (l3_.Count > 0)
                    {
                        int pick = RandomNumberGenerator.RndInt.Next(0, l3_.Count);
                        int possible_move_pick = RandomNumberGenerator.RndInt.Next(0, l3_[pick].possible_moves.Count);
                        Console.WriteLine("-- Point Block strategy ");
                        return l3_[pick].possible_moves[possible_move_pick];
                    }
                    if (l2_.Count > 0)
                    {
                        int pick = RandomNumberGenerator.RndInt.Next(0, l2_.Count);
                        int possible_move_pick = RandomNumberGenerator.RndInt.Next(0, l2_[pick].possible_moves.Count);
                        // Console.WriteLine("-- Point Block strategy ");
                        return l2_[pick].possible_moves[possible_move_pick];
                    }
                }
            }
            if (WhiteSpaceBlockStrategies.Count > 0)
            {
                foreach(Strategy s in WhiteSpaceBlockStrategies)
                {
                    if (s.WhiteSpaceBlockPriority >= 4)
                    {
                        // Analyze the possible moves and make sure they don't endanger the player
                        int count = 0;
                        // Pick a possible move
                        while (true)
                        {
                            if (count >= s.possible_moves.Count + 5)
                            {
                                break;
                            }
                            
                             int pick = RandomNumberGenerator.RndInt.Next(0, s.possible_moves.Count);
                            Tuple<int, int> choice_move = s.possible_moves[pick];
                            
                            if (!WillMoveEndangerAIPlayer(choice_move, boardstate))
                            {
                               Console.WriteLine("White_Space defensive Block strategy chosen");
                                return choice_move;
                            } else
                            {
                                count++;
                            } 
                        }
                    }
                }
            }
            if (PointBuildingStrategies.Count > 0 )
            {
                // Here we need some nuance. Let's also check if there are any white space strategies
                if (this.EnableWhiteSpaceStrategy)
                {
                    if (WhiteSpaceStrategies.Count > 0)
                    {
                        /* We should make sure we check for any white space*/
                        List<Strategy> selectedAdvancedStrategies = new List<Strategy>();
                        foreach (Strategy white_space in WhiteSpaceStrategies)
                        {
                            // Prioritize white space strategies already in play
                            // Get those first
                            if (white_space.surrounding_pieces_diagonals.ContainsCountOf(this.PieceColor, boardstate) > 0 &&
                                white_space.surrounding_pieces_diagonals.ContainsCountOf(this.PieceColor, boardstate) < white_space.surrounding_pieces_diagonals.Count
                                && white_space.surrounding_pieces_diagonals.ContainsCountOf(Game.Empty, boardstate) > 0)
                            {
                                selectedAdvancedStrategies.Add(white_space);
                            }
                        }
                        // Let's check this list and make sure it's greater than zero.
                        
                        if (selectedAdvancedStrategies.Count > 0)
                        {
                            // Choose the highest
                            int highCount = 1; // Default
                            int targetIndex = -1;
                            int c = 0;
                            foreach(Strategy _s in selectedAdvancedStrategies)
                            {
                                
                                if (_s.surrounding_pieces_diagonals.ContainsCountOf(this.PieceColor, boardstate) > highCount)
                                {
                                    highCount = _s.surrounding_pieces_diagonals.ContainsCountOf(this.PieceColor, boardstate);
                                    targetIndex = c;
                                }
                                c++;
                            }
                            if (targetIndex >= 0)
                            {
                                Tuple<int, int> ws_return_move = selectedAdvancedStrategies[targetIndex].possible_moves_diagonal.GetRandom();
                                Console.WriteLine("White space advanced offensive strategy chosen");
                                return ws_return_move;
                                
                            }
                            
                        }
                    }
                }
                Tuple<List<Strategy>, List<Strategy>, List<Strategy>> getExtracted = PreparePointBuildingStrategies(PointBuildingStrategies, this.PieceColor);
                // Build points
                List<Strategy> l4_ = getExtracted.Item1;
                List<Strategy> l2_ = getExtracted.Item2;
                List<Strategy> l3_ = getExtracted.Item3;
                
                if (l4_.Count > 0)
                {
                    int countbreak = 0;
                    while (true)
                    {
                        if (countbreak >= boardstate.EmptySpaceCount)
                        {
                            break; // Break the loop
                        }
                        int pb_pick = RandomNumberGenerator.RndInt.Next(0, l4_.Count);
                        int pick2 = RandomNumberGenerator.RndInt.Next(0, l4_[pb_pick].possible_moves.Count);
                        
                        Tuple<int, int> moveChoice = l4_[pb_pick].possible_moves[pick2];
                        if (!WillMoveEndangerAIPlayer(moveChoice, boardstate) && !WillMoveSpoilWhiteSpaceStrategy(moveChoice, boardstate))
                        {
                            Console.WriteLine("-- Point Building strategy ");
                            return moveChoice;
                        } else
                        {
                            countbreak++;
                        }
                    }
                }          
                if (l3_.Count > 0)
                {
                    int countbreak = 0;
                    while (true)
                    {
                        if (countbreak >= boardstate.EmptySpaceCount)
                        {
                            break; // Break the loop
                        }
                        int pb_pick = RandomNumberGenerator.RndInt.Next(0, l3_.Count);
                        int pick2 = RandomNumberGenerator.RndInt.Next(0, l3_[pb_pick].possible_moves.Count);

                        Tuple<int, int> moveChoice = l3_[pb_pick].possible_moves[pick2];
                        if (!WillMoveEndangerAIPlayer(moveChoice, boardstate))
                        {
                            Console.WriteLine("-- Point Building strategy ");
                            return moveChoice;
                        }
                        else
                        {
                            countbreak++;
                        }
                    }
                }
                if (l2_.Count > 0)
                {
                    int countbreak = 0;
                    while (true)
                    {
                        if (countbreak >= boardstate.EmptySpaceCount)
                        {
                            break; // Break the loop
                        }
                        int pb_pick = RandomNumberGenerator.RndInt.Next(0, l2_.Count);
                        int pick2 = RandomNumberGenerator.RndInt.Next(0, l2_[pb_pick].possible_moves.Count);

                        Tuple<int, int> moveChoice = l2_[pb_pick].possible_moves[pick2];
                        if (!WillMoveEndangerAIPlayer(moveChoice, boardstate))
                        {
                            Console.WriteLine("-- Point Building strategy ");
                            return moveChoice;
                        }
                        else
                        {
                            countbreak++;
                        }
                    }
                }
            }
            // If at this point, no strategy has been found, it's probably toward the end of the game
            if (RemainingStrategies.Count > 0)
            {
                // We'll have to sort out the remaining strategies
                // The list should consist of strategies where there are possible moves
                // How about we randomly pick a strategy and see if it contains a possible move that will not endanger the AI player
                int count = 0;
                while (true)
                {
                    if (count > boardstate.Space_Tracker.Count)
                    {
                        break; // break loop, force the AI to play that move
                    }

                    int strat_pick = RandomNumberGenerator.RndInt.Next(0, RemainingStrategies.Count); // Random number to pick a strategy
                    Strategy chosenStrategy = RemainingStrategies[strat_pick]; // Randomly pick a strategy
                    foreach(Tuple<int, int> pm in chosenStrategy.possible_moves)
                    {
                        if (!WillMoveEndangerAIPlayer(pm, boardstate))
                        {
                            return pm;
                        }
                    }
                    count++;
                }

                // Pick a random empty spot
                Tuple<int, int> final_rnd_spot = boardstate.Space_Tracker.GetRandom();
                return final_rnd_spot;
            }
            return return_move;

        }
        
        private Tuple<List<Strategy>, List<Strategy>, List<Strategy>> PreparePointBuildingStrategies(List<Strategy> master_scorebuilding_list, int builder)
        {
            /*
             Sort out the score builidng strategies */
            List<Strategy> p4 = new List<Strategy>();
            List<Strategy> p3 = new List<Strategy>();
            List<Strategy> p2 = new List<Strategy>();

            foreach (Strategy s in master_scorebuilding_list)
            {
                if (s.ScoreBuildingOpportunity == true)
                {
                    if (s.ScoreBuilder == builder)
                    {
                        switch (s.ScoreBuildingPriority)
                        {
                            case 4:
                                p4.Add(s);
                                break;
                            case 3:
                                p3.Add(s);
                                break;
                            case 2:
                                p2.Add(s);
                                break;
                        }
                    }
                }
            }
            return new Tuple<List<Strategy>, List<Strategy>, List<Strategy>>(p4, p3, p2);

        }
        private List<Strategy> PrepareWhiteSpaceDefensiveBlockStrategies(List<Strategy> master)
        {
            List<Strategy> r_List = new List<Strategy>();

            foreach(Strategy s in master)
            {
                if (s.WhiteSpaceBlockOpportunity == true && s.WhiteSpaceBlockPriority >= 2)
                {
                    r_List.Add(s);
                }
            }
            return r_List;
        }
        
        private Tuple<List<Strategy>, List<Strategy>, List<Strategy>> PrepareBlockingStrategies (List<Strategy> master_block_list, int defender)
        {
            // This will extract blocking strategies by priority level
            List<Strategy> p4_ = new List<Strategy>();
            List<Strategy> p3_ = new List<Strategy>();
            List<Strategy> p2_ = new List<Strategy>();

            
            foreach (Strategy s in master_block_list)
            {
                if (s.BlockPriority == 4 && s.BlockDefender == defender)
                {
                    p4_.Add(s);
                } else if (s.BlockPriority == 3 && s.BlockDefender == defender)
                {
                    p3_.Add(s);
                } else if (s.BlockPriority == 2 && s.BlockDefender == defender)
                {
                    p2_.Add(s);
                }
            }
            Tuple<List<Strategy>, List<Strategy>, List<Strategy>> returnList = new Tuple<List<Strategy>, List<Strategy>, List<Strategy>>(p4_, p3_, p2_);
            return returnList;
        }
        private List<Strategy> GetPointBlockStrategies (List<Strategy> source)
        {
            List<Strategy> returnList = new List<Strategy>();
            foreach(Strategy s in source)
            {
                if (s.BlockPriority >= 2 && s.BlockOpportunity == true && s.BlockDefender == this.PieceColor)
                {
                    returnList.Add(s);
                }
                
            }
            return returnList;
        }
        private List<Strategy> GetAIPointScoringStrategies(List<Strategy> source)
        {
            List<Strategy> returnList = new List<Strategy>();
            foreach (Strategy S in source)
            {
                if (S.ScoringPlayer == this.PieceColor)
                {
                    returnList.Add(S);
                }
            }
            return returnList;
        }
        public bool WillMoveEndangerAIPlayer(Tuple<int, int> move, GameBoard boardrefence)
        {
            /* This funtion returns true if:
             the AI places a piece in a spot in an opponent's surround position or:
             the AI places a piece in a spot where the opponent's next piece placement causes the AI to be scored upon*/
            List<Tuple<int, int>> s_pieces = boardrefence.GetSurroundingPieces(move); // Get the pieces surrounding the place the AI wants to make
            int opp_piece = GameBoard.GetOppColor(PieceColor); // Gets your opponent's color

            // The AI's move should be a whitespace
            if (boardrefence.Grid[move.Item1, move.Item2] != Game.Empty)
            {
                throw new Exception("Unable to evaluate move, as location isn't empty: AI can only make a move on an empty space.");
            }
            // Must contain one empty space
            if (s_pieces.ContainsCountOf(Game.Empty, boardrefence) == 1)
            {
                if (s_pieces.ContainsCountOf(opp_piece, boardrefence) == s_pieces.Count - 1)
                {
                    return true;
                }
            }

            if (s_pieces.ContainsCountOf(opp_piece, boardrefence) == s_pieces.Count)
            {
                // This is essentially placing a piece right in the middle of the opponents capturing position
                return true;
            }
            return false;
        }
        public bool WillMoveSpoilWhiteSpaceStrategy(Tuple<int, int> move, GameBoard boardreference)
        {
            // This will evaluate if the next move will cancel out a white space hole / trap. The AI shouldn't spoil its own white spaces
            List<Tuple<int, int>> s_pieces = boardreference.GetSurroundingPieces(move);
            if (s_pieces.ContainsCountOf(this.PieceColor, boardreference) == s_pieces.Count)
            {
                return true;
            }

            return false;
        }
        public List<Strategy> GetPointBuildingStrategies(List<Strategy> masterList)
        {
            List<Strategy> returnList = new List<Strategy>();
            foreach(Strategy s in masterList)
            {
                if (s.ScoreBuilder == this.PieceColor)
                {
                    returnList.Add(s);
                }
            }
            return returnList;
        }
        private List<Strategy> GetRemainingStrategies(List<Strategy> masterList)
        {
            // Return all strategies that have possible move count > 0
            List<Strategy> returnList = new List<Strategy>(); // for return
            foreach(Strategy s in masterList)
            {
                // Go through each possible move
                if (s.possible_moves.Count > 0)
                {
                    returnList.Add(s);
                }
            }
            return returnList;
        }
        private Tuple<int, int> GetBackUpMove(GameBoard boardstate)
        {
            // This function will force the AI to make a move. This usually needs to be called toward the end of the game. The AI strategy needs to prioritize
            // certain moves, and should avoid playing a move that will cause it to score when there are other moves available
            return new Tuple<int, int>(-1, -1);
        }
        private List<Strategy> FilterWhiteSpaceBuildingStrategies(List<Strategy> masterList, GameBoard boardstate)
        {
            List<Strategy> returnList = new List<Strategy>();

            // We're going to search for potential white space strategies. We want any strategy where the center is empty or our in piece. We also want any
            // strategy has diagonals that have in pieces or are all empty
            int OppColor = GameBoard.GetOppColor(this.PieceColor);
            foreach (Strategy s in masterList)
            {
                if (s.PieceColorAtCenter == Game.Empty || s.PieceColorAtCenter == this.PieceColor)
                {
                    if (s.surrounding_pieces_diagonals.ContainsCountOf(OppColor, boardstate) > 0 || s.surrounding_pieces_diagonals.ContainsCountOf(Game.Empty, boardstate) == 0)
                    {
                        continue;
                    } else
                    {
                        returnList.Add(s);
                    }
                }
            }
            return returnList;
        }
    }
    public class Strategy
    {
        public Tuple<int, int> center; // The target piece. Pieces surround it.
        public int PieceColorAtCenter;
        public List<Tuple<int, int>> surrounding_pieces = new List<Tuple<int, int>>(); // can be either 4, 3 or 2 in length. They surround the center
        public List<Tuple<int, int>> surrounding_pieces_diagonals = new List<Tuple<int, int>>();
        public List<Tuple<int, int>> possible_moves_diagonal = new List<Tuple<int, int>>();
        public List<Tuple<int, int>> possible_moves = new List<Tuple<int, int>>(); // an array of blank spaces where AI may place a tile
        public int ScoringPlayer; // The player that will score the point (either 1, or 2 || or 0 for no player)

        public Tuple<int, int> ScoringMove; // The (row, col) location of the move that will score a point
        public int ScoreBuildingPriority = 0;
        public bool ScoreBuildingOpportunity = false;
        public int ScoreBuilder = 0; // Should be 1, 2 or 0 for no score building
        public string Tag = "";

        public bool BlockOpportunity = false;
        public int BlockPriority = 0;
        public bool WhiteSpaceBlockOpportunity = false;
        public int WhiteSpaceBlockPriority = 0;

        public int BlockDefender = 0;
        private int in_piece = 0;
        private int out_piece = 0;

        // Strategy type ? Block, Win, Build
        public Strategy(Tuple<int, int> centerpoint, GameBoard boardstate, int _InPiece)
        {
            // Initialize / calculate the properties
            center = centerpoint;
            PieceColorAtCenter = boardstate.Grid[center.Item1, center.Item2];
            in_piece = _InPiece; // This should be the AI's piece
            out_piece = GameBoard.GetOppColor(in_piece);

            surrounding_pieces = boardstate.GetSurroundingPieces(center);
            surrounding_pieces_diagonals = boardstate.GetSurroundingPieces(center, true);

            possible_moves = GetPossibleMoves(boardstate);
            possible_moves_diagonal = GetPossibleMoves(boardstate, true);
            DetermineScoringMoveAndPlayer(boardstate);
            DetermineScoreBuildingChance(boardstate); // This completes the score building properties
            DetermineBlockingStatus(boardstate);
            WhiteSpaceBlockPriority = DetermineWhiteSpaceDefensiveBlockStrategy(boardstate);
            
        }
        private int DetermineWhiteSpaceDefensiveBlockStrategy(GameBoard boardstate)
        {
            //The center space needs to be white
            int r_value = 0;
            if (this.PieceColorAtCenter == Game.Empty)
            {
                if (surrounding_pieces.ContainsCountOf(Game.Empty, boardstate) > 0 && surrounding_pieces.ContainsCountOf(in_piece, boardstate) == 0)
                {
                    if (surrounding_pieces.ContainsCountOf(out_piece, boardstate) > 0)
                    {
                      r_value = 6 - this.possible_moves.Count - this.possible_moves.ContainsCountOf(Game.Empty, boardstate);
                        WhiteSpaceBlockOpportunity = true;
                    }
                }
            }
            
            return r_value;
        }
        private List<Tuple<int, int>> GetPossibleMoves(GameBoard boardstate, bool diagonals=false)
        {
            // Determines possible moves surrounding the tile at center.
            List<Tuple<int, int>> returnValue = new List<Tuple<int, int>>();
            if (!diagonals)
            {
                foreach (Tuple<int, int> inList in this.surrounding_pieces)
                {
                    if (boardstate.Grid[inList.Item1, inList.Item2] == Game.Empty)
                    {
                        // if an element in surrounding_pieces(inList) is empty, then it's a possible move; add it to the list
                        returnValue.Add(inList);
                    }
                }
            } else
            {
                foreach (Tuple<int, int> inList in this.surrounding_pieces_diagonals)
                {
                    if (boardstate.Grid[inList.Item1, inList.Item2] == Game.Empty)
                    {
                        // if an element in surrounding_pieces_diagonals(inList) is empty, then it's a possible move; add it to the list
                        returnValue.Add(inList);
                    }
                }
            }

            return returnValue;
        }
        private void DetermineScoringMoveAndPlayer(GameBoard boardstate)
        {
            // This will populate the properties: ScoringPlayer and ScoringMove
            // We'll use the already populated surrounding_pieces property

            // Make sure we are working on a non-empty piece
            if (boardstate.Grid[center.Item1, center.Item2] != Game.Empty)
            {
                if (this.surrounding_pieces.ContainsCountOf(GameBoard.GetOppColor(boardstate.Grid[center.Item1, center.Item2]), boardstate) == this.surrounding_pieces.Count - 1)
                {
                    // This should equate to a single empty space left for the opposing piece of piece atCenter to score a point
                    List<Tuple<int, int>> output = this.surrounding_pieces.GetTuplesContaining(Game.Empty, boardstate);
                    if (output.Count == 1)
                    {
                        ScoringPlayer = GameBoard.GetOppColor(boardstate.Grid[center.Item1, center.Item2]);
                        ScoringMove = output[0];
                    } else
                    {
                       // throw new InvalidOperationException("Error in Determing ScoringMoveAndPlayer");
                    }
                    
                }
            } else
            {
                ScoringPlayer = 0;
                ScoringMove = new Tuple<int, int>(-1, -1);
            }
        }
        public static List<Strategy> GetAllStrategies(GameBoard boardstate, int homepiece)
        {
            // Function will return a list of strategies given the current boardstate
            List<Strategy> returnList = new List<Strategy>();
            for (int rows = 0; rows < boardstate.Grid.GetLength(0); rows++)
            {
                for (int cols = 0; cols < boardstate.Grid.GetLength(1); cols++)
                {
                    Strategy sStrat = new Strategy(new Tuple<int, int>(rows, cols), boardstate, homepiece);

                    // Check if the possible strategies for sStrat > 0, if so, add it to the master returnList
                    returnList.Add(sStrat);
                }
            }
            return returnList;
        }
        private void DetermineScoreBuildingChance(GameBoard boardstate)
        {
            /* This determines the score building status of this particular strategy based on piece at [y,x] */
            
            int OppColor = GameBoard.GetOppColor(boardstate.Grid[center.Item1, center.Item2]); // If it's 1, return 2, if 2, return 1.
            int ColorAtCenter = boardstate.Grid[center.Item1, center.Item2];

            if (ColorAtCenter == Game.Empty)
            {
                // This is not a point scoring strategy
                this.ScoreBuilder = 0;
                this.ScoreBuildingOpportunity = false;
                this.ScoreBuildingPriority = 0;
                this.Tag = "Not a score-building chance;";
                return;
            }
            // Let's indicate out a spoiled point-scoring strategies
           // Scoring chances are eliminated when one of the surrounding pieces is the same as the pieceatcenter
           if (surrounding_pieces.ContainsCountOf(ColorAtCenter, boardstate) > 0)
           {
                // This is not a point scoring strategy
                this.ScoreBuilder = 0;
                this.ScoreBuildingOpportunity = false;
                this.ScoreBuildingPriority = 0;
                this.Tag = "Not a score-building chance as it has been blocked;";
                return;
           }

           // A score building opporunity exists if there are >= 0 white spaces in the surrounding pieces but one less than the surrounding_piece count
           if (surrounding_pieces.ContainsCountOf(Game.Empty, boardstate) > 0 && surrounding_pieces.ContainsCountOf(OppColor, boardstate) < surrounding_pieces.Count)
           {
                // This is not a point scoring strategy
                this.ScoreBuilder = OppColor;
                this.ScoreBuildingOpportunity = true;
                this.ScoreBuildingPriority = 6 - surrounding_pieces.ContainsCountOf(Game.Empty, boardstate); 
                
                this.Tag = ("Player " + OppColor + " has a score building chance");
                return;
            }
        
        }
        public void DetermineBlockingStatus(GameBoard boardstate)
        {
            // This determines whether the piece at center (one's own piece is in the process of being surrounded, and determines a move that will block scoring of the opponent
            /*
             * 4- Top priority one move left until opponent scores
             * 3- Two spots left
             * 2 - Three spots left
             * 1 - Four spots left
             */

            // Get the color at center
            int Agressor = 0; // Keeps track of the person who is trying to score
            int targetColorAtCenter = boardstate.Grid[center.Item1, center.Item2];
            if (targetColorAtCenter != Game.Empty)
            {
                Agressor = GameBoard.GetOppColor(targetColorAtCenter);
            } else
            {
                // Write it off
                BlockOpportunity = false;
                BlockPriority = 0;
                return;
            }
            
            // Evaluate the surrounding pieces
            if (surrounding_pieces.ContainsCountOf(targetColorAtCenter, boardstate) > 0 )
            {
                // Abort as the defender has blocked the aggressor
                BlockOpportunity = false;
                BlockPriority = 0;
                return;
            } else
            {
                if (surrounding_pieces.ContainsCountOf(Game.Empty, boardstate) > 0)
                {
                    // surrounding pieces contains empty space and not of the defender's pieces
                    BlockOpportunity = true;
                    this.BlockDefender = targetColorAtCenter; // The defender clearly is the piece at center
                    this.BlockPriority = 5 - surrounding_pieces.ContainsCountOf(Game.Empty, boardstate);
                }
            }
        }
    }
    public static class ExtensionMethods
    {
        public static int ContainsCountOf(this List<Tuple<int, int>> ob, int piece, GameBoard boardstate)
        {
            int int_count = 0;
            foreach(Tuple<int, int> T in ob)
            {
                if (boardstate.Grid[T.Item1, T.Item2] == piece)
                {
                    int_count++;
                }
            }
            return int_count;
        }
        public static List<Tuple<int, int>> GetTuplesContaining(this List<Tuple<int, int>> ob, int piece, GameBoard boardstate)
        {
            // This will return a list of [row,col] where piece is found on the gameboard
            List<Tuple<int, int>> returnList = new List<Tuple<int, int>>();
            foreach(Tuple<int, int> T in ob)
            {
                if (boardstate.Grid[T.Item1, T.Item2] == piece)
                {
                    returnList.Add(new Tuple<int, int>(T.Item1, T.Item2));
                }
            }
            return returnList;
        }
        public static List<Tuple<int, int>> RemoveOutOfRangeElements(this List<Tuple<int, int>> ob, GameBoard boardstate)
        {
            // This function will remove any tuples that are out of bounds in the boardstate.grid matrix
            List<Tuple<int, int>> returnList = new List<Tuple<int, int>>();
            foreach (Tuple<int, int> el in ob)
            {
                if (el.Item1 >= 0 && el.Item1 < boardstate.Grid.GetLength(0))
                {
                    if (el.Item2 >= 0 && el.Item2 < boardstate.Grid.GetLength(1))
                    {
                        returnList.Add(el);
                    }
                }
            }
            return returnList;
        }
        
        public static bool IsInBounds(this Tuple<int, int> ob, int[,] m)
        {
            // Returns true if the tuple is within the bounds of the maxtrix m

            if (ob.Item1 >= 0 && ob.Item1 <= m.GetLength(0))
            {
                if (ob.Item2 >= 0 && ob.Item2 <= m.GetLength(1))
                {
                    return true;
                }
            }
            return false;
        }
        public static T GetRandom<T>(this T[] ob)
        {
            if (ob.Length > 0)
            {
                int Pick = RandomNumberGenerator.RndInt.Next(0, ob.Length);
                return ob[Pick];
            } else
            {
                return default(T);
            }
        }
        public static T GetRandom<T>(this List<T> ob)
        {
            if (ob.Count > 0)
            {
                int Pick = RandomNumberGenerator.RndInt.Next(0, ob.Count);
                return ob[Pick];
            }
            else
            {
                return default(T);
            }
        }
        public static int[,] Multiply(this int[,] ob, int multiplier)
        {
            int[,] r_result = ob;
            for (int r = 0; r < ob.GetLength(0); r++)
            {
                for (int c = 0; c < ob.GetLength(1); c++)
                {
                    r_result[r, c] = ob[r, c] * multiplier;
                }
            }
            return r_result;
        }
        public static void Print(this int[,] ob)
        {
            for (int r = 0; r < ob.GetLength(0); r++)
            {
                Console.WriteLine("");
                for (int col = 0; col < ob.GetLength(1); col++)
                {
                    Console.Write(ob[r, col]);
                }
            }
            
            
        }
    }
}
