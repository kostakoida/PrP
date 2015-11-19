using System;
using System.Numerics;

namespace PP
{
    public class MatrixHelper
    {
        private readonly static int VectorSize = Vector<float>.Count;

        public static bool IsSimdVectorEqualNotSimd(float[] woutSimd, Vector<float>[] withSimd)
        {
            for (var i = 0; i < withSimd.Length; i++)
            {
                for (var j = 0; j < VectorSize; j++)
                {
                    var diffX = Math.Pow(10, GetError(woutSimd[i * VectorSize], withSimd[i][j]));
                    if (Math.Max(woutSimd[i * VectorSize], withSimd[i][j]) - Math.Min(woutSimd[i], withSimd[i][j]) > diffX)
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        public static int GetError(float a, float b)
        {
            if (a < b)
            {
                return GetError(b, a);
            }
            var isEnd = 0;
            var res = 1;
            b = 6;
            while (a > 0 && isEnd < 2)
            {
                a /= 10;
                res += b > 0 ? 0 : 1;
                b--;
                if (a < 10) isEnd++;
            }
            res += res >= 7 ? 1 : 0;
            return res;
        }

        public static bool IsVectorsEqual(float[] a, float[] b)
        {
            for (var i = 0; i < a.Length; i++)
            {
                var max = a[i] >= b[i] ? a[i] : b[i];
                var min = a[i] >= b[i] ? b[i] : a[i];
                if (max - min != 0)
                    Console.Write("");
                var diff = Math.Pow(10, GetError(a[i], b[i]));

                if (max - min > diff)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsEqual(float[,] matrix1, float[,] matrix2, int size)
        {
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    var max = matrix1[i, j] >= matrix2[i, j] ? matrix1[i, j] : matrix2[i, j];
                    var min = matrix1[i, j] >= matrix2[i, j] ? matrix2[i, j] : matrix1[i, j];
                    if (max - min != 0)
                        Console.Write("");
                    var diff = Math.Pow(10, GetError(matrix1[i, j], matrix2[i, j]));

                    if (max - min > diff)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
