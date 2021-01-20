#if _SERVER

using CommonLib.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonLib.Server
{
    public interface ITickable
    {
        void Tick(float delta);
        string Name { get; }
    }

    delegate void OnTick(float delta);

    class LoopWorker
    {
        private static readonly TimeSpan DISABLED = new TimeSpan(-1);
        private static readonly TimeSpan NOW = new TimeSpan(0);

        public readonly int uid;

        private bool active;
        private readonly TimeSpan interval;
        private readonly float delta;
        private OnTick work;
        private Timer timer;
        private DateTime lastRun;

        public LoopWorker(int id, int ticksPerSecond, OnTick workDelegate)
        {
            uid = id;
            active = true;
            interval = new TimeSpan((long)((1.0f / ticksPerSecond) * GameLoopServer.TICKS_IN_SECOND));
            work = workDelegate;
            timer = new Timer(DoWork, this, interval, DISABLED);
            lastRun = DateTime.Now;
            delta = 1.0f / ticksPerSecond;

            timer.Change(interval, DISABLED);
        }

        public void ScheduleNextRun()
        {
            var elapsed = DateTime.Now - lastRun;
            var localTimer = timer;

            if (localTimer == null)
                return;

            if (elapsed.Ticks < 10) //Error margin of 10 ticks
            {
                localTimer.Change(interval, DISABLED);
            }
            else
            {
                var newTime = interval.Subtract(elapsed);
#if _DEBUG
                if (newTime.Ticks < 10)
                {
                    CLog.W("Worker {0} took more ({1}ms) than it's tick interval ({2}ms). Something wrong isn't right.", work.Target, elapsed.Milliseconds, interval.Milliseconds);
                }
#endif
                localTimer.Change((newTime.Ticks < 10) ? NOW : newTime, DISABLED);
            }
        }

        public void DoWork(object state)
        {
            if (!active)
            {
                work = null; //Set this as null to avoid Memoryleak, since we are holding a reference to an object.
                timer = null;
                return;
            }

            lastRun = DateTime.Now;
            work(delta);
            ScheduleNextRun();
        }

        public void Deactive()
        {
            active = false;
        }
    }

    public abstract class GameLoopServer : BaseServer, ITickable
    {
        public static readonly float TICKS_IN_SECOND = 10000000f;
        protected uint _tps;

        protected Stopwatch _counter;

        private Dictionary<int, LoopWorker> _workers;

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public GameLoopServer(uint instanceId, string name, string version, uint ticksPerSecond) : base(instanceId, name, version)
        {
            _tps = ticksPerSecond;
            _workers = new Dictionary<int, LoopWorker>();
        }

        public void Register(ITickable work, int ticksPerSecond)
        {
            var loopWorker = new LoopWorker(work.Name.GetHashCode(), ticksPerSecond, work.Tick);

            lock (_workers)
            {
                _workers.Add(loopWorker.uid, loopWorker);
            }
        }

        public void Unregister(ITickable work)
        {
            lock (_workers)
            {
                if (_workers.TryGetValue(work.Name.GetHashCode(), out LoopWorker item))
                {
                    _workers.Remove(item.uid);
                    item.Deactive();
                }
            }
        }

        public override bool Start()
        {
            if (!base.Start())
                return false;

            OnStart();

            Register(this, (int)_tps);

            StartEventLoop();

            Unregister(this);

            return true;
        }

        protected abstract void OnStart();
        public abstract void Tick(float delta);
    }
}

#endif