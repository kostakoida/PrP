﻿
using System;
using System.Linq;
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


        public static double GetError(float a, float b)
        {
            var diff = Math.Abs(a) > Math.Pow(10, 7) ? GetBigError(a, b) : GetSmallError(a, b);
            if (diff > Math.Abs(a) / 10)
                Console.WriteLine($"!!!!!!!!!!!!!!!!!!!!!!!!!!!{diff}  {Math.Abs(a) / 10}");
            return diff;
        }

        private static float GetSmallError(float a, float b)
        {
            return Math.Abs(a - b) / Math.Max(Math.Abs(a), Math.Abs(b));
        }

        private static double GetBigError(float a, float b)
        {
            if (a < b)
            {
                return GetBigError(b, a);
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
            return Math.Pow(10, res);
        }

        public static bool IsVectorsEqual(float[] a, float[] b)
        {
            return !a.Where((t, i) => Math.Abs(t - b[i]) > GetError(t, b[i])).Any();
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