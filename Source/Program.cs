namespace Console2048
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class Program
    {
        //main method
        static void Main(string[] args)
        {
            Game_2048 game = new Game_2048();
            game.Run();
        }
    }

    class Game_2048
    {
        public ulong Score { get; private set; }
        public ulong[,] Board { get; private set; }

        private readonly int rows;
        private readonly int cols;
        private readonly Random random = new Random();

        //constructor
        public Game_2048()
        {
            this.Board = new ulong[4, 4];
            this.rows = this.Board.GetLength(0);
            this.cols = this.Board.GetLength(1);
            this.Score = 0;
        }

        //called to run the main loop of the game
        public void Run()
        {
            bool hasUpdated = true;
            do
            {
                if (hasUpdated)
                {
                    PutNewValue();
                }

                Display();

                if (IsDead())
                {
                    using (new ColorOutput(ConsoleColor.Red))
                    {
                        Console.WriteLine("GAME OVER!!!");
                        break;
                    }
                }

                Console.WriteLine("Controls: Arrows Keys or WASD - Up/Down/Left/Right.  \n" +
                    "Exit: Ctrl-C");
                ConsoleKeyInfo input = Console.ReadKey(true); // BLOCKING TO WAIT FOR INPUT
                Console.WriteLine(input.Key.ToString());

                switch (input.Key)
                {
                    case ConsoleKey.UpArrow:
                        hasUpdated = Update(Direction.Up);
                        break;

                    case ConsoleKey.DownArrow:
                        hasUpdated = Update(Direction.Down);
                        break;

                    case ConsoleKey.LeftArrow:
                        hasUpdated = Update(Direction.Left);
                        break;

                    case ConsoleKey.RightArrow:
                        hasUpdated = Update(Direction.Right);
                        break;
                    case ConsoleKey.W:
                        hasUpdated = Update(Direction.Up);
                        break;

                    case ConsoleKey.S:
                        hasUpdated = Update(Direction.Down);
                        break;

                    case ConsoleKey.A:
                        hasUpdated = Update(Direction.Left);
                        break;

                    case ConsoleKey.D:
                        hasUpdated = Update(Direction.Right);
                        break;

                    default:
                        hasUpdated = false;
                        break;
                }
            }
            while (true); // use CTRL-C to break out of loop

            Console.WriteLine("CTRL - C to break out of loop...");
            Console.Read();
        }

        //Beautification of the Console for different numbers within the game.
        private static ConsoleColor GetNumberColor(ulong num)
        {
            switch (num)
            {
                case 0:
                    return ConsoleColor.DarkGray;
                case 2:
                    return ConsoleColor.Cyan;
                case 4:
                    return ConsoleColor.Magenta;
                case 8:
                    return ConsoleColor.Red;
                case 16:
                    return ConsoleColor.Green;
                case 32:
                    return ConsoleColor.Yellow;
                case 64:
                    return ConsoleColor.Yellow;
                case 128:
                    return ConsoleColor.DarkCyan;
                case 256:
                    return ConsoleColor.Cyan;
                case 512:
                    return ConsoleColor.DarkMagenta;
                case 1024:
                    return ConsoleColor.Magenta;
                default:
                    return ConsoleColor.Red;
            }
        }

        //Update the board on receiving player input related to the direction, to calculate the drop,
        //reverse drop, get and set value for existing tiles on board and for score calculation.
        private static bool Update(ulong[,] board, Direction direction, out ulong score)
        {
            int nRows = board.GetLength(0);
            int nCols = board.GetLength(1);

            score = 0;
            bool hasUpdated = false;

            // Check if GameOver at the end of the Update()

            // Drop along row or column
            // if true process inner along row
            // else process inner along column
            bool isAlongRow = direction == Direction.Left || direction == Direction.Right;

            // Should inner dimension be processed in increasing order
            bool isIncreasing = direction == Direction.Left || direction == Direction.Up;

            int outterCount = isAlongRow ? nRows : nCols;
            int innerCount = isAlongRow ? nCols : nRows;
            int innerStart = isIncreasing ? 0 : innerCount - 1;
            int innerEnd = isIncreasing ? innerCount - 1 : 0;

            //Delegates for Dropping, Reverse Dropping, Getting value, Setting Value
            //Methods are subscribed to these events and get called when these are triggered.
            Func<int, int> drop = isIncreasing
                ? new Func<int, int>(innerIndex => innerIndex - 1)
                : new Func<int, int>(innerIndex => innerIndex + 1);

            Func<int, int> reverseDrop = isIncreasing
                ? new Func<int, int>(innerIndex => innerIndex + 1)
                : new Func<int, int>(innerIndex => innerIndex - 1);

            Func<ulong[,], int, int, ulong> getValue = isAlongRow
                ? new Func<ulong[,], int, int, ulong>((x, i, j) => x[i, j])
                : new Func<ulong[,], int, int, ulong>((x, i, j) => x[j, i]);

            Action<ulong[,], int, int, ulong> setValue = isAlongRow
                ? new Action<ulong[,], int, int, ulong>((x, i, j, v) => x[i, j] = v)
                : new Action<ulong[,], int, int, ulong>((x, i, j, v) => x[j, i] = v);

            Func<int, bool> innerCondition = index => Math.Min(innerStart, innerEnd) <= index && index <= Math.Max(innerStart, innerEnd);

            for (int i = 0; i < outterCount; i++)
            {
                for (int j = innerStart; innerCondition(j); j = reverseDrop(j))
                {
                    if (getValue(board, i, j) == 0)
                    {
                        continue;
                    }

                    int newJ = j;
                    do
                    {
                        newJ = drop(newJ);
                    }
                    // Continue probing along as long as we haven't hit the boundary and the new position isn't occupied
                    while (innerCondition(newJ) && getValue(board, i, newJ) == 0);

                    if (innerCondition(newJ) && getValue(board, i, newJ) == getValue(board, i, j))
                    {
                        // We did not hit the canvas boundary (we hit a node) AND no previous merge occurred AND the nodes' values are the same
                        // Let's merge
                        ulong newValue = getValue(board, i, newJ) * 2;
                        setValue(board, i, newJ, newValue);
                        setValue(board, i, j, 0);

                        hasUpdated = true;
                        score += newValue;
                    }
                    else
                    {
                        // Reached the boundary OR...
                        // we hit a node with different value OR...
                        // we hit a node with same value BUT a prevous merge had occurred
                        // 
                        // Simply stack along
                        newJ = reverseDrop(newJ); // reverse back to its valid position
                        if (newJ != j)
                        {
                            // there's an update
                            hasUpdated = true;
                        }

                        ulong value = getValue(board, i, j);
                        setValue(board, i, j, 0);
                        setValue(board, i, newJ, value);
                    }
                }
            }

            return hasUpdated;
        }
        
        //Has the input affected the board, if yes increase the score
        //Cause the input may not be valid if on extremeties.
        private bool Update(Direction dir)
        {
            ulong score;
            bool isUpdated = Game_2048.Update(this.Board, dir, out score);
            this.Score += score;
            return isUpdated;
        }

        //has the game over condition been triggerd
        //is there any more place for the player to move if not then game over.
        private bool IsDead()
        {
            ulong score;
            foreach (Direction dir in new Direction[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right })
            {
                ulong[,] clone = (ulong[,])Board.Clone();
                if (Game_2048.Update(clone, dir, out score))
                {
                    return false;
                }
            }

            // tried all directions. none worked.
            return true;
        }

        //Displaying the 2048 Board with Colors on the Console.
        private void Display()
        {
            Console.Clear();
            Console.WriteLine("Welcome to Console 2048.\nA Console take on our favourite game 2048 if it had existed in the 1980s");
            Console.WriteLine();
            Console.WriteLine("Score: {0}", this.Score);
            Console.WriteLine();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    using (new ColorOutput(Game_2048.GetNumberColor(Board[i, j])))
                    {
                        Console.Write(string.Format("{0,6}", Board[i, j]));
                    }
                }

                Console.WriteLine();
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        //On board update, add a new value b/w 2 and 4 (some chance) at a random place on the grid
        //if atleast one empty slot is available
        //also helps in checking isDead.
        private void PutNewValue()
        {
            // Find all empty slots
            List<Tuple<int, int>> emptySlots = new List<Tuple<int, int>>();
            for (int iRow = 0; iRow < rows; iRow++)
            {
                for (int iCol = 0; iCol < cols; iCol++)
                {
                    if (Board[iRow, iCol] == 0)
                    {
                        emptySlots.Add(new Tuple<int, int>(iRow, iCol));
                    }
                }
            }

            // at least 1 empty slot. as the user is not dead
            int iSlot = random.Next(0, emptySlots.Count); // randomly pick an empty slot
            ulong value = random.Next(0, 100) < 95 ? (ulong)2 : (ulong)4; // randomly pick 2 (with 95% chance) or 4 (rest of the chance)
            Board[emptySlots[iSlot].Item1, emptySlots[iSlot].Item2] = value;
        }
        //utils
        #region Utility Classes
        enum Direction
        {
            Up,
            Down,
            Right,
            Left,
        }

        class ColorOutput : IDisposable
        {
            public ColorOutput(ConsoleColor fg, ConsoleColor bg = ConsoleColor.Black)
            {
                Console.ForegroundColor = fg;
                Console.BackgroundColor = bg;
            }

            public void Dispose()
            {
                Console.ResetColor();
            }
        }
        #endregion Utility Classes
    }
}