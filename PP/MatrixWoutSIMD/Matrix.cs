using System;

namespace MatrixWoutSIMD
{
    public class Matrix
    {
        #region properties 
        private const int MaxNum = 100000;
        public float[,] matrix;
        private readonly int _size;
        private static readonly string Ls = Environment.NewLine;
        #endregion

        #region init 
        public Matrix(int size)
        {
            if (size <= 0)
            {
                Console.WriteLine("Incorrect size");
                return;
            }
            matrix = new float[size, size];
            _size = size;
        }

        public Matrix(float[,] matrix)
        {
            this.matrix = matrix;
            _size = matrix.GetLength(0);
        }

        public Matrix(MatrixWithSimd.Matrix simdMatrix)
        {
            this._size = simdMatrix.matrix.GetLength(0);
            this.matrix = new float[_size, _size];
            for (var i = 0; i < _size; i++)
            {
                for (var j = 0; j < _size; j += 4)
                {
                    matrix[i, j] = simdMatrix.matrix[i, j / 4].X;
                    matrix[i, j + 1] = simdMatrix.matrix[i, j / 4].Y;
                    matrix[i, j + 2] = simdMatrix.matrix[i, j / 4].Z;
                    matrix[i, j + 3] = simdMatrix.matrix[i, j / 4].W;
                }
            }
        }

        public Matrix(MatrixWithSharpParWoutSimd.Matrix m)
        {
            this._size = m.matrix.GetLength(0);
            this.matrix = new float[_size, _size];
            for (var i = 0; i < _size; i++)
            {
                for (var j = 0; j < _size; j++)
                {
                    matrix[i, j] = m.matrix[i, j];
                }
            }
        }

        public Matrix(MatrixWithSharpPar.Matrix simdMatrix)
        {
            this._size = simdMatrix.matrix.GetLength(0);
            this.matrix = new float[_size, _size];
            for (var i = 0; i < _size; i++)
            {
                for (var j = 0; j < _size; j += 4)
                {
                    matrix[i, j] = simdMatrix.matrix[i, j / 4].X;
                    matrix[i, j + 1] = simdMatrix.matrix[i, j / 4].Y;
                    matrix[i, j + 2] = simdMatrix.matrix[i, j / 4].Z;
                    matrix[i, j + 3] = simdMatrix.matrix[i, j / 4].W;
                }
            }
        }

        //заполнение матрицы 
        public void FillMatrix(Random rand)
        {
            for (var i = 0; i < _size; i++)
                for (var j = 0; j < _size; j++)
                {
                    matrix[i, j] = rand.Next(0, MaxNum);
                }
        }


        public float[] FillVector(Random rand)
        {
            var vector = new float[_size];
            for (var i = 0; i < _size; i++)
                vector[i] = rand.Next(MaxNum);
            return vector;
        }

        //метод вывода матрицы в консоль 
        public void Output()
        {
            for (var i = 0; i < _size; i++)
            {
                for (var j = 0; j < _size; j++)
                    Console.Write("{0},", matrix[i, j]);
                Console.Write(Ls);
            }
        }

        //вывод вектора в консоль 
        public static void OutputVector(float[] vector)
        {
            foreach (var i in vector)
            {
                Console.Write("{0},", i);
            }
            Console.Write(Ls);
        }

        #endregion

        #region methods 
        //поиск максимального элемента 
        public Element GetMaxValue(Element element)
        {
            for (var i = 0; i < _size; i++)
            {
                for (var j = 0; j < _size; j++)
                {
                    if (!(matrix[i, j] > element.Value)) continue;
                    element.Value = matrix[i, j];
                    element.Row = i;
                    element.Column = j;
                }
            }
            return element;
        }

        //Умножение матрицы на вектор 
        public float[] MultWithVector(float[] vector)
        {
            if (vector.Length != _size)
                return null;
            var res = new float[_size];
            for (var i = 0; i < _size; i++)
            {
                float sum = 0;
                for (var j = 0; j < _size; j++)
                {
                    sum += matrix[i, j] * vector[j];
                }
                res[i] = sum;
            }
            return res;
        }

        //перемножение матриц. Вариант1 
        public Matrix MultipleMatrixVer1(Matrix mulMatrix)
        {
            var result = new float[_size, _size];
            for (var i = 0; i < _size; i++)
            {
                for (var j = 0; j < _size; j++)
                {
                    result[i, j] = 0;
                    for (var k = 0; k < _size; k++)
                    {
                        result[i, j] += matrix[i, k] * mulMatrix.matrix[k, j];
                    }
                }
            }
            return new Matrix(result);
        }

        //перемножение матриц. Вариант2. Алгоритм Штрассена 
        public Matrix MultipleMatrixVer2(Matrix mulMatrix)
        {
            if (_size <= 64)
                return MultipleMatrixVer1(mulMatrix);
            var a = CropMatrix();
            var b = mulMatrix.CropMatrix();

            var p1 = (Add(a[0], a[3])).MultipleMatrixVer2(Add(b[0], b[3]));
            var p2 = (Add(a[2], a[3])).MultipleMatrixVer2(b[0]);
            var p3 = a[0].MultipleMatrixVer2(Delete(b[1], b[3]));
            var p4 = a[3].MultipleMatrixVer2(Delete(b[2], b[0]));
            var p5 = (Add(a[0], a[1])).MultipleMatrixVer2(b[3]);
            var p6 = (Delete(a[2], a[0])).MultipleMatrixVer2(Add(b[0], b[1]));
            var p7 = (Delete(a[1], a[3])).MultipleMatrixVer2(Add(b[2],b[3]));

            var c11 = Add(Delete(Add(p1, p4), p5), p7);
            var c12 = Add(p3, p5);
            var c21 = Add(p2, p4);
            var c22 = Add(Add(Delete(p1, p2), p3), p6);

            return Combine(c11, c12, c21, c22);
        }

        private Matrix Combine(Matrix c11, Matrix c12, Matrix c21, Matrix c22)
        {
            var res = new Matrix(_size);
            var cropedSize = _size / 2;
            for (var i = 0; i < cropedSize; i++)
            {
                for (var j = 0; j < cropedSize; j++)
                {
                    res.matrix[i, j] = c11.matrix[i, j];
                    res.matrix[i, j + cropedSize] = c12.matrix[i, j];
                    res.matrix[i + cropedSize, j] = c21.matrix[i, j];
                    res.matrix[i + cropedSize, j + cropedSize] = c22.matrix[i, j];
                }
            }
            return res;
        }

        private Matrix[] CropMatrix()
        {
            var croppedSize = _size / 2;
            var m1 = new Matrix(croppedSize);
            var m2 = new Matrix(croppedSize);
            var m3 = new Matrix(croppedSize);
            var m4 = new Matrix(croppedSize);
            for (var i = 0; i < croppedSize; i++)
            {
                for (var j = 0; j < croppedSize; j++)
                {
                    m1.matrix[i, j] = matrix[i, j];
                    m2.matrix[i, j] = matrix[i, j + croppedSize];
                    m3.matrix[i, j] = matrix[i + croppedSize, j];
                    m4.matrix[i, j] = matrix[i + croppedSize, j + croppedSize];
                }
            }
            return new[] { m1, m2, m3, m4 };
        }

        public static Matrix Add(Matrix m1, Matrix m2)
        {
            var res = new Matrix(m1._size);
            for (var i = 0; i < m1._size; i++)
            {
                for (var j = 0; j < m1._size; j++)
                {
                    res.matrix[i, j] = m1.matrix[i, j] + m2.matrix[i, j];
                }
            }
            return res;
        }

        public static Matrix Delete(Matrix m1, Matrix m2)
        {
            var res = new Matrix(m1._size);
            for (var i = 0; i < m1._size; i++)
            {
                for (var j = 0; j < m1._size; j++)
                {
                    res.matrix[i, j] = m1.matrix[i, j] - m2.matrix[i, j];
                }
            }
            return res;
        }
        #endregion
    }
}
