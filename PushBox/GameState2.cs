using System;
using System.Collections.Generic;
using System.Linq;

namespace PushBox
{
    class GameState2 : GameState
    {
        private GameState2 LastState { get; }

        private readonly HashSet<int> freeArea = new HashSet<int>();
        private List<int> beforeMove;
        public Step LastMove { get; }
        public GameState2(Game game) : base(game)
        {
            this.CheckFreeArea(Y * W + X);
        }
        public GameState2(GameState2 game, Step move) : base(game, -1)
        {
            this.LastState = game;
            this.LastMove = move;
        }
        /// <summary>
        /// 移动一步
        /// </summary>
        /// <param name="ms">方向：0左；1上；2右；3下</param>
        public GameState2 Move(Step ms)
        {
            var state = new GameState2(this, ms);

            var lp = Y * W + X;
            state.Map[lp] = state.Map[lp] == 6 ? 3 : 0;
            int i = ms.Position;
            state.Map[i] = state.Map[i] == 3 ? 6 : 5;
            var dir = ms.Direction;
            int v = dir == 0 ? -1 : dir == 1 ? -W : dir == 2 ? 1 : W;
            if (state.Map[i + v] == 1)
            {
                return null;
            }
            if (state.Map[i + v] == 2 || state.Map[i + v] == 4)
            {
                if (state.Map[i + 2 * v] == 1 || state.Map[i + 2 * v] == 2 || state.Map[i + 2 * v] == 4)
                {
                    return null;
                }

                state.Map[i] = state.Map[i] == 6 ? 3 : 0;
                state.Map[i + v] = state.Map[i + v] == 4 ? 6 : 5;
                if (state.Map[i + 2 * v] == 3) state.Map[i + 2 * v] = 4;
                else if (state.Map[i + 2 * v] == 0) state.Map[i + 2 * v] = 2;
                state.HasPush = true;
            }
            else if (state.Map[i + v] == 0 || state.Map[i + v] == 3)
            {
                state.Map[i] = state.Map[i] == 6 ? 3 : 0;
                state.Map[i + v] = state.Map[i + v] == 3 ? 6 : 5;
            }
            state.X = (i + v) % W;
            state.Y = (i + v) / W;

            state.beforeMove = GetMoveFreeArea(lp, i);
            state.CheckFreeArea(i + v);
            return state;
        }

        public List<Step> GetPushs()
        {
            var paths = new List<Step>();
            var gs = this;
            while (gs?.LastMove != null)
            {
                paths.Add(gs.LastMove);
                gs.LastMove.State = gs.W + ";" + gs.H + ";" + gs;
                gs = gs.LastState;
            }

            if (paths.Any())
            {
                paths.Reverse();
                return paths;
            }
            return null;
        }
        public override List<int> GetPaths()
        {
            var paths = new List<int>();
            var gs = this;
            while (gs?.LastMove != null)
            {
                paths.Add(gs.LastMove.Direction);
                if (gs.beforeMove != null)
                {
                    paths.AddRange(gs.beforeMove);
                }
                gs = gs.LastState;
            }

            if (paths.Any())
            {
                paths.Reverse();
                return paths;
            }
            return null;
        }

        public override string ToString()
        {
            return string.Join("", Map).Replace("5", "0").Replace("6", "3") + ";f" + this.freeArea.Min();
        }
        /// <summary>
        /// 找到所有箱子的所有可能移动步骤
        /// </summary>
        /// <returns></returns>
        public List<Step> GetMovableStep()
        {
            var steps = new List<Step>();
            var boxs = Enumerable.Range(0, this.Map.Length).Where(IsBox);
            foreach (var box in boxs)
            {
                for (var dir = 0; dir < 4; dir++)
                {
                    int v = dir == 0 ? -1 : dir == 1 ? -W : dir == 2 ? 1 : W;
                    var pos = box - v;
                    var to = box + v;
                    if (IsFreeCell(to) && this.freeArea.Contains(pos))
                    {
                        if (to == this.LastMove?.Position && Math.Abs(dir - LastMove.Direction) == 2)
                        {
                            continue;
                        }
                        steps.Add(new Step(pos, dir, true));
                    }
                }
            }

            return steps;
        }

        /// <summary>
        /// 添加所有的可移动到的点到自由区域表
        /// 这里因为要遍历所有可能的点，时间复杂度一致的，空间复杂度尽量小，所以采用简单的递归回溯深度优先搜索算法，
        /// </summary>
        /// <param name="i"></param>
        private void CheckFreeArea(int i)
        {
            freeArea.Add(i);
            var list = new List<int>() { i };
            foreach (var i1 in list)
            {
                for (int dir = 0; dir < 4; dir++)
                {
                    int v = dir == 0 ? -1 : dir == 1 ? -W : dir == 2 ? 1 : W;
                    var t = i1 + v;
                    if (!freeArea.Contains(t) && IsFreeCell(t))
                    {
                        CheckFreeArea(t);
                    }
                }
            }
        }
        /// <summary>
        /// 计算从指定点到目标点的最短路径，
        /// 因为要求最优解，所以采用广度优先搜索算法
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public List<int> GetMoveFreeArea(int from, int to)
        {
            if (from == to)
            {
                return new List<int>();
            }

            var fromMove = new MoveStep() { To = from };
            HashSet<MoveStep> area2 = new HashSet<MoveStep>() { fromMove };
            var list = new List<MoveStep>() { fromMove };
            while (list.Any())
            {
                var newList = new List<MoveStep>();
                foreach (var i1 in list)
                {
                    for (int dir = 0; dir < 4; dir++)
                    {
                        int v = dir == 0 ? -1 : dir == 1 ? -W : dir == 2 ? 1 : W;
                        var t = i1.To + v;

                        var m = new MoveStep { Last = i1, To = t, Direction = dir };
                        if (t == to)
                        {
                            return m.GetMoves();
                        }

                        if (area2.All(x => x.To != t) && IsFreeCell(t))
                        {
                            area2.Add(m);
                            newList.Add(m);
                        }
                    }
                }

                list = newList;
            }

            return null;
        }

        private bool IsFreeCell(int i)
        {
            return this.Map[i] == 0 || this.Map[i] == 3 || this.Map[i] == 5 || this.Map[i] == 6;
        }
    }

    class MoveStep
    {
        public MoveStep Last { get; set; }
        public int To { get; set; }
        public int? Direction { get; set; }

        public List<int> GetMoves()
        {
            var moves = new List<int>();
            var ms = this;
            while (ms?.Direction.HasValue == true)
            {
                moves.Add(ms.Direction.Value);
                ms = ms.Last;
            }
            return moves;
        }
    }
}