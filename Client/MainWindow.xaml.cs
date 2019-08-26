using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            new CheckersBoard(CheckersGrid);
        }

    }
    public class Piece
    {
        public int currentRow { get; set; }
        public int currentCol { get; set; }

        public string color { get; set; }

        //A piece on the grid is shown as a button textured from a picture of a checkers piece
        public Button piece { get; set; }

        public Piece(string color, int row, int col, UniformGrid CheckersGrid, int pieceNum)
        {
            var brush = new ImageBrush();

            //Load images for the buttons
            if (color == "Black")
            {
                this.color = "Black";
                brush.ImageSource = new BitmapImage(new Uri("black.png", UriKind.Relative));
            }
            else
            {
                this.color = "Red";
                brush.ImageSource = new BitmapImage(new Uri("red.png", UriKind.Relative));
            }

            currentRow = row;
            currentCol = col;

            //Button setup
            piece = new Button();
            piece.Name = this.color + pieceNum;
            piece.Height = 60;
            piece.Width = 60;
            piece.Background = brush;
            StackPanel stackPanel = (StackPanel)GetGridElement(CheckersGrid, row, col);
            stackPanel.Children.Add(piece);
        }

        /* Gets the element on the grid at row r and col c */
        UIElement GetGridElement(UniformGrid g, int r, int c)
        {
            for (int i = 0; i < g.Children.Count; i++)
            {
                UIElement e = g.Children[i];
                if (Grid.GetRow(e) == r && Grid.GetColumn(e) == c)
                    return e;
            }
            return null;
        }
    }
    public class CheckersBoard
    {
        public UniformGrid CheckersGrid { get; set; }
        public Piece[] BlackPieces { get; set; }
        public Piece[] RedPieces { get; set; }
        public Piece SelectedPiece { get; set; }
        //Grid spaces that are highlighted when a piece is selected
        private List<StackPanel> Highlighted;

        private Dictionary<Point, List<StackPanel>> CaptureMoves;

        public CheckersBoard(UniformGrid checkersGrid)
        {
            this.CheckersGrid = checkersGrid;
            Highlighted = new List<StackPanel>();
            MakeBoard();
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
            makePieces();
        }

        /* Called when a button on grid is clicked (When a piece is selected)
         * sender - the object calling the method
         * e - arguments passed with the call which should be none in this case
         */
        public void selectPiece(Object sender, RoutedEventArgs e)
        {

            Button piece = (Button)sender;
            CaptureMoves = new Dictionary<Point, List<StackPanel>>();
            RemoveHighlight();
            

            //Get possible move locations for the slected piece
            List<StackPanel> moves;
            SelectedPiece = getPiece((Button)sender);
            moves = getPossibleMoves();

            //Remove previously highlighted spaces
            
            

            //Add a button to highlight the move location if there isn't a piece there already and bind it to Move function
            foreach (StackPanel stackPanel in moves)
            {
                //StackPanel stackPanel = (StackPanel)GetGridElement(CheckersGrid, (int)move.X, (int)move.Y);
                //Check is a piece exists in the spot
                //if (stackPanel.Children.Count == 0)
                //{
                    Button possibleMove = new Button();
                    possibleMove.Click += new RoutedEventHandler(Move);
                    possibleMove.Height = 60;
                    possibleMove.Width = 60;
                    possibleMove.Background = Brushes.Yellow;
                    stackPanel.Children.Add(possibleMove);
                    Highlighted.Add(stackPanel);
            }
        }

        /* Gets all possible moves for a piece and return the locations for those moves*/
        public List<StackPanel> getPossibleMoves()
        {
            List<StackPanel> possibleMoves;
            int rowMovement;

            if (SelectedPiece.color == "Black")
                rowMovement = -1;
            else
                rowMovement = 1;

            possibleMoves = MoveHelper(rowMovement, SelectedPiece.currentRow, SelectedPiece.currentCol);

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
                if (col - 1 >= 0) { 
                    StackPanel leftPanel = (StackPanel)GetGridElement(CheckersGrid, newRow, col - 1);
                    if(leftPanel.Children.Count == 0)
                        possibleMoves.Add(leftPanel);
                }
                if (col + 1 <= 7)
                {
                    StackPanel rightPanel = (StackPanel)GetGridElement(CheckersGrid, newRow, col + 1);
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
                StackPanel leftPanel = (StackPanel)GetGridElement(CheckersGrid, row + rowIncrement, col - 1);
                possibleMoves.AddRange(GetJumpsHelper(leftPanel, rowIncrement * 2, -2, row, col));
            }
            if (col + 1 <= 7 && row + rowIncrement >= 0 && row + rowIncrement <= 7) { 
                StackPanel rightPanel = (StackPanel)GetGridElement(CheckersGrid, row + rowIncrement, col + 1);
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
                Piece jumpable = getPiece(howCanHeJump);
                if (jumpable.color != SelectedPiece.color)
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
                StackPanel panel = (StackPanel)GetGridElement(CheckersGrid, row + rowIncrement, col + colIncrement);
                if (panel.Children.Count == 0) { 
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
        public void Move(Object sender, RoutedEventArgs e)
        {
            //Remove the selected peice from the original location
            Button selected = SelectedPiece.piece;
            StackPanel selectedPanel = (StackPanel)selected.Parent;
            Piece piece = getPiece(selected);
            selectedPanel.Children.Remove(selected);

            //Retrieve new row and column for location to move to
            Button newLocation = (Button)sender;
            StackPanel newLocationPanel = (StackPanel)newLocation.Parent;
            int newRow = Grid.GetRow(newLocationPanel);
            int newCol = Grid.GetColumn(newLocationPanel);

            List<StackPanel> captured = new List<StackPanel>();
            Point newLoc = new Point(newRow, newCol);
            foreach(KeyValuePair<Point,List<StackPanel>> entry in CaptureMoves){
                if (entry.Key.Equals(newLoc))
                {
                    captured = entry.Value;
                }
            }
            //MessageBox.Show(captured.Count.ToString());
            foreach (StackPanel panel in captured)
            {
                panel.Children.Clear();
            }

            //Unhighlight
            RemoveHighlight();

            //Set the piece in the new location
            newLocationPanel.Children.Add(selected);
            piece.currentCol = newCol;
            piece.currentRow = newRow;
        }

        /* Get the piece that a button represents by parsing the name of the button */
        private Piece getPiece(Button b)
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
        UIElement GetGridElement(UniformGrid g, int r, int c)
        {
            for (int i = 0; i < g.Children.Count; i++)
            {
                UIElement e = g.Children[i];
                if (Grid.GetRow(e) == r && Grid.GetColumn(e) == c)
                    return e;
            }
            return null;
        }

        /* Set up the pieces on the board */
        public void makePieces()
        {
            BlackPieces = new Piece[12];
            RedPieces = new Piece[12];

            RedPieces[0] = new Piece("Red", 0, 1, CheckersGrid, 0);
            RedPieces[0].piece.Click += new RoutedEventHandler(selectPiece);
            RedPieces[1] = new Piece("Red", 0, 3, CheckersGrid, 1);
            RedPieces[1].piece.Click += new RoutedEventHandler(selectPiece);
            RedPieces[2] = new Piece("Red", 0, 5, CheckersGrid, 2);
            RedPieces[2].piece.Click += new RoutedEventHandler(selectPiece);
            RedPieces[3] = new Piece("Red", 0, 7, CheckersGrid, 3);
            RedPieces[3].piece.Click += new RoutedEventHandler(selectPiece);

            RedPieces[4] = new Piece("Red", 1, 0, CheckersGrid, 4);
            RedPieces[4].piece.Click += new RoutedEventHandler(selectPiece);
            RedPieces[5] = new Piece("Red", 1, 2, CheckersGrid, 5);
            RedPieces[5].piece.Click += new RoutedEventHandler(selectPiece);
            RedPieces[6] = new Piece("Red", 1, 4, CheckersGrid, 6);
            RedPieces[6].piece.Click += new RoutedEventHandler(selectPiece);
            RedPieces[7] = new Piece("Red", 1, 6, CheckersGrid, 7);
            RedPieces[7].piece.Click += new RoutedEventHandler(selectPiece);

            RedPieces[8] = new Piece("Red", 2, 1, CheckersGrid, 8);
            RedPieces[8].piece.Click += new RoutedEventHandler(selectPiece);
            RedPieces[9] = new Piece("Red", 2, 3, CheckersGrid, 9);
            RedPieces[9].piece.Click += new RoutedEventHandler(selectPiece);
            RedPieces[10] = new Piece("Red", 2, 5, CheckersGrid, 10);
            RedPieces[10].piece.Click += new RoutedEventHandler(selectPiece);
            RedPieces[11] = new Piece("Red", 2, 7, CheckersGrid, 11);
            RedPieces[11].piece.Click += new RoutedEventHandler(selectPiece);

            BlackPieces[0] = new Piece("Black", 5, 0, CheckersGrid, 0);
            BlackPieces[0].piece.Click += new RoutedEventHandler(selectPiece);
            BlackPieces[1] = new Piece("Black", 5, 2, CheckersGrid, 1);
            BlackPieces[1].piece.Click += new RoutedEventHandler(selectPiece);
            BlackPieces[2] = new Piece("Black", 5, 4, CheckersGrid, 2);
            BlackPieces[2].piece.Click += new RoutedEventHandler(selectPiece);
            BlackPieces[3] = new Piece("Black", 5, 6, CheckersGrid, 3);
            BlackPieces[3].piece.Click += new RoutedEventHandler(selectPiece);

            BlackPieces[4] = new Piece("Black", 6, 1, CheckersGrid, 4);
            BlackPieces[4].piece.Click += new RoutedEventHandler(selectPiece);
            BlackPieces[5] = new Piece("Black", 6, 3, CheckersGrid, 5);
            BlackPieces[5].piece.Click += new RoutedEventHandler(selectPiece);
            BlackPieces[6] = new Piece("Black", 6, 5, CheckersGrid, 6);
            BlackPieces[6].piece.Click += new RoutedEventHandler(selectPiece);
            BlackPieces[7] = new Piece("Black", 6, 7, CheckersGrid, 7);
            BlackPieces[7].piece.Click += new RoutedEventHandler(selectPiece);

            BlackPieces[8] = new Piece("Black", 7, 0, CheckersGrid, 8);
            BlackPieces[8].piece.Click += new RoutedEventHandler(selectPiece);
            BlackPieces[9] = new Piece("Black", 7, 2, CheckersGrid, 9);
            BlackPieces[9].piece.Click += new RoutedEventHandler(selectPiece);
            BlackPieces[10] = new Piece("Black", 7, 4, CheckersGrid, 10);
            BlackPieces[10].piece.Click += new RoutedEventHandler(selectPiece);
            BlackPieces[11] = new Piece("Black", 7, 6, CheckersGrid, 11);
            BlackPieces[11].piece.Click += new RoutedEventHandler(selectPiece);

        }
    }
}
