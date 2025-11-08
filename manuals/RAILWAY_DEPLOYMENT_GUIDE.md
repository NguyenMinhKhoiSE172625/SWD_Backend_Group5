# 🚂 Hướng dẫn Deploy Database lên Railway

## 📋 Tổng quan

Railway là một trong những dịch vụ **đơn giản nhất** để deploy database. Setup chỉ mất **5 phút** và không cần cấu hình phức tạp như Azure.

## ⚠️ Lưu ý quan trọng

**CHỈ CẦN DEPLOY DATABASE THÔI!**

- ✅ **Database trên Railway**: Lưu trữ dữ liệu
- ❌ **Backend trên Railway**: KHÔNG CẦN (trừ khi bạn muốn deploy cả backend)
- ✅ **Backend local**: Chạy trên máy bạn, kết nối đến database trên Railway

Nếu bạn thấy service backend bị lỗi trên Railway, đó là vì bạn đã deploy nhầm backend application. **Chỉ cần xóa service đó và chỉ tạo database thôi!**

📖 **Xem hướng dẫn:** [RAILWAY_DATABASE_ONLY.md](./RAILWAY_DATABASE_ONLY.md)

## ✅ Tại sao chọn Railway?

- ⚡ **Setup cực nhanh**: Chỉ cần vài click (5 phút)
- 🆓 **Free tier tốt**: $5 credit/tháng (đủ cho development/staging)
- 🔗 **Connection string tự động**: Copy-paste là xong
- 🔥 **Không cần firewall**: Railway tự động xử lý
- 📊 **Hỗ trợ PostgreSQL**: Free tier tốt
- 🎯 **Phù hợp cho team**: Dễ share connection string

## 🎯 So sánh với Azure

| Tính năng | Railway | Azure SQL |
|-----------|---------|-----------|
| Setup time | ⚡ **5 phút** | ⏰ 15-20 phút |
| Firewall config | ✅ **Không cần** | ❌ Cần cấu hình |
| Free tier | 🆓 $5/tháng | 💰 ~$5/tháng |
| Connection string | ✅ **Tự động** | ⚙️ Tự tạo |
| Phù hợp | Development/Staging | Production |

**Kết luận**: Railway **đơn giản hơn rất nhiều**! 🎉

---

## 🚀 Hướng dẫn Deploy (5 phút)

### Bước 1: Đăng ký Railway (1 phút)

1. Truy cập: https://railway.app
2. Click **"Start a New Project"** hoặc **"Login"**
3. Đăng nhập bằng **GitHub** (khuyến nghị) hoặc Email
4. Chấp nhận các điều khoản

### Bước 2: Tạo PostgreSQL Database (2 phút)

1. Trong Railway Dashboard, click **"New Project"**
2. Chọn **"Empty Project"**
3. Click **"+ New"** → **"Database"** → **"Add PostgreSQL"**
4. Railway sẽ tự động tạo database cho bạn
5. Đợi 1-2 phút để database được khởi tạo

### Bước 3: Lấy Connection String (1 phút)

1. Click vào database vừa tạo
2. Vào tab **"Variables"** 
3. Tìm biến **`DATABASE_URL`** hoặc **`POSTGRES_URL`**
4. Copy connection string, có 2 format:

**Format 1 (URL format):**
```
postgresql://postgres:password@containers-us-west-xxx.railway.app:5432/railway
```

**Format 2 (cho .NET - cần convert):**
```
Host=containers-us-west-xxx.railway.app;Port=5432;Database=railway;Username=postgres;Password=your_password;SSL Mode=Require;
```

### Bước 4: Cấu hình Backend

#### 4.1. Cài package PostgreSQL

```bash
cd src/EVRentalSystem.Infrastructure
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

#### 4.2. Sửa `Program.cs`

Cập nhật để hỗ trợ cả SQL Server và PostgreSQL:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string không được cấu hình!");
    }
    
    // Detect database type từ connection string
    if (connectionString.Contains("Host=") || 
        connectionString.Contains("postgresql://") ||
        connectionString.StartsWith("postgres://"))
    {
        // PostgreSQL (Railway)
        // Convert URL format nếu cần
        if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
        {
            connectionString = ConvertPostgresUrlToConnectionString(connectionString);
        }
        options.UseNpgsql(connectionString);
    }
    else if (connectionString.Contains("Server=tcp:") || 
             (connectionString.Contains("Server=") && connectionString.Contains("Database=") && !connectionString.Contains("Host=")))
    {
        // SQL Server (Azure hoặc local)
        options.UseSqlServer(connectionString);
    }
    else if (connectionString.Contains("Data Source=") && connectionString.EndsWith(".db"))
    {
        // SQLite (Local development)
        options.UseSqlite(connectionString);
    }
    else
    {
        // Default: SQL Server
        options.UseSqlServer(connectionString);
    }
    
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Helper method để convert PostgreSQL URL to connection string
string ConvertPostgresUrlToConnectionString(string url)
{
    // Format: postgresql://user:password@host:port/database
    var uri = new Uri(url);
    var host = uri.Host;
    var port = uri.Port;
    var database = uri.AbsolutePath.TrimStart('/');
    var username = uri.UserInfo.Split(':')[0];
    var password = uri.UserInfo.Split(':')[1];
    
    return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;";
}
```

#### 4.3. Thêm connection string

**Option A: Dùng biến môi trường (Khuyến nghị)**

```powershell
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Host=containers-us-west-xxx.railway.app;Port=5432;Database=railway;Username=postgres;Password=xxx;SSL Mode=Require;"
```

**Option B: Thêm vào `appsettings.Development.json`**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=containers-us-west-xxx.railway.app;Port=5432;Database=railway;Username=postgres;Password=xxx;SSL Mode=Require;"
  }
}
```

⚠️ **Lưu ý**: KHÔNG commit file này vào Git nếu có password thật!

### Bước 5: Chạy Migrations

**5.1. Tạo migration mới cho PostgreSQL:**

```bash
dotnet ef migrations add RailwayPostgreSQL --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API
```

**5.2. Chạy migrations:**

```bash
dotnet ef database update --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API
```

Hoặc ứng dụng sẽ tự động chạy migrations khi khởi động.

### Bước 6: Test

```bash
dotnet run --project src/EVRentalSystem.API
```

Kiểm tra logs để đảm bảo:
- ✅ Database connection thành công
- ✅ Migrations đã được áp dụng
- ✅ Dữ liệu mẫu đã được seed

---

## 🔄 Quick Start (Copy-paste)

### 1. Tạo database trên Railway

1. Đăng ký: https://railway.app
2. New Project → Add PostgreSQL
3. Copy connection string từ Variables

### 2. Cài package

```bash
dotnet add src/EVRentalSystem.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
```

### 3. Set connection string

```powershell
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Host=xxx.railway.app;Port=5432;Database=railway;Username=postgres;Password=xxx;SSL Mode=Require;"
```

### 4. Chạy migrations

```bash
dotnet ef database update --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API
```

### 5. Test

```bash
dotnet run --project src/EVRentalSystem.API
```

**Xong! 🎉**

---

## 🔒 Bảo mật

### ⚠️ KHÔNG commit Connection String

1. **Dùng biến môi trường** (khuyến nghị)
2. **Dùng User Secrets**:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=xxx.railway.app;Port=5432;Database=railway;Username=postgres;Password=xxx;SSL Mode=Require;"
```

3. **Share qua private channel** (Slack/Discord) với team

---

## 🐛 Troubleshooting

### Lỗi: "No connection could be made"

**Giải pháp**: 
- Kiểm tra connection string
- Đợi 1-2 phút sau khi tạo database
- Kiểm tra database status trong Railway

### Lỗi: "SSL connection required"

**Giải pháp**: Thêm `SSL Mode=Require;` vào connection string

### Lỗi: "relation does not exist"

**Giải pháp**: Chạy migrations:
```bash
dotnet ef database update --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API
```

---

## 💰 Chi phí

- **Free tier**: $5 credit/tháng
- **PostgreSQL**: ~$5/tháng (1GB RAM, 10GB storage)
- **Tự động pause** khi không dùng

**Đủ cho development/staging!** ✅

---

## ✅ Checklist

- [ ] Đã đăng ký Railway
- [ ] Đã tạo PostgreSQL database
- [ ] Đã copy connection string
- [ ] Đã cài package `Npgsql.EntityFrameworkCore.PostgreSQL`
- [ ] Đã sửa `Program.cs`
- [ ] Đã set connection string (biến môi trường)
- [ ] Đã chạy migrations
- [ ] Đã test kết nối

---

**Happy Deploying! 🚂🎉**
