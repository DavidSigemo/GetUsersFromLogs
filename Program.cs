using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace GetUsersFromLogs
{
    class Program
    {
        static void Main(string[] args)
        {
            //const string basePath = @"C:\Test\GetUsersFromLogs";
            var currentDir = Directory.GetCurrentDirectory();
            var files = Directory.GetFiles($@"{currentDir}\Production\", "log-*.log", SearchOption.AllDirectories);
            var debugRowRegex = new Regex(@"\[Debug\]");
            var rowReplaceRegex = new Regex(@"\[Debug\]\[(\w+)\]");
            var userNames = new HashSet<string>();
            foreach (var filePath in files)
            {
                var fileDateString = Path.GetFileNameWithoutExtension(filePath)?.Replace("log-", "").Replace("major-", "");

                var underScoreIndex = fileDateString.IndexOf("_", StringComparison.CurrentCulture);

                if (underScoreIndex != -1)
                    fileDateString = fileDateString.Remove(underScoreIndex);

                Console.WriteLine(fileDateString);
                var fileDate = DateTime.ParseExact(fileDateString, "yyyyMMdd", new DateTimeFormatInfo());
                if (fileDate.Year != 2018 || DateTime.Now.DayOfYear == fileDate.DayOfYear)
                    continue;

                var fileText = File.ReadAllLines(filePath);

                foreach (var textRow in fileText)
                {
                    if (!debugRowRegex.Match(textRow).Success) continue;

                    var name = rowReplaceRegex.Replace(textRow, ":::START:::$&");

                    if (!name.Contains(":::START:::")) continue;

                    var startIndex = name.IndexOf(":::START:::", StringComparison.CurrentCulture);
                    name = name.Substring(startIndex + 19);

                    var closingBracketIndex = name.IndexOf("]", StringComparison.CurrentCulture);
                    name = name.Substring(0, closingBracketIndex);

                    if (name == "null")
                        continue;

                    var added = userNames.Add(name);

                    if (added) Console.WriteLine($"Added {name} to list of names");
                }

                Console.WriteLine();

            }

            using (var sw = new StreamWriter($@"{currentDir}\users.txt"))
            {
                sw.WriteLine("INSERT INTO XXX (YYY)");
                sw.WriteLine("VALUES");
                foreach (var userName in userNames.OrderBy(x => x))
                {
                    sw.WriteLine($"('{userName}'),");
                    Console.WriteLine($"Wrote {userName} to file");
                }
                sw.WriteLine("GO");
            }
        }
    }
}
