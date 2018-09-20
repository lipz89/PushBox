using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PushBox
{
    /// <summary>
    /// 逐层并行深度优先搜索
    /// 逐层搜索，针对每一层开启多线程，每一层释放上一层的搜索队列并开辟一个新的搜索队列，
    /// 优点，相对于传统深度优先搜索能提高搜索效率，
    /// 缺点，由于线程分配的随机性，搜索算法会出现少量随机性，因此最终解偶尔会不是最优解
    /// </summary>
    class AutoAsync : BaseAuto
    {
        public AutoAsync(Action<string> handler) : base(handler)
        {
            Backup(path);
        }

        private int Width;
        private int Depth;
        private int TaskCount;
        private CancellationTokenSource token;
        private const string path = "AutoAsync.solve";
        private readonly object _lock = new object();
        private readonly List<string> visitedStates = new List<string>();
        private Queue<GameState> states;
        private Queue<GameState> newStates = new Queue<GameState>();
        private GameState result = null;
        private readonly List<Task> tasks = new List<Task>();

        public override List<int> Run(Game game)
        {
            var st = Stopwatch.StartNew();
            var paths = RunMain(game);
            st.Stop();
            using (var fs = File.Open(path, FileMode.Append))
            {
                using (var wr = new StreamWriter(fs))
                {
                    wr.WriteLine("关卡{0}:", game.Level);
                    if (paths == null)
                    {
                        Info = string.Format("无解,搜索深度{0},线程峰值{1},队列峰值{2},耗时{3}ms", Depth, TaskCount, Width, st.ElapsedMilliseconds);
                    }
                    else
                    {
                        Info = string.Format("最优解{0}步,线程峰值{1},队列峰值{2},耗时{3}ms", paths.Count, TaskCount, Width, st.ElapsedMilliseconds);
                        var str = string.Join("", paths.Select(x => x == 0 ? "左" : x == 1 ? "上" : x == 2 ? "右" : "下"));
                        wr.WriteLine(str);
                    }
                    wr.WriteLine(Info);
                    wr.WriteLine();
                    wr.Close();
                }
                fs.Close();
            }
            this.handler?.Invoke(Info);
            return paths;
        }

        private List<int> RunMain(Game game)
        {
            token = new CancellationTokenSource();
            TaskCount = 0;
            Width = 0;
            Depth = 0;
            var state = new GameState(game);
            visitedStates.Clear();
            visitedStates.Add(state.ToString());
            newStates.Clear();
            result = null;
            newStates.Enqueue(state);
            Action action = this.Search;
            while (true)
            {
                if (result != null)
                {
                    token.Cancel();
                    return result.GetPaths();
                }
                states = newStates;
                if (!states.Any())
                {
                    token.Cancel();
                    return null;
                }
                Depth++;
                if (states.Count > Width)
                {
                    Width = states.Count;
                }
                newStates = new Queue<GameState>();
                var l = states.Count / 30 + 1;
                TaskCount = 0;
                tasks.Clear();
                while (l-- > 0)
                {
                    TaskCount++;
                    tasks.Add(Task.Run(action, token.Token));
                }
                //this.handler?.Invoke(string.Format("深度{0},线程峰值{1},队列峰值{2},当前队列{3}", Depth,TaskCount, Width, states.Count));
                Task.WaitAll(tasks.ToArray());
            }
        }

        private void Search()
        {
            if (states.Count > Width)
            {
                Width = states.Count;
            }
            GameState stt;
            while (true)
            {
                stt = null;
                lock (_lock)
                {
                    if (states.Any())
                        stt = states.Dequeue();
                    if (stt == null)
                        return;
                }
                for (var dir = 0; dir < 4; dir++)
                {
                    if (stt.LastDir != -1 && !stt.HasPush && Math.Abs(stt.LastDir - dir) == 2)
                    {
                        continue;
                    }
                    var newState = stt.Move(dir);
                    if (newState != null)
                    {
                        if (newState.Check())
                        {
                            result = newState;
                            return;
                        }

                        var str = newState.ToString();
                        lock (_lock)
                        {
                            if (!visitedStates.Contains(str))
                            {
                                visitedStates.Add(str);
                                if (!newState.HasDeathArea())
                                {
                                    newStates.Enqueue(newState);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Stop()
        {
            token?.Cancel();
        }
    }
}