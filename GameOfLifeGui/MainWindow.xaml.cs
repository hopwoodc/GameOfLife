using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GameOfLifeGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameOfLifeCanvas gameOfLife = new(30, 20);
        private DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(.0001);
            timer.Tick += timerTick;
            timer.Start();
            gameOfLife.DrawBoard(MyCanvas);
        }

        void timerTick(object sender, EventArgs e)
        {
            gameOfLife.board.TimeStep();
            //MyCanvas.UpdateLayout();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timer.Interval = TimeSpan.FromSeconds(e.NewValue / Speed.Maximum);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void Randomize_Click(object sender, RoutedEventArgs e)
        {
            gameOfLife.board.Randomize();
        }
    }
}
