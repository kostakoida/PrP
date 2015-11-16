using System;
using System.Numerics;
using MatrixWoutSIMD;

namespace PP
{
    public class MatrixHelper
    {
        public static bool IsSimdVectorEqualNotSimd(float[] woutSimd, Vector4[] withSimd)
        {
            for (var i = 0; i < withSimd.Length; i++)
            {
                var diffX = Math.Pow(10, getError(woutSimd[i], withSimd[i].X));
                var diffY = Math.Pow(10, getError(woutSimd[i + 1], withSimd[i].Y));
                var diffW = Math.Pow(10, getError(woutSimd[i + 2], withSimd[i].W));
                var diffZ = Math.Pow(10, getError(woutSimd[i + 3], withSimd[i].Z));


                if (Math.Max(woutSimd[i],withSimd[i].X) - Math.Min(woutSimd[i], withSimd[i].X) > diffX
                    && Math.Max(woutSimd[i], withSimd[i].Y) - Math.Min(woutSimd[i], withSimd[i].Y) > diffY
                    && Math.Max(woutSimd[i], withSimd[i].W) - Math.Min(woutSimd[i], withSimd[i].W) > diffW
                    && Math.Max(woutSimd[i], withSimd[i].Z) - Math.Min(woutSimd[i], withSimd[i].Z) > diffZ)
                {
                    return false;
                }
            }
            return true;
        }
        

        public static int getError(float a, float b)
        {
            if (a < b)
            {
                return getError(b, a);
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
    }
}
