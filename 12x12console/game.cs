﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace _12x12console
{
    public enum PlacementErrors {TileOccupied = -1, OutOfBounds = -2}
    public class Game
    {
        public enum GameMode {PlayerVAI = 0, AIvAI = 1}
        
        public const int Empty = 0;
        public const int Blue = 1;
        public const int Red = 2;

        public GameBoard Board;
        public Player Player1;
        public Player Player2;

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
       
        public Game(Tuple<int, int> size, GameMode mode)
        {
            // Initialize the game. Set the colors, players and create a gameboard
            Board = new GameBoard(size); // Initialize
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

        public int MakeMove(int P, Tuple<int, int>loc)
        {
            int p_color;
            if (P == 1)
            {
                p_color = Blue;
            } else
            {
                p_color = Red;
            }
          
            // Make a move on a tile
            if (Board.Grid[loc.Item1, loc.Item2] == Empty)
            {
                Board.Grid[loc.Item1, loc.Item2] = p_color;
                SweepForScore();
                return 0;
            }
            return (int) PlacementErrors.TileOccupied; // Move is invalid
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
                Console.WriteLine("Not implemented yet");
                
            }
        }
        public bool IsComplete()
        {
            // Returns true if all spaces are occupied with tiles
            for (int rows = 0; rows < this.Board.Grid.GetLength(0); rows++)
            {
                for (int cols = 0; cols < this.Board.Grid.GetLength(1); cols++)
                {
                    if (this.Board.Grid[rows, cols] == Empty)
                    {
                        return false;
                    }
                }
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
                    Tuple<int, int> result = IsPieceCaptured(new Tuple<int, int>(rows, cols));
                    _Player1_Score += result.Item1;
                    _Player2_Score += result.Item2;
                }
            }
            // Print the score in the console
            Console.WriteLine("Score: Blue: {0}", _Player1_Score);
            Console.WriteLine("Score: Red: {0}", _Player2_Score);
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
        
        private Tuple<int, int> IsPieceCaptured(Tuple<int, int> atCenter)
        {
            // Should return score value <blue, red>
            // What is the color of piece atCenter?
            if (Board.Grid[atCenter.Item1, atCenter.Item2] == Game.Empty)
            {
                return new Tuple<int, int>(0, 0);
            }
            int pColorAtCenter = this.Board.Grid[atCenter.Item1, atCenter.Item2];
            int OpposingPiece = GetOppColor(pColorAtCenter);
            List<Tuple<int, int>> s_pieces = Board.GetSurroundingPieces(atCenter);
            int s_count = 0;
            foreach(Tuple<int, int> spot in s_pieces)
            {
                if (Board.Grid[spot.Item1, spot.Item2] == OpposingPiece)
                {
                    s_count++;
                }
            }
            if (s_count == s_pieces.Count)
            {
                if (OpposingPiece == Game.Red)
                {
                    return new Tuple<int, int>(0, 1);
                } else if (OpposingPiece == Game.Blue)
                {
                    return new Tuple<int, int>(1, 0);
                }
                
            }

            return new Tuple<int, int>(0, 0);
        }
        
    }

    public class GameBoard
    {
        public int[,] Grid;
        public List<Tuple<int, int>> AIMoveCache = new List<Tuple<int, int>>();
        public GameBoard(Tuple<int, int> size)
        {
            // Initialize a new maxtrix
            Grid = new int[size.Item1, size.Item2];
            
        }
        
        public void Print()
        {
            // Print out a helper guide
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Green;
            for (int colCount = 0; colCount < Grid.GetLength(1); colCount++)
            {
                Console.Write(colCount);
            }
            // New line
            Console.WriteLine("");
            Console.ResetColor();
            // Prints the game board to the console
            if (Grid != null)
            {
                for (int numRows = 0; numRows < Grid.GetLength(0); numRows++)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(numRows + " "); // Row guide
                    Console.ResetColor();
                    for (int numCols = 0; numCols < Grid.GetLength(1); numCols++)
                    {
                        if (Grid[numRows, numCols] == Game.Blue)
                        {
                            if (IsPieceCaptured(new Tuple<int, int>(numRows, numCols)))
                            {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                            }
                        } else if (Grid[numRows, numCols] == Game.Red)
                        {
                            if (IsPieceCaptured(new Tuple<int, int>(numRows, numCols)))
                            {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                            }
                        } else
                        {
                            Console.ResetColor();
                        }
                        Console.Write(Grid[numRows, numCols]);
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
        private bool IsPieceCaptured(Tuple<int, int> atCenter)
        {
            // Should return true if the piece atCenter is captured
            // What is the color of piece atCenter?
            
            int pColorAtCenter = this.Grid[atCenter.Item1, atCenter.Item2];
            int OpposingPiece = GetOppColor(pColorAtCenter);
            List<Tuple<int, int>> s_pieces = GetSurroundingPieces(atCenter);
            int s_count = 0;
            foreach (Tuple<int, int> spot in s_pieces)
            {
                if (this.Grid[spot.Item1, spot.Item2] == OpposingPiece)
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
        public AIPlayer()
        {
            
        }
        
        public Tuple<int, int> DoAIMove(GameBoard boardstate)
        {
            /* AI Logic! */
            // Executes an AI Move based on the current game board state. 
            Tuple<int, int> return_move = new Tuple<int, int>(-1,-1); // Default value for an invalid move
            //***************************************
            List<Strategy> masterStrategyList = Strategy.GetAllStrategies(boardstate); // Get a list of all the master strategies and plays
            //***************************************

            // First let's find all winning strategies, which is defined as the next move shall score a point for the  AI
            // 1.
            List<Strategy> PointScoringStrategies = GetAIPointScoringStrategies(masterStrategyList);

            // Get the point-blocking strategies - prevent the opponent from scoring a point
            List<Strategy> PointBlockStrategies = GetPointBlockStrategies(masterStrategyList);

            // Get block opponent white space strategy

            // Get White space move opportunities
            List<Strategy> WhiteSpaceStrategies = new List<Strategy>();

            // Get all strategies where there is a potential to build a score
            List<Strategy> PointBuildingStrategies = GetPointBuildingStrategies(masterStrategyList);

            

            // Next we'll
            Console.WriteLine("There are {0} point scoring strategies", PointScoringStrategies.Count);
            return return_move;

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
            if (boardrefence.Grid[move.Item1, move.Item1] != Game.Empty)
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
    }
    public class Strategy
    {
        public Tuple<int, int> center; // The target piece. Pieces surround it.
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

        public int BlockDefender = 0;

        // Strategy type ? Block, Win, Build
        public Strategy(Tuple<int, int> centerpoint, GameBoard boardstate)
        {
            // Initialize / calculate the properties
            center = centerpoint; 
            surrounding_pieces = boardstate.GetSurroundingPieces(center);
            surrounding_pieces_diagonals = boardstate.GetSurroundingPieces(center, true);

            possible_moves = GetPossibleMoves(boardstate);
            possible_moves_diagonal = GetPossibleMoves(boardstate, true);
            DetermineScoringMoveAndPlayer(boardstate);
            DetermineScoreBuildingChance(boardstate); // This completes the score building properties
            DetermineBlockingStatus(boardstate);
            
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
                        throw new InvalidOperationException("Error in Determing ScoringMoveAndPlayer");
                    }
                    
                }
            } else
            {
                ScoringPlayer = 0;
                ScoringMove = new Tuple<int, int>(-1, -1);
            }
        }
        public static List<Strategy> GetAllStrategies(GameBoard boardstate)
        {
            // Function will return a list of strategies given the current boardstate


            List<Strategy> returnList = new List<Strategy>();

           

            for (int rows = 0; rows < boardstate.Grid.GetLength(0); rows++)
            {
                for (int cols = 0; cols < boardstate.Grid.GetLength(1); cols++)
                {
                    Strategy sStrat = new Strategy(new Tuple<int, int>(rows, cols), boardstate);

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
            if (surrounding_pieces.ContainsCountOf(boardstate.Grid[center.Item1, center.Item2], boardstate) > 0 )
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
                    this.BlockDefender = boardstate.Grid[center.Item1, center.Item2]; // The defender clearly is the piece at center
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
    }
}
