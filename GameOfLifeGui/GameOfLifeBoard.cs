using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Security;

namespace GameOfLifeGui
{
    interface IGameOfLifeCell
    {
        /*
         * Interface for a game of life cell.
         */

        public bool Alive { get; set; }

        public void AddNeighbor(IGameOfLifeCell neighbor);
        public void ApplyRules(IGameOfLifeRules rules);
        public void ApplyStep();
        public int CountNeighbors();
        public void Randomize();
        
    }

    interface IGameOfLifeRules
    {
        /*
         * Interface for Game Of Life rules
         *
         * Allows different rulesets to be swapped in.
         */
        bool CalcNextState(IGameOfLifeCell cell);
    }

    class ConwaysGameOfLifeRules : IGameOfLifeRules
    {
        /*
         * Implement the canonical ruleset:
         * Conway's Game Of Life.
         */
        public bool CalcNextState(IGameOfLifeCell cell)
        {
            var count = cell.CountNeighbors();
            if (cell.Alive)
            {
                /*
                    * In Conways Game Of Life, alive cells stay alive
                    * only if they have 2 or 3 alive neighbors
                    */

                return count == 2 || count == 3;
            }
            else if (count == 3)
            {
                //If the cell is dead and has exactly 3 neighbors, it becomes alive.
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    class GameOfLifeBoard<T> where T : IGameOfLifeCell, new()
    {
        /*
         * Represents all logic behind a Conways Game Of Life and stores the state.
         */
        //first index is x (width), second is y (height)
        public List<List<T>> cells {get; private set; }
        private IGameOfLifeRules ruleset;
        public GameOfLifeBoard(int width, int height, IGameOfLifeRules ruleset)
        {
            /*
                * width: width of the game of life board
                * height: height of the game of life board
                * ruleset: The rules for this instanced of Game Of Life.
                * TODO: wrapped: will we consider the edges of the board to be adjacent to the opposite side?
                * 
                * Skipping implementing the wrapped, it complicates the neighbor calculation too much.
                * Just get a basic GoL board working, worry about wrapping later.
                */
            this.ruleset = ruleset;
            //First, lets contstruct all the cells.
            cells = new List<List<T>>();
            for (int x = 0; x < width; x++)
            {
                cells.Add(new List<T>());
                for (int y = 0; y < height; y++)
                {
                    cells[x].Add(new T());
                }
            }

            //Now, set up the cells with their neighbors.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var currentCell = cells[x][y];

                    /*
                        * Neighbors in Conways Game of Life are up/down/left/right and diagonals
                        */
                    currentCell.AddNeighbor(cells[wrapWidth(x - 1)][wrapHeight(y - 1)]);
                    currentCell.AddNeighbor(cells[x][wrapHeight(y - 1)]);
                    currentCell.AddNeighbor(cells[wrapWidth(x + 1)][wrapHeight(y - 1)]);

                    currentCell.AddNeighbor(cells[wrapWidth(x - 1)][y]);
                    // In Game Of Life, we don't count the cell itself
                    //currentCell.AddNeighbor(cells[x][y]);
                    currentCell.AddNeighbor(cells[wrapWidth(x + 1)][y]);

                    currentCell.AddNeighbor(cells[wrapWidth(x - 1)][wrapHeight(y + 1)]);
                    currentCell.AddNeighbor(cells[x][wrapHeight(y + 1)]);
                    currentCell.AddNeighbor(cells[wrapWidth(x + 1)][wrapHeight(y + 1)]);
                }
            }
        }

        public int width
        {
            /*
             * Get board width
             */
            get { return cells.Count; }
        }

        public int height
        {
            /*
             * Get board height
             */
            get { return cells[0].Count; }
        }

        public bool getState(int x, int y)
        {
            /*
             * Get single cell's state from the board
             */
            Debug.Assert(x >= 0 && y >= 0 && x < width && y < height);
            return cells[x][y].Alive;
        }

        public void setState(int x, int y, bool state)
        {
            /*
             * Set single cell's state from the board
             */
            Debug.Assert(x >= 0 && y >= 0 && x < width && y < height);
            cells[x][y].Alive = state;
        }


        private int wrapWidth(int x)
        {
            /*
                * Calculates the X coordinate depending on whether we're wrapping
                */
            if (x < 0)
            {
                return width + x;
            }
            else
            {
                return x % width;
            }
        }
        private int wrapHeight(int y)
        {
            /*
                * Calculates the Y coordinate depending on whether we're wrapping
                */
            if (y < 0)
            {
                return height + y;
            }
            else
            {
                return y % height;
            }
        }

        public void TimeStep(bool verbose = false)
        {
            /*
             * Apply the rules to the game state, then update to next state.
             */
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells[x][y].ApplyRules(ruleset);
                }
            }
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells[x][y].ApplyStep();
                }
            }
        }

        public void Randomize()
        {
            /*
             * Randomize the board's state
             */
            for (int x = 0; x < width; x++)
            {
                for (int y=0; y < height; y++)
                {
                    cells[x][y].Randomize();
                }
            }
        }
    }

    class GameOfLifeCell : IGameOfLifeCell
    {
        List<IGameOfLifeCell> Neighbors;

        static Random rnd = new Random();

        //Are we "alive"?
        public bool Alive { get; set; }
        /*
        * Next "alive" state.
        * 
        * Since next state relies on current state of neighbors,
        * we must not change our Alive state until all neighbors have
        * calculated their own NextAlive state.
        */
        public bool NextState { get; set; }

        public GameOfLifeCell()
        {
            Neighbors = new List<IGameOfLifeCell>();
            Alive = false;
            NextState = false;
        }

        public void AddNeighbor(IGameOfLifeCell neighbor)
        {
            /*
             * Game of Life cells need to know who their neighbors are.
             * Use this function to inform the cell of that information.
             */
            Neighbors.Add(neighbor);
        }

        public int CountNeighbors()
        {
            /*
             * Game of Life cells need to know how many neighbors are alive.
             *
             * This function will determine that.
             */
            int count = 0;
            for (int idx = 0; idx < Neighbors.Count; idx++)
            {
                if (Neighbors[idx].Alive)
                {
                    count++;
                }
            }
            return count;
        }

        public virtual void ApplyStep()
        {
            /*
             * After having calculated the next state for all cells, this
             * will be called to update the cell to the next state.
             */
            Alive = NextState;
        }

        public void ApplyRules(IGameOfLifeRules rules)
        {
            /*
             * Apply rules. The Rules will take a reference to our cell
             * and tell us if we're still alive.
             */
            NextState = rules.CalcNextState(this);
        }

        public void Randomize()
        {
            /*
             * Use the shared random object to randomize this cell.
             *
             * Immediately apply the change.
             */
            NextState = 1==rnd.Next(2);
            ApplyStep();
        }
    }
}
