**BuildVer**

Modify AssemblyInfoFile and assign custom version numbering during the pre-build event within Visual Studio.

Ussage:

Place into pre build event of visual studio.

Sample #1
<BduildVer location>\BuildVer.exe -p $(ProjectName) -a "$(SolutionDir)$(ProjectName)\Properties\AssemblyInfo.cs" -v ShortYear -m Quarter -b Date -r Time
  
Results in versions # 22.1.121.1342

Sample #2
<BduildVer location>\BuildVer.exe -p $(ProjectName) -a "$(SolutionDir)$(ProjectName)\Properties\AssemblyInfo.cs" -v ShortYear -m Quarter -b None -r None
  
Results in versions # 22.1.0.0
