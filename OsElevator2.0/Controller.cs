using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Threading;

namespace OsElevator
{
    class Controller
   {    
        //请求链表
        private List<Request> listRes;

        //每个电梯是否可以使用
        private bool[] couldUse;

        //电梯数组
        public Elevator[] elevator;

        //构造函数
        public Controller(Elevator[] ele)
        {
            couldUse = new bool[5];
            for(int i=0;i<5;i++)
            {
                couldUse[i] = true;
            }
            listRes = new List<Request>();
            elevator = ele;
        }

        //判断电梯n能否使用
        public bool CanUse(int n)
        {
            return couldUse[n];
        }
        //设置电梯n的状态
        public void SetUse(int n, bool ability)
        {
            couldUse[n] = ability;
        }

        //为一个请求分配电梯去响应
        public bool SolveRequest(Request re)
        {
            int minDis = 1000;
            int eleNum = 0;
            for (int i = 0; i < 5; i++)
            {
                if (!CanUse(i) || elevator[i].HasRequire(re.Floor)) continue;
                Direction runState = elevator[i].RunDirection;
                int elefloor = (int)elevator[i].Floor;
                int dis = 0;
                if (runState == re.WantDir || runState == Direction.Still)
                {
                    dis = Math.Abs(elefloor - re.Floor);
                    switch (runState)
                    {
                        case Direction.Up:
                            if (elefloor <= re.Floor)
                            {
                                if (dis < minDis)
                                {
                                    minDis = dis;
                                    eleNum = i;
                                }
                            }
                            break;
                        case Direction.Down:
                            if (elefloor >= re.Floor)
                            {
                                if (dis < minDis)
                                {
                                    minDis = dis;
                                    eleNum = i;
                                }
                            }
                            break;
                        case Direction.Still:
                            if (dis < minDis)
                            {
                                minDis = dis;
                                eleNum = i;
                            }
                            break;
                    }
                }
            }
            if (minDis == 1000)
            {
                return false;
            }
            else
            {
                elevator[eleNum].SendMessage(re);
                return true;
            }
        }

        //向调度类发送请求
        public bool SendRequset(Request re)
        {
            bool flag = true;
            foreach (var a in listRes)
            {
                if (a.Floor == re.Floor && a.WantDir == re.WantDir)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
                listRes.Add(re);
            return true;
        }

        //调度控制类的运行函数
        public void Run()
        {
            Stack<int> willDel = new Stack<int>();
            while (true)
            {
                if (listRes.Count != 0)
                {

                    for (int i = 0; i < listRes.Count; i++)
                    {
                        if (SolveRequest(listRes[i]))
                            willDel.Push(i);
                    }
                    while (willDel.Count != 0)
                    {
                        listRes.RemoveAt(willDel.Pop());
                    }
                }
                Thread.Sleep(300);
            }
        }
    }
}
