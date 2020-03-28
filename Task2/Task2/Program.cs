using System;

namespace Task2
{
    class Program
    {
        public static void Main(string[] args)
        {
            var cache = new Cache();
            Action action = null;
            string path = @"C:\Users\Талгат\Desktop\SampleScene.unity";
            var filep = new AssetCache();
            cache = filep.Build(path, action);
            filep.WriteToFile();
        }
    }
}
