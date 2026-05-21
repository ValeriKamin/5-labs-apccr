using System;
using System.Collections.Generic;

class TransportSolver
{
    public static void Solve(TransportData data, string method)
    {
        data.PrintProblem();

        double[,] plan;
        bool[,] basic;

        if (method == "nw")
        {
            Console.WriteLine("\nПошук опорного плану методом північно-західного кута:");
            plan = NorthWestCorner(data, out basic);
        }
        else
        {
            Console.WriteLine("\nПошук опорного плану методом мінімального елемента:");
            plan = MinimumElement(data, out basic);
        }

        FixDegeneracy(data, basic);

        Console.WriteLine("\nОпорний план перевезень:");
        Printer.PrintPlan(plan);
        Console.WriteLine($"Вартість за опорним планом: {TotalCost(data, plan):F2}");

        OptimizeByPotentials(data, plan, basic);
    }

    static double[,] NorthWestCorner(TransportData data, out bool[,] basic)
    {
        double[,] plan = new double[data.M, data.N];
        basic = new bool[data.M, data.N];

        double[] supply = (double[])data.Supply.Clone();
        double[] demand = (double[])data.Demand.Clone();

        int i = 0;
        int j = 0;

        while (i < data.M && j < data.N)
        {
            double value = Math.Min(supply[i], demand[j]);

            plan[i, j] = value;
            basic[i, j] = true;

            Console.WriteLine($"x[{i + 1},{j + 1}] = min({supply[i]:F2}, {demand[j]:F2}) = {value:F2}");

            supply[i] -= value;
            demand[j] -= value;

            if (Math.Abs(supply[i]) < TransportData.EPS && Math.Abs(demand[j]) < TransportData.EPS)
            {
                i++;
                j++;
            }
            else if (Math.Abs(supply[i]) < TransportData.EPS)
            {
                i++;
            }
            else
            {
                j++;
            }
        }

        return plan;
    }

    static double[,] MinimumElement(TransportData data, out bool[,] basic)
    {
        double[,] plan = new double[data.M, data.N];
        basic = new bool[data.M, data.N];

        double[] supply = (double[])data.Supply.Clone();
        double[] demand = (double[])data.Demand.Clone();

        bool[] rowDone = new bool[data.M];
        bool[] colDone = new bool[data.N];

        while (true)
        {
            int bestI = -1;
            int bestJ = -1;
            double bestCost = double.MaxValue;

            for (int i = 0; i < data.M; i++)
            {
                if (rowDone[i]) continue;

                for (int j = 0; j < data.N; j++)
                {
                    if (colDone[j]) continue;

                    if (data.Cost[i, j] < bestCost)
                    {
                        bestCost = data.Cost[i, j];
                        bestI = i;
                        bestJ = j;
                    }
                }
            }

            if (bestI == -1)
                break;

            double value = Math.Min(supply[bestI], demand[bestJ]);

            plan[bestI, bestJ] = value;
            basic[bestI, bestJ] = true;

            Console.WriteLine($"C[{bestI + 1},{bestJ + 1}] = {bestCost:F2}");
            Console.WriteLine($"x[{bestI + 1},{bestJ + 1}] = min({supply[bestI]:F2}, {demand[bestJ]:F2}) = {value:F2}");

            supply[bestI] -= value;
            demand[bestJ] -= value;

            if (Math.Abs(supply[bestI]) < TransportData.EPS)
                rowDone[bestI] = true;

            if (Math.Abs(demand[bestJ]) < TransportData.EPS)
                colDone[bestJ] = true;
        }

        return plan;
    }

    static void OptimizeByPotentials(TransportData data, double[,] plan, bool[,] basic)
    {
        int iteration = 1;

        while (true)
        {
            Console.WriteLine($"\nІтерація {iteration}. Метод потенціалів:");

            double[] u = new double[data.M];
            double[] v = new double[data.N];
            bool[] uKnown = new bool[data.M];
            bool[] vKnown = new bool[data.N];

            CalculatePotentials(data, basic, u, v, uKnown, vKnown);

            Console.Write("Потенціали постачальників u: ");
            Printer.PrintArray(u);

            Console.Write("Потенціали споживачів v: ");
            Printer.PrintArray(v);

            int enterI = -1;
            int enterJ = -1;
            double minDelta = 0;

            Console.WriteLine("Оцінки Δ:");

            for (int i = 0; i < data.M; i++)
            {
                for (int j = 0; j < data.N; j++)
                {
                    if (!basic[i, j])
                    {
                        double delta = data.Cost[i, j] - u[i] - v[j];
                        Console.WriteLine($"Δ[{i + 1},{j + 1}] = {delta:F2}");

                        if (delta < minDelta)
                        {
                            minDelta = delta;
                            enterI = i;
                            enterJ = j;
                        }
                    }
                }
            }

            if (enterI == -1)
            {
                Console.WriteLine("Усі Δ >= 0. Оптимальний план знайдено.");
                Console.WriteLine("\nОптимальний план перевезень:");
                Printer.PrintPlan(plan);
                Console.WriteLine($"Мінімальна вартість S = {TotalCost(data, plan):F2}");
                return;
            }

            Console.WriteLine($"Проблемна клітинка: [{enterI + 1},{enterJ + 1}]");

            List<Cell> cycle = FindCycle(data, basic, enterI, enterJ);

            if (cycle == null)
            {
                Console.WriteLine("Не вдалося побудувати цикл.");
                return;
            }

            double theta = double.MaxValue;

            for (int k = 1; k < cycle.Count; k += 2)
                theta = Math.Min(theta, plan[cycle[k].I, cycle[k].J]);

            Console.WriteLine($"θ = {theta:F2}");

            for (int k = 0; k < cycle.Count; k++)
            {
                Cell cell = cycle[k];

                if (k % 2 == 0)
                    plan[cell.I, cell.J] += theta;
                else
                    plan[cell.I, cell.J] -= theta;
            }

            basic[enterI, enterJ] = true;

            for (int k = 1; k < cycle.Count; k += 2)
            {
                Cell cell = cycle[k];

                if (Math.Abs(plan[cell.I, cell.J]) < TransportData.EPS)
                {
                    basic[cell.I, cell.J] = false;
                    break;
                }
            }

            Console.WriteLine("Новий план:");
            Printer.PrintPlan(plan);

            iteration++;
        }
    }

    static void CalculatePotentials(
        TransportData data,
        bool[,] basic,
        double[] u,
        double[] v,
        bool[] uKnown,
        bool[] vKnown)
    {
        u[0] = 0;
        uKnown[0] = true;

        bool changed = true;

        while (changed)
        {
            changed = false;

            for (int i = 0; i < data.M; i++)
            {
                for (int j = 0; j < data.N; j++)
                {
                    if (!basic[i, j]) continue;

                    if (uKnown[i] && !vKnown[j])
                    {
                        v[j] = data.Cost[i, j] - u[i];
                        vKnown[j] = true;
                        changed = true;
                    }
                    else if (!uKnown[i] && vKnown[j])
                    {
                        u[i] = data.Cost[i, j] - v[j];
                        uKnown[i] = true;
                        changed = true;
                    }
                }
            }
        }
    }

    static List<Cell> FindCycle(TransportData data, bool[,] basic, int startI, int startJ)
    {
        List<Cell> path = new List<Cell>();
        path.Add(new Cell(startI, startJ));

        bool[,] temp = (bool[,])basic.Clone();
        temp[startI, startJ] = true;

        if (SearchCycle(data, temp, path, startI, startJ, true))
            return path;

        return null;
    }

    static bool SearchCycle(
        TransportData data,
        bool[,] basic,
        List<Cell> path,
        int startI,
        int startJ,
        bool rowMove)
    {
        Cell last = path[path.Count - 1];

        if (rowMove)
        {
            for (int j = 0; j < data.N; j++)
            {
                if (j == last.J) continue;

                if (last.I == startI && j == startJ && path.Count >= 4)
                    return true;

                if (basic[last.I, j] && !Contains(path, last.I, j))
                {
                    path.Add(new Cell(last.I, j));

                    if (SearchCycle(data, basic, path, startI, startJ, false))
                        return true;

                    path.RemoveAt(path.Count - 1);
                }
            }
        }
        else
        {
            for (int i = 0; i < data.M; i++)
            {
                if (i == last.I) continue;

                if (i == startI && last.J == startJ && path.Count >= 4)
                    return true;

                if (basic[i, last.J] && !Contains(path, i, last.J))
                {
                    path.Add(new Cell(i, last.J));

                    if (SearchCycle(data, basic, path, startI, startJ, true))
                        return true;

                    path.RemoveAt(path.Count - 1);
                }
            }
        }

        return false;
    }

    static bool Contains(List<Cell> path, int i, int j)
    {
        foreach (Cell cell in path)
            if (cell.I == i && cell.J == j)
                return true;

        return false;
    }

    static void FixDegeneracy(TransportData data, bool[,] basic)
    {
        int need = data.M + data.N - 1;
        int count = 0;

        for (int i = 0; i < data.M; i++)
            for (int j = 0; j < data.N; j++)
                if (basic[i, j])
                    count++;

        for (int i = 0; i < data.M && count < need; i++)
        {
            for (int j = 0; j < data.N && count < need; j++)
            {
                if (!basic[i, j])
                {
                    basic[i, j] = true;
                    count++;
                }
            }
        }
    }

    static double TotalCost(TransportData data, double[,] plan)
    {
        double sum = 0;

        for (int i = 0; i < data.M; i++)
            for (int j = 0; j < data.N; j++)
                sum += plan[i, j] * data.Cost[i, j];

        return sum;
    }
}