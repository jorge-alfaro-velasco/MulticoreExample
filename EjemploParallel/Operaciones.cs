using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace EjemploParallel
{
    class Operaciones
    {
        static Stopwatch temporizador;
        static Int64 cont, dValorPow, dValorPow1, dValorPow2;
        static Int64 lim = 1000000000; //Límite es igual a 1 billón 

        /// <summary>
        /// Método sencuencial. Se realiza una cantidad determinada de iteraciones y en cada
        /// una se calcula la potencia a la 2 para el contador (cont * cont)
        /// </summary>
        /// <returns>Retorna la sumatoria de las potencias efectuadas en las iteraciones</returns>
        public static double OperacionesSecuenciales()
        {
            temporizador = Stopwatch.StartNew(); //Se toma el tiempo que toman en ejecutarse las instrucciones
            dValorPow = 0; cont = 0;

            while (cont < lim)
            {
                dValorPow += Convert.ToInt64(Math.Pow(cont, 2));
                cont += 1;
            }   

            Console.WriteLine();
            Console.WriteLine("Tiempo de ejecución - modalidad secuencial: " + temporizador.ElapsedMilliseconds + " milisegundos");
            Console.WriteLine("Resultado = " + dValorPow);
            Console.WriteLine("");

            return dValorPow;
        }

        public static double OpMulti(Modo _modo)
        {
            dValorPow1 = 0; dValorPow2 = 0; cont = 0;
            temporizador = Stopwatch.StartNew();

            /*
             * Se utiliza la versión del ciclo (FOR) en modo paralelo. 
             * Observemos como se presentan inconvenientes en el resultado, debido 
             * a la posibilidad de que múltiples instancias o subprocesos
             * accedan al cuerpo del método.
             */
            if (_modo.Equals(Modo.ParallelFor))
            {
                Parallel.For(0, lim, cont =>
                {
                    dValorPow1 += Convert.ToInt64(Math.Pow(cont, 2));
                    //cont += 1;
                });

                Console.WriteLine("Parallel.For");
            }
            /*
             * Para evitar los inconvenientes ocasionados por la concurrencia
             * al utilizar variables compartidas, se utiliza una instrucción que 
             * indica que ciertas instrucciones deben ejecutarse en exclusión mutua. 
             * 
             * En este caso se utiliza un lock(). Si bien se soluciona el problema, 
             * se afecta el rendimiento o las ventajas del multiprocesamiento
             */
            else if (_modo.Equals(Modo.ParallelFor_ExclusionMutua))
            {
                object sync = new object();

                Parallel.For(0, lim, cont =>
                {
                    /*
                     * Dentro del lock, las intrucciones únicamente pueden ser ejecutadas por 
                     * un proceso o instancia a la vez
                     */
                    lock (sync)
                    {
                        dValorPow1 += Convert.ToInt64(Math.Pow(cont, 2));
                    }
                });

                Console.WriteLine("Parallel.For con exclusión mútua (LOCK)");

            }
            /*
             * Se introducen condiciones sobre la ejecución multicore de manera
             * que limita el número de procesos a la cantidad de CPU´s 
             * detectados (físicos y lógicos)
             */
            else if (_modo.Equals(Modo.ParallelFor_LimitadoPorCores))
            {
                int degreeOfParallelism = Environment.ProcessorCount;
                object sync = new object();

                Parallel.For(0, degreeOfParallelism, workerId =>
                {
                    var max = lim * (workerId + 1) / degreeOfParallelism;
                    for (int i = (int)lim * workerId / degreeOfParallelism; i < max; i++)
                        lock (sync)
                        {
                            dValorPow1 += Convert.ToInt64(Math.Pow(i, 2));
                        }
                });

                Console.WriteLine("Parallel.For limitado por los (" + degreeOfParallelism + ") procesadores detectados");

            }
            /*
             * Se utiliza el método Parallel.Invoke, que permite 
             * ejecutar en paralelo (en la medida de lo posible) las acciones 
             * que se le suministran.
             * 
             * En este caso, se divide en dos las iteraciones, de manera 
             * que se emplean dos ciclos para realizar los cálculos en forma simultánea
             */
            else if (_modo.Equals(Modo.ParallelInvoke))
            {
                Parallel.Invoke(() =>
                {
                    for (double cont = 0; cont < (lim / 2); cont++)
                    {
                        dValorPow1 += Convert.ToInt64(Math.Pow(cont, 2));
                    }
                }
                , () =>
                {
                    for (double cont = (lim / 2); cont < lim; cont++)
                    {
                        dValorPow2 += Convert.ToInt64(Math.Pow(cont, 2));
                    }
                });

                Console.WriteLine("Parallel.Invoke()");

            }
            /*
             * Se utiliza el mecanismo de "hilos locales a nivel de datos" o "thread-local data", 
             * de manera que se realizan los cálculos sin necesidad de acceder direactamente al recurso compartido. 
             * 
             * Se ejecutan las tareas en hilos-locales y se actualiza el resultado una vez que las 
             * iteraciones de la tarea se han completado.
             */
            else if (_modo.Equals(Modo.ParallelForEach_ThreadLocalData))
            {
                object sync = new object();

                Parallel.ForEach(Enumerable.Range(0, Convert.ToInt32(lim)),
                    () => 0L,
                    (value, pls, localTotal) =>
                    {
                        return localTotal += Convert.ToInt64(Math.Pow(value, 2));
                    },
                    localTotal =>
                    {
                        lock (sync)
                        {
                            dValorPow1 += localTotal;
                        }
                    });

                Console.WriteLine("Parallel.ForEach with thread-local data");
            }

            Console.WriteLine("Tiempo de ejecución - modalidad concurrente: " + temporizador.ElapsedMilliseconds + " milisegundos");
            Console.WriteLine("Resultado = " + (dValorPow1 + dValorPow2));         

            return (dValorPow1 + dValorPow2);
        }
    }
}
