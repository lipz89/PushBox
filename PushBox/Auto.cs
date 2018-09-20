using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PushBox
{
    /// <summary>
    /// 传统算法，深度优先搜索
    /// 逐层搜索，每一层释放上一层的搜索队列并开辟一个新的搜索队列，
    /// 优点，算法稳定
    /// 缺点，耗费空间，单线程搜索对于复杂的局面耗时很长
    /// </summary>
    class Auto : BaseAuto
    {
        public Auto(Action<string> handler) : base(handler)
        {
            Backup(path);
        }

        private int Width;
        private int Depth;
        private const string path = "Auto.solve";

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

        private List<int> RunMain(Game game)
        {
            Depth = 0;
            Width = 0;
            var state = new GameState(game);
            var visitedStates = new List<string> { state.ToString() };
            var states = new List<GameState> { state };

            while (true)
            {
                Depth++;
                if (!states.Any())
                {
                    return null;
                }

                if (states.Count > Width)
                {
                    Width = states.Count;
                }
                //this.handler?.Invoke(string.Format("深度{0},当前队列{1},队列峰值{2}", Depth, states.Count, Width));
                var newStates = new List<GameState>();
                foreach (var stt in states)
                {
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
                                return newState.GetPaths();
                            }
                            var str = newState.ToString();
                            if (!visitedStates.Contains(str))
                            {
                                visitedStates.Add(str);
                                if (!newState.HasDeathArea())
                                {
                                    newStates.Add(newState);
                                }
                            }
                        }

                    }
                }
                states = newStates;
            }
        }

        public override void Stop()
        {

        }
    }
}