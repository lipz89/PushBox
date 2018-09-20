using System;
using System.Collections.Generic;
using System.IO;

namespace PushBox
{
    internal interface IAuto
    {
        List<int> Run(Game game);

        void Stop();
    }

    abstract class BaseAuto : IAuto
    {
        protected Action<string> handler;

        protected BaseAuto(Action<string> handler)
        {
            this.handler = handler;
        }

        public string Info { get; protected set; }

        public abstract List<int> Run(Game game);

        public abstract void Stop();

        protected static void Backup(string path)
        {

            if (File.Exists(path))
            {
                File.Move(path, path + "." + DateTime.Now.ToString("yyyyMMddHHmmss.backup"));
            }
        }
    }
}