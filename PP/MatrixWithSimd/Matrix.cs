using System;
using System.Collections.Generic;
using System.Numerics;
using MatrixWithSimd;

namespace MatrixWithSIMD
{
    public class Matrix
    {
        #region properties
        public Vector4[,] matrix;
        private readonly int _size;

        public float this[int row, int column]
        {
            get
            {
                var ind = column;
                var i = ind / 4;
                switch (ind % 4)
                {
                    case 0:
                        return matrix[row, i].X;
                    case 1:
                        return matrix[row, i].Y;
                    case 2:
                        return matrix[row, i].Z;
                    case 3:
                        return matrix[row, i].W;
                    default:
                        return -1;
                }
            }
            set
            {
                var ind = column;
                var i = ind / 4;
                switch (ind % 4)
                {
                    case 0:
                        matrix[row, i].X = value;
                        break;
                    case 1:
                        matrix[row, i].Y = value;
                        break;
                    case 2:
                        matrix[row, i].Z = value;
                        break;
                    case 3:
                        matrix[row, i].W = value;
                        break;
                }
            }
        }

        public Vector4 this[int index]
        {
            get
            {
                if (index == 0) return matrix[0, 0];
                var tinySize = _size / 4;
                var row = index % tinySize >= 0 ? index / tinySize : index / tinySize - 1;
                var column = index % (tinySize);
                return matrix[row, column];
            }
            set
            {
                if (index == 0) matrix[0, 0] = value;
                var tinySize = _size / 4;
                var row = index % tinySize >= 0 ? index / tinySize : index / tinySize - 1;
                var column = index % (tinySize);
                matrix[row, column] = value;
            }
        }
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
            this.matrix = new Vector4[matrix.GetLength(0), matrix.GetLength(0) / 4];
            this._size = matrix.GetLength(0);
            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                for (var j = 0; j < matrix.GetLength(0); j += 4)
                {
                    this.matrix[i, j / 4].X = matrix[i, j];
                    this.matrix[i, j / 4].Y = matrix[i, j + 1];
                    this.matrix[i, j / 4].Z = matrix[i, j + 2];
                    this.matrix[i, j / 4].W = matrix[i, j + 3];
                }
            }
        }

        public Vector4[] FillVector(float[] vector)
        {
            var v = new Vector4[vector.Length / 4];
            for (var i = 0; i < vector.Length; i += 4)
            {
                v[i / 4].X = vector[i];
                v[i / 4].Y = vector[i + 1];
                v[i / 4].Z = vector[i + 2];
                v[i / 4].W = vector[i + 3];
            }
            return v;
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
        public float[] MultWithVector(Vector4[] vector)
        {
            if (vector.Length != _size / 4)
                return null;
            var res = new float[_size];
            for (var i = 0; i < _size; i++)
            {
                var sum = Vector4.Zero;
                for (var j = 0; j < vector.Length; j++)
                {
                    sum += matrix[i, j] * vector[j];
                }
                res[i] = sum.X + sum.Y + sum.Z + sum.W;
            }
            return res;
        }

        //перемножение матриц. Вариант1
        public Matrix MultipleMatrixVer1(Matrix mulMatrix)
        {
            var result = new Vector4[_size, _size / 4];
            var transposed = mulMatrix.Transpose();
            for (var i = 0; i < _size; i++)
            {
                for (var j = 0; j < _size; j++)
                {
                    var temp = Vector4.Zero;
                    for (var k = 0; k < _size / 4; k++)
                    {
                        temp += this[i * _size / 4 + k] * transposed[j * _size / 4 + k];
                    }
                    this[i, j] = temp.X + temp.Y + temp.Z + temp.W;
                }
            }
            return new Matrix(result);
        }

        public Matrix Transpose()
        {
            var transposeMatrix = new Matrix(_size);
            for (var i = 0; i < _size; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    transposeMatrix[i, j] = this[j, i];
                    transposeMatrix[j, i] = this[i, j];
                }
            }
            return transposeMatrix;
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
            var p7 = (Delete(a[1], a[3])).MultipleMatrixVer2(Add(b[2], b[3]));

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
            var tinySize = _size / 4;
            var m1 = new Matrix(croppedSize);
            var m2 = new Matrix(croppedSize);
            var m3 = new Matrix(croppedSize);
            var m4 = new Matrix(croppedSize);
            const int ind = 0;
            for (var i = 0; i < croppedSize; i++)
            {
                for (var j = 0; j < croppedSize / 4; j++)
                {
                    m1[ind] = this[i * tinySize / 4 + j];
                    m2[ind] = this[i * tinySize / 4 + j + croppedSize / 4];
                    m3[ind] = this[i * tinySize / 4 + j + _size * croppedSize / 4];
                    m4[ind] = this[i * tinySize + j + (_size + 1) * croppedSize / 4];
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
