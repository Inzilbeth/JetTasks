using System;


namespace Task2
{
    class Program
    {
        public static void Main(string[] args)
        {
            Action action = null;
            string path = @"C:\Users\Талгат\Desktop\dummy.txt";
            var filep = new FileParser();
            filep.Build(path, action);
            
        }
    }
}
