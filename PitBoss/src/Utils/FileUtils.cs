using System;
using System.IO;
using System.Linq;

namespace PitBoss.Utils
{
    public static class FileUtils
    {
        public static string GetBasePath()
        {
            var startDir = Directory.GetCurrentDirectory();
            var currentDir = startDir;
            while(Directory.GetDirectoryRoot(startDir) != currentDir)
            {
                if(Directory.GetDirectories(currentDir).Select(x => Path.GetFileName(x)).Contains(".git"))
                {
                    return currentDir;
                }
                currentDir = Directory.GetParent(currentDir).FullName;
            }
            return Directory.GetDirectoryRoot(startDir);
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}