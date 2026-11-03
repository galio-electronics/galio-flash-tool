$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$out = Join-Path $PSScriptRoot 'bin'
New-Item -ItemType Directory -Force $out | Out-Null
& $compiler /nologo /target:exe "/out:$out\LoaderTests.exe" "$root\Galio Flash Tool\FirmwareImage.cs" "$root\Galio Flash Tool\StationLoader.cs" "$PSScriptRoot\LoaderTests.cs"
if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed' }
& "$out\LoaderTests.exe"
if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }
