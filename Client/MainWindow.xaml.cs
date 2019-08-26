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

        private string color;

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
        /* Gets the possible moves for a piece
         * At the moment, this function returns the 1 or 2 spots that a piece can move to even if they are occupied
         */
        public List<Point> getPossibleMoves()
        {
            List<Point> possibleMoves = new List<Point>();
            int rowMovement;
            if (color == "Black")
                rowMovement = -1;
            else
                rowMovement = 1;
            int newRow = currentRow + rowMovement;
            if (newRow >= 0 && newRow <= 7)
            {
                if (currentCol - 1 >= 0)
                    possibleMoves.Add(new Point(newRow, currentCol - 1));
                if (currentCol + 1 <= 7)
                    possibleMoves.Add(new Point(newRow, currentCol + 1));
            }
            return possibleMoves;
        }
    }
    public class CheckersBoard
    {
        public UniformGrid CheckersGrid { get; set; }
        public Piece[] BlackPieces { get; set; }
        public Piece[] RedPieces { get; set; }
        public Piece SelectedPiece { get; set; }
        //Grid spaces that are highlighted when a piece is selected
        private List<Point> Highlighted;

        public CheckersBoard(UniformGrid checkersGrid)
        {
            this.CheckersGrid = checkersGrid;
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

            //Get possible move locations for the slected piece
            List<Point> moves;
            SelectedPiece = getPiece((Button)sender);
            moves = SelectedPiece.getPossibleMoves();

            //Remove previously highlighted spaces
            RemoveHighlight();
            Highlighted = new List<Point>();

            //Add a button to highlight the move location if there isn't a piece there already and bind it to Move function
            foreach (Point move in moves)
            {
                StackPanel stackPanel = (StackPanel)GetGridElement(CheckersGrid, (int)move.X, (int)move.Y);
                //Check is a piece exists in the spot
                if (stackPanel.Children.Count == 0)
                {
                    Button possibleMove = new Button();
                    possibleMove.Click += new RoutedEventHandler(Move);
                    possibleMove.Height = 60;
                    possibleMove.Width = 60;
                    possibleMove.Background = Brushes.Yellow;
                    stackPanel.Children.Add(possibleMove);
                    Highlighted.Add(move);
                }
            }
        }

        /* Unhighlight all spots by removing the button in the spot and reset Highlighted list */
        private void RemoveHighlight()
        {
            if (Highlighted == null)
                return;
            foreach (Point move in Highlighted)
            {
                StackPanel stackPanel = (StackPanel)GetGridElement(CheckersGrid, (int)move.X, (int)move.Y);
                stackPanel.Children.Clear();
            }
            Highlighted = new List<Point>();
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
