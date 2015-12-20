using System;
using System.Diagnostics;

namespace PP
{
    class Program
    {
        private static void Main(string[] args)
        {
            var rand = new Random();
            var st = new Stopwatch();


            for (var i = 32; i <= 8192; i *= 2)
            {
                var element = new MatrixWoutSIMD.Element
                {
                    Value = int.MinValue,
                    Column = -1,
                    Row = -1
                };
                var element2 = new MatrixWithSIMD.Element
                {
                    Value = int.MinValue,
                    Column = -1,
                    Row = -1
                };

                var element3 = new MatrixWithSharpPar.Element
                {
                    Value = int.MinValue,
                    Column = -1,
                    Row = -1
                };
                var element4 = new MatrixWithSharpParWoutSimd.Element
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
                matrix2.FillMatrix(rand);
                var matrixSharpSimd = new MatrixWithSharpPar.Matrix(matrix.matrix);
                var matrixSharpSimd2 = new MatrixWithSharpPar.Matrix(matrix2.matrix);
                var matrixSharpWoutSimd = new MatrixWithSharpParWoutSimd.Matrix(matrix.matrix);
                var matrixSharpWoutSimd2 = new MatrixWithSharpParWoutSimd.Matrix(matrix2.matrix);
                var simdMatrix = new MatrixWithSimd.Matrix(matrix.matrix);
                var simdMatrix2 = new MatrixWithSimd.Matrix(matrix2.matrix);
                var vector = matrix.FillVector(rand);
                var SimdVector = simdMatrix.FillVector(vector);
                var vectorParSimd = matrixSharpSimd.FillVector(vector);
                var vectorParWoutSimd = matrixSharpWoutSimd.FillVector(vector);
                #endregion
                #region proccessing 
                
                Console.WriteLine("Max***********");
                st.Restart();
                element = matrix.GetMaxValue(element);
                st.Stop();
                Console.WriteLine("{0}, Time:{1}, Ticks:{2}", element, st.ElapsedMilliseconds, st.ElapsedTicks);
                st.Restart();
                element2 = simdMatrix.GetMaxValue(element2);
                st.Stop();
                Console.WriteLine("Simd: {0}, Time:{1}, Ticks:{2}", element2, st.ElapsedMilliseconds, st.ElapsedTicks);
                st.Restart();
                element3 = matrixSharpSimd.GetMaxValuePar(element3);
                st.Stop();
                Console.WriteLine("c#PArSimd: {0}, Time:{1}, Ticks:{2}", element3, st.ElapsedMilliseconds, st.ElapsedTicks);
                st.Restart();
                element4 = matrixSharpWoutSimd.GetMaxValuePar(element4);
                st.Stop();
                Console.WriteLine("c#PArWoutSimd: {0}, Time:{1}, Ticks:{2}", element4, st.ElapsedMilliseconds, st.ElapsedTicks);

                ///////end MAx
                Console.WriteLine("MulVector***********");
                st.Restart();
                var multWithVector = matrix.MultWithVector(vector);
                st.Stop();
                Console.WriteLine("Matrix was multiply with vector, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                st.Restart();
                var multWithSimdVector = simdMatrix.MultWithVector(SimdVector);
                st.Stop();
                Console.WriteLine("Simd: Matrix was multiply with vector, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                st.Restart();
                var multWithcParallelsSimd = matrixSharpSimd.MultWithVector(vectorParSimd);
                st.Stop();
                Console.WriteLine("C#ParallelsSimd: Matrix was multiply with vector, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                st.Restart();
                var multWithcParallelsWoutSimd = matrixSharpWoutSimd.MultWithVector(vectorParWoutSimd);
                st.Stop();
                Console.WriteLine("C#ParallelsWoutSimd: Matrix was multiply with vector, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                Console.WriteLine($"Is vectors equal: {MatrixHelper.IsVectorsEqual(multWithVector, multWithSimdVector)}");
                Console.WriteLine($"Is vectors equal: {MatrixHelper.IsVectorsEqual(multWithcParallelsSimd, multWithSimdVector)}");
                Console.WriteLine($"Is vectors equal: {MatrixHelper.IsVectorsEqual(multWithcParallelsSimd, multWithcParallelsWoutSimd)}");

                //////End MulWithVecor
                Console.WriteLine("FirstMulType***********");
                st.Restart();
                var res1 = matrix.MultipleMatrixVer1(matrix2);
                st.Stop();
                Console.WriteLine("Matrix was multiply with another matrix by simpe algoritm, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                st.Restart();
                var resSimd1 = simdMatrix.MultipleMatrixVer1(simdMatrix2);
                st.Stop();
                Console.WriteLine("Simd: SIMDMatrix was multiply with another matrix by simpe algoritm, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                st.Restart();
                var resSharpParSimd = matrixSharpSimd.MultipleMatrixVer1(matrixSharpSimd2);
                st.Stop();
                Console.WriteLine("SharpParalSimd: SIMDMatrix was multiply with another matrix by simpe algoritm, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                st.Restart();
                var resSharpParWoutSimd = matrixSharpWoutSimd.MultipleMatrixVer1(matrixSharpWoutSimd2);
                st.Stop();
                Console.WriteLine("SharpParalWoutSimd: SIMDMatrix was multiply with another matrix by simpe algoritm, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                Console.WriteLine($"Is Simd Matrix and old MAtrix equal {MatrixHelper.IsEqual(new MatrixWoutSIMD.Matrix(resSimd1).matrix, new MatrixWoutSIMD.Matrix(resSharpParSimd).matrix, resSharpParSimd.matrix.GetLength(0))}");
                Console.WriteLine($"Is Simd Matrix and old MAtrix equal {MatrixHelper.IsEqual(new MatrixWoutSIMD.Matrix(resSimd1).matrix, new MatrixWoutSIMD.Matrix(resSharpParWoutSimd).matrix, resSharpParWoutSimd.matrix.GetLength(0))}");

                ///EndSimpleMul
                Console.WriteLine("SeconMulType***********");
                st.Restart();
                var res2 = matrix.MultipleMatrixVer2(matrix2);
                st.Stop();
                Console.WriteLine("Matrix was multiply with another matrix by Strassen algorithm, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                st.Restart();
                var resSimd2 = simdMatrix.MultipleMatrixVer2(simdMatrix2);
                st.Stop();
                Console.WriteLine("Simd: Matrix was multiply with another matrix by Strassen algorithm, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                st.Restart();
                var resSharpParSimd2 = matrixSharpSimd.MultipleMatrixVer2(matrixSharpSimd2);
                st.Stop();
                Console.WriteLine("SharpParalWithtSimd2: SIMDMatrix was multiply with another matrix by simpe algoritm, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                Console.WriteLine("Is SIMD Matrix equal not Simd Matrix: {0}", MatrixHelper.IsEqual(new MatrixWoutSIMD.Matrix(resSharpParSimd2).matrix, res2.matrix, i));
                st.Restart();
                var resSharpParWoutSimd2 = matrixSharpWoutSimd.MultipleMatrixVer2SecondVar(matrixSharpWoutSimd2);
                st.Stop();
                Console.WriteLine("SharpParalWoutSimd2: SIMDMatrix was multiply with another matrix by simpe algoritm, Time:{0}, Ticks:{1}", st.Elapsed, st.ElapsedTicks);
                Console.WriteLine("Is SIMD Matrix equal not Simd Matrix: {0}", MatrixHelper.IsEqual(new MatrixWoutSIMD.Matrix(resSharpParWoutSimd2).matrix, res2.matrix, i));
                
                //MylMatrix(i);
                Console.WriteLine("**************************End by {0} size ******************************************", i);

                #endregion
            }

            Console.ReadLine();

        }

        private async static void MylMatrix(int i)
        {
            var rand = new Random();
            var st = new Stopwatch();
            var matrix = new MatrixWoutSIMD.Matrix(i);
            matrix.FillMatrix(rand);
            var matrix2 = new MatrixWoutSIMD.Matrix(i);
            matrix2.FillMatrix(rand);
            var matrixSharp = new MatrixWithSharpParWoutSimd.Matrix(matrix.matrix);
            var matrixSharp2 = new MatrixWithSharpParWoutSimd.Matrix(matrix2.matrix);
            st.Restart();
            var resSharpPar2 = await matrixSharp.MultipleMatrixVer2(matrixSharp2, matrixSharp2.size);
            st.Stop();
            Console.WriteLine("SharpParal: Matrix was multiply with another matrix by Strassen algorithm, Time:{0}, Ticks:{1}, SIZE{2}", st.Elapsed, st.ElapsedTicks, i);


        }
    }
}