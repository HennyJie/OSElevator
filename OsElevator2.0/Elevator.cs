using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Media;

namespace OsElevator
{
    //控制电梯不同状态下的逗留时间
    public class TimeSpanWaitor
    {
        //默认随机等待时间上限
        public const int DefaultMaxWaitTimeMillSeconds = 100;
        //默认随机等待时间下限
        public const int DefaultMinWaitTimeMillSeconds = 10;
        //随机等待时间上限
        private int maxWaitMillSeconds = 0;
        //随机等待时间下限
        private int minWaitMillSeconds = 0;
        //随机等待时间
        private Random waitTimeRandom;
        //电梯继续运行
        private int continueFlag = 0;
       

        //构造函数
        public TimeSpanWaitor(int min, int max)
        {
            waitTimeRandom = new Random();
            minWaitMillSeconds = min;
            maxWaitMillSeconds = max;
        }
        public TimeSpanWaitor() : this(DefaultMinWaitTimeMillSeconds, DefaultMaxWaitTimeMillSeconds) { }

        //判断是否还处于等待阶段内
        private bool WaitTime(ref TimeSpan waitTimeOut, ref DateTime dt)
        {
            Thread.Sleep(waitTimeRandom.Next(minWaitMillSeconds, maxWaitMillSeconds));
            waitTimeOut -= getNowDateTimeSpan(ref dt);
            return (waitTimeOut.Ticks > 0);
        }
        //计算已经经过的等待时间
        private TimeSpan getNowDateTimeSpan(ref DateTime tp)
        {
            DateTime temp = tp;
            tp = DateTime.Now;
            return tp.Subtract(temp);
        }

        //按下关门按钮以后调用，停止等待
        public void StopWait()
        {
            Interlocked.Exchange(ref continueFlag, 1);
        }

        private TimeSpan recordTime;

        //按下开门按钮以后调用，延长等待时间
        public void AddTime()
        {
            recordTime = recordTime + GlobalVar.WaitTime;
        }

        //状态逗留，timeout为该状态的具体逗留时间
        public void WaitForTime(TimeSpan timeout)
        {
            recordTime = timeout;
            DateTime nowTime = DateTime.Now;
            while (true)
            {
                if ((!WaitTime(ref recordTime, ref nowTime)) || continueFlag == 1)
                    break;
            }
            //默认情况下，continueFlag为0
            Interlocked.Exchange(ref continueFlag, 0);
        }
    }

    class Elevator
    {
        //按照楼层记录请求
        private SortedDictionary<int, Request> requests;
        //等待类
        private TimeSpanWaitor timeSpanWaitor;
        //请求数目
        public int RequestNum { get { return requests.Count; } }
        //电梯是否处于等待状态
        private int waiting = 0;
        //等待延长的次数
       // private int waitTimes = 1;

        //电梯所在楼层
        private int floor;
        public int Floor { get { return floor; } }

        //电梯方向
        private Direction runDirection;
        public Direction RunDirection { get { return runDirection; } }
     
        //电梯绑定的文本框
        private TextBlock eletb;

        //判断电梯是否已响应某个楼层的请求
        public bool HasRequire(int floorNum)
        {
            return requests.ContainsKey(floorNum);
        }

        //构造函数
        public Elevator(TextBlock tb)
        {
            floor = 1;
            runDirection = Direction.Still;
            eletb = tb;
            requests = new SortedDictionary<int, Request>();
            timeSpanWaitor = new TimeSpanWaitor();
        }

        //按下关门按钮，提前结束在当前层的停留时间
        public void CloseDoor()
        {
            if (waiting == 1)
                timeSpanWaitor.StopWait();
        }

        //按下开门按钮，电梯延长在当前层的停留时间
        public void OpenDoor()
        {
            if (waiting == 1)
                timeSpanWaitor.AddTime();
        }

        //向电梯发请求
        public bool SendMessage(Request re)
        {
            if (re.Floor > floor && runDirection == Direction.Down ||
                re.Floor < floor && runDirection == Direction.Up)
                return false;
            if (!HasRequire(re.Floor))
                requests.Add(re.Floor, re);
            return true;
        }

        //更新电梯的状态
        private EleState updateState()
        {
            //待处理请求数目为0，电梯进入still状态
            if (RequestNum == 0)
                return EleState.Still;
            //当前楼层请求已被响应，进入等待乘客状态
            if (requests.ContainsKey(Floor))
                return EleState.Wait;
            switch (RunDirection)
            {
                //当前向下运行，若下方还有请求则继续向下
                case Direction.Down:
                    return hasLowerReq(floor) ? EleState.Down : EleState.Still;
                //当前向上运行，若上方还有请求则继续向上
                case Direction.Up:
                    return hasHigherReq(floor) ? EleState.Up : EleState.Still;
                //当前处于still状态，电梯向待处理楼层多的方向运行
                default:
                    return getHigherReq(floor) > getLowerReq(floor) ? EleState.Up : EleState.Down;
            }
        }

        //等待乘客上梯状态
        private void onWait()
        {
            Button btn = requests[floor].WasClick;
           
            try
            {
                btn.Dispatcher.Invoke(new Action(() =>
                {
                    btn.IsEnabled = true;
                }));

                requests.Remove(floor);
                eletb.Dispatcher.Invoke(new Action(() =>
                {
                    eletb.Background = Brushes.Gold;
                    eletb.Text = floor.ToString() + " Wait";
                }));
            }
            catch
            {

            }         
        }

        //电梯暂无需处理请求 或者 下一步即将掉换方向
        private void onStill()
        {
            try
            {
                eletb.Dispatcher.Invoke(new Action(() =>
                {
                    eletb.Background = Brushes.LightCoral;
                    eletb.Text = floor.ToString() + " Still";
                }));
            }
            catch
            {

            }       
        }

        //电梯下行
        private void onDown()
        {
            floor--;
            try
            {
                eletb.Dispatcher.Invoke(new Action(() =>
                {
                    eletb.SetValue(Grid.RowProperty, 21 - floor);
                    eletb.Background = Brushes.LightCoral;
                    eletb.Text = floor.ToString() + " Down";
                }));
            }
            catch
            {

            }    
        }

        //电梯上行
        private void onUp()
        {
            floor++;
            try
            {
                eletb.Dispatcher.Invoke(new Action(() =>
                {
                    eletb.SetValue(Grid.RowProperty, 21 - floor);
                    eletb.Background = Brushes.LightCoral;
                    eletb.Text = floor.ToString() + " Up";
                }));
            }
            catch
            {

            }         
        }

        //判断高层是否还有请求
        private bool hasHigherReq(int floor)
        {
            for(int i=floor+1;i<21;i++)
            {
                if (requests.ContainsKey(i))
                    return true;
            }
            return false;
        }

        //判断低层是否还有请求
        private bool hasLowerReq(int floor)
        {
            for (int i = floor -1; i > 0; i--)
            {
                if (requests.ContainsKey(i))
                    return true;
            }
            return false;
        }

        //计算当前电梯所在的楼层以上楼层的请求数目
        private int getHigherReq(int floor)
        {
            int aboveRequ = 0;
            for (int i = floor + 1; i <= 20; i++)
            {
                if (requests.ContainsKey(i))
                    aboveRequ++;
            }
            return aboveRequ;
        }

        //计算当前电梯所在的楼层以下楼层的请求数目
        private int getLowerReq(int floor)
        {
            int belowRequ = 0;
            for (int i = floor - 1; i >= 1; i--)
            {
                if (requests.ContainsKey(i))
                    belowRequ++;
            }
            return belowRequ;
        }
    
        //进程的运行
        public void Run()
        {
            while (true)
            {
                switch(updateState())
                {
                    case EleState.Down:
                        runDirection = Direction.Down;
                        onDown();
                        timeSpanWaitor.WaitForTime(GlobalVar.RunTime);
                        break;
                    case EleState.Up:
                        runDirection = Direction.Up;
                        onUp();
                        timeSpanWaitor.WaitForTime(GlobalVar.RunTime);
                        break;
                    case EleState.Still:
                        runDirection = Direction.Still;
                        onStill();
                        timeSpanWaitor.WaitForTime(GlobalVar.RunTime);
                        break;
                    case EleState.Wait:
                        runDirection = Direction.Still;
                        onWait();
                        Interlocked.Exchange(ref waiting, 1);                       
                        timeSpanWaitor.WaitForTime(GlobalVar.WaitTime);
                        Interlocked.Exchange(ref waiting, 0);
                        break;
                }
            }
        }
    }
}
