# Clean project script
Write-Host "Cleaning project..." -ForegroundColor Green

# Remove bin, obj, logs folders in src directory
$folders = Get-ChildItem -Path ./src -Include bin,obj,logs -Recurse -Directory -Force -ErrorAction SilentlyContinue
$totalSize = 0
if ($folders) {
    $totalSize = ($folders | Get-ChildItem -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "Total size to delete: $([math]::Round($totalSize, 2)) MB" -ForegroundColor Yellow
    
    $folders | ForEach-Object { 
        Write-Host "Removing: $($_.FullName)" -ForegroundColor Gray
        Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue 
    }
}

# Remove .vs folder if exists
if (Test-Path ".vs") {
    $vsSize = (Get-ChildItem -Path .vs -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "Removing .vs folder: $([math]::Round($vsSize, 2)) MB" -ForegroundColor Gray
    Remove-Item .vs -Recurse -Force -ErrorAction SilentlyContinue
    $totalSize += $vsSize
}

Write-Host "`nCleaning completed! Freed up: $([math]::Round($totalSize, 2)) MB" -ForegroundColor Green
Write-Host "`nTo compress the project, use:" -ForegroundColor Cyan
Write-Host "Compress-Archive -Path . -DestinationPath SWD_Backend_Group5.zip -Force" -ForegroundColor Yellow





