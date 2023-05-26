rem Build dll
set msbuild_path=msbuild
if not "%JetBrains Rider%"=="" (
	echo Loading JetBrains Rider Tools
	set msbuild_path="%JetBrains Rider:;=%\..\tools\MSBuild\Current\Bin\MSBuild.exe"
) else (
	echo Loading Visual Studio Tools
	call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat"
)

echo Compiling Dll
%msbuild_path% ..\KeeTheme.sln /p:Configuration=Release /p:LangVersion=3

echo Releasing Dll
copy ..\KeeTheme\bin\Release\KeeTheme.dll .\KeeTheme.dll

rem Build plgx
echo Deleting existing PlgX folder
rmdir /s /q "PlgX"

echo Creating PlgX folder
mkdir "PlgX"

echo Copying files
xcopy "..\KeeTheme\*" "PlgX" /s /e /exclude:PlgXExclude.txt

echo Compiling PlgX
"C:\Program Files\KeePass Password Safe 2\KeePass.exe" /plgx-create "%cd%\PlgX" --debug --plgx-prereq-net:3.5

echo Releasing PlgX
move /y "PlgX.plgx" "KeeTheme.plgx"

echo Cleaning up
rmdir /s /q "PlgX"

pause