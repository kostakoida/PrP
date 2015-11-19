using System;
using System.Numerics;
using MatrixWithSIMD;

namespace MatrixWithSimd
{
    public class Matrix
    {

        #region properties
        public Vector<float>[,] matrix;
        private readonly int size;
        private readonly static int VectorSize = Vector<float>.Count;

        public float this[int row, int column]
        {
            get
            {
                var i = column / VectorSize;
                return matrix[row, i][column % VectorSize];
            }
            set
            {
                var i = column / VectorSize;
                var arr = new float[VectorSize];
                matrix[row, i].CopyTo(arr);
                arr[column % VectorSize] = value;
                matrix[row, i] = new Vector<float>(arr);
            }
        }

        public void SetValueToVector(int row, int column, float value)
        {
            var i = column / VectorSize;
            var arr = new float[VectorSize];
            matrix[row, i].CopyTo(arr);
            arr[column % VectorSize] = value;
            matrix[row, i] = new Vector<float>(arr);
        }

        public Vector<float> this[int index]
        {
            get
            {
                if (index == 0) return matrix[0, 0];
                var tinySize = size / VectorSize;
                var row = index % tinySize >= 0 ? index / tinySize : index / tinySize - 1;
                var column = index % (tinySize);
                return matrix[row, column];
            }
            set
            {
                if (index == 0) matrix[0, 0] = value;
                var tinySize = size / VectorSize;
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
            matrix = new Vector<float>[size, size / VectorSize];
            this.size = size;
        }

        public Matrix(Vector<float>[,] matrix)
        {
            this.matrix = matrix;
            size = matrix.GetLength(0);
        }

        public Matrix(float[,] matrix)
        {
            this.matrix = new Vector<float>[matrix.GetLength(0), matrix.GetLength(0) / VectorSize];
            this.size = matrix.GetLength(0);
            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                for (var j = 0; j < matrix.GetLength(0); j += VectorSize)
                {
                    for (var z = 0; z < VectorSize; z++)
                    {
                        this[i, j / VectorSize] = matrix[i, j];
                    }
                }
            }
        }

        public Vector<float>[] FillVector(float[] vector)
        {
            var v = new Vector<float>[vector.Length / VectorSize];
            for (var i = 0; i < vector.Length; i += VectorSize)
            {
                for (var j = 0; j < VectorSize; j++)
                {
                    var arr = new float[VectorSize];
                    v[i/VectorSize].CopyTo(arr);
                    arr[j] = vector[i + j];
                    v[i/VectorSize] = new Vector<float>(arr);
                }
            }
            return v;
        }

        #endregion

        #region methods

        //поиск максимального элемента
        public Element GetMaxValue(Element element)
        {
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size / VectorSize; j++)
                {
                    for (var z = 0; z < VectorSize; z++)
                    {
                        if (matrix[i, j][z] <= element.Value)
                        {
                            element = setElement(matrix[i, j][z], i, j);
                        }
                    }
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
        public float[] MultWithVector(Vector<float>[] vector)
        {
            if (vector.Length != size / VectorSize)
                return null;
            var res = new float[size];
            for (var i = 0; i < size; i++)
            {
                var sum = Vector<float>.Zero;
                for (var j = 0; j < vector.Length; j++)
                {
                    sum += matrix[i, j] * vector[j];
                }
                for (var j = 0; j < VectorSize; j++)
                {
                    res[i] += sum[j];
                }
            }
            return res;
        }

        //перемножение матриц. Вариант1
        public Matrix MultipleMatrixVer1(Matrix mulMatrix)
        {
            var result = new Vector<float>[size, size / VectorSize];
            var transposed = mulMatrix.Transpose();
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    var temp = Vector<float>.Zero;
                    for (var k = 0; k < size / VectorSize; k++)
                    {
                        temp += this[i * size / VectorSize + k] * transposed[j * size / VectorSize + k];
                    }
                    for (var z = 0; z < VectorSize; z++)
                    {
                        this[i, j] += temp[z];
                    }
                }
            }
            return new Matrix(result);
        }

        public Matrix Transpose()
        {
            var transposeMatrix = new Matrix(size);
            for (var i = 0; i < size; i++)
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
            if (size <= VectorSize)
                return MultipleMatrixVer1(mulMatrix);
            var a = CropMatrix();
            var b = mulMatrix.CropMatrix();

            var p1 = (Add(a[0], a[3])).MultipleMatrixVer2(Add(b[0], b[3]));
            var p2 = (Add(a[2], a[3])).MultipleMatrixVer2(b[0]);
            var p3 = a[0].MultipleMatrixVer2(Delete(b[1], b[3]));
            var pVectorSize = a[3].MultipleMatrixVer2(Delete(b[2], b[0]));
            var p5 = (Add(a[0], a[1])).MultipleMatrixVer2(b[3]);
            var p6 = (Delete(a[2], a[0])).MultipleMatrixVer2(Add(b[0], b[1]));
            var p7 = (Delete(a[1], a[3])).MultipleMatrixVer2(Add(b[2], b[3]));

            var c11 = Add(Delete(Add(p1, pVectorSize), p5), p7);
            var c12 = Add(p3, p5);
            var c21 = Add(p2, pVectorSize);
            var c22 = Add(Add(Delete(p1, p2), p3), p6);

            return Combine(c11, c12, c21, c22);
        }

        private Matrix Combine(Matrix c11, Matrix c12, Matrix c21, Matrix c22)
        {
            var res = new Matrix(size);
            var cropedSize = size / 2;
            var ind = 0;
            var tinySize = size / VectorSize;
            for (var i = 0; i < cropedSize; i++)
            {
                for (var j = 0; j < cropedSize / VectorSize; j++)
                {
                    res[i * tinySize + j] = c11[ind];
                    res[i * tinySize + j + cropedSize / VectorSize] = c12[ind];
                    res[i * tinySize + j + size * cropedSize / VectorSize] = c21[ind];
                    res[i * tinySize + j + (size + 1) * cropedSize / VectorSize] = c22[ind];
                    ind++;
                }
            }
            return res;
        }

        private Matrix[] CropMatrix()
        {
            var croppedSize = size / 2;
            var tinySize = size / VectorSize;
            var m1 = new Matrix(croppedSize);
            var m2 = new Matrix(croppedSize);
            var m3 = new Matrix(croppedSize);
            var mVectorSize = new Matrix(croppedSize);
            var ind = 0;
            for (var i = 0; i < croppedSize; i++)
            {
                for (var j = 0; j < croppedSize / VectorSize; j++)
                {
                    m1[ind] = this[i * tinySize + j];
                    m2[ind] = this[i * tinySize + j + croppedSize / VectorSize];
                    m3[ind] = this[i * tinySize + j + size * croppedSize / VectorSize];
                    mVectorSize[ind] = this[i * tinySize + j + (size + 1) * croppedSize / VectorSize];
                    ind++;
                }
            }
            return new[] { m1, m2, m3, mVectorSize };
        }

        public static Matrix Add(Matrix m1, Matrix m2)
        {
            var res = new Matrix(m1.size);
            for (var i = 0; i < m1.size; i++)
            {
                for (var j = 0; j < m1.size / VectorSize; j++)
                {
                    res.matrix[i, j] = m1.matrix[i, j] + m2.matrix[i, j];
                }
            }
            return res;
        }

        public static Matrix Delete(Matrix m1, Matrix m2)
        {
            var res = new Matrix(m1.size);
            for (var i = 0; i < m1.size; i++)
            {
                for (var j = 0; j < m1.size / VectorSize; j++)
                {
                    res.matrix[i, j] = m1.matrix[i, j] - m2.matrix[i, j];
                }
            }
            return res;
        }

        #endregion

    }
}
