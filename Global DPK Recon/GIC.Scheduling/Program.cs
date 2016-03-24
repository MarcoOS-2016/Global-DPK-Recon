using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIC.Business;

namespace GIC.Scheduling
{
    public class Program
    {
        static void Main(string[] args)
        {
            InFileHandler infilehandler = new InFileHandler(DateTime.Now);
            infilehandler.Process();
        }
    }
}
