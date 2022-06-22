using System;
using Common;

namespace Installer
{
    public class Program
    {
        public static void Main()
        {
            LogConsole.Log(LogLevel.Information, "Welcome to the DefQed Installer!");
            LogConsole.Log(LogLevel.Information, "This program is licensed under BSD-3 Clause \"New\" or \"Revised\" License." +
                "Press ENTER to continue if you accept the license.");
            Console.ReadLine();

            Steps.RequirementChecker.Perform();
        }
    }
}