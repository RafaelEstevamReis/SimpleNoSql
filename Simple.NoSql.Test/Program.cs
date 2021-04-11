using System;

namespace Simple.NoSql.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            for (int i = 0; i <= 0xFF; i++)
            {
                Console.WriteLine($"Val {i:X2}");
            }
        }
    }
}
