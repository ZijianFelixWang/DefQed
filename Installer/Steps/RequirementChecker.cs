using Common;

namespace Installer.Steps
{
    internal static class RequirementChecker
    {
        // Function: check if MySQL is correctly installed.
        public static void Perform()
        {
            LogConsole.Log(LogLevel.Information, "Checking MySQL installation.");

            if (MySQLDriver.TestInstallation())
            {
                LogConsole.Log(LogLevel.Information, "MySQL installation check success.");
                return;
            }
            else
            {
                LogConsole.Log(LogLevel.Warning, "MySQL installation cannot be detected.");
                System.Console.WriteLine("Do you want to install it? Y/n");

                while (true)
                {
                    bool exit = false;

                    switch (System.Console.ReadLine().Trim().ToLower())
                    {
                        case "y":
                            InstallMySQL();
                            exit = true;
                            break;
                        case "n":
                        case "":
                            exit = true;
                            break;
                        default:
                            LogConsole.Log(LogLevel.Error, "You should enter either Y or N.");
                            break;
                    }

                    if (exit)
                    {
                        break;
                    }
                }
            }
        }

        private static void InstallMySQL()
        {
            LogConsole.Log(LogLevel.Information, "Preparing to perform MySQL installation.");

        }
    }
}