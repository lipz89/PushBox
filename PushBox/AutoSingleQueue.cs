using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PushBox
{
    /// <summary>
    /// 深度优先搜索
    /// 搜索过程中将搜索过的局面从队列移除，将新的局面添加到队列的尾部
    /// 测试结果显示速度和传统深度优先搜索几乎相同（在不进行更新界面信息的操作及队列空间和搜索深度的统计时）
    /// 如果实时需要更新界面信息的话还是建议使用传统深度优先搜索
    /// </summary>
    class AutoSingleQueue : BaseAuto
    {
        public AutoSingleQueue(Action<string> handler) : base(handler)
        {
            Backup(path);
        }

        private int Width;
        private int Depth;
        private const string path = "AutoSingleQueue.solve";

        public override List<int> Run(Game game)
        {
            var st = Stopwatch.StartNew();
            var state = RunMain(game);
            st.Stop();
            List<int> paths = state?.GetPaths(); ;
            using (var fs = File.Open(path, FileMode.Append))
            {
                using (var wr = new StreamWriter(fs))
                {
                    wr.WriteLine("关卡{0}:", game.Level);
                    if (paths == null)
                    {
                        Info = string.Format("无解,搜索深度{0},队列峰值{1},耗时{2}ms", Depth, Width, st.ElapsedMilliseconds);
                    }
                    else
                    {
                        Info = string.Format("最优解{0}步,队列峰值{1},耗时{2}ms", paths.Count, Width, st.ElapsedMilliseconds);
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

        private GameState RunMain(Game game)
        {
            Width = 0;
            var state = new GameState(game);
            var visitedStates = new List<string> { state.ToString() };
            var states = new Queue<GameState>();
            states.Enqueue(state);

            while (states.Any())
            {
                GameState stt = states.Dequeue();
                if (stt == null)
                {
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
                            return newState;
                        }

                        var str = newState.ToString();
                        var hasDeath = newState.HasDeathArea();
                        if (!visitedStates.Contains(str))
                        {
                            visitedStates.Add(str);
                            if (!hasDeath)
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
            return null;
        }

        public override void Stop()
        {

        }
    }
}