using System;

namespace KAMI.Linux
{
    class Program
    {
        static void Main(string[] args)
        {
            KAMICore kami = new KAMICore();
            kami.Start();
            Console.WriteLine(kami.Connected);
        }
    }
}
