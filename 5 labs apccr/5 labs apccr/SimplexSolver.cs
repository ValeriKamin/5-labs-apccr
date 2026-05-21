using System;

class SimplexSolver
{
    const double EPS = 1e-9;

    public static void Solve(TransportData data)
    {
        data.PrintProblem();

        Console.WriteLine("\nПошук оптимального розв'язку задачі лінійного програмування:");
        PrintLinearProgrammingStatement(data);

        int variableCount = data.M * data.N;
        int constraintCount = data.M + data.N;
        int artificialCount = constraintCount;
        int totalVars = variableCount + artificialCount;

        double[,] simplex = new double[constraintCount + 1, totalVars + 1];
        int[] basis = new int[constraintCount];

        FillSimplexTable(data, simplex, basis, variableCount);

        Console.WriteLine("\nВхідна симплекс-таблиця:");

        double[] phase1C = new double[totalVars];

        for (int j = variableCount; j < totalVars; j++)
            phase1C[j] = -1;

        RecalculateRow(simplex, basis, phase1C);
        PrintSimplexTable(simplex, basis, variableCount);

        Console.WriteLine("\nФаза 1. Пошук опорного розв'язку:");
        SimplexMax(simplex, basis, phase1C, variableCount, true);

        int lastRow = simplex.GetLength(0) - 1;
        int lastCol = simplex.GetLength(1) - 1;

        if (Math.Abs(simplex[lastRow, lastCol]) > EPS)
        {
            Console.WriteLine("Опорний розв'язок не знайдено.");
            return;
        }

        Console.WriteLine("\nЗнайдено опорний розв'язок.");

        Console.WriteLine("\nФаза 2. Пошук оптимального розв'язку:");

        double[] phase2C = new double[totalVars];

        for (int i = 0; i < data.M; i++)
            for (int j = 0; j < data.N; j++)
                phase2C[VarIndex(data, i, j)] = -data.Cost[i, j];

        RecalculateRow(simplex, basis, phase2C);
        PrintSimplexTable(simplex, basis, variableCount);

        SimplexMax(simplex, basis, phase2C, variableCount, false);

        PrintResult(data, simplex, basis, variableCount);
    }

    static void FillSimplexTable(
        TransportData data,
        double[,] simplex,
        int[] basis,
        int variableCount)
    {
        int row = 0;
        int totalVars = simplex.GetLength(1) - 1;

        for (int i = 0; i < data.M; i++)
        {
            for (int j = 0; j < data.N; j++)
                simplex[row, VarIndex(data, i, j)] = 1;

            simplex[row, variableCount + row] = 1;
            simplex[row, totalVars] = data.Supply[i];
            basis[row] = variableCount + row;
            row++;
        }

        for (int j = 0; j < data.N; j++)
        {
            for (int i = 0; i < data.M; i++)
                simplex[row, VarIndex(data, i, j)] = 1;

            simplex[row, variableCount + row] = 1;
            simplex[row, totalVars] = data.Demand[j];
            basis[row] = variableCount + row;
            row++;
        }
    }

    static void SimplexMax(
        double[,] simplex,
        int[] basis,
        double[] c,
        int realVars,
        bool phase1)
    {
        int rows = simplex.GetLength(0);
        int cols = simplex.GetLength(1);
        int lastRow = rows - 1;
        int lastCol = cols - 1;

        int iteration = 1;

        while (true)
        {
            int pivotCol = -1;
            double min = 0;

            for (int j = 0; j < lastCol; j++)
            {
                if (!phase1 && j >= realVars) continue;

                if (simplex[lastRow, j] < min)
                {
                    min = simplex[lastRow, j];
                    pivotCol = j;
                }
            }

            if (pivotCol == -1)
            {
                Console.WriteLine("Критерій оптимальності виконується.");
                PrintSimplexTable(simplex, basis, realVars);
                return;
            }

            int pivotRow = -1;
            double minRatio = double.MaxValue;

            for (int i = 0; i < lastRow; i++)
            {
                if (simplex[i, pivotCol] > EPS)
                {
                    double ratio = simplex[i, lastCol] / simplex[i, pivotCol];

                    if (ratio < minRatio)
                    {
                        minRatio = ratio;
                        pivotRow = i;
                    }
                }
            }

            if (pivotRow == -1)
            {
                Console.WriteLine("Цільова функція необмежена.");
                return;
            }

            Console.WriteLine($"\nІтерація {iteration}");
            Console.WriteLine($"Розв'язувальний стовпець: x{pivotCol + 1}");
            Console.WriteLine($"Розв'язувальний рядок: y{pivotRow + 1}");
            Console.WriteLine($"Розв'язувальний елемент: {simplex[pivotRow, pivotCol]:F2}");

            Pivot(simplex, pivotRow, pivotCol);
            basis[pivotRow] = pivotCol;

            RecalculateRow(simplex, basis, c);
            PrintSimplexTable(simplex, basis, realVars);

            iteration++;
        }
    }

    static void Pivot(double[,] table, int pivotRow, int pivotCol)
    {
        int rows = table.GetLength(0);
        int cols = table.GetLength(1);

        double pivot = table[pivotRow, pivotCol];

        for (int j = 0; j < cols; j++)
            table[pivotRow, j] /= pivot;

        for (int i = 0; i < rows; i++)
        {
            if (i == pivotRow) continue;

            double factor = table[i, pivotCol];

            for (int j = 0; j < cols; j++)
                table[i, j] -= factor * table[pivotRow, j];
        }
    }

    static void RecalculateRow(double[,] table, int[] basis, double[] c)
    {
        int rows = table.GetLength(0);
        int cols = table.GetLength(1);
        int lastRow = rows - 1;
        int lastCol = cols - 1;

        for (int j = 0; j <= lastCol; j++)
            table[lastRow, j] = 0;

        for (int j = 0; j < lastCol; j++)
        {
            double zj = 0;

            for (int i = 0; i < lastRow; i++)
                zj += c[basis[i]] * table[i, j];

            table[lastRow, j] = zj - c[j];
        }

        double z = 0;

        for (int i = 0; i < lastRow; i++)
            z += c[basis[i]] * table[i, lastCol];

        table[lastRow, lastCol] = z;
    }

    static void PrintResult(
        TransportData data,
        double[,] simplex,
        int[] basis,
        int variableCount)
    {
        int constraintCount = data.M + data.N;
        int lastCol = simplex.GetLength(1) - 1;

        double[] x = new double[variableCount];

        for (int i = 0; i < constraintCount; i++)
        {
            if (basis[i] < variableCount)
                x[basis[i]] = simplex[i, lastCol];
        }

        Console.WriteLine("\nЗнайдено оптимальний розв'язок:");
        Console.Write("X = (");

        for (int i = 0; i < variableCount; i++)
        {
            Console.Write($"{x[i]:F2}");

            if (i < variableCount - 1)
                Console.Write("; ");
        }

        Console.WriteLine(")");

        double minZ = 0;

        for (int i = 0; i < data.M; i++)
            for (int j = 0; j < data.N; j++)
                minZ += x[VarIndex(data, i, j)] * data.Cost[i, j];

        Console.WriteLine($"Min(Z) = {minZ:F2}");

        double[,] plan = new double[data.M, data.N];

        for (int i = 0; i < data.M; i++)
            for (int j = 0; j < data.N; j++)
                plan[i, j] = x[VarIndex(data, i, j)];

        Console.WriteLine("\nОптимальний план перевезень за симплекс-методом:");
        Printer.PrintPlan(plan);
    }

    static void PrintLinearProgrammingStatement(TransportData data)
    {
        Console.WriteLine("\nПостановка задачі:");
        Console.Write("Z = ");

        bool first = true;

        for (int i = 0; i < data.M; i++)
        {
            for (int j = 0; j < data.N; j++)
            {
                if (!first) Console.Write(" + ");
                Console.Write($"{data.Cost[i, j]:F0}x{VarIndex(data, i, j) + 1}");
                first = false;
            }
        }

        Console.WriteLine(" -> min");

        Console.WriteLine("\nПерехід до задачі максимізації:");
        Console.Write("Z' = ");

        first = true;

        for (int i = 0; i < data.M; i++)
        {
            for (int j = 0; j < data.N; j++)
            {
                if (!first) Console.Write(" ");
                Console.Write($"- {data.Cost[i, j]:F0}x{VarIndex(data, i, j) + 1}");
                first = false;
            }
        }

        Console.WriteLine(" -> max");

        Console.WriteLine("\nОбмеження:");

        for (int i = 0; i < data.M; i++)
        {
            for (int j = 0; j < data.N; j++)
            {
                if (j > 0) Console.Write(" + ");
                Console.Write($"x{VarIndex(data, i, j) + 1}");
            }

            Console.WriteLine($" = {data.Supply[i]:F0}");
        }

        for (int j = 0; j < data.N; j++)
        {
            for (int i = 0; i < data.M; i++)
            {
                if (i > 0) Console.Write(" + ");
                Console.Write($"x{VarIndex(data, i, j) + 1}");
            }

            Console.WriteLine($" = {data.Demand[j]:F0}");
        }

        Console.WriteLine("x[j] >= 0");
    }

    static void PrintSimplexTable(double[,] table, int[] basis, int realVars)
    {
        int rows = table.GetLength(0);
        int cols = table.GetLength(1);

        Console.Write("\nБазис\t");

        for (int j = 0; j < cols - 1; j++)
        {
            if (j < realVars)
                Console.Write($"x{j + 1}\t");
            else
                Console.Write($"a{j - realVars + 1}\t");
        }

        Console.WriteLine("b");

        for (int i = 0; i < rows - 1; i++)
        {
            if (basis[i] < realVars)
                Console.Write($"x{basis[i] + 1}\t");
            else
                Console.Write($"a{basis[i] - realVars + 1}\t");

            for (int j = 0; j < cols; j++)
                Console.Write($"{table[i, j]:F2}\t");

            Console.WriteLine();
        }

        Console.Write("Z\t");

        for (int j = 0; j < cols; j++)
            Console.Write($"{table[rows - 1, j]:F2}\t");

        Console.WriteLine("\n");
    }

    static int VarIndex(TransportData data, int i, int j)
    {
        return i * data.N + j;
    }
}