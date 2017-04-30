using System;
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
using System.Threading;

namespace OsElevator2._0
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        //构造函数
        public MainWindow()
        {
            InitializeComponent();
            initThread();

        }

        Controller controller;
        Thread[] thread;
        private void initThread()
        {

            TextBlock[] tb = new TextBlock[5];
            tb[0] = eleFir;
            tb[1] = eleSec;
            tb[2] = eleThi;
            tb[3] = eleFou;
            tb[4] = eleFiv;
            thread = new Thread[5];
            Elevator[] ele = new Elevator[5];
            for(int i=0;i<5;i++)
            {
                ele[i] = new Elevator(tb[i]);
                thread[i] = new Thread(ele[i].Run);
                thread[i].Start();
            }
            controller = new Controller(ele);
            new Thread(controller.Run).Start();

        }


        private void OpenDoor(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int column = (int)btn.GetValue(Grid.ColumnProperty);
            int eleNum = (column - 5) / 3;
            if (!controller.CanUse(eleNum)) return;
            controller.elevator[eleNum].OpenDoor();
        }

        private void CloseDoor(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int column = (int)btn.GetValue(Grid.ColumnProperty);
            int eleNum = (column - 4) / 3;
            if (!controller.CanUse(eleNum)) return;
            controller.elevator[eleNum].CloseDoor();
        }


        private void SelectFloor(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            btn.IsEnabled = false;
            int row = int.Parse((string)btn.Content);
            int column = (int)btn.GetValue(Grid.ColumnProperty);
            int eleNum = (column - 4) / 3;
            if (!controller.CanUse(eleNum)) return;        
            controller.elevator[eleNum].SendMessage(new Request(row, Direction.Still, btn));
        }

        //电梯外请求按钮被按下
        private void SendRequriement(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int row = (int)btn.GetValue(Grid.RowProperty);
            btn.IsEnabled = false;
            int column = (int)btn.GetValue(Grid.ColumnProperty);
           
            Direction dir = (Direction)column;
            row = 21 - row;
            controller.SendRequset(new Request(row, dir, btn));
            System.Diagnostics.Trace.WriteLine("楼层" + row +
                "电梯方向" + dir);

        }

        //报警按钮被按下
        private void WarningBtn(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int column = (int)btn.GetValue(Grid.ColumnProperty);
            if (btn.Background==Brushes.Red)
            {
                controller.SetUse((column - 4) / 3, false);
                thread[(column - 4) / 3].Suspend();
                btn.Background = Brushes.Green;
            }
            else
            {

                controller.SetUse((column - 4) / 3, true);
                thread[(column - 4) / 3].Resume();
                btn.Background = Brushes.Red;
            }
        }

       
    }

    class GlobalVar
    {
        public static TimeSpan WaitTime = new TimeSpan(20000000);
        public static TimeSpan RunTime = new TimeSpan(5000000);
    }

    enum Direction
    {
        Still,
        Up,
        Down,
    };

    enum EleState
    {
        Still,
        Wait,
        Down,
        Up
        
    }

    //请求类
    class Request
    {
        private int floor;
        private Direction wantDir;
        private Button wasClick;

        public Request() { }

        public Request(int floorNum, Direction dir, Button bu)
        {
            floor = floorNum;
            wantDir = dir;
            wasClick = bu;
        }

        public int Floor { get { return floor; } }
        public Direction WantDir { get { return wantDir; } }
        public Button WasClick { get { return wasClick; } }

    }
}
