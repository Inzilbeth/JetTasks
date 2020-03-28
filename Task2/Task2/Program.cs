using System;

namespace Task2
{
    class Program
    {
        public static void Main(string[] args)
        {
            object cache = new Cache();
            Action action = null;
            string path = @"C:\Users\Талгат\Desktop\dummy.txt";
            var filep = new AssetCache();
            cache = filep.Build(path, action);
            filep.WriteToFile(@"C:\Users\Талгат\Desktop\output.txt");
            Console.WriteLine(filep.GetGuidUsages("0000000000000000e000000000000000"));
            Console.WriteLine(filep.GetLocalAnchorUsages(6));
            foreach (ulong id in filep.GetComponentsFor(6))
            {
                Console.WriteLine(id);
            }
        }
    }
}
