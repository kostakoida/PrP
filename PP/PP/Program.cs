using System;
using System.Diagnostics;
using System.Globalization;
using MatrixWoutSIMD;
using MatrixWithSIMD;

namespace PP
{
    class Program
    {
        private static void Main(string[] args)
        {
            var rand = new Random();
            var st = new Stopwatch();


            for (var i = 32; i <= 2048; i *= 2)
            {
                var element = new Element
                {
                    Value = int.MinValue,
                    Column = -1,
                    Row = -1
                };
                Console.WriteLine(i);
                #region init Matrix and vector 
                var matrix = new MatrixWoutSIMD.Matrix(i);
                var matrix2 = new MatrixWoutSIMD.Matrix(i);
                var simdMatrix = new MatrixWithSIMD.Matrix(matrix.matrix);
                var vector = matrix.FillVector(rand);
                var SimdVector = simdMatrix.FillVector(vector);
                matrix.FillMatrix(rand);
                matrix2.FillMatrix(rand);
                Console.WriteLine("Is Matrix which were multiply by 2 different algoitms equal: {0}", matrix.Equal(matrix2));

                #endregion
                #region proccessing
                st.Restart();
                matrix?.GetMaxValue(element);
                st.Stop();
                Console.WriteLine("{0}, Time:{1}, Ticks:{2}", element, st.ElapsedMilliseconds, st.ElapsedTicks);
                st.Restart();
                var multWithVector = matrix.MultWithVector(vector);
                st.Stop();
                Console.WriteLine("Matrix was multiply with vector, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                //st.Restart();
                //var res1 = matrix.MultipleMatrixVer1(matrix2);
                //st.Stop();
                //Console.WriteLine("Matrix was multiply with another matrix by simpe algoritm, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                //st.Restart();
                //var res2 = matrix.MultipleMatrixVer2(matrix2);
                //st.Stop();
                //Console.WriteLine("Matrix was multiply with another matrix by Strassen algorithm, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                //Console.WriteLine("Is Matrix which were multiply by 2 different algoitms equal: {0}", res1.Equal(res2));

                //Console.WriteLine("Is Matrix which were multiply by 2 different algoitms equal: {0}", Matrix.IsEqual(res1, res2, i));
                //Console.WriteLine("**************************End by {0} size ******************************************", i);
                Console.ReadLine();

                #endregion
            }


        }
    }
}
