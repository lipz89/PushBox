namespace PushBox
{
    /// <summary>
    /// 游戏步骤
    /// </summary>
    class Step
    {
        /// <summary>
        /// 移动的方向：0左；1上；2右；3下
        /// </summary>
        public int Direction { get; private set; }

        /// <summary>
        /// 是否有箱子被移动
        /// </summary>
        public bool Push { get; private set; }

        /// <summary>
        /// 当前位置
        /// </summary>
        public int Position { get; private set; }
        public string State { get; set; }

        /// <summary>
        /// 实例化一个步骤
        /// </summary>
        /// <param name="positioni">当前位置</param>
        /// <param name="direction">移动的方向：0左；1上；2右；3下</param>
        /// <param name="push">是否有箱子被移动</param>
        public Step(int positioni, int direction, bool push)
        {
            this.Position = positioni;
            this.Direction = direction;
            this.Push = push;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", Position, Direction);
        }
    }
}