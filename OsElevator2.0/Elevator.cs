using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Media;

namespace OsElevator2._0
{
    class Elevator
    {

        private SortedDictionary<int, Request> requests;

        private TimeSpanWaitor timeSpanWaitor;

        public int RequestNum { get { return requests.Count; } }


        public void CloseDoor()
        {
            if (waiting == 1)
                timeSpanWaitor.StopWait();
            
        }

        public void OpenDoor()
        {
            if (waiting == 1)
                timeSpanWaitor.AddTime();

        }


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

        //构造干啥
        public Elevator(TextBlock tb)
        {
            floor = 1;
            runDirection = Direction.Still;
            eletb = tb;
            requests = new SortedDictionary<int, Request>();
            timeSpanWaitor = new TimeSpanWaitor();
        }


        //向电梯发请求
        public void SendMessage(Request re)
        {
            if (!HasRequire(re.Floor))
                requests.Add(re.Floor, re);
        }

        private EleState updateState()
        {
            if (RequestNum == 0)
                return EleState.Still;
            if (requests.ContainsKey(Floor))
                return EleState.Wait;
            switch (RunDirection)
            {
                case Direction.Down:
                    return hasLowerReq(floor) ? EleState.Down : EleState.Still;
                case Direction.Up:
                    return hasHigherReq(floor) ? EleState.Up : EleState.Still;
                default:
                    return getHigherReq(floor) > getLowerReq(floor) ? EleState.Up : EleState.Down;
            }

        }


        private void onWait()
        {
            Button btn = requests[floor].WasClick;
            waitTimes = 1;
            try
            {
                btn.Dispatcher.Invoke(new Action(() =>
                {
                    btn.IsEnabled = true;
                }));

                requests.Remove(floor);
                eletb.Dispatcher.Invoke(new Action(() =>
                {
                    eletb.Background = Brushes.Brown;
                    eletb.Text = floor.ToString() + " Wait";
                }));
            }
            catch
            {

            }
          
        }

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

        private bool hasHigherReq(int floor)
        {
            for(int i=floor+1;i<21;i++)
            {
                if (requests.ContainsKey(i))
                    return true;
            }
            return false;
        }

        private bool hasLowerReq(int floor)
        {
            for (int i = floor -1; i > 0; i--)
            {
                if (requests.ContainsKey(i))
                    return true;
            }
            return false;
        }

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

        private int waiting = 0;
        private int waitTimes = 1;
       

        //进程的运行
        public void Run()
        {
            while (true)
            {
                switch(updateState())
                {
                    case EleState.Down:
                        onDown();
                        timeSpanWaitor.WaitForTime(GlobalVar.RunTime);
                        break;
                    case EleState.Up:
                        onUp();
                        timeSpanWaitor.WaitForTime(GlobalVar.RunTime);
                        break;
                    case EleState.Still:
                        onStill();
                        timeSpanWaitor.WaitForTime(GlobalVar.RunTime);
                        break;
                    case EleState.Wait:
                        onWait();
                        Interlocked.Exchange(ref waiting, 1);
                        //while(waitTimes--!=0)
                        timeSpanWaitor.WaitForTime(GlobalVar.WaitTime);
                        Interlocked.Exchange(ref waiting, 0);
                        break;
                }
            }
        }


    }

    public class TimeSpanWaitor
    {
        public TimeSpanWaitor(int min, int max)
        {
            waitTimeRandom = new Random();
            minWaitMillSeconds = min;
            maxWaitMillSeconds = max;
        }

        public TimeSpanWaitor() : this(DefaultMinWaitTimeMillSeconds, DefaultMaxWaitTimeMillSeconds) { }

        public const int DefaultMaxWaitTimeMillSeconds = 100;

        public const int DefaultMinWaitTimeMillSeconds = 10;

        private int maxWaitMillSeconds = 0;

        private int minWaitMillSeconds = 0;

        private int continueFlag = 0;

        private Random waitTimeRandom;

        private bool WaitTime(ref TimeSpan waitTimeOut, ref DateTime dt)
        {

            Thread.Sleep(waitTimeRandom.Next(minWaitMillSeconds, maxWaitMillSeconds));
            waitTimeOut -= getNowDateTimeSpan(ref dt);
            return (waitTimeOut.Ticks > 0);

        }

        private TimeSpan getNowDateTimeSpan(ref DateTime tp)
        {
            DateTime temp = tp;
            tp = DateTime.Now;
            return tp.Subtract(temp);
        }

        public void StopWait()
        {
            Interlocked.Exchange(ref continueFlag, 1);
        }


        private TimeSpan recordTime;

        public void AddTime()
        {
            recordTime = recordTime + GlobalVar.WaitTime;
        }


        public void WaitForTime(TimeSpan timeout)
        {
            recordTime = timeout;
            DateTime nowTime = DateTime.Now;

            while (true)
            {
                if ((!WaitTime(ref recordTime, ref nowTime)) || continueFlag==1)
                    break;
            }

            Interlocked.Exchange(ref continueFlag, 0);

        }
    }
}
