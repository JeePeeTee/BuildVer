
#region usings

using System.Text;
using System.Text.RegularExpressions;
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
// IF $(ConfigurationName) == Debug([BuildVer location]\BuildVer.exe -p $(ProjectName) -a "$(SolutionDir)$(ProjectName)\Properties\AssemblyInfo.cs" -v Current -m Current -b Increment -r UTCTime)
// ELSE ([BuildVer location]\BuildVer.exe -p $(ProjectName) -a "$(SolutionDir)$(ProjectName)\Properties\AssemblyInfo.cs" -v Current -m Increment -b Increment -r UTCTime)
// Results when in RELEASE old version:  3.4.5.678 >> new version: 3.4.6.789
// Results when in DEBUG old version: 3.4.5.678 >> new version: 3.5.6.789

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
                    ReplaceLineContent(o, "AssemblyInformationalVersion", assemblyInfoFile, line);
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

        var pattern = new Regex(@"(?<Major>\d+)(\.)(?<Minor>\d+)(\.)(?<Build>\d+)(\.)(?<Revision>\d+)");
        var m = pattern.Match(line);

        var currMajor = Convert.ToInt32(m.Groups["Major"].Value);
        var currMinor = Convert.ToInt32(m.Groups["Minor"].Value);
        var currBuild = Convert.ToInt32(m.Groups["Build"].Value);
        var currRevision = Convert.ToInt32(m.Groups["Revision"].Value);

        var newMajor = GetValue(o.Version, currMajor, line);
        var newMinor = GetValue(o.Minor, currMinor, line);
        var newBuild = GetValue(o.Build, currBuild, line);
        var newRevision = GetValue(o.Revision, currRevision, line);

        var newline = $"[assembly: {lineType}(\"{newMajor}.{newMinor}.{newBuild}.{newRevision}\")]";

        var fileContent = File.ReadAllText(assemblyInfoFile);
        var newContent = fileContent.Replace(line, newline);

        //write the text back into the file
        File.WriteAllText(assemblyInfoFile, newContent, Encoding.UTF8);

        if (lineType == "AssemblyVersion") Console.WriteLine($"{o.Project} Old version: {currMajor}.{currMinor}.{currBuild}.{currRevision}  >> New version: {newMajor}.{newMinor}.{newBuild}.{newRevision}");
    }

    private static int GetQuarter(this DateTime date) {
        return (date.Month + 2) / 3;
    }

    private static int GetValue(string? param, int currValue, string line) {
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
            "DeltaDays" => (DateTime.Today - new DateTime(2000,1,1)).Days,
            "UTCSeconds" => Convert.ToInt32((DateTime.Now - DateTime.Today).TotalSeconds), // Utc or Local == same!?
            "None" => 0,
            "Increment" => currValue + 1,
            "Current" => currValue,
            "Reset" => 0,

            _ => 0
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
