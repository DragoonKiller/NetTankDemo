using System;
using System.Web;
using System.IO;

namespace Utils
{

    public static class ExPath
    {
        public static string FullPath(this string path)
        {
            return Path.GetFullPath(path).Replace('\\', '/');
        }
        
        public static string RelativePathTo(this string path, string relativeTo)
        {
            var fa = new Uri(path);
            var fb = new Uri(relativeTo);
            return fa.MakeRelativeUri(fb).LocalPath;
        }
    }

}
