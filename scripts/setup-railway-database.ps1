# PowerShell Script để Setup Database trên Railway
# Usage: .\scripts\setup-railway-database.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🚂 Setup Database trên Railway" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "📋 Hướng dẫn nhanh:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Đăng ký Railway: https://railway.app" -ForegroundColor Gray
Write-Host "2. Tạo New Project → Add PostgreSQL" -ForegroundColor Gray
Write-Host "3. Copy connection string từ Variables tab" -ForegroundColor Gray
Write-Host "4. Chạy script này với connection string" -ForegroundColor Gray
Write-Host ""

$connectionString = Read-Host "Nhập connection string từ Railway (hoặc Enter để bỏ qua)"

if ([string]::IsNullOrWhiteSpace($connectionString)) {
    Write-Host ""
    Write-Host "⚠️  Bạn có thể chạy lại script sau khi có connection string" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "📖 Xem hướng dẫn chi tiết: manuals/RAILWAY_DEPLOYMENT_GUIDE.md" -ForegroundColor Cyan
    exit 0
}

Write-Host ""
Write-Host "🔍 Đang kiểm tra connection string..." -ForegroundColor Yellow

# Kiểm tra format
$isPostgresUrl = $connectionString -match "^postgresql://" -or $connectionString -match "^postgres://"
$isPostgresConnection = $connectionString -match "Host=" -or $isPostgresUrl

if (-not $isPostgresConnection) {
    Write-Host "⚠️  Connection string không phải PostgreSQL format!" -ForegroundColor Yellow
    Write-Host "   Railway cung cấp PostgreSQL database" -ForegroundColor Gray
    Write-Host "   Connection string nên có dạng: postgresql://... hoặc Host=..." -ForegroundColor Gray
    $continue = Read-Host "   Bạn có muốn tiếp tục? (y/n)"
    if ($continue -ne "y" -and $continue -ne "Y") {
        exit 1
    }
}

Write-Host "✅ Connection string hợp lệ" -ForegroundColor Green
Write-Host ""

# Bước 1: Cài package Npgsql
Write-Host "📦 Bước 1: Cài package Npgsql.EntityFrameworkCore.PostgreSQL..." -ForegroundColor Yellow
try {
    Push-Location "src/EVRentalSystem.Infrastructure"
    dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.0
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✅ Đã cài package thành công!" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  Có thể package đã được cài hoặc có lỗi" -ForegroundColor Yellow
    }
    Pop-Location
} catch {
    Write-Host "   ❌ Lỗi khi cài package: $_" -ForegroundColor Red
    Pop-Location
    exit 1
}

Write-Host ""

# Bước 2: Set connection string
Write-Host "⚙️  Bước 2: Cấu hình connection string..." -ForegroundColor Yellow

# Option: Set environment variable
Write-Host ""
Write-Host "Chọn cách lưu connection string:" -ForegroundColor Cyan
Write-Host "1. Environment variable (khuyến nghị - không commit vào Git)" -ForegroundColor Gray
Write-Host "2. User Secrets (khuyến nghị - không commit vào Git)" -ForegroundColor Gray
Write-Host "3. appsettings.Development.json (cần cẩn thận - có thể commit vào Git)" -ForegroundColor Gray
Write-Host ""

$choice = Read-Host "Chọn (1/2/3)"

switch ($choice) {
    "1" {
        Write-Host ""
        Write-Host "🔧 Set environment variable..." -ForegroundColor Yellow
        $env:ConnectionStrings__DefaultConnection = $connectionString
        Write-Host "   ✅ Đã set environment variable!" -ForegroundColor Green
        Write-Host ""
        Write-Host "💡 Để set vĩnh viễn (Windows):" -ForegroundColor Cyan
        Write-Host "   [System.Environment]::SetEnvironmentVariable('ConnectionStrings__DefaultConnection', '$connectionString', 'User')" -ForegroundColor Gray
        Write-Host ""
        Write-Host "💡 Hoặc chạy ứng dụng với:" -ForegroundColor Cyan
        Write-Host "   `$env:ConnectionStrings__DefaultConnection='$connectionString'" -ForegroundColor Gray
        Write-Host "   dotnet run --project src/EVRentalSystem.API" -ForegroundColor Gray
    }
    "2" {
        Write-Host ""
        Write-Host "🔧 Set User Secrets..." -ForegroundColor Yellow
        Push-Location "src/EVRentalSystem.API"
        dotnet user-secrets set "ConnectionStrings:DefaultConnection" $connectionString
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ Đã set User Secrets!" -ForegroundColor Green
        } else {
            Write-Host "   ❌ Lỗi khi set User Secrets" -ForegroundColor Red
        }
        Pop-Location
    }
    "3" {
        Write-Host ""
        Write-Host "⚠️  Cảnh báo: File appsettings.Development.json có thể bị commit vào Git!" -ForegroundColor Yellow
        $confirm = Read-Host "Bạn có chắc chắn? (y/n)"
        if ($confirm -eq "y" -or $confirm -eq "Y") {
            $appsettingsPath = "src/EVRentalSystem.API/appsettings.Development.json"
            if (Test-Path $appsettingsPath) {
                $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
                $appsettings.ConnectionStrings.DefaultConnection = $connectionString
                $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
                Write-Host "   ✅ Đã cập nhật appsettings.Development.json!" -ForegroundColor Green
            } else {
                Write-Host "   ❌ Không tìm thấy file appsettings.Development.json" -ForegroundColor Red
            }
        }
    }
    default {
        Write-Host "   ⚠️  Lựa chọn không hợp lệ" -ForegroundColor Yellow
    }
}

Write-Host ""

# Bước 3: Chạy migrations
Write-Host "🔄 Bước 3: Chạy migrations..." -ForegroundColor Yellow
$runMigrations = Read-Host "Bạn có muốn chạy migrations ngay bây giờ? (y/n)"

if ($runMigrations -eq "y" -or $runMigrations -eq "Y") {
    Write-Host ""
    Write-Host "   Đang chạy migrations..." -ForegroundColor Gray
    
    # Set environment variable nếu chưa set
    if (-not $env:ConnectionStrings__DefaultConnection) {
        $env:ConnectionStrings__DefaultConnection = $connectionString
    }
    
    dotnet ef database update --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✅ Migrations đã được áp dụng thành công!" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  Có lỗi khi chạy migrations" -ForegroundColor Yellow
        Write-Host "   💡 Bạn có thể chạy lại sau:" -ForegroundColor Cyan
        Write-Host "      dotnet ef database update --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API" -ForegroundColor Gray
    }
} else {
    Write-Host "   ⏭️  Bỏ qua migrations" -ForegroundColor Yellow
    Write-Host "   💡 Chạy migrations sau:" -ForegroundColor Cyan
    Write-Host "      dotnet ef database update --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ Setup hoàn tất!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 Next steps:" -ForegroundColor Cyan
Write-Host "1. Test kết nối: dotnet run --project src/EVRentalSystem.API" -ForegroundColor Gray
Write-Host "2. Kiểm tra logs để đảm bảo database đã kết nối" -ForegroundColor Gray
Write-Host "3. Mở Swagger: http://localhost:5085/swagger" -ForegroundColor Gray
Write-Host ""
Write-Host "🔒 Lưu ý bảo mật:" -ForegroundColor Yellow
Write-Host "- KHÔNG commit connection string vào Git!" -ForegroundColor Gray
Write-Host "- Chia sẻ connection string qua private channel với team" -ForegroundColor Gray
Write-Host ""
Write-Host "📖 Xem thêm: manuals/RAILWAY_DEPLOYMENT_GUIDE.md" -ForegroundColor Cyan
Write-Host ""

