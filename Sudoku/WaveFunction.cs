using System;
using System.Linq;
using System.Collections.Generic;

namespace Sudoku
{
    class Cell
    {
        int x, y;
        public HashSet<byte> states;

        public int X { get { return x; } }
        public int Y { get { return y; } }
        public int StatesCount { get { return states.Count; } }
        public byte FirstState { get { return states.ElementAt(0); } }
        public bool IsPropagated = false;
        public Cell(int x, int y, int possible_states = 9)
        {
            this.x = x;
            this.y = y;
            states = new HashSet<byte>(possible_states);
            for (byte i = 1; i <= possible_states; i++)
                states.Add(i);
        }
        public Cell(Cell cell)
        {
            states = new HashSet<byte>(cell.StatesCount);
            foreach (byte state in cell.states)
                states.Add(state);
            x = cell.X;
            y = cell.Y;
            IsPropagated = cell.IsPropagated;
        }
        public bool RemoveState(byte state)
        {
            bool ret = states.Remove(state);
            /*
            if(states.Count == 0)
                throw new Exception("Warning: Possible States equal 0");
            */
            return ret;
        }
        public void Collapse()
        {
            Random r = new Random();
            byte one = states.ElementAt(r.Next(states.Count));
            states.Clear();
            states.Add(one);
        }
        public void Collapse(byte state)
        {
            states.Clear();
            states.Add(state);
        }
    }
    struct Vector2D
    {
        public int x, y;
        public Vector2D(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    //Recursive version
    class SudokuR
    {
        public Cell[,] matrix = new Cell[9, 9];
        int count = 0;
        public int Count { get { return count; } }
        public SudokuR(int[] known_points)
        {
            Init();
            foreach (int point in known_points)
            {
                if (point < 100 || point > 999)
                {
                    throw new Exception("Error: Known Points not in format " + point);
                }
                int x = point / 100;
                int y = point / 10 % 10;
                byte v = (byte)(point % 10);
                matrix[x - 1, y - 1].Collapse(v);
            }
        }
        void Init()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    matrix[i, j] = new Cell(i, j);
        }
        public bool Solve()
        {
            Vector2D xy = SelectCellwithLowestEntropy(matrix);
            if (xy.x == -1)
                return true;

            Cell[,] matrix_copy = new Cell[9, 9];
            CopyMatrix(matrix, matrix_copy);
            foreach (byte state in matrix_copy[xy.x, xy.y].states)
            {
                count++;
                if (Propagate(matrix[xy.x, xy.y], state) && Solve())
                    return true;
                CopyMatrix(matrix_copy, matrix);
            }
            return false;
        }
        void CopyMatrix(Cell[,] src, Cell[,] dest)
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    dest[i, j] = new Cell(src[i, j]);
        }
        Vector2D SelectCellwithLowestEntropy(Cell[,] cells)
        {
            int x = -1, y = -1;
            int min = 10;
            foreach (Cell cell in cells)
            {
                if (cell.StatesCount < min && !cell.IsPropagated)
                {
                    min = cell.StatesCount;
                    x = cell.X;
                    y = cell.Y;
                }
            }
            return new Vector2D(x, y);
        }
        bool Propagate(Cell cell, byte state)
        {
            matrix[cell.X, cell.Y].Collapse(state);
            cell.IsPropagated = true;

            //on row,col
            for (int i = 0; i < 9; i++)
            {
                if (i != cell.X)
                {
                    matrix[i, cell.Y].RemoveState(state);
                    if (matrix[i, cell.Y].StatesCount == 0)
                        return false;
                }

                if (i != cell.Y)
                {
                    matrix[cell.X, i].RemoveState(state);
                    if (matrix[cell.X, i].StatesCount == 0)
                        return false;
                }
            }
            //sub grid
            int row = cell.Y / 3 * 3;
            int col = cell.X / 3 * 3;
            for (int i = row; i < row + 3; i++)
                for (int j = col; j < col + 3; j++)
                    if (i != cell.X && i != cell.Y)
                    {
                        matrix[i, j].RemoveState(state);
                        if (matrix[i, j].StatesCount == 0)
                            return false;
                    }
            return true;
        }
    }
    class Sudoku
    {
        public Cell[,] matrix = new Cell[9, 9];
        HashSet<Vector2D>[] index = new HashSet<Vector2D>[10]; //0 for propagated cell; 1-9 for cells with corresponding possible states 
        Stack<Cell[,]> matrix_stack = new Stack<Cell[,]>(81);
        Stack<HashSet<Vector2D>[]> index_stack = new Stack<HashSet<Vector2D>[]>(81);

        public Sudoku(int[] known_points)
        {
            Init();
            foreach (int point in known_points)
            {
                if (point < 100 || point > 999)
                {
                    throw new Exception("Error: Known Points not in format " + point);
                }
                int x = point / 100;
                int y = point / 10 % 10;
                byte v = (byte)(point % 10);
                matrix[x - 1, y - 1].Collapse(v);
            }
            BuildIndex();
        }
        void Init()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    matrix[i, j] = new Cell(i, j);

            for (int i = 0; i < index.Length; i++)
                index[i] = new HashSet<Vector2D>();
        }

        void BuildIndex()
        {

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    int ps = matrix[i, j].StatesCount; //possible states
                    index[ps].Add(new Vector2D(i, j));
                }
            }
        }
        //save the current global state
        void PushA()
        {
            Cell[,] matrix_copy = new Cell[9, 9];
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    matrix_copy[i, j] = new Cell(matrix[i, j]);
            matrix_stack.Push(matrix_copy);

            HashSet<Vector2D>[] index_copy = new HashSet<Vector2D>[10];
            for (int i = 0; i < 10; i++)
                index_copy[i] = new HashSet<Vector2D>(index[i]);
            index_stack.Push(index_copy);
        }

        //load previous global state
        bool PopA()
        {
            while (index_stack.Peek()[0].Contains(new Vector2D(-1, -1)))
            {
                matrix_stack.Pop();
                index_stack.Pop();
                if (matrix_stack.Count == 0) return false;
            }
            Cell[,] matrix_copy = matrix_stack.Pop();
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    matrix[i, j] = matrix_copy[i, j];

            HashSet<Vector2D>[] index_copy = index_stack.Pop();
            for (int i = 0; i < 10; i++)
                index[i] = index_copy[i];

            return true;
        }

        public bool Solve()
        {
            Console.WriteLine("{0}", matrix_stack.Count);

            Random r = new Random();
            PushA();
            for (int i = 1; i <= 9; i++)
            {
                if (index[i].Count > 0)
                {
                    Vector2D pos = index[i].ElementAt(r.Next(index[i].Count));
                    Cell cell = matrix[pos.x, pos.y];

                    if (i > 1)
                    {
                        int states_count_tmp = cell.StatesCount;
                        cell.Collapse();
                        Cell[,] matrix_tmp = matrix_stack.Peek();
                        matrix_tmp[cell.X, cell.Y].RemoveState(cell.FirstState); //won't try again when recall
                        HashSet<Vector2D>[] index_tmp = index_stack.Peek();
                        index_tmp[states_count_tmp].Remove(new Vector2D(cell.X, cell.Y));
                        index_tmp[states_count_tmp - 1].Add(new Vector2D(cell.X, cell.Y));
                    }
                    else
                    {
                        HashSet<Vector2D>[] index_tmp = index_stack.Peek(); //won't try again when recall for the whole step
                        index_tmp[0].Add(new Vector2D(-1, -1)); //mark the step as not possible
                    }
                    index[i].Remove(pos);
                    //index[0].Add(pos);

                    try
                    {
                        Propagate(cell);
                    }
                    catch
                    {
                        if (!PopA()) return false;
                    }
                    break;
                }
            }
            return true;
        }
        void Propagate(Cell cell)
        {
            byte state = cell.FirstState;
            //on row,col
            for (int i = 0; i < 9; i++)
            {
                if (i != cell.X)
                {
                    if (matrix[i, cell.Y].RemoveState(state))
                    {
                        index[matrix[i, cell.Y].StatesCount + 1].Remove(new Vector2D(i, cell.Y));
                        index[matrix[i, cell.Y].StatesCount].Add(new Vector2D(i, cell.Y));
                    }
                }

                if (i != cell.Y)
                {
                    if (matrix[cell.X, i].RemoveState(state))
                    {
                        index[matrix[cell.X, i].StatesCount + 1].Remove(new Vector2D(cell.X, i));
                        index[matrix[cell.X, i].StatesCount].Add(new Vector2D(cell.X, i));
                    }
                }
            }
            //sub grid
            int row = cell.Y / 3 * 3;
            int col = cell.X / 3 * 3;
            for (int i = row; i < row + 3; i++)
            {
                for (int j = col; j < col + 3; j++)
                {
                    if (i != cell.X && i != cell.Y)
                    {
                        if (matrix[i, j].RemoveState(state))
                        {
                            index[matrix[i, j].StatesCount + 1].Remove(new Vector2D(i, j));
                            index[matrix[i, j].StatesCount].Add(new Vector2D(i, j));
                        }
                    }

                }
            }
        }
    }
}
