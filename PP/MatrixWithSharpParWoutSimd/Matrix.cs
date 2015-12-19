using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MatrixWithSharpParWoutSimd
{
    public class Matrix
    {
        #region properties 
        private const int MaxNum = 1000000;
        public float[,] matrix;
        public int size;
        private static readonly string Ls = Environment.NewLine;
        private object locker => new object();
        private static int procCount => Environment.ProcessorCount;
        private List<Task> tasks;
        private Element _element;
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
            tasks = new List<Task>();
            this.size = size;
        }

        public Matrix(float[,] matrix)
        {
            this.matrix = matrix;
            tasks = new List<Task>();
            size = matrix.GetLength(0);
        }

        //заполнение матрицы 
        public void FillMatrix(Random rand)
        {
            for (var i = 0; i < size; i++)
                for (var j = 0; j < size; j++)
                {
                    matrix[i, j] = rand.Next(0, MaxNum);
                }
        }


        public float[] FillVector(Random rand)
        {
            var vector = new float[size];
            for (var i = 0; i < size; i++)
                vector[i] = rand.Next(MaxNum);
            return vector;
        }

        public float[] FillVector(float[] vector)
        {
            var v = new float[vector.Length];
            for (var i = 0; i < vector.Length; i++)
            {
                v[i] = vector[i];
                
            }
            return v;
        }

        //метод вывода матрицы в консоль 
        public void Output()
        {
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
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
        public Element GetMaxValuePar(Element element)
        {
            for (var i = 0; i < procCount; i++)
            {
                var it = i;
                tasks.Add(Task.Factory.StartNew(() => GetMaxValue(element, size * it / procCount, ((it + 1) * size) / procCount)));
            }
            foreach (var task in tasks)
            {
                task.Wait();
            }
            tasks.Clear();
            return element;
        }

        private void ChangeMax(Element element, float value, int row, int column)
        {
            lock (locker)
            {
                element.Row = row;
                element.Value = value;
                element.Column = column;
            }
        }
        public Element GetMaxValue(Element element, int start, int end)
        {
            for (; start < end; start++)
            {
                for (var j = 0; j < size; j++)
                {
                    if (matrix[start, j] > element.Value)
                    {
                        ChangeMax(element, matrix[start, j], start, j);
                        element.Value = matrix[start, j];
                    }
                }
            }
            return element;
        }

        //Умножение матрицы на вектор 
        public float[] MultWithVector(float[] vector)
        {
            if (vector.Length != size)
                return null;
            var res = new float[size];
            for (var i = 0; i < procCount; i++)
            {
                var it = i;
                tasks.Add(Task.Factory.StartNew(() => MultWithVector(size * it / procCount, ((it + 1) * size) / procCount, res, vector)));
            }
            foreach (var task in tasks)
            {
                task.Wait();
            }
            tasks.Clear();
            return res;
        }

        private void MultWithVector(int start, int end, float[] res, float[] vector)
        {
            for (; start < end; start++)
            {
                float sum = 0;
                for (var j = 0; j < size; j++)
                {
                    sum += matrix[start, j] * vector[j];
                }
                res[start] = sum;
            }
        }

        //перемножение матриц. Вариант1 
        public Matrix MultipleMatrixVer1(Matrix mulMatrix)
        {
            var result = new float[size, size];
            for (var i = 0; i < procCount; i++)
            {
                var it = i;
                tasks.Add(Task.Factory.StartNew(() => MultipleMatrixVer1(size * it / procCount, ((it + 1) * size) / procCount, mulMatrix, result)));
            }
            foreach (var task in tasks)
            {
                task.Wait();
            }
            tasks.Clear();
            return new Matrix(result);
        }

        private void MultipleMatrixVer1(int start, int end, Matrix mulMatrix, float[,] result)
        {
            for (; start < end; start++)
            {
                for (var j = 0; j < size; j++)
                {
                    result[start, j] = 0;
                    for (var k = 0; k < size; k++)
                    {
                        result[start, j] += matrix[start, k] * mulMatrix.matrix[k, j];
                    }
                }
            }
        }

        //перемножение матриц. Вариант2. Алгоритм Штрассена. 1 вариант парализации
        public async Task<Matrix> MultipleMatrixVer2(Matrix mulMatrix, int rootMatrixSize)
        {
            if (size <= 64)
                return MultipleMatrixVer1(mulMatrix);

            var a = CropMatrix();
            var b = mulMatrix.CropMatrix();
            return await MultipleMatrixVer2Helper(a, b, rootMatrixSize);
        }

        private async Task<Matrix> MultipleMatrixVer2Helper(Matrix[] a, Matrix[] b, int rootMatrixSize)
        {
            var p1 = (Add(a[0], a[3])).MultipleMatrixVer2(Add(b[0], b[3]), rootMatrixSize);
            var p2 = (Add(a[2], a[3])).MultipleMatrixVer2(b[0], rootMatrixSize);
            var p3 = a[0].MultipleMatrixVer2(Delete(b[1], b[3]), rootMatrixSize);
            var p4 = a[3].MultipleMatrixVer2(Delete(b[2], b[0]), rootMatrixSize);
            var p5 = (Add(a[0], a[1])).MultipleMatrixVer2(b[3], rootMatrixSize);
            var p6 = (Delete(a[2], a[0])).MultipleMatrixVer2(Add(b[0], b[1]), rootMatrixSize);
            var p7 = (Delete(a[1], a[3])).MultipleMatrixVer2(Add(b[2], b[3]), rootMatrixSize);

            var c11 = Add(Delete(Add(await p1, await p4), await p5), await p7);
            var c12 = Add(await p3, await p5);
            var c21 = Add(await p2, await p4);
            var c22 = Add(Add(Delete(await p1, await p2), await p3), await p6);

            return Combine(c11, c12, c21, c22);
        }

        private Matrix Combine(Matrix c11, Matrix c12, Matrix c21, Matrix c22)
        {
            var res = new Matrix(size);
            var cropedSize = size / 2;
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
            var croppedSize = size / 2;
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
            var res = new Matrix(m1.size);
            for (var i = 0; i < m1.size; i++)
            {
                for (var j = 0; j < m1.size; j++)
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
                for (var j = 0; j < m1.size; j++)
                {
                    res.matrix[i, j] = m1.matrix[i, j] - m2.matrix[i, j];
                }
            }
            return res;
        }

        //Алгоритм Штрассена. 2 вариант парализации
        public Matrix MultipleMatrixVer2SecondVar(Matrix mulMatrix)
        {
            if (size <= 128)
                return MultipleMatrixVer1WoutPar(mulMatrix);
            var a = CropMatrix();
            var b = mulMatrix.CropMatrix();
            var p1 = new Matrix(a[0].size);
            var p2 = new Matrix(a[0].size);
            var p3 = new Matrix(a[0].size);
            var p4 = new Matrix(a[0].size);
            var p5 = new Matrix(a[0].size);
            var p6 = new Matrix(a[0].size);
            var p7 = new Matrix(a[0].size);

            Parallel.Invoke(
            () => ConverLink(p1, () => (Add(a[0], a[3])).MultipleMatrixVer2SecondVar(Add(b[0], b[3]))),
            () => ConverLink(p2, () => (Add(a[2], a[3])).MultipleMatrixVer2SecondVar(b[0])),
            () => ConverLink(p3, () => a[0].MultipleMatrixVer2SecondVar(Delete(b[1], b[3]))),
            () => ConverLink(p4, () => a[3].MultipleMatrixVer2SecondVar(Delete(b[2], b[0]))),
            () => ConverLink(p5, () => (Add(a[0], a[1])).MultipleMatrixVer2SecondVar(b[3])),
            () => ConverLink(p6, () => (Delete(a[2], a[0])).MultipleMatrixVer2SecondVar(Add(b[0], b[1]))),
            () => ConverLink(p7, () => (Delete(a[1], a[3])).MultipleMatrixVer2SecondVar(Add(b[2], b[3]))));
            var c11 = new Matrix(a[0].size);
            var c12 = new Matrix(a[0].size);
            var c21 = new Matrix(a[0].size);
            var c22 = new Matrix(a[0].size);

            Parallel.Invoke(
                () => ConverLink(c11, () => Add(Delete(Add(p1, p4), p5), p7)),
                () => ConverLink(c12, () => Add(p3, p5)),
                () => ConverLink(c21, () => Add(p2, p4)),
                () => ConverLink(c22, () => Add(Add(Delete(p1, p2), p3), p6)));

            return Combine(c11, c12, c21, c22);
        }

        private static void ConverLink(Matrix m, Func<Matrix> a)
        {
            var c = a();
            m.matrix = c.matrix;
        }

        public Matrix MultipleMatrixVer1WoutPar(Matrix mulMatrix)
        {
            var result = new float[size, size];
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    result[i, j] = 0;
                    for (var k = 0; k < size; k++)
                    {
                        result[i, j] += matrix[i, k] * mulMatrix.matrix[k, j];
                    }
                }
            }
            return new Matrix(result);
        }
        #endregion
    }
}
