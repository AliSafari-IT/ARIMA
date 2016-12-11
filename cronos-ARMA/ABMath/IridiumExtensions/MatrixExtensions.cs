#region License Info
//Component of Cronos Package, http://www.codeplex.com/cronos
//Copyright (C) 2009 Anthony Brockwell

//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation; either version 2
//of the License, or (at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
#endregion

using System;
using MathNet.Numerics.LinearAlgebra;

namespace ABMath.IridiumExtensions
{
    public static class MatrixExtensions
    {
        public static Vector ToVector(this Matrix m)
        {
            var retval = new Vector(m.RowCount * m.ColumnCount);
            for (int r = 0; r < m.RowCount; ++r)
                for (int c = 0; c < m.ColumnCount; ++c )
                    retval[c * m.RowCount + r] = m[r, c];
            return retval;
        }

        public static Matrix ToMatrix(this Vector v, int rows, int cols)
        {
            if (rows*cols != v.Length)
                throw new ApplicationException("Invalid Vector to Matrix conversion, rows and cols not valid.");
            var stuff = v.CopyToArray();
            return new Matrix(stuff, rows);
        }

        public static Vector MultiplyBy(this Matrix m, Vector other)
        {
            return (m * other.ToColumnMatrix()).ToVector();
        }

        /// <summary>
        /// Returns a new matrix object containing a column of the original matrix.
        /// </summary>
        public static Vector ExtractColumn(this Matrix matrix, int columnIndex)
        {
            var retval = new Vector(matrix.RowCount);
            for (int i = 0; i < matrix.RowCount; ++i)
                retval[i] = matrix[i, columnIndex];
            return retval;
        }
  

        /// <summary>
        /// Returns new matrix containing values of submatrix of original.
        /// </summary>
        /// <param name="matrix">this matrix</param>
        /// <param name="r0">top row</param>
        /// <param name="c0">left col</param>
        /// <param name="r1">row AFTER bottom row</param>
        /// <param name="c1">column AFTER right col</param>
        /// <returns></returns>
        public static Matrix SubMatrix(this Matrix matrix, int r0, int c0, int r1, int c1)
        {
            if ((r0 < 0) || (r1 < 0) || (r0 >= r1) || (r0 > matrix.RowCount - 1) || (r1 > matrix.RowCount))
                throw new ApplicationException("Invalid row specification in SubMatrix call.");
            if ((c0 < 0) || (c1 < 0) || (c0 >= c1) || (c0 > matrix.ColumnCount - 1) || (c1 > matrix.ColumnCount))
                throw new ApplicationException("Invalid col specification in SubMatrix call.");
            int newrowcount = r1 - r0, newcolcount = c1 - c0;
            var retval = new Matrix(newrowcount, newcolcount);

            for (int i = r0; i < r1; ++i)
                for (int j = c0; j < c1; ++j)
                    retval[i - r0, j - c0] = matrix[i, j];

            return retval;
        }



        /// <summary>
        /// This function creates a matrix [[A B];[C D]]
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <param name="D"></param>
        /// <returns></returns>
        public static Matrix CreateBlockMatrixFrom(Matrix A, Matrix B, Matrix C, Matrix D)
        {
            int nrA = A.RowCount, nrB = B.RowCount, nrC = C.RowCount, nrD = D.RowCount;
            int ncA = A.ColumnCount, ncB = B.ColumnCount, ncC = C.ColumnCount, ncD = D.ColumnCount;

            if (nrA != nrB || nrC != nrD || ncA != ncC || ncB != ncD)
                throw new ArgumentException("Invalid sizes of matrices going into block matrix.");

            var block = new Matrix(nrA + nrC, ncB + ncD);
            for (int i = 0; i < nrA; ++i)
                for (int j = 0; j < ncA; ++j)
                    block[i, j] = A[i, j];
            for (int i = 0; i < nrB; ++i)
                for (int j = 0; j < ncB; ++j)
                    block[i, j + ncA] = B[i, j];
            for (int i = 0; i < nrC; ++i)
                for (int j = 0; j < ncC; ++j)
                    block[i+nrA, j] = C[i, j];
            for (int i = 0; i < nrD; ++i)
                for (int j = 0; j < ncD; ++j)
                    block[i + nrA, j + ncA] = D[i, j];

            return block;
        }

        /// <summary>
        /// Combines 2 matrices together horizontally and returns new matrix.
        /// Matlab equivalent, returns [m1 m2].
        /// </summary>
        /// <param name="m1">matrix to go on left</param>
        /// <param name="m2">matrix to go on right</param>
        /// <returns></returns>
        public static Matrix CreateBlockMatrixHorizontally(Matrix m1, Matrix m2)
        {
            if (m1.RowCount != m2.RowCount)
                throw new ArgumentException("Matrices to be bound must have same number of rows.");

            var retval = new Matrix(m1.RowCount, m1.ColumnCount + m2.ColumnCount);

            for (int i = 0; i < m1.RowCount; ++i)
            {
                for (int j = 0; j < m1.ColumnCount; ++j)
                    retval[i, j] = m1[i, j];
                for (int j = 0; j < m2.ColumnCount; ++j)
                    retval[i, j + m1.ColumnCount] = m2[i, j];
            }

            return retval;
        }

        /// <summary>
        /// Combines 2 matrices together horizontally and returns new matrix.
        /// Matlab equivalent, returns [m1 m2].
        /// </summary>
        /// <param name="m1">matrix to go on left</param>
        /// <param name="m2">matrix to go on right</param>
        /// <returns></returns>
        public static Matrix CreateBlockMatrixVertically(Matrix m1, Matrix m2)
        {
            if (m1.ColumnCount != m2.ColumnCount)
                throw new ArgumentException("Matrices to be bound must have same number of rows.");

            var retval = new Matrix(m1.RowCount + m2.RowCount, m1.ColumnCount);

            for (int j = 0; j < retval.ColumnCount; ++j)
            {
                for (int i = 0; i < m1.ColumnCount; ++i)
                    retval[i, j] = m1[i, j];
                for (int i = 0; i < m2.ColumnCount; ++i)
                    retval[i+m1.RowCount, j] = m2[i, j];
            }

            return retval;
        }


        /// <summary>
        /// Compute means down the columns of a matrix.
        /// </summary>
        /// <returns>a row vector (1xn matrix) of the column means</returns>
        public static Matrix Mean(this Matrix matrix)
        {
            var retval = new Matrix(1, matrix.ColumnCount);

            for (int rc = 0; rc < matrix.RowCount; ++rc)
                for (int cc = 0; cc < matrix.ColumnCount; ++cc)
                    retval[0, cc] += matrix[rc, cc];

            for (int cc = 0; cc < matrix.ColumnCount; ++cc)
                retval[0, cc] /= matrix.RowCount;

            return retval;
        }

        /// <summary>
        /// Compute covariance matrix of the collection of row-vectors making up the matrix.
        /// </summary>
        /// <returns>the covariance matrix</returns>
        public static Matrix Covariance(this Matrix matrix)
        {
            Matrix tmt = matrix.Clone();
            tmt.Transpose();
            Matrix xtx = tmt*matrix*(1.0/matrix.RowCount);  // this is now E[X^T X]

            Matrix y = matrix.Mean();
            Matrix yt = y.Clone();
            yt.Transpose();
            Matrix yty = yt*y;

            return (xtx - yty);
        }

        /// <summary>
        /// Compute covariance matrix of the collection of row-vectors making up the matrix.
        /// </summary>
        /// <returns>the covariance matrix</returns>
        public static Matrix Correlation(this Matrix matrix)
        {
            Matrix temp = matrix.Covariance();
            Matrix copied = temp.Clone();
            for (int i = 0; i < copied.RowCount; ++i)
                for (int j = 0; j < copied.ColumnCount; ++j )
                    copied[i, j] /= Math.Sqrt(temp[i, i]*temp[j, j]);
            return copied;
        }
    }
}