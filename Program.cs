using System;
using BTM.Text;
using BTM.Hashmap;
using System.Collections.Generic;
using System.Collections;

namespace BTM
{
    class Program
    {
        static void Main(string[] args)
        {
            BTMSystem.GetInstance().SetBaseExample();

            Terminal terminal = new Terminal();
            terminal.Run();
        }
    }
}
