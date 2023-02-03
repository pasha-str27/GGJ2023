using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class Matrix
    {
        public static T[,] RotateMatrixClockwise<T>(T[,] oldMatrix)
        {
            var dimensionX = oldMatrix.GetLength(1);
            var dimensionY = oldMatrix.GetLength(0);
            var newMatrix = new T[dimensionX, dimensionY];
            var newRow = 0;

            for (var oldColumn = dimensionX - 1; oldColumn >= 0; oldColumn--)
            {
                for (var oldRow = 0; oldRow < dimensionY; oldRow++)
                {
                    newMatrix[newRow, oldRow] = oldMatrix[oldRow, oldColumn];
                }
                newRow++;
            }
            return newMatrix;
        }

        public static string ToSting<T>(T[,] matrix)
        {
            string result = "";

            for (int i = 0; i < matrix.GetLength(1); ++i)
            {
                for (int j = 0; j < matrix.GetLength(0); ++j)
                    result += matrix[j, i].ToString() + "   ";
                result += "\n";
            }

            return result;
        }
    }
}
