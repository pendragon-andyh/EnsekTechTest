namespace Ensek.DbUp;

using System.Reflection;
using global::DbUp;

internal class Program
{
    private static int Main(string[] args)
    {
        var connectionString = args.FirstOrDefault()
                               ?? "Data Source=.;Initial Catalog=EnsekDb;Integrated Security=True;Trust Server Certificate=True;";

        EnsureDatabase.For.SqlDatabase(connectionString);

        var upgradeProcess = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build();

        var result = upgradeProcess.PerformUpgrade();

        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(result.Error);
            Console.ResetColor();
            return -1;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Success!");
        Console.ResetColor();
        return 0;
    }
}