using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace EjemploParallel
{

    enum Modo
    {
        ParallelFor = 1,
        ParallelFor_ExclusionMutua = 2,
        ParallelFor_LimitadoPorCores = 3,
        ParallelInvoke = 4,
        ParallelForEach_ThreadLocalData = 5
    }
    class Program
    {      
        static void Main(string[] args)
        {
            Console.WriteLine(Operaciones.OperacionesSecuenciales() - Operaciones.OpMulti(Modo.ParallelForEach_ThreadLocalData));
            Console.ReadLine();
        }


    }
}
