using System;
using System.Globalization;

class Program
{
    static TransportData data = new TransportData();

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        while (true)
        {
            Console.WriteLine("\nПРАКТИЧНА РОБОТА №5");
            Console.WriteLine("1 - Ввести дані вручну");
            Console.WriteLine("2 - Завантажити мій варіант 12");
            Console.WriteLine("3 - Показати задачу");
            Console.WriteLine("4 - Метод північно-західного кута");
            Console.WriteLine("5 - Метод мінімального елемента");
            Console.WriteLine("6 - Розв'язати симплекс-методом");
            Console.WriteLine("0 - Вихід");
            Console.Write("Ваш вибір: ");

            int choice = int.Parse(Console.ReadLine());

            if (choice == 0) break;

            switch (choice)
            {
                case 1:
                    data.InputData();
                    break;
                case 2:
                    data.LoadVariant12();
                    break;
                case 3:
                    if (data.CheckData()) data.PrintProblem();
                    break;
                case 4:
                    if (data.CheckData()) TransportSolver.Solve(data, "nw");
                    break;
                case 5:
                    if (data.CheckData()) TransportSolver.Solve(data, "min");
                    break;
                case 6:
                    if (data.CheckData()) SimplexSolver.Solve(data);
                    break;
                default:
                    Console.WriteLine("Невірний вибір.");
                    break;
            }
        }
    }
}