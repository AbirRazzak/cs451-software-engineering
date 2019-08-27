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
        }

        private void HostButton_Click(object sender, RoutedEventArgs e)
        {
            StartButtons.Visibility = Visibility.Collapsed;
            GameGrid.Visibility = Visibility.Visible;
            string code = GenerateCode();
            new CheckersBoard(CheckersGrid, "Black", code);
        }

        private string GenerateCode()
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < 7; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            if (IdBox.Text.Length != 7) { 
                StartButtons.Visibility = Visibility.Collapsed;
                GameGrid.Visibility = Visibility.Visible;
                new CheckersBoard(CheckersGrid, "Red", IdBox.Text);
            }
            
        }
    }
}
