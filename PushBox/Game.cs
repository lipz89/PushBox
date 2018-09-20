using System.Collections.Generic;
using System.Linq;

namespace PushBox
{
    /// <summary>
    /// 推箱子游戏类
    /// </summary>
    class Game
    {
        private int level, boxCount;
        private readonly List<Step> back = new List<Step>();

        /// <summary>
        /// 实例化一个游戏
        /// </summary>
        public Game()
        {
            level = 0;
            Step = 0;
            InitGame();
        }
        /// <summary>
        /// 游戏级别
        /// </summary>
        public int Level
        {
            get { return level + 1; }
        }
        /// <summary>
        /// 游戏移动的步数
        /// </summary>
        public int Step { get; private set; }

        /// <summary>
        /// 游戏地图数组
        /// </summary>
        public int[] Map { get; private set; }

        /// <summary>
        /// 游戏完成过关
        /// </summary>
        public bool IsFinished { get; private set; }

        public int X { get; private set; }

        public int Y { get; private set; }

        public int W { get; private set; }

        public int H { get; private set; }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        public void InitGame()
        {
            Step = 0;
            IsFinished = false;
            CreateMap();
            boxCount = Map.Count(i => i == 2 || i == 4);
            back.Clear();
        }
        /// <summary>
        /// 创建地图
        /// </summary>
        private void CreateMap()
        {
            var map = Maps.AllMap[level];
            var ms = map.Split(';');
            W = int.Parse(ms[0]);
            H = int.Parse(ms[1]);
            Map = new int[ms[2].Length];
            for (int i = 0; i < Map.Length; i++)
            {
                Map[i] = ms[2][i] - 48;
                if (Map[i] == 5 || Map[i] == 6)
                {
                    X = i % W;
                    Y = i / W;
                }
            }
        }

        public void LoadFrom(string map)
        {
            var ms = map.Split(';');
            W = int.Parse(ms[0]);
            H = int.Parse(ms[1]);
            Map = new int[ms[2].Length];
            for (int i = 0; i < Map.Length; i++)
            {
                Map[i] = ms[2][i] - 48;
                if (Map[i] == 5 || Map[i] == 6)
                {
                    X = i % W;
                    Y = i / W;
                }
            }
        }
        /// <summary>
        /// 加载下一级别
        /// </summary>
        public void LoadNextLevel()
        {
            if (level < Maps.AllMap.Count - 1)
                level++;
            InitGame();
        }
        /// <summary>
        /// 加载上一级别
        /// </summary>
        public void LoadPrevLevel()
        {
            if (level > 0)
                level--;
            InitGame();
        }
        /// <summary>
        /// 移动一步
        /// </summary>
        /// <param name="dir">方向：0左；1上；2右；3下</param>
        public void Move(int dir)
        {
            int i = X + Y * W;
            bool push = false;
            int v = dir == 0 ? -1 : dir == 1 ? -W : dir == 2 ? 1 : W;
            if (Map[i + v] == 1)
            {
                return;
            }
            if (Map[i + v] == 2 || Map[i + v] == 4)
            {
                if (Map[i + 2 * v] == 1 || Map[i + 2 * v] == 2 || Map[i + 2 * v] == 4)
                {
                    return;
                }
                else
                {
                    Map[i] = Map[i] == 6 ? 3 : 0;
                    Map[i + v] = Map[i + v] == 4 ? 6 : 5;
                    if (Map[i + 2 * v] == 3) Map[i + 2 * v] = 4;
                    else if (Map[i + 2 * v] == 0) Map[i + 2 * v] = 2;
                    push = true;
                }
            }
            else if (Map[i + v] == 0 || Map[i + v] == 3)
            {
                Map[i] = Map[i] == 6 ? 3 : 0;
                Map[i + v] = Map[i + v] == 3 ? 6 : 5;
            }
            X = (i + v) % W;
            Y = (i + v) / W;
            back.Add(new Step(i, dir, push));
            Step++;
            IsFinished = boxCount == InPlaceCount();
        }
        /// <summary>
        /// 后退一步
        /// </summary>
        public void Back()
        {
            if (back.Count > 0)
            {
                int dir = back[back.Count - 1].Direction;
                int i = back[back.Count - 1].Position;
                bool push = back[back.Count - 1].Push;
                int v = dir == 0 ? -1 : dir == 1 ? -W : dir == 2 ? 1 : W;
                Map[i] = Map[i] == 0 ? 5 : 6;
                if (push)
                {
                    Map[i + v] = Map[i + v] == 5 ? 2 : 4;
                    Map[i + 2 * v] = Map[i + 2 * v] == 2 ? 0 : 3;
                }
                else
                {
                    Map[i + v] = Map[i + v] == 5 ? 0 : 3;
                }
                X = i % W;
                Y = i / W;
                back.RemoveAt(back.Count - 1);
                Step--;
            }
        }
        /// <summary>
        /// 计算就位的箱子数目
        /// </summary>
        /// <returns>就位的箱子数目</returns>
        private int InPlaceCount()
        {
            return Map.Count(t => t == 4);
        }
    }
}
