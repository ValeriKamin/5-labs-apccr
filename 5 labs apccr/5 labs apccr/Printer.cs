using System;

class Printer
{
    public static void PrintMatrix(double[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                Console.Write($"{matrix[i, j],8:F2}");

            Console.WriteLine();
        }

        Console.WriteLine();
    }

    public static void PrintArray(double[] array)
    {
        for (int i = 0; i < array.Length; i++)
            Console.Write($"{array[i]:F2} ");

        Console.WriteLine();
    }

    public static void PrintPlan(double[,] plan)
    {
        int rows = plan.GetLength(0);
        int cols = plan.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (Math.Abs(plan[i, j]) < TransportData.EPS)
                    Console.Write($"{"x",8}");
                else
                    Console.Write($"{plan[i, j],8:F2}");
            }

            Console.WriteLine();
        }

        Console.WriteLine();
    }
}