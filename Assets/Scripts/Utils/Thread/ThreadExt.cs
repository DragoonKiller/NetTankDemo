using System;
using System.Threading;

namespace Utils
{
    public static class ThreadExt
    {
        
        public static Thread Start(Action t)
        {
            var thr = new Thread(new ThreadStart(t));
            thr.Start();
            return thr;
        }
    }
    
}
