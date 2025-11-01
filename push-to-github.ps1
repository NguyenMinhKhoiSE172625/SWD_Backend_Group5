# Script tự động push dự án lên GitHub
# Sử dụng: .\push-to-github.ps1

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  🚀 Push EV Rental System lên GitHub" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra Git đã cài chưa
try {
    git --version | Out-Null
} catch {
    Write-Host "❌ Git chưa được cài đặt!" -ForegroundColor Red
    Write-Host "   Vui lòng cài Git từ: https://git-scm.com/download/win" -ForegroundColor Yellow
    exit 1
}

# Kiểm tra đã có Git repo chưa
if (-not (Test-Path ".git")) {
    Write-Host "📦 Khởi tạo Git repository..." -ForegroundColor Yellow
    git init
    Write-Host "✅ Đã khởi tạo Git repository" -ForegroundColor Green
    Write-Host ""
}

# Nhập thông tin GitHub
Write-Host "📝 Nhập thông tin GitHub repository:" -ForegroundColor Cyan
Write-Host ""

$username = Read-Host "GitHub Username"
$repoName = Read-Host "Repository Name (mặc định: ev-rental-system)"

if ([string]::IsNullOrWhiteSpace($repoName)) {
    $repoName = "ev-rental-system"
}

$remoteUrl = "https://github.com/$username/$repoName.git"

Write-Host ""
Write-Host "🔗 Remote URL: $remoteUrl" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra remote đã tồn tại chưa
$existingRemote = git remote get-url origin 2>$null

if ($existingRemote) {
    Write-Host "⚠️  Remote 'origin' đã tồn tại: $existingRemote" -ForegroundColor Yellow
    $replace = Read-Host "Bạn có muốn thay thế? (y/n)"
    
    if ($replace -eq "y" -or $replace -eq "Y") {
        git remote remove origin
        git remote add origin $remoteUrl
        Write-Host "✅ Đã cập nhật remote origin" -ForegroundColor Green
    }
} else {
    git remote add origin $remoteUrl
    Write-Host "✅ Đã thêm remote origin" -ForegroundColor Green
}

Write-Host ""

# Kiểm tra files sẽ được commit
Write-Host "📋 Kiểm tra files..." -ForegroundColor Yellow
Write-Host ""

$status = git status --short

if ($status) {
    Write-Host "Files sẽ được commit:" -ForegroundColor Cyan
    git status --short
    Write-Host ""
    
    # Cảnh báo nếu có file nhạy cảm
    $sensitiveFiles = @(".env", "*.db", "appsettings.Production.json")
    $foundSensitive = $false
    
    foreach ($pattern in $sensitiveFiles) {
        $files = git ls-files $pattern 2>$null
        if ($files) {
            $foundSensitive = $true
            Write-Host "⚠️  CẢNH BÁO: Tìm thấy file nhạy cảm: $pattern" -ForegroundColor Red
        }
    }
    
    if ($foundSensitive) {
        Write-Host ""
        Write-Host "❌ Vui lòng xóa các file nhạy cảm trước khi push!" -ForegroundColor Red
        Write-Host "   Sử dụng: git rm --cached <filename>" -ForegroundColor Yellow
        Write-Host ""
        $continue = Read-Host "Bạn có chắc muốn tiếp tục? (y/n)"
        if ($continue -ne "y" -and $continue -ne "Y") {
            Write-Host "❌ Đã hủy" -ForegroundColor Red
            exit 1
        }
    }
    
    Write-Host ""
    $confirm = Read-Host "Tiếp tục? (y/n)"
    
    if ($confirm -ne "y" -and $confirm -ne "Y") {
        Write-Host "❌ Đã hủy" -ForegroundColor Red
        exit 1
    }
    
    # Add files
    Write-Host ""
    Write-Host "📦 Đang add files..." -ForegroundColor Yellow
    git add .
    
    # Commit
    Write-Host ""
    $commitMessage = Read-Host "Commit message (mặc định: 'Initial commit')"
    
    if ([string]::IsNullOrWhiteSpace($commitMessage)) {
        $commitMessage = "Initial commit: EV Rental System Backend API"
    }
    
    git commit -m $commitMessage
    Write-Host "✅ Đã commit" -ForegroundColor Green
    
} else {
    Write-Host "ℹ️  Không có thay đổi để commit" -ForegroundColor Yellow
}

Write-Host ""

# Đổi branch thành main
Write-Host "🔄 Đổi branch thành 'main'..." -ForegroundColor Yellow
git branch -M main
Write-Host "✅ Đã đổi branch" -ForegroundColor Green

Write-Host ""

# Push
Write-Host "🚀 Đang push lên GitHub..." -ForegroundColor Yellow
Write-Host ""
Write-Host "⚠️  Lưu ý: Nếu yêu cầu password, hãy dùng Personal Access Token!" -ForegroundColor Yellow
Write-Host "   Tạo token tại: https://github.com/settings/tokens" -ForegroundColor Cyan
Write-Host ""

try {
    git push -u origin main
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  ✅ PUSH THÀNH CÔNG!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "🌐 Repository URL: https://github.com/$username/$repoName" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📝 Next steps:" -ForegroundColor Yellow
    Write-Host "   1. Vào https://github.com/$username/$repoName" -ForegroundColor White
    Write-Host "   2. Kiểm tra README.md hiển thị đúng" -ForegroundColor White
    Write-Host "   3. Thêm topics: dotnet, csharp, api, clean-architecture" -ForegroundColor White
    Write-Host "   4. Thêm description cho repository" -ForegroundColor White
    Write-Host ""
    
    # Mở browser
    $openBrowser = Read-Host "Mở repository trong browser? (y/n)"
    if ($openBrowser -eq "y" -or $openBrowser -eq "Y") {
        Start-Process "https://github.com/$username/$repoName"
    }
    
} catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ❌ PUSH THẤT BẠI!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Lỗi: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Giải pháp:" -ForegroundColor Yellow
    Write-Host "   1. Kiểm tra repository đã tạo trên GitHub chưa" -ForegroundColor White
    Write-Host "   2. Kiểm tra username và repo name đúng chưa" -ForegroundColor White
    Write-Host "   3. Sử dụng Personal Access Token thay vì password" -ForegroundColor White
    Write-Host "   4. Đọc file GITHUB_SETUP.md để biết chi tiết" -ForegroundColor White
    Write-Host ""
    exit 1
}

