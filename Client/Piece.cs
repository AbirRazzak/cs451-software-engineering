using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Client
{
    public class Piece
    {
        public int CurrentRow { get; set; }
        public int CurrentCol { get; set; }
        public string Color { get; set; }
        //A piece on the grid is shown as a button textured from a picture of a checkers piece
        public Button PieceButton { get; set; }
        public Boolean IsKing { get; set; }

        public Piece(string color, int row, int col, UniformGrid CheckersGrid, int pieceNum)
        {
            var brush = new ImageBrush();

            //Load images for the buttons
            if (color == "Black")
            {
                this.Color = "Black";
                brush.ImageSource = new BitmapImage(new Uri("black.png", UriKind.Relative));
            }
            else
            {
                this.Color = "Red";
                brush.ImageSource = new BitmapImage(new Uri("red.png", UriKind.Relative));
            }

            CurrentRow = row;
            CurrentCol = col;

            //Button setup
            PieceButton = new Button
            {
                Name = this.Color + pieceNum,
                Height = 60,
                Width = 60,
                Background = brush
            };
            StackPanel stackPanel = (StackPanel)GetGridElement(CheckersGrid, row, col);
            stackPanel.Children.Add(PieceButton);
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
}
