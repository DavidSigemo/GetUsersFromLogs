using System;
using System.Collections.Generic;
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
            var files = Directory.GetFiles(@"E:\Work\OAS-GetUsersFromLogs\Production\", "log-*.log", SearchOption.AllDirectories);
            Regex debugRowRegex = new Regex(@"\[Debug\]");
            Regex rowReplaceRegex = new Regex(@"\[Debug\]\[(\w+)\]");
            HashSet<string> userNames = new HashSet<string>();
            foreach (var filePath in files)
            {
                var fileText = File.ReadAllLines(filePath);

                foreach (var textRow in fileText)
                {
                    if (debugRowRegex.Match(textRow).Success)
                    {
                        var name = rowReplaceRegex.Replace(textRow, ":::START:::$&");
                        if (name.Contains(":::START:::"))
                        {
                            var startIndex = name.IndexOf(":::START:::", StringComparison.CurrentCulture);
                            name = name.Substring(startIndex + 19);

                            var closingBracketIndex = name.IndexOf("]", StringComparison.CurrentCulture);
                            name = name.Substring(0, closingBracketIndex);
                            userNames.Add(name);
                        }
                    }
                }
            }

            foreach (var userName in userNames.OrderBy(x => x))
            {
                Console.WriteLine(userName);
            }

            Console.ReadLine();
        }
    }
}
