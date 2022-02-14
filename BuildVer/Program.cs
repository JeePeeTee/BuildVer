
#region usings

using System.Text;
using CommandLine;

#endregion

namespace BuildVer;

// Ussage: Pre build Event
// Sample #1
// <BuildVer location>\BuildVer.exe -p $(ProjectName) -a "$(SolutionDir)$(ProjectName)\Properties\AssemblyInfo.cs" -v ShortYear -m Quarter -b Date -r Time
// Results in versions # 22.1.121.1342
// Sample #2
// <BuildVer location>\BuildVer.exe -p $(ProjectName) -a "$(SolutionDir)$(ProjectName)\Properties\AssemblyInfo.cs" -v ShortYear -m Quarter -b None -r None
// Results in versions # 22.1.0.0

// Sample #3 (Debug & Release) variants
// IF $(ConfigurationName) == Debug([BuildVer location]\BuildVer.exe -p $(ProjectName) -a "$(SolutionDir)$(ProjectName)\Properties\AssemblyInfo.cs" -v None -m None -b Increment -r UTCTime)
// ELSE ([BuildVer location]\BuildVer.exe -p $(ProjectName) -a "$(SolutionDir)$(ProjectName)\Properties\AssemblyInfo.cs" -v None -m Increment -b Increment -r UTCTime)

internal static class Program {
    private const int idxVersion = 0;
    private const int idxMinor = 1;
    private const int idxBuild = 2;
    private const int idxRevision = 3;

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
        if (line.IndexOf("//", StringComparison.Ordinal) < line.IndexOf(lineType, StringComparison.Ordinal)) return;

        // [assembly: AssemblyVersion("22.1.0.0")]
        // [assembly: AssemblyFileVersion("22.1.0.0")]

        var major = GetValue(o.Version, idxVersion, line);
        var minor = GetValue(o.Minor, idxMinor, line);
        var build = GetValue(o.Build, idxBuild, line);
        var revision = GetValue(o.Revision, idxRevision, line);

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

    private static int GetValue(string? param, int idx, string line) {
        return param switch {
            "ShortYear" => DateTime.Now.Year % 100,
            "Year" => DateTime.Now.Year,
            "Quarter" => DateTime.Now.GetQuarter(),
            "Date" => DateTime.Now.Date.Month * 100 + DateTime.Now.Date.Day,
            "Day" => DateTime.Now.Date.Day,
            "Month" => DateTime.Now.Date.Month,
            "YearMonth" => (DateTime.Now.Year % 100) * 100 + DateTime.Now.Date.Month,
            "Time" => DateTime.Now.Hour * 100 + DateTime.Now.Minute,
            "UTCTime" => DateTime.UtcNow.Hour * 100 + DateTime.UtcNow.Minute,
            "DayOfYear" => DateTime.Now.DayOfYear,
            "DateYear" => (DateTime.Now.Year % 100) * 1000 + DateTime.Now.DayOfYear,
            "None" => 0,
            "Increment" => GetCurrent(line, idx) + 1,
            "Reset" => 0,

            _ => 0

            // ToDo
            // Increment 
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

    private static short GetCurrent(string line, int idx) {
        var sub = "";

        if (idx == idxVersion)
        {
            sub = line[(line.IndexOf('"') + 1)..];
        }
        else
        {
            var tmpIdx = -1;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            tmpIdx = line.Select((c, i) => new { Char = c, Index = i })
                .Where(item => item.Char == '.')
                .Skip(idx - 1)
                .FirstOrDefault().Index;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            sub = line[(tmpIdx + 1)..];
        }

        sub = sub[..(idx == idxRevision ? sub.IndexOf('"') : sub.IndexOf('.'))];

        short.TryParse(sub, out var val);

        return val;
    }
}