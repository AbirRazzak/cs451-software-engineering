using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Timers;

namespace Client
{
    public class CheckersBoard
    {
        public UniformGrid CheckersGrid { get; set; }
        public Piece[] BlackPieces { get; set; }
        public Piece[] RedPieces { get; set; }
        public Piece SelectedPiece { get; set; }
        //Grid spaces that are highlighted when a piece is selected
        private List<StackPanel> Highlighted;
        private Dictionary<Point, List<StackPanel>> CaptureMoves;

        private string CurrentTurn;
        private string ClientColor;
        private string ServerLink = "http://192.168.0.182:55555/";
        private string LatestMove;
        private Timer MoveTimer;
        private string GameId;
        private int RedPiecesLeft;
        private int BlackPiecesLeft;

        public CheckersBoard(UniformGrid checkersGrid, string color, string code)
        {
            GameId = code;
            CheckersGrid = checkersGrid;
            Highlighted = new List<StackPanel>();
            CaptureMoves = new Dictionary<Point, List<StackPanel>>();
            ClientColor = color;
            CurrentTurn = "Black";
            MakeBoard();
            if(color == "Red")
                StartRedTurn();
        }

        private void StartRedTurn()
        {
            MoveTimer = new Timer(5000);
            MoveTimer.Elapsed += GetStartingMove;
            MoveTimer.AutoReset = true;
            MoveTimer.Enabled = true;
        }

        private void GetStartingMove(Object Source, ElapsedEventArgs e)
        {

            WebRequest wr = WebRequest.Create(ServerLink + "/getallmoves/" + GameId);
            wr.Method = "GET";
            WebResponse resp = wr.GetResponse();
            string move = LatestMove;
            using (Stream dataStream = resp.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                move = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
            }
            if (move.Length > 0) {
                MoveTimer.Stop();
                MoveTimer.Dispose();
                MakeOpponentMove(move);
            }
                
        }
        /* Initalize the board by adding panels to the grid
         * Panels will hold the buttons that represent the pieces and the move locations
         */
        private void MakeBoard()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    StackPanel stackPanel = new StackPanel();
                    if (r % 2 == 0)
                    {
                        if (c % 2 == 1)
                            stackPanel.Background = Brushes.Black;
                    }
                    else
                    {
                        if (c % 2 == 0)
                            stackPanel.Background = Brushes.Black;
                    }
                    Grid.SetRow(stackPanel, r);
                    Grid.SetColumn(stackPanel, c);
                    CheckersGrid.Children.Add(stackPanel);
                }
            }
            MakePieces();
        }

        /* Called when a button on grid is clicked (When a piece is selected)
         * sender - the object calling the method
         * e - arguments passed with the call which should be none in this case
         */
        public void SelectPiece(Object sender, RoutedEventArgs e)
        {

            Button piece = (Button)sender;
            CaptureMoves = new Dictionary<Point, List<StackPanel>>();
            //Remove previously highlighted spaces
            RemoveHighlight();

            //Get possible move locations for the slected piece
            List<StackPanel> moves = new List<StackPanel>();
            SelectedPiece = GetPiece((Button)sender);
            if (CurrentTurn == SelectedPiece.Color && CurrentTurn == ClientColor) 
                moves = GetPossibleMoves();

            //Add a button to highlight the move location if there isn't a piece there already and bind it to Move function
            foreach (StackPanel stackPanel in moves)
            {
                Button possibleMove = new Button();
                possibleMove.Click += new RoutedEventHandler(MoveClick);
                possibleMove.Height = 60;
                possibleMove.Width = 60;
                possibleMove.Background = Brushes.Yellow;
                stackPanel.Children.Add(possibleMove);
                Highlighted.Add(stackPanel);
            }
        }

        /* Gets all possible moves for a piece and return the locations for those moves*/
        public List<StackPanel> GetPossibleMoves()
        {
            List<StackPanel> possibleMoves;
            int rowMovement;

            if (SelectedPiece.Color == "Black")
                rowMovement = -1;
            else
                rowMovement = 1;

            possibleMoves = MoveHelper(rowMovement, SelectedPiece.CurrentRow, SelectedPiece.CurrentCol);
            if (SelectedPiece.IsKing)
            {
                if (SelectedPiece.Color == "Black")
                    possibleMoves.AddRange(MoveHelper(1, SelectedPiece.CurrentRow, SelectedPiece.CurrentCol));
                else
                    possibleMoves.AddRange(MoveHelper(-1, SelectedPiece.CurrentRow, SelectedPiece.CurrentCol));
            }

            return possibleMoves;
        }

        /* Helper function for generation move list
         * Check diagonal block to determine if piece can move there and calls helper function to find jump spots
         */
        private List<StackPanel> MoveHelper(int increment, int row, int col)
        {
            List<StackPanel> possibleMoves = new List<StackPanel>();
            int newRow = row + increment;
            if (newRow >= 0 && newRow <= 7)
            {
                if (col - 1 >= 0)
                {
                    StackPanel leftPanel = (StackPanel)GetGridElement(newRow, col - 1);
                    if (leftPanel.Children.Count == 0)
                        possibleMoves.Add(leftPanel);
                }
                if (col + 1 <= 7)
                {
                    StackPanel rightPanel = (StackPanel)GetGridElement(newRow, col + 1);
                    if (rightPanel.Children.Count == 0)
                        possibleMoves.Add(rightPanel);
                }
                possibleMoves.AddRange(GetJumps(increment, row, col));
            }
            return possibleMoves;
        }
        /* Get all the locations a piece can jump to */
        private List<StackPanel> GetJumps(int rowIncrement, int row, int col)
        {
            List<StackPanel> possibleMoves = new List<StackPanel>();
            if (col - 1 >= 0 && row + rowIncrement >= 0 && row + rowIncrement <= 7)
            {
                StackPanel leftPanel = (StackPanel)GetGridElement(row + rowIncrement, col - 1);
                possibleMoves.AddRange(GetJumpsHelper(leftPanel, rowIncrement * 2, -2, row, col));
            }
            if (col + 1 <= 7 && row + rowIncrement >= 0 && row + rowIncrement <= 7)
            {
                StackPanel rightPanel = (StackPanel)GetGridElement(row + rowIncrement, col + 1);
                possibleMoves.AddRange(GetJumpsHelper(rightPanel, rowIncrement * 2, 2, row, col));
            }
            return possibleMoves;
        }

        /* Helper function for GetJumps that recursively calculates jumps and multiple jumps */
        private List<StackPanel> GetJumpsHelper(StackPanel panel, int rowIncrement, int columnIncrement, int row, int col)
        {
            List<StackPanel> possibleMoves = new List<StackPanel>();
            if (panel.Children.Count != 0)
            {
                //Check is piece is an enemy piece to proceed forward and determine if they can be jumped over
                Button howCanHeJump = (Button)panel.Children[0];
                Piece jumpable = GetPiece(howCanHeJump);
                if (jumpable.Color != SelectedPiece.Color)
                {
                    //Get all the jump locations
                    List<StackPanel> moves = CalculateJump(rowIncrement, columnIncrement, row, col);
                    List<StackPanel> capturable = new List<StackPanel>();
                    foreach (StackPanel move in moves)
                    {
                        Point np = new Point(Grid.GetRow(move), Grid.GetColumn(move));

                        //Add all capturable pieces to the Dictionary to allow capturing multiple enemy pieces in one move
                        List<StackPanel> previous = new List<StackPanel>();
                        foreach (KeyValuePair<Point, List<StackPanel>> entry in CaptureMoves)
                        {
                            if (entry.Key.Equals(np))
                            {
                                previous = entry.Value;
                            }
                        }
                        if (previous.Count != 0)
                        {
                            previous.Add(panel);
                        }
                    }
                    //Add move entry to Dictionary
                    if (moves.Count > 0)
                    {
                        StackPanel testPanel = moves[0];

                        Point testPoint = new Point(Grid.GetRow(testPanel), Grid.GetColumn(testPanel));
                        capturable.Add(panel);
                        CaptureMoves.Add(testPoint, capturable);
                    }
                    possibleMoves.AddRange(moves);
                }
            }
            return possibleMoves;
        }

        /* Find if spot next to enemy pieces are empty to jump over piece */
        private List<StackPanel> CalculateJump(int rowIncrement, int colIncrement, int row, int col)
        {

            List<StackPanel> moves = new List<StackPanel>();
            if (col + colIncrement >= 0 && col + colIncrement <= 7 && row + rowIncrement >= 0 && row + rowIncrement <= 7)
            {
                StackPanel panel = (StackPanel)GetGridElement(row + rowIncrement, col + colIncrement);
                if (panel.Children.Count == 0)
                {
                    moves.Add(panel);
                }
                moves.AddRange(GetJumps(rowIncrement / 2, row + rowIncrement, col + colIncrement));
            }
            return moves;
        }

        /* Unhighlight all spots by removing the button in the spot and reset Highlighted list */
        private void RemoveHighlight()
        {
            if (Highlighted == null)
                return;
            foreach (StackPanel stackPanel in Highlighted)
            {
                stackPanel.Children.Clear();
            }
            Highlighted = new List<StackPanel>();
        }

        /* Logic for when a highlighted square is selected */
        public void MoveClick(Object sender, RoutedEventArgs e)
        {
            //Remove the selected peice from the original location
            Button selected = SelectedPiece.PieceButton;
            StackPanel selectedPanel = (StackPanel)selected.Parent;

            //Retrieve new row and column for location to move to
            Button newLocation = (Button)sender;
            StackPanel newLocationPanel = (StackPanel)newLocation.Parent;
            int newRow = Grid.GetRow(newLocationPanel);
            int newCol = Grid.GetColumn(newLocationPanel);

            SendMove(SelectedPiece.CurrentRow, SelectedPiece.CurrentCol, newRow, newCol);

            Move(selectedPanel, newLocationPanel);
        }
        private void Move(StackPanel original, StackPanel newLocation) {
            Button selected = (Button)original.Children[0];
            Piece piece = GetPiece(selected);
            original.Children.Clear();

            int newRow = Grid.GetRow(newLocation);
            int newCol = Grid.GetColumn(newLocation);

            List<StackPanel> captured = new List<StackPanel>();
            Point newLoc = new Point(newRow, newCol);
            foreach (KeyValuePair<Point, List<StackPanel>> entry in CaptureMoves)
            {
                if (entry.Key.Equals(newLoc))
                {
                    captured = entry.Value;
                }
            }
            foreach (StackPanel panel in captured)
            {
                if (SelectedPiece.Color == "Black")
                    RedPiecesLeft--;
                else
                    BlackPiecesLeft--;
                panel.Children.Clear();
            }

            RemoveHighlight();

            //Set the piece in the new location
            newLocation.Children.Add(selected);
            piece.CurrentCol = newCol;
            piece.CurrentRow = newRow;
            if (newRow == 0 && piece.Color == "Black")
                piece.KingMe();
            else if (newRow == 7 && piece.Color == "Red")
                piece.KingMe();

            string message = "";
            if (RedPiecesLeft == 0)
            {
                if (ClientColor == "Red")
                    message = "You Lose! Do you want to play again?";
                else
                    message = "You Win! Do you want to play again?";
            }
            else if (BlackPiecesLeft == 0)
            {
                if (ClientColor == "Black")
                    message = "You Lose! Do you want to play again?";
                else
                    message = "You Win! Do you want to play again?";
            }
            if (message != "") {
                MessageBoxResult result = MessageBox.Show(message, "Game Over", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    //reset board
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }


            SwitchTurn();
        }
        private void SendMove(int currentRow, int currentCol, int newRow, int newCol)
        {
            string move = "/" + currentRow + currentCol + "," + newRow + newCol;
            LatestMove = "" + currentRow + currentCol + "," + newRow + newCol;
            WebRequest wr = WebRequest.Create(ServerLink + "/move/" + GameId + move);
            wr.Method = "POST";
            wr.ContentLength = 0;
            wr.GetResponse();
        }

        private void SwitchTurn()
        {
            if (SelectedPiece.Color == "Black")
                CurrentTurn = "Red";
            else
                CurrentTurn = "Black";
            if (CurrentTurn != ClientColor)
            {
                string move = LatestMove;
                MoveTimer = new Timer(7000);
                MoveTimer.Elapsed += getLatestMove;
                MoveTimer.AutoReset = true;
                MoveTimer.Enabled = true;
            }
        }

        private void getLatestMove(Object Source, ElapsedEventArgs e)
        {
            WebRequest wr = WebRequest.Create(ServerLink + "/getlatestmove/" + GameId);
            wr.Method = "GET";
            WebResponse resp = wr.GetResponse();
            string move = LatestMove;
            using (Stream dataStream = resp.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                move = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
            }
            if (!move.Equals(LatestMove)) {
                MoveTimer.Stop();
                MakeOpponentMove(move);
            }
        }

        private void MakeOpponentMove(string move)
        {
            Application.Current.Dispatcher.Invoke(() => { 
                StackPanel originalLocation = (StackPanel)GetGridElement((move[0] - '0'), (move[1] - '0'));
                StackPanel newPanel = (StackPanel)GetGridElement((move[3] - '0'), (move[4] - '0'));
                SelectedPiece = GetPiece((Button)originalLocation.Children[0]);
                _ = GetPossibleMoves();
                Move(originalLocation, newPanel);
            });
        }

        /* Get the piece that a button represents by parsing the name of the button */
        private Piece GetPiece(Button b)
        {
            string buttonName = b.Name;
            Piece piece;
            if (buttonName.Substring(0, 3) == "Red")
            {
                int pieceNumber = Int32.Parse(buttonName.Substring(3));
                piece = RedPieces[pieceNumber];
            }
            else
            {
                int pieceNumber = Int32.Parse(buttonName.Substring(5));
                piece = BlackPieces[pieceNumber];

            }
            return piece;
        }

        /* Gets the element on the grid at row r and col c */
        UIElement GetGridElement(int r, int c)
        {
            for (int i = 0; i < CheckersGrid.Children.Count; i++)
            {
                UIElement e = CheckersGrid.Children[i];
                if (Grid.GetRow(e) == r && Grid.GetColumn(e) == c)
                    return e;
            }
            return null;
            
        }

        /* Set up the pieces on the board */
        public void MakePieces()
        {
            BlackPieces = new Piece[12];
            BlackPiecesLeft = 12;
            RedPieces = new Piece[12];
            RedPiecesLeft = 12;

            RedPieces[0] = new Piece("Red", 0, 1, CheckersGrid, 0);
            RedPieces[0].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            RedPieces[1] = new Piece("Red", 0, 3, CheckersGrid, 1);
            RedPieces[1].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            RedPieces[2] = new Piece("Red", 0, 5, CheckersGrid, 2);
            RedPieces[2].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            RedPieces[3] = new Piece("Red", 0, 7, CheckersGrid, 3);
            RedPieces[3].PieceButton.Click += new RoutedEventHandler(SelectPiece);

            RedPieces[4] = new Piece("Red", 1, 0, CheckersGrid, 4);
            RedPieces[4].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            RedPieces[5] = new Piece("Red", 1, 2, CheckersGrid, 5);
            RedPieces[5].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            RedPieces[6] = new Piece("Red", 1, 4, CheckersGrid, 6);
            RedPieces[6].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            RedPieces[7] = new Piece("Red", 1, 6, CheckersGrid, 7);
            RedPieces[7].PieceButton.Click += new RoutedEventHandler(SelectPiece);

            RedPieces[8] = new Piece("Red", 2, 1, CheckersGrid, 8);
            RedPieces[8].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            RedPieces[9] = new Piece("Red", 2, 3, CheckersGrid, 9);
            RedPieces[9].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            RedPieces[10] = new Piece("Red", 2, 5, CheckersGrid, 10);
            RedPieces[10].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            RedPieces[11] = new Piece("Red", 2, 7, CheckersGrid, 11);
            RedPieces[11].PieceButton.Click += new RoutedEventHandler(SelectPiece);

            BlackPieces[0] = new Piece("Black", 5, 0, CheckersGrid, 0);
            BlackPieces[0].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            BlackPieces[1] = new Piece("Black", 5, 2, CheckersGrid, 1);
            BlackPieces[1].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            BlackPieces[2] = new Piece("Black", 5, 4, CheckersGrid, 2);
            BlackPieces[2].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            BlackPieces[3] = new Piece("Black", 5, 6, CheckersGrid, 3);
            BlackPieces[3].PieceButton.Click += new RoutedEventHandler(SelectPiece);

            BlackPieces[4] = new Piece("Black", 6, 1, CheckersGrid, 4);
            BlackPieces[4].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            BlackPieces[5] = new Piece("Black", 6, 3, CheckersGrid, 5);
            BlackPieces[5].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            BlackPieces[6] = new Piece("Black", 6, 5, CheckersGrid, 6);
            BlackPieces[6].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            BlackPieces[7] = new Piece("Black", 6, 7, CheckersGrid, 7);
            BlackPieces[7].PieceButton.Click += new RoutedEventHandler(SelectPiece);

            BlackPieces[8] = new Piece("Black", 7, 0, CheckersGrid, 8);
            BlackPieces[8].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            BlackPieces[9] = new Piece("Black", 7, 2, CheckersGrid, 9);
            BlackPieces[9].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            BlackPieces[10] = new Piece("Black", 7, 4, CheckersGrid, 10);
            BlackPieces[10].PieceButton.Click += new RoutedEventHandler(SelectPiece);
            BlackPieces[11] = new Piece("Black", 7, 6, CheckersGrid, 11);
            BlackPieces[11].PieceButton.Click += new RoutedEventHandler(SelectPiece);

        }
    }
}
