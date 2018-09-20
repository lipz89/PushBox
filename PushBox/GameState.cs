using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;

namespace PushBox
{
    class GameState
    {
        private GameState LastState { get; }

        public int Depth { get; protected set; }
        /// <summary>
        /// 游戏地图数组
        /// </summary>
        protected int[] Map { get; set; }

        public int Y { get; protected set; }

        public int X { get; protected set; }

        public int W { get; protected set; }

        public int H { get; protected set; }

        public bool HasPush { get; protected set; }

        public int LastDir { get; } = -1;

        protected GameState()
        {
        }

        public GameState(Game game)
        {
            this.Depth = 1;
            this.Map = game.Map.ToArray();
            this.X = game.X;
            this.Y = game.Y;
            this.W = game.W;
            this.H = game.H;
        }
        public GameState(GameState game, int dir)
        {
            this.Map = game.Map.ToArray();
            this.LastState = game;
            this.X = game.X;
            this.Y = game.Y;
            this.W = game.W;
            this.H = game.H;
            this.LastDir = dir;
            this.Depth = game.Depth + 1;
        }
        /// <summary>
        /// 移动一步
        /// </summary>
        /// <param name="dir">方向：0左；1上；2右；3下</param>
        public GameState Move(int dir)
        {
            var state = new GameState(this, dir);
            int i = X + Y * W;
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
            return state;
        }

        public bool Check()
        {
            return !this.Map.Any(x => x == 2);
        }

        public bool HasDeathArea()
        {
            if (!HasPush)
            {
                return false;
            }

            var boxs = Enumerable.Range(0, this.Map.Length).Where(x => this.Map[x] == 2).ToList();
            return boxs.Any(IsDeath) || boxs.Any(IsDeath2);
        }

        protected bool IsDeath2(int i)
        {
            //var lv = new List<int>();
            //var lh = new List<int>();
            return IsDeathH(i/*, lv, lh*/) && IsDeathV(i/*, lv, lh*/);
        }

        protected bool IsDeathH(int i/*, List<int> lh, List<int> lv*/)
        {
            var _l = i - 1;
            var _r = i + 1;
            if (IsWall(_l) || IsWall(_r))
            {
                return true;
            }

            //if (!lh.Contains(_l))
            //{
            //    lh.Add(_l);
            if (IsBox(_l) && IsDeathV(_l/*, lv, lh*/))
            {
                return true;
            }
            //}
            //else
            //{
            //    return true;
            //}

            //if (!lh.Contains(_r))
            //{
            //    lh.Add(_r);
            if (IsBox(_r) && IsDeathV(_r/*, lv, lh*/))
            {
                return true;
            }
            //}
            //else
            //{
            //    return true;
            //}

            return false;
        }

        protected bool IsDeathV(int i/*, List<int> lv, List<int> lh*/)
        {
            var _u = i - W;
            var _d = i + W;
            if (IsWall(_u) || IsWall(_d))
            {
                return true;
            }

            //if (!lv.Contains(_u))
            //{
            //    lv.Add(_u);
            if (IsBox(_u) && IsDeathH(_u/*, lv, lh*/))
            {
                return true;
            }
            //}
            //else
            //{
            //    return true;
            //}

            //if (!lv.Contains(_d))
            //{
            //    lv.Add(_d);
            if (IsBox(_d) && IsDeathH(_d/*, lv, lh*/))
            {
                return true;
            }
            //}
            //else
            //{
            //    return true;
            //}

            return false;
        }

        protected bool IsDeath(int i)
        {
            var _l = i - 1;
            var _u = i - W;
            var _lu = _u - 1;
            if (IsWall(_l) && IsWall(_u))
            {
                return true;
            }
            if (IsWallOrBox(_l) && IsWallOrBox(_u) && IsWallOrBox(_lu))
            {
                return true;
            }
            var _r = i + 1;
            var _ru = _u + 1;
            if (IsWall(_r) && IsWall(_u))
            {
                return true;
            }
            if (IsWallOrBox(_r) && IsWallOrBox(_u) && IsWallOrBox(_ru))
            {
                return true;
            }
            var _d = i + W;
            var _ld = _d - 1;
            if (IsWall(_l) && IsWall(_d))
            {
                return true;
            }
            if (IsWallOrBox(_l) && IsWallOrBox(_d) && IsWallOrBox(_ld))
            {
                return true;
            }

            var _rd = _d + 1;
            if (IsWall(_r) && IsWall(_d))
            {
                return true;
            }
            if (IsWallOrBox(_r) && IsWallOrBox(_d) && IsWallOrBox(_rd))
            {
                return true;
            }
            return false;
        }

        protected bool IsWallOrBox(int i)
        {
            return IsWall(i) || IsBox(i);
        }
        protected bool IsBox(int i)
        {
            return this.Map[i] == 2 || this.Map[i] == 4;
        }

        protected bool IsWall(int i)
        {
            return this.Map[i] == 1;
        }

        public virtual List<int> GetPaths()
        {
            var paths = new List<int>();
            var gs = this;
            while (gs != null && gs.LastDir > -1)
            {
                paths.Insert(0, gs.LastDir);
                gs = gs.LastState;
            }
            return paths.Any() ? paths : null;
        }

        public override string ToString()
        {
            return string.Join("", Map);
        }
    }
}