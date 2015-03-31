SET MSBUILD="C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe"

msbuild ReactiveUIFody.sln

cd Nuget
mkdir lib
mkdir lib\net45
mkdir lib\Xamarin.iOS10

copy ..\ReactiveUI.Fody\bin\Debug\ReactiveUI.Fody.* .
copy ..\ReactiveUI.Fody.Helpers.Ios\bin\iPhone\Debug\*.* lib\Xamarin.iOS10
copy ..\ReactiveUI.Fody.Helpers.Net45\bin\Debug\*.* lib\net45

nuget pack ReactiveUIFody.nuspec

rmdir lib /S /Q
del ReactiveUI.Fody.dll
del ReactiveUI.Fody.dll.mdb
del ReactiveUI.Fody.pdb

cd ..