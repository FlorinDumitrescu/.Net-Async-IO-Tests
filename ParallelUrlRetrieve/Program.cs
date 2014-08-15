using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelUrlRetrieve
{
    class Program
    {
        static void Main(string[] args)
        {
            //(new Tester()).TestParallel();
            //(new Tester()).TestParallel2();
            //(new Tester()).TestParallel3();
            //(new Tester()).TestSynchronous();
            //(new Tester()).TestSynchronous2();
            //(new Tester()).TestAsyncWithDelay();
            (new Tester()).TestAsync();
            //(new Tester()).TestAsync2();
            //(new Tester()).TestAsyncWithDelay2();
            Console.ReadLine();
        }        
    }
}
