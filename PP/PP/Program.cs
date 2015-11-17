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


            for (var i = 4; i <= 2048; i *= 2)
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
                matrix.FillMatrix(rand);
                var matrix2 = new MatrixWoutSIMD.Matrix(i);
                var simdMatrix = new MatrixWithSIMD.Matrix(matrix.matrix);
                var simdMatrix2 = new MatrixWithSIMD.Matrix(matrix2.matrix);
                var vector = matrix.FillVector(rand);
                var SimdVector = simdMatrix.FillVector(vector);

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
                st.Restart();
                var multWithSimdVector = simdMatrix.MultWithVector(SimdVector);
                st.Stop();
                Console.WriteLine("Matrix was multiply with vector, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                Console.WriteLine($"Is vectors equal: {MatrixHelper.IsVectorsEqual(multWithVector, multWithSimdVector)}");
                st.Restart();
                var res1 = matrix.MultipleMatrixVer1(matrix2);
                st.Stop();
                Console.WriteLine("Matrix was multiply with another matrix by simpe algoritm, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                st.Restart();
                var resSimd1 = simdMatrix.MultipleMatrixVer1(simdMatrix2); 
                st.Stop();
                Console.WriteLine("SIMDMatrix was multiply with another matrix by simpe algoritm, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                Console.WriteLine($"Is Simd Matrix and old MAtrix equal {MatrixHelper.IsEqual(res1.matrix, new MatrixWoutSIMD.Matrix(resSimd1).matrix, res1.matrix.GetLength(0))}");
                st.Restart();
                var res2 = matrix.MultipleMatrixVer2(matrix2);
                st.Stop();
                Console.WriteLine("Matrix was multiply with another matrix by Strassen algorithm, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                st.Restart();
                var resSimd2 = simdMatrix.MultipleMatrixVer2(simdMatrix2);
                st.Stop();
                Console.WriteLine("Matrix was multiply with another matrix by Strassen algorithm, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                Console.WriteLine("Is Matrix which were multiply by 2 different algoritms equal: {0}", MatrixHelper.IsEqual(res1.matrix, res2.matrix, i));
                Console.WriteLine("Is SIMD Matrix equal not Simd Matrix: {0}", MatrixHelper.IsEqual(new MatrixWoutSIMD.Matrix(resSimd2).matrix, res2.matrix, i));
                Console.WriteLine("**************************End by {0} size ******************************************", i);

                #endregion
            }

            Console.ReadLine();

        }
    }
}
