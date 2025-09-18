REM call postbuild.bat $(OutDir) $(AssemblyName)



REM bin/Release/netstandard2.1
SET Output=%1
echo %output%


REM AssemblyName.dll
SET Dll=%2.dll



REM bin/Release/netstandard2.1/AssemblyName.dll
SET OutputDll=%Output%%Dll%



REM weavers ref to game files
SET Libs=Weaver\libs
SET Core=%Libs%\UnityEngine.CoreModule.dll
SET UNet=%Libs%\com.unity.multiplayer-hlapi.Runtime.dll



REM WEEEEAVER
.\Weaver\Unity.UNetWeaver.exe   %Core%   %UNet%   %Output%   %OutputDll%   %Libs%