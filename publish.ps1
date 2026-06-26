$projectDir = $PSScriptRoot
$desktop = [Environment]::GetFolderPath("Desktop")
$exeName = "AppStudioJsonViewer.exe"

Write-Host "Building..."
dotnet publish "$projectDir" /p:PublishProfile=Release
if (-not $?) { Write-Host "Build failed."; exit 1 }

$src = Join-Path $projectDir "bin\Release\framework-dependent\$exeName"
$dst = Join-Path $desktop $exeName

Copy-Item $src $dst -Force
Write-Host "Done. Copied to $dst"
