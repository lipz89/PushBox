using System.Collections.Generic;
using System.IO;

namespace PushBox
{
    /// <summary>
    /// 游戏地图集合
    /// </summary>
    class Maps
    {
        static Maps()
        {
            string path = "PushBox.map";
            using (var fs = File.OpenRead(path))
            {
                using (var fr = new StreamReader(fs))
                {
                    string line;
                    while (!string.IsNullOrWhiteSpace(line = fr.ReadLine()))
                    {
                        map.Add(line);
                    }
                    fr.Close();
                }
                fs.Close();
            }
        }
        private static readonly List<string> map = new List<string>();

        /// <summary>
        /// 返回所有地图
        /// </summary>
        public static List<string> AllMap { get; } = Maps.map;
    }
}
