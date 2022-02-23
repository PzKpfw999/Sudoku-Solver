using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sudoku
{
    public partial class Form1 : Form
    {
        Button[,] tiles = new Button[9, 9];
        Button solve = new Button();
        SudokuR s;

        void SolveBotton(object sender,EventArgs e)
        {
            s.Solve();
            SuspendLayout();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (s.matrix[i, j].StatesCount == 1)
                    {
                        tiles[i, j].Text = s.matrix[i, j].FirstState.ToString();
                    }
                    else
                    {
                        tiles[i, j].Text = "";
                    }
                }
            }
            ResumeLayout(false);
            Console.WriteLine("Steps: {0}", s.Count);
        }

        public Form1()
        {

            InitializeComponent();

            int[] known_points = { 123 };
            s = new SudokuR(known_points); 

            SuspendLayout();
            
            ClientSize = new Size(675, 800);

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    tiles[i, j] = new Button();
                    Controls.Add(tiles[i,j]);
                    tiles[i, j].Font = new Font(tiles[i, j].Font.FontFamily, 30);
                    tiles[i, j].Location = new Point(i * 75, j * 75);
                    tiles[i, j].Size = new Size(75, 75);
                    if (s.matrix[i, j].StatesCount == 1)
                        tiles[i, j].Text = s.matrix[i, j].FirstState.ToString();
                    else
                        tiles[i, j].Text = "";
                }

            Controls.Add(solve);
            solve.Location = new Point(760, 300);
            solve.Text = "Solve";
            solve.Click += new EventHandler(SolveBotton);

            ResumeLayout(false);
        }
    }
}
