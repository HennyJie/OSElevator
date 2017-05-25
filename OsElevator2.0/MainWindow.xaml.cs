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

namespace OsElevator
{
    class GlobalVar
    {
        public static TimeSpan WaitTime = new TimeSpan(20000000);//wait状态的时间间隔
        public static TimeSpan RunTime = new TimeSpan(5000000);//run和still状态的时间间隔
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

    public partial class MainWindow : Window
    {
        //控制类
        Controller controller;
        //线程数组
        Thread[] thread;

        //构造函数
        public MainWindow()
        {
            InitializeComponent();
            initThread();
        }

        //控制类线程和电梯线程启动
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

        //开门按钮
        private void OpenDoor(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int column = (int)btn.GetValue(Grid.ColumnProperty);
            int eleNum = (column - 6) / 4;
            if (!controller.CanUse(eleNum)) return;
            controller.elevator[eleNum].OpenDoor();
        }

        //关门按钮
        private void CloseDoor(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int column = (int)btn.GetValue(Grid.ColumnProperty);
            int eleNum = (column - 5) / 4;
            if (!controller.CanUse(eleNum)) return;
            controller.elevator[eleNum].CloseDoor();
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

        //电梯内楼层选择按钮被按下
        private void SelectFloor(object sender, RoutedEventArgs e)
        {
            if (warningFlag==1) return;
            Button btn = (Button)sender;
            int row = int.Parse((string)btn.Content);
            int column = (int)btn.GetValue(Grid.ColumnProperty);
            int eleNum = (column - 5) / 4;
            if (!controller.CanUse(eleNum)) return;
            btn.IsEnabled = 
                controller.elevator[eleNum].SendMessage(new Request(row, Direction.Still, btn)) ? false : true;
        }

        int warningFlag = 0;

        //报警按钮被按下
        private void WarningBtn(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int column = (int)btn.GetValue(Grid.ColumnProperty);
            if (btn.Background==Brushes.Red)
            {
                warningFlag = 1;
                controller.SetUse((column - 5) / 4, false);
                thread[(column - 5) / 4].Suspend();
                btn.Background = Brushes.Green;
            }
            else
            {
                warningFlag = 0;
                controller.SetUse((column - 5) / 4, true);
                thread[(column - 5) / 4].Resume();
                btn.Background = Brushes.Red;
            }
        }
   
    }


}
