using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AlarmTimerLibrary
{
    /// <summary>
    /// 定时任务委托方法
    /// </summary>
    public delegate void TimerTaskDelegate(params object[] parm);

    /// <summary>
    /// 定时任务接口类
    /// </summary>
    public interface ITimerTask
    {
        /// <summary>
        /// 执行
        /// </summary>
        void Run();
    }

    /// <summary>
    /// 定时任务服务类
    /// 作者：Duyong 修改：贾世义
    /// 编写日期：2010-07-25 2011-07-01
    ///</summary> 
    public class TimerTaskService
    {

        #region  定时任务实例成员

        private TimerInfo timerInfo;  //定时信息

        private TimerTaskDelegate TimerTaskDelegateFun = null; //执行具体任务的委托方法

        private object[] parm = null; //参数

        private ITimerTask TimerTaskInstance = null; //执行具体任务的实例

        private System.Timers.Timer time;//定时器

        /// <summary>
        /// 根据定时信息和执行具体任务的实例构造定时任务服务
        /// </summary>
        /// <param name="_timer">定时信息</param>
        /// <param name="_interface">执行具体任务的实例</param>
        private TimerTaskService(TimerInfo _timer, ITimerTask _interface)
        {
            timerInfo = _timer;
            TimerTaskInstance = _interface;
        }

        /// <summary>
        /// 根据定时信息和执行具体任务的委托方法构造定时任务服务
        /// </summary>
        /// <param name="_timer">定时信息</param>
        /// <param name="trd">执行具体任务的委托方法</param>
        private TimerTaskService(TimerInfo _timer, TimerTaskDelegate trd)
        {
            timerInfo = _timer;
            TimerTaskDelegateFun = trd;
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="_parm"></param>
        private void setParm(params object[] _parm)
        {
            parm = _parm;
        }

        /// <summary>
        /// 启动定时任务
        /// </summary>
        public void Start()
        {
            if (timerInfo.Type == TimerTypes.TimeSpan)
            {
                time = new System.Timers.Timer(1000 * 60 * timerInfo.Value);
                time.Enabled = true;
                time.Elapsed += this.TimeOut;
                time.Start();
            }
            else
            {
                //检查定时器
                CheckTimer();
            }
        }

        /// <summary>
        /// 检查定时器
        /// </summary>
        private void CheckTimer()
        {
            //计算下次执行时间
            DateTime? NextRunTime = getNextRunTime();
            //如果无法获得执行时间则 不再执行
            if (NextRunTime.HasValue)
            {
                while (true)
                {
                    DateTime DateTimeNow = DateTime.Now;

                    //时间比较
                    bool dateComp = DateTimeNow.Year == NextRunTime.Value.Year && DateTimeNow.Month == NextRunTime.Value.Month && DateTimeNow.Day == NextRunTime.Value.Day;

                    bool timeComp = DateTimeNow.Hour == NextRunTime.Value.Hour && DateTimeNow.Minute == NextRunTime.Value.Minute && DateTimeNow.Second == NextRunTime.Value.Second;

                    //睡够一分钟 防止一分钟内重复执行
                    Thread.Sleep(60 * 1000);
                    //如果当前时间等式下次运行时间,则调用线程执行方法
                    if (dateComp && timeComp)
                    {
                        //调用执行处理方法
                        if (TimerTaskDelegateFun != null)
                        {
                            TimerTaskDelegateFun(parm);
                        }
                        else if (TimerTaskInstance != null)
                        {
                            TimerTaskInstance.Run();
                        }
                        //重新计算下次执行时间
                        NextRunTime = getNextRunTime();
                    }
                }
            }
        }
        /// <summary>
        /// 定时器执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeOut(object sender, EventArgs e)
        {
            //调用执行处理方法
            if (TimerTaskDelegateFun != null)
            {
                TimerTaskDelegateFun(parm);
            }
            else if (TimerTaskInstance != null)
            {
                TimerTaskInstance.Run();
            }
        }

        /// <summary>
        /// 计算下一次执行时间
        /// </summary>
        /// <returns></returns>
        private DateTime? getNextRunTime()
        {
            DateTime now = DateTime.Now;
            if (now > timerInfo.StartDate)
            {
                int nowHH = now.Hour;
                int nowMM = now.Minute;
                int nowSS = now.Second;

                int timeHH = timerInfo.RunTime.Hour;
                int timeMM = timerInfo.RunTime.Minute;
                int timeSS = timerInfo.RunTime.Second;

                //设置执行时间对当前时间进行比较
                bool nowTimeComp = nowHH < timeHH || (nowHH <= timeHH && nowMM < timeMM) || (nowHH <= timeMM && nowMM <= timeMM && nowSS < timeSS);
                switch (timerInfo.Type)
                {
                    //每天
                    case TimerTypes.EveryDay:
                        if (nowTimeComp)
                        {
                            return new DateTime(now.Year, now.Month, now.Day, timeHH, timeMM, timeSS);
                        }
                        else
                        {
                            return new DateTime(now.Year, now.Month, now.Day, timeHH, timeMM, timeSS).AddDays(1);
                        }
                        break;
                    //每周
                    case TimerTypes.DayOfWeek:
                        DayOfWeek ofweek = DateTime.Now.DayOfWeek;

                        int dayOfweek = Convert.ToInt32(DateTime.Now.DayOfWeek);

                        if (ofweek == DayOfWeek.Sunday) dayOfweek = 7;

                        if (dayOfweek < timerInfo.Value)
                        {
                            int addDays = timerInfo.Value - dayOfweek;
                            return new DateTime(now.Year, now.Month, now.Day, timeHH, timeMM, timeSS).AddDays(addDays);
                        }
                        else if (dayOfweek == timerInfo.Value && nowTimeComp)
                        {
                            return new DateTime(now.Year, now.Month, now.Day, timeHH, timeMM, timeSS);

                        }
                        else
                        {
                            int addDays = 7 - (dayOfweek - timerInfo.Value);
                            return new DateTime(now.Year, now.Month, now.Day, timeHH, timeMM, timeSS).AddDays(addDays);
                        }
                        break;
                    //每月
                    case TimerTypes.DayOfMonth:
                        if (now.Day < timerInfo.Value)
                        {
                            return new DateTime(now.Year, now.Month, timerInfo.Value, timeHH, timeMM, timeSS);
                        }
                        else if (now.Day == timerInfo.Value && nowTimeComp)
                        {
                            return new DateTime(now.Year, now.Month, now.Day, timeHH, timeMM, timeSS);
                        }
                        else
                        {
                            return new DateTime(now.Year, now.Month, timerInfo.Value, timeHH, timeMM, timeSS).AddMonths(1);
                        }
                        break;
                }
            }
            return null;
        }

        #endregion


        #region 创建定时任务静态方法

        /// <summary>
        /// 使用委托方法创建定时任务
        /// </summary>
        /// <param name="info"></param>
        /// <param name="_ptrd"></param>
        /// <param name="parm"></param>
        /// <returns></returns>
        public static Thread CreateTimerTaskService(TimerInfo info, TimerTaskDelegate _ptrd, params object[] parm)
        {
            TimerTaskService tus = new TimerTaskService(info, _ptrd);
            tus.setParm(parm);

            //创建启动线程
            Thread ThreadTimerTaskService = new Thread(new ThreadStart(tus.Start));
            return ThreadTimerTaskService;
        }

        /// <summary>
        /// 使用实现定时接口ITimerTask的实例创建定时任务
        /// </summary>
        /// <param name="info"></param>
        /// <param name="_ins"></param>
        /// <returns></returns>
        public static Thread CreateTimerTaskService(TimerInfo info, ITimerTask _ins)
        {
            TimerTaskService tus = new TimerTaskService(info, _ins);
            //创建启动线程
            Thread ThreadTimerTaskService = new Thread(new ThreadStart(tus.Start));
            return ThreadTimerTaskService;
        }

        #endregion
    }

    /// <summary>
    /// 定时信息类
    /// </summary>
    public class TimerInfo
    {
        /// <summary>
        /// 类型：EveryDay(每天),DayOfWeek(每周),DayOfMonth(每月),DesDate(指定日期),LoopDays(循环天数)
        /// </summary>
        public TimerTypes Type = TimerTypes.EveryDay;
        /// <summary>
        /// 日期值：DayOfWeek,值为1-7表示周一到周日；DayOfMonth,值为1-31表示1号到31号；TimeSpan,值为间隔分钟数；TimerType为其它值时,此值无效
        /// </summary>
        public int Value = 1;
        /// <summary>
        /// 指定开始执行日期
        /// </summary>
        public DateTime StartDate = DateTime.MinValue;
        /// <summary>
        /// 设置的执行时间 仅时间有效
        /// </summary>
        public DateTime RunTime = DateTime.Now;
    }

    /// <summary>
    /// 循环类型
    /// </summary>
    public enum TimerTypes
    {
        /// <summary>
        /// 每天
        /// </summary>
        EveryDay = 0,
        /// <summary>
        /// 每周 几
        /// </summary>
        DayOfWeek = 1,
        /// <summary>
        /// 每月 几日
        /// </summary>
        DayOfMonth = 2,
        /// <summary>
        /// 每隔几分钟执行一次
        /// </summary>
        TimeSpan = 3
    }
}