Param (
    [Parameter(Mandatory=$true)]
    [string]$Version,

    [string]$MSBuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe",

    [string]$NuGetPath = "NuGet\nuget.exe"
)

Try {
    Write-Host "Updating NuGet..."
    & $NuGetPath update -self

    Write-Host "Restoring NuGet packages..."
    & $NuGetPath restore ReactiveUIFody.sln

    Write-Host "Building the solution..."
    & $MSBuildPath ReactiveUIFody.sln /t:rebuild /v:minimal /p:Configuration=Release
    If ($LASTEXITCODE -Ne 0) {
        Throw "An error occurred building the solution"
    }

    Write-Host "Running unit tests..."
    dotnet vstest (Get-ChildItem -Recurse **\bin\Release\**\*.Tests.dll)
    If ($LASTEXITCODE -Ne 0) {
        Throw "One or more unit tests failed"
    }

    Write-Host "Creating NuGet package..."
    & $NuGetPath pack Nuget\ReactiveUIFody.nuspec -Version $Version
    If ($LASTEXITCODE -Ne 0) {
        Throw "An error occurred packaging the solution"
    }

    Exit 0
} Catch {
    Write-Error $_
    Exit 1
}