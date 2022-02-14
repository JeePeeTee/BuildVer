**BuildVer**

Modify AssemblyInfoFile and assign custom version numbering during the pre-build event within Visual Studio.

Ussage:

Place into pre build event of visual studio.

Sample #1

[BuildVer location]\BuildVer.exe -p $(ProjectName) -a "$(SolutionDir)$(ProjectName)\Properties\AssemblyInfo.cs" -v ShortYear -m Quarter -b Date -r Time
  
Results in versions # 22.1.121.1342

Sample #2
  
[BuildVer location]\BuildVer.exe -p $(ProjectName) -a "$(SolutionDir)$(ProjectName)\Properties\AssemblyInfo.cs" -v ShortYear -m Quarter -b None -r None
  
Results in versions # 22.1.0.0

Sample #3 (Debug & Release) variants

IF $(ConfigurationName) == Debug([BuildVer location]\BuildVer.exe -p $(ProjectName) -a "$(SolutionDir)$(ProjectName)\Properties\AssemblyInfo.cs" -v Current -m Current -b Increment -r UTCTime) ELSE ([BuildVer location]\BuildVer.exe -p $(ProjectName) -a "$(SolutionDir)$(ProjectName)\Properties\AssemblyInfo.cs" -v Current -m Increment -b Increment -r UTCTime)

Results when in RELEASE old version:  3.4.5.678 >> new version: 3.4.6.789

Results when in DEBUG old version: 3.4.5.678 >> new version: 3.5.6.789
