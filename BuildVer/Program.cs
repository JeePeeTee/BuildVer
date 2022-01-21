
#region usings

using System.Text;
using CommandLine;

#endregion

namespace BuildVer;

internal static class Program {
    static void Main(string[] args) {
        try {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o => {
                //the first argument must be the assembly info file
                var assemblyInfoFile = o.Assembly;

                if (!File.Exists(assemblyInfoFile)) {
                    Console.WriteLine("Unable to locate 'AssemblyFileVersion' text in file");
                    return;
                }

                var allLines = File.ReadAllLines(assemblyInfoFile);

                foreach (var line in allLines) {
                    ReplaceLineContent(o, "AssemblyFileVersion", assemblyInfoFile, line);
                    ReplaceLineContent(o, "AssemblyVersion", assemblyInfoFile, line);
                }
            });
        }
        catch (Exception ex) {
            Console.WriteLine("An Error occured");
            Console.WriteLine(" ");
            Console.WriteLine("Error Message : " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }

    private static void ReplaceLineContent(Options o, string lineType, string assemblyInfoFile, string line) {
        
        if (!line.Contains(lineType)) return;

        // [assembly: AssemblyVersion("22.1.0.0")]
        // [assembly: AssemblyFileVersion("22.1.0.0")]

        var major = GetValue(o.Version);
        var minor = GetValue(o.Minor);
        var build = GetValue(o.Build);
        var revision = GetValue(o.Revision);

        var newline = $"[assembly: {lineType}(\"{major}.{minor}.{build}.{revision}\")]";

        var fileContent = File.ReadAllText(assemblyInfoFile);
        var newContent = fileContent.Replace(line, newline);

        //write the text back into the file
        File.WriteAllText(assemblyInfoFile, newContent, Encoding.UTF8);

        if (lineType == "AssemblyVersion") Console.WriteLine($"{o.Project} version: {major}.{minor}.{build}.{revision}");
    }

    private static int GetQuarter(this DateTime date) {
        return (date.Month + 2) / 3;
    }

    private static int GetValue(string? param) {
        return param switch {
            "ShortYear" => DateTime.Now.Year % 100,
            "Year" => DateTime.Now.Year,
            "Quarter" => DateTime.Now.GetQuarter(),
            "Date" => DateTime.Now.Date.Month * 100 + DateTime.Now.Date.Day,
            "Day" => DateTime.Now.Date.Day,
            "Month" => DateTime.Now.Date.Month,
            "YearMonth" => (DateTime.Now.Year % 100) * 100 + DateTime.Now.Date.Month,
            "Time" => DateTime.Now.Hour * 100 + DateTime.Now.Minute,
            "UTCTime" => DateTime.Now.ToUniversalTime().Hour * 100 + DateTime.Now.ToUniversalTime().Minute,
            "None" => 0,
            _ => 0

            // ToDo
            // Increment 
            // DayOfYear (ddd)
            // DateYear (yyddd)
            // DeltaDays (days since 1/1/2000)
            // UTCSeconds (seconds since midnight)
        };
    }

    public class Options {
        [Option('p', "project", Required = true, HelpText = "Project name")]
        public string? Project { get; set; }

        [Option('a', "assembly", Required = true, HelpText = "Full Assembly file name")]
        public string? Assembly { get; set; }

        [Option('v', "version", Required = true, HelpText = "Version format (ShortYear)")]
        public string? Version { get; set; }

        [Option('m', "minor", Required = true, HelpText = "Minor format (ShortYear)")]
        public string? Minor { get; set; }

        [Option('b', "build", Required = true, HelpText = "Build format (ShortYear)")]
        public string? Build { get; set; }

        [Option('r', "revision", Required = true, HelpText = "Revision format (ShortYear)")]
        public string? Revision { get; set; }
    }
}