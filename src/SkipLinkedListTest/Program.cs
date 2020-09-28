using SkipLinkedListImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkipLinkedListTest
{
    class Program
    {
        static void Main(string[] args)
        {

            var skipLinkedList = new SkipLinkedList<int>();
            skipLinkedList.AddLast(1);
            skipLinkedList.AddLast(2);
            skipLinkedList.AddLast(3);

            foreach (var item in skipLinkedList)
            {
                Console.WriteLine(item);
            }

            Console.ReadKey();
        }
    }
}
