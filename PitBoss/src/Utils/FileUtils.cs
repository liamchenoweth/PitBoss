using System;
using System.IO;
using System.Linq;
using System.Text;  
using System.Security.Cryptography;

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

        public static string Sha256Hash(string value)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())  
            {  
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(value));  
  
                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();  
                for (int i = 0; i < bytes.Length; i++)  
                {  
                    builder.Append(bytes[i].ToString("x2"));  
                }  
                return builder.ToString();  
            }
        }
    }
}