using System;
using System.Reflection;

namespace lightchain.db
{
    class Program
    {
        static void Main(string[] args)
        {
            var version = Assembly.GetEntryAssembly().GetName().Version;
            Console.WriteLine("lightchain " +version);
            while (true)
            {
                Console.Write("-->");
                Console.ReadLine();
            }
        }
    }
}
