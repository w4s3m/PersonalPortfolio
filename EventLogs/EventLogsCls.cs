using System;
using System.IO;

namespace PersonelProtfolio.EventLogs
{
    public static class clsEventLog
    {
        private static string CurrentClientPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static string FilePath = Path.Combine(CurrentClientPath, "EventLogsFile.txt");

        public static void EnterEventLog(Exception ex, string ExceptionLocation)
        {
         
                if (!Directory.Exists(CurrentClientPath))
                    Directory.CreateDirectory(CurrentClientPath);

            string ExceptionMessage = $"\n[ {DateTime.Now} ]\n" +
                                 $"Message: {ex.Message}\n" +
                                 $"StackTrace: {ex.StackTrace}\n" +
                                 $"Exception Location : {ExceptionLocation} !"; 
                                     new string('-', 50);
                // (Open, Create, Write, Close, LastLine).
                File.AppendAllText(FilePath, ExceptionMessage);
          
        }
    }
}