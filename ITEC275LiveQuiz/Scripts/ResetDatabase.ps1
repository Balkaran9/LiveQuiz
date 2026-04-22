# PowerShell Script to Reset Database
# This will delete the database and let the app recreate it with correct schema

Write-Host "=== ITEC275LiveQuiz Database Reset ===" -ForegroundColor Cyan
Write-Host ""

$databaseName = "ITEC275LiveQuiz"
$instanceName = "mssqllocaldb"

Write-Host "Stopping LocalDB instance..." -ForegroundColor Yellow
try {
    sqllocaldb stop $instanceName 2>$null
    Start-Sleep -Seconds 2
    Write-Host "? Instance stopped" -ForegroundColor Green
} catch {
    Write-Host "? Could not stop instance (may not be running)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Starting LocalDB instance..." -ForegroundColor Yellow
try {
    sqllocaldb start $instanceName
    Start-Sleep -Seconds 2
    Write-Host "? Instance started" -ForegroundColor Green
} catch {
    Write-Host "? Could not start instance" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Deleting database $databaseName..." -ForegroundColor Yellow

$connectionString = "Server=(localdb)\$instanceName;Integrated Security=true;Connection Timeout=30;"

try {
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection
    $sqlConnection.ConnectionString = $connectionString
    $sqlConnection.Open()
    
    # Kill all connections to the database
    $killQuery = @"
        IF EXISTS (SELECT name FROM sys.databases WHERE name = N'$databaseName')
        BEGIN
            ALTER DATABASE [$databaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            DROP DATABASE [$databaseName];
        END
"@
    
    $command = $sqlConnection.CreateCommand()
    $command.CommandText = $killQuery
    $command.ExecuteNonQuery() | Out-Null
    
    $sqlConnection.Close()
    
    Write-Host "? Database deleted successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Now run your application (F5) and the database will be recreated with correct schema." -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Demo login credentials:" -ForegroundColor White
    Write-Host "  Username: demo" -ForegroundColor Yellow
    Write-Host "  Password: Password123!" -ForegroundColor Yellow
    
} catch {
    Write-Host "? Could not delete database: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Alternative: Run this in SQL Server Management Studio:" -ForegroundColor Cyan
    Write-Host "  DROP DATABASE [ITEC275LiveQuiz];" -ForegroundColor White
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
