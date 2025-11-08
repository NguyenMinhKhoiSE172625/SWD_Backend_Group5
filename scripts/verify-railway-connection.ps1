# Script để verify kết nối đến Railway database

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🔍 Verify Railway Database Connection" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra User Secrets
Write-Host "1. Kiểm tra User Secrets..." -ForegroundColor Yellow
$secrets = dotnet user-secrets list --project src/EVRentalSystem.API 2>&1

if ($secrets -match "ConnectionStrings:DefaultConnection") {
    Write-Host "   ✅ Connection string đã được set trong User Secrets" -ForegroundColor Green
    
    # Lấy connection string (ẩn password)
    $connString = dotnet user-secrets get "ConnectionStrings:DefaultConnection" --project src/EVRentalSystem.API 2>&1
    
    if ($connString -match "railway") {
        Write-Host "   ✅ Connection string chứa 'railway' - Đúng!" -ForegroundColor Green
        
        # Extract thông tin
        if ($connString -match "gondola\.proxy\.rlwy\.net|railway\.internal") {
            Write-Host "   ✅ Host: Railway (gondola.proxy.rlwy.net hoặc railway.internal)" -ForegroundColor Green
        }
        
        if ($connString -match "Database=([^;]+)") {
            $dbName = $matches[1]
            Write-Host "   📦 Database name: $dbName" -ForegroundColor Cyan
        } elseif ($connString -match "/([^?]+)") {
            $dbName = $matches[1]
            Write-Host "   📦 Database name: $dbName" -ForegroundColor Cyan
        }
    } else {
        Write-Host "   ⚠️  Connection string KHÔNG chứa 'railway'" -ForegroundColor Yellow
        Write-Host "   💡 Có thể đang dùng connection string từ appsettings.json" -ForegroundColor Gray
    }
} else {
    Write-Host "   ❌ Connection string CHƯA được set trong User Secrets" -ForegroundColor Red
    Write-Host "   💡 Chạy: dotnet user-secrets set 'ConnectionStrings:DefaultConnection' 'postgresql://...'" -ForegroundColor Gray
}

Write-Host ""

# Kiểm tra appsettings files
Write-Host "2. Kiểm tra appsettings files..." -ForegroundColor Yellow

if (Test-Path "src/EVRentalSystem.API/appsettings.Development.json") {
    $devSettings = Get-Content "src/EVRentalSystem.API/appsettings.Development.json" | ConvertFrom-Json
    if ($devSettings.ConnectionStrings.DefaultConnection -match "railway") {
        Write-Host "   ✅ appsettings.Development.json có Railway connection string" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  appsettings.Development.json có connection string khác (SQL Server local?)" -ForegroundColor Yellow
        Write-Host "      Connection: $($devSettings.ConnectionStrings.DefaultConnection.Substring(0, [Math]::Min(50, $devSettings.ConnectionStrings.DefaultConnection.Length)))..." -ForegroundColor Gray
    }
}

Write-Host ""

# Hướng dẫn verify
Write-Host "3. Cách verify đã kết nối đến Railway:" -ForegroundColor Yellow
Write-Host ""
Write-Host "   a) Kiểm tra logs khi chạy ứng dụng:" -ForegroundColor Cyan
Write-Host "      - '📊 Sử dụng PostgreSQL database' ✅" -ForegroundColor Gray
Write-Host "      - '📦 Database: railway' (KHÔNG phải EVRentalSystemDB)" -ForegroundColor Gray
Write-Host "      - '✅ Database đã sẵn sàng!'" -ForegroundColor Gray
Write-Host ""
Write-Host "   b) Kiểm tra trong Railway Dashboard:" -ForegroundColor Cyan
Write-Host "      - Vào Database → Metrics" -ForegroundColor Gray
Write-Host "      - Xem 'Connections' - phải > 0 khi app chạy" -ForegroundColor Gray
Write-Host "      - Xem 'Queries' - phải có queries khi test API" -ForegroundColor Gray
Write-Host ""
Write-Host "   c) Test API và kiểm tra dữ liệu:" -ForegroundColor Cyan
Write-Host "      - Chạy: dotnet run --project src/EVRentalSystem.API" -ForegroundColor Gray
Write-Host "      - Test API: http://localhost:5085/swagger" -ForegroundColor Gray
Write-Host "      - Tạo dữ liệu mới → Kiểm tra trong Railway Dashboard" -ForegroundColor Gray
Write-Host ""

# Kiểm tra package
Write-Host "4. Kiểm tra package PostgreSQL..." -ForegroundColor Yellow
$package = dotnet list src/EVRentalSystem.Infrastructure package | Select-String "Npgsql"
if ($package) {
    Write-Host "   ✅ Package Npgsql đã được cài đặt" -ForegroundColor Green
} else {
    Write-Host "   ❌ Package Npgsql CHƯA được cài đặt" -ForegroundColor Red
    Write-Host "   💡 Chạy: dotnet add src/EVRentalSystem.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ Kiểm tra hoàn tất!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan

