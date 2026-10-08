$db = Join-Path (Get-Location) "data\medical-orders.db"

if (Test-Path $db) {
    Remove-Item $db -Force
    Write-Host "SQLite database removed: $db"
}
else {
    Write-Host "Database does not exist: $db"
}
