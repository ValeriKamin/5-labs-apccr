using System;
using System.Linq;
using System.Reflection;

class TransportData
{
    public const double EPS = 1e-9;

    public int M;
    public int N;
    public double[,] Cost;
    public double[] Supply;
    public double[] Demand;

    public void InputData()
    {
        Console.Write("Кількість пунктів відправлення: ");
        M = int.Parse(Console.ReadLine());

        Console.Write("Кількість пунктів призначення: ");
        N = int.Parse(Console.ReadLine());

        Cost = new double[M, N];
        Supply = new double[M];
        Demand = new double[N];

        Console.WriteLine("\nМатриця вартостей:");
        for (int i = 0; i < M; i++)
            for (int j = 0; j < N; j++)
            {
                Console.Write($"C[{i + 1},{j + 1}] = ");
                Cost[i, j] = double.Parse(Console.ReadLine());
            }

        Console.WriteLine("\nВектор запасів:");
        for (int i = 0; i < M; i++)
        {
            Console.Write($"PO[{i + 1}] = ");
            Supply[i] = double.Parse(Console.ReadLine());
        }

        Console.WriteLine("\nВектор заявок:");
        for (int j = 0; j < N; j++)
        {
            Console.Write($"PN[{j + 1}] = ");
            Demand[j] = double.Parse(Console.ReadLine());
        }

        BalanceProblem();
    }

    public void LoadVariant12()
    {
        M = 3;
        N = 4;

        Cost = new double[,]
        {
            { 9, 8, 6, 9 },
            { 4, 7, 5, 10 },
            { 10, 8, 6, 8 }
        };

        Supply = new double[] { 70, 65, 55 };
        Demand = new double[] { 85, 35, 30, 40 };

        BalanceProblem();
        Console.WriteLine("Завантажено варіант 12.");
    }

    public bool CheckData()
    {
        if (Cost == null)
        {
            Console.WriteLine("Спочатку введіть або завантажте дані.");
            return false;
        }

        return true;
    }

    public void BalanceProblem()
    {
        double sumSupply = Supply.Sum();
        double sumDemand = Demand.Sum();

        Console.WriteLine($"\nСума запасів = {sumSupply:F2}");
        Console.WriteLine($"Сума заявок = {sumDemand:F2}");

        if (Math.Abs(sumSupply - sumDemand) < EPS)
        {
            Console.WriteLine("Транспортна задача закрита.");
            return;
        }

        if (sumSupply > sumDemand)
            AddFakeDemand(sumSupply - sumDemand);
        else
            AddFakeSupply(sumDemand - sumSupply);
    }

    void AddFakeDemand(double value)
    {
        Console.WriteLine("Задача відкрита. Додаємо фіктивний пункт призначення.");

        double[,] newCost = new double[M, N + 1];

        for (int i = 0; i < M; i++)
            for (int j = 0; j < N; j++)
                newCost[i, j] = Cost[i, j];

        double[] newDemand = new double[N + 1];

        for (int j = 0; j < N; j++)
            newDemand[j] = Demand[j];

        newDemand[N] = value;

        Cost = newCost;
        Demand = newDemand;
        N++;
    }

    void AddFakeSupply(double value)
    {
        Console.WriteLine("Задача відкрита. Додаємо фіктивний пункт відправлення.");

        double[,] newCost = new double[M + 1, N];

        for (int i = 0; i < M; i++)
            for (int j = 0; j < N; j++)
                newCost[i, j] = Cost[i, j];

        double[] newSupply = new double[M + 1];

        for (int i = 0; i < M; i++)
            newSupply[i] = Supply[i];

        newSupply[M] = value;

        Cost = newCost;
        Supply = newSupply;
        M++;
    }

    public void PrintProblem()
    {
        Console.WriteLine("\nМатриця вартостей:");
        Printer.PrintMatrix(Cost);

        Console.Write("Вектор запасів: ");
        Printer.PrintArray(Supply);

        Console.Write("Вектор заявок: ");
        Printer.PrintArray(Demand);
    }
}