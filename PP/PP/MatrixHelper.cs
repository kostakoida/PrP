using System;
using System.Numerics;

namespace PP
{
    public class MatrixHelper
    {
        public static bool IsSimdVectorEqualNotSimd(float[] woutSimd, Vector4[] withSimd)
        {
            for (var i = 0; i < withSimd.Length; i++)
            {
                var diffX = Math.Pow(10, GetError(woutSimd[i * 4], withSimd[i].X));
                var diffY = Math.Pow(10, GetError(woutSimd[i * 4 + 1], withSimd[i].Y));
                var diffW = Math.Pow(10, GetError(woutSimd[i * 4 + 2], withSimd[i].W));
                var diffZ = Math.Pow(10, GetError(woutSimd[i * 4 + 3], withSimd[i].Z));


                if (Math.Max(woutSimd[i * 4], withSimd[i].X) - Math.Min(woutSimd[i], withSimd[i].X) > diffX
                    && Math.Max(woutSimd[i * 4 + 1], withSimd[i].Y) - Math.Min(woutSimd[i], withSimd[i].Y) > diffY
                    && Math.Max(woutSimd[i * 4 + 2], withSimd[i].W) - Math.Min(woutSimd[i], withSimd[i].W) > diffW
                    && Math.Max(woutSimd[i * 4 + 3], withSimd[i].Z) - Math.Min(woutSimd[i], withSimd[i].Z) > diffZ)
                {
                    return false;
                }
            }
            return true;
        }


        public static float GetError(float a, float b)
        {
            return Math.Abs(a - b) / Math.Max(Math.Abs(a), Math.Abs(b));
        }

        public static bool IsVectorsEqual(float[] a, float[] b)
        {
            for (var i = 0; i < a.Length; i++)
            {
                if (Math.Abs(a[i] - b[i]) > GetError(a[i], b[i]))
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
                    if (Math.Abs(matrix1[i, j] - matrix2[i, j]) > GetError(matrix1[i, j], matrix2[i, j]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
