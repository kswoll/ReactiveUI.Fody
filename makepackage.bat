"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe" ReactiveUIFody.sln

cd Nuget
mkdir lib
mkdir lib\net45
mkdir lib\Xamarin.iOS10
mkdir lib\MonoAndroid

copy ..\ReactiveUI.Fody\bin\Debug\ReactiveUI.Fody.* .
copy ..\ReactiveUI.Fody.Helpers.Ios\bin\iPhone\Debug\ReactiveUI.Fody.Helpers.* lib\Xamarin.iOS10
copy ..\ReactiveUI.Fody.Helpers.Net45\bin\Debug\ReactiveUI.Fody.Helpers.* lib\net45
copy ..\ReactiveUI.Fody.Helpers.Android\bin\Debug\ReactiveUI.Fody.Helpers.* lib\MonoAndroid

nuget pack ReactiveUIFody.nuspec

rmdir lib /S /Q
del ReactiveUI.Fody.dll
del ReactiveUI.Fody.dll.mdb
del ReactiveUI.Fody.pdb

cd ..