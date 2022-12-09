using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using System.Diagnostics;

namespace GameOfLifeGui
{

    class GameOfLifeCellRect : GameOfLifeCell
    {
        public Rectangle? rect;
        override public void ApplyStep()
        {
            if (NextState != Alive)
            {
                Debug.Assert(rect != null);
                rect.Fill = NextState ? Brushes.Black : Brushes.White;
                Alive = NextState;
            }
        }
    }
    internal class GameOfLifeCanvas
    {
        public GameOfLifeBoard<GameOfLifeCellRect> board;
        //Temporary size variable, calculate it properly later
        private int size = 10;
        private int space = 2;
        public GameOfLifeCanvas(int width, int height)
        {
            board = new GameOfLifeBoard<GameOfLifeCellRect>(width, height, new ConwaysGameOfLifeRules());
            //Hardcode a glider for initial testing.
            board.setState(0, 0, false);
            board.setState(1, 0, false);
            board.setState(2, 0, true);
            board.setState(0, 1, true);
            board.setState(1, 1, false);
            board.setState(2, 1, true);
            board.setState(0, 2, false);
            board.setState(1, 2, true);
            board.setState(2, 2, true);
        }

        public void DrawBoard(Canvas MyCanvas)
        {
            /*
             * Heavily borrowed from this:
             * https://www.ictdemy.com/csharp/wpf/drawing-on-canvas-in-csharp-net-wpf/
             */
            for (int y = 0; y < board.height; y++)
            {
                for (int x = 0; x < board.width; x++)
                {
                    var rectangle = new Rectangle
                    {
                        Height = size,
                        Width = size
                    };

                    board.cells[x][y].rect = rectangle;
                    rectangle.Fill = board.getState(x, y) ? Brushes.Black : Brushes.White;
                    MyCanvas.Children.Add(rectangle);

                    Canvas.SetLeft(rectangle, x * (size + space));
                    Canvas.SetTop(rectangle, y * (size + space));
                }
            }
        }
    }
}
