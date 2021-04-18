using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.NoSql.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = new NoSqlDB(".");

            Console.WriteLine(DateTime.Now);

            generate(db, 100_000).Wait();

            Console.WriteLine(DateTime.Now);

            //var data = new MyData()
            //{
            //    MyUID = Guid.Parse("c78f0541-57e8-417a-bd3c-413f741a7f5b"),
            //    MyWebsite = new Uri("https://example.com")
            //};
            //db.Insert("key123", data);
            //var d2 = db.Get<MyData>("key123");
        }
        private static async Task generate(NoSqlDB db, int ammount)
        {
            for (int i = 0; i < ammount; i++)
            {
                var data = new MyData()
                {
                    MyUID = Guid.NewGuid(),
                    MyWebsite = new Uri("https://example.com")
                };
                db.Insert(data.MyUID.ToString(), data);
            }
        }
    }
}
