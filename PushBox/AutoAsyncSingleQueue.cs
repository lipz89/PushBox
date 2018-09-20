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
    /// 普通并行深度优先搜索
    /// 多个线程同时搜索同一个队列，搜索过程中将新的局面添加到队列的尾部，
    /// 优点，相对于传统深度优先搜索能提高搜索效率，
    /// 缺点，由于线程分配的随机性，搜索算法会出现少量随机性，因此最终解偶尔会不是最优解
    /// 在多线程中对普通集合通过锁的方式限制异步要比使用线程安全的集合（System.Collections.Concurrent）速度快得多
    /// </summary>
    class AutoAsyncSingleQueue : BaseAuto
    {
        public AutoAsyncSingleQueue(Action<string> handler) : base(handler)
        {
            Backup(path);
        }

        private int Width;
        private int Depth;
        private int TaskCount;
        private CancellationTokenSource token;
        private const string path = "AutoAsyncSingleQueue.solve";
        private readonly object _lock = new object();

        public override List<int> Run(Game game)
        {
            var st = Stopwatch.StartNew();
            var state = RunMainAsync(game);
            st.Stop();
            List<int> paths = state?.GetPaths(); ;
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
                        Info = string.Format("最优解{0},线程峰值{1},队列峰值{2},耗时{3}ms", paths.Count, TaskCount, Width, st.ElapsedMilliseconds);
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

        private GameState RunMainAsync(Game game)
        {
            TaskCount = 0;
            token = new CancellationTokenSource();
            GameState result = null;
            Width = 0;
            var state = new GameState(game);
            var visitedStates = new List<string> { state.ToString() };
            var states = new Queue<GameState>();
            states.Enqueue(state);

            Action action = () =>
            {
                while (true)
                {
                    if (result == null)
                    {
                        GameState stt = null;
                        lock (_lock)
                        {
                            if (states.Any())
                                stt = states.Dequeue();
                            if (stt == null)
                                break;
                        }

                        if (Depth != stt.Depth)
                        {
                            Depth = stt.Depth;
                            //this.handler?.Invoke(string.Format("深度{0},队列峰值{1},当前队列{2}", Depth, Width, states.Count));
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
                                            states.Enqueue(newState);
                                            if (states.Count > Width)
                                            {
                                                Width = states.Count;
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        break;
                    }
                }
            };
            while (true)
            {
                if (result != null || states.Count == 0)
                {
                    token.Cancel();
                    break;
                }
                var l = states.Count / 30 + 1;
                while (TaskCount < l)
                {
                    TaskCount++;
                    Task.Run(action, token.Token);
                }
                Thread.Sleep(100);
            }
            return result;
        }

        public override void Stop()
        {
            token?.Cancel();
        }
    }
}