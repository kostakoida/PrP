using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Numerics;
using MatrixWithSimd;

namespace MatrixWithSIMD
{
    public class Matrix
    {
        #region properties
        private const int MaxNum = 10000000;
        public Vector4[,] matrix;
        private readonly int _size;
        private static string _ls = Environment.NewLine;
        #endregion

        #region init
        public Matrix(int size)
        {
            if (size <= 0)
            {
                Console.WriteLine("Incorrect size");
                return;
            }
            matrix = new Vector4[size, size / 4];
            _size = size;
        }

        public Matrix(Vector4[,] matrix)
        {
            this.matrix = matrix;
            _size = matrix.GetLength(0);
        }

        public Matrix(float[,] matrix)
        {
            
        }

        //заполнение матрицы
        public void FillMatrix(Random rand)
        {
            for (var i = 0; i < _size; i++)
                for (var j = 0; j < _size / 4; j++)
                {
                    matrix[i, j].X = rand.Next(0, MaxNum);
                    matrix[i, j].Y = rand.Next(0, MaxNum);
                    matrix[i, j].W = rand.Next(0, MaxNum);
                    matrix[i, j].Z = rand.Next(0, MaxNum);
                }
        }

        public Vector4[] FillVector(Random rand)
        {
            var vector = new Vector4[_size / 4];
            for (var i = 0; i < _size / 4; i++)
            {
                vector[i].X = rand.Next(0, MaxNum);
                vector[i].Y = rand.Next(0, MaxNum);
                vector[i].W = rand.Next(0, MaxNum);
                vector[i].Z = rand.Next(0, MaxNum);
            }
            return vector;
        }

        public Vector4[] FillVector(float[] vector)
        {
            var v = new Vector4[_size / 4];
            for (var i = 0; i < _size / 4; i++)
            {
                v[i].X = vector[i];
                v[i].Y = vector[i + 1];
                v[i].W = vector[i + 2];
                v[i].Z = vector[i + 3];
            }
            return v;
        }

        //метод вывода матрицы в консоль
        public void Output()
        {
            for (var i = 0; i < _size; i++)
            {
                for (var j = 0; j < _size; j++)
                    Console.Write("{0},", matrix[i, j]);
                Console.Write(_ls);
            }
        }

        //вывод вектора в консоль
        public static void OutputVector(Vector4[] vector)
        {
            foreach (var i in vector)
            {
                Console.Write("{0},", i);
            }
            Console.Write(_ls);
        }

        #endregion

        #region methods
        //поиск максимального элемента
        public Element GetMaxValue(Element element)
        {
            var elements = new List<Element>();
            for (var i = 0; i < _size; i++)
            {
                for (var j = 0; j < _size; j++)
                {
                    if (matrix[i, j].X <= element.Value)
                    {
                        elements.Add(setElement(matrix[i, j].X, i, j));
                    }
                    if (matrix[i, j].Y <= element.Value)
                    {
                        elements.Add(setElement(matrix[i, j].Y, i, j));
                    }
                    if (matrix[i, j].W <= element.Value)
                    {
                        elements.Add(setElement(matrix[i, j].W, i, j));
                    }
                    if (matrix[i, j].Z <= element.Value)
                    {
                        elements.Add(setElement(matrix[i, j].Z, i, j));
                    }
                }
            }

            var o = 0;
            for (var i = 0; i < _size; i++)
            {
                for (var j = 0; j < _size / 4; j++)
                {

                }
            }
            return element;
        }

        private Element setElement(float value, int row, int column)
        {
            return new Element
            {
                Value = value,
                Row = row,
                Column = column
            };
        }

        //Умножение матрицы на вектор
        public Vector4[] MultWithVector(Vector4[] vector)
        {
            if (vector.Length != _size)
                return null;
            var res = new Vector4[_size];
            for (var i = 0; i < _size; i++)
            {
                var sum = Vector4.Zero;
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
            var p7 = (Delete(a[1], a[3])).MultipleMatrixVer2(Add(b[2],
                b[3]));

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

        public static bool IsEqual(Matrix matrix1, Matrix matrix2, int size)
        {
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    var max = matrix1.matrix[i, j] >= matrix2.matrix[i, j] ? matrix1.matrix[i, j] : matrix2.matrix[i, j];
                    var min = matrix1.matrix[i, j] >= matrix2.matrix[i, j] ? matrix2.matrix[i, j] : matrix1.matrix[i, j];
                    if (max - min != 0)
                        Console.Write("");
                    var diff = Math.Pow(10, getError(matrix1.matrix[i, j], matrix2.matrix[i, j]));

                    if (max - min > diff)
                    {
                        return false;
                    }
                }
            }

            return true;
        }



        private static int getError(float a, float b)
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


        public bool Equal(Matrix other)
        {
            if (other == null)
                return false;

            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    if (this.matrix[i, j] != other.matrix[i, j])
                        if (Math.Abs(this.matrix[i, j] - other.matrix[i, j]) > Math.Abs(this.matrix[i, j]))
                            return false;
                }
            }
            return true;
        }
        #endregion

    }
}
