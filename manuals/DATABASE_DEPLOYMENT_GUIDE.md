# 🗄️ Hướng dẫn Deploy Database - EV Rental System

## 📋 Vấn đề

Hiện tại, mỗi thành viên trong team clone project về và chạy backend trên máy local sẽ có **database riêng biệt**. Điều này dẫn đến:

- ❌ Dữ liệu không đồng bộ giữa các thành viên
- ❌ Frontend không thể test với dữ liệu thống nhất
- ❌ Khó khăn trong việc demo và test integration
- ❌ Mỗi người phải tự seed data riêng

## 🎯 Giải pháp

Có **3 phương án** tùy thuộc vào mục đích sử dụng:

---

## 📌 Phương án 1: SQLite cho Development (Không cần deploy) ✅

### Khi nào dùng:
- ✅ Development cá nhân
- ✅ Test nhanh tính năng mới
- ✅ Không cần dữ liệu đồng bộ

### Ưu điểm:
- ✅ Không cần cài đặt SQL Server
- ✅ Setup nhanh, dễ dàng
- ✅ Database là file `.db`, có thể xóa và tạo lại dễ dàng
- ✅ Phù hợp cho development

### Cách setup:

1. **Sửa `appsettings.Development.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=EVRentalSystem.db"
  }
}
```

2. **Sửa `Program.cs` để hỗ trợ SQLite:**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Dùng SQLite cho development
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
    else
    {
        // Dùng SQL Server cho production
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});
```

---

## 📌 Phương án 2: Database Chung cho Team (Cần deploy) 🚀

### Khi nào dùng:
- ✅ Frontend cần test với dữ liệu thống nhất
- ✅ Team cần dữ liệu đồng bộ để demo
- ✅ Cần test integration giữa các thành viên

### Ưu điểm:
- ✅ Dữ liệu đồng bộ cho toàn team
- ✅ Frontend có thể test với backend chung
- ✅ Dễ dàng demo và test integration

### Nhược điểm:
- ⚠️ Cần setup database server (SQL Server, PostgreSQL, hoặc Cloud Database)
- ⚠️ Cần quản lý connection string
- ⚠️ Có thể có xung đột khi nhiều người cùng test

### Các lựa chọn deploy:

#### Option A: SQL Server trên máy của một thành viên (Local Network)

**Setup:**
1. Một thành viên mở SQL Server và cho phép remote connection
2. Tạo database chung: `EVRentalSystemDB_Shared`
3. Cấu hình connection string cho tất cả thành viên:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.1.100\\SQLEXPRESS;Database=EVRentalSystemDB_Shared;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

**Lưu ý:**
- ⚠️ Máy host phải luôn bật
- ⚠️ Cần cấu hình firewall
- ⚠️ Chỉ phù hợp khi team làm việc cùng mạng LAN

#### Option B: Cloud Database (Khuyến nghị) ☁️

**Các dịch vụ đề xuất:**

1. **Azure SQL Database** (Microsoft)
   - Free tier: 32GB, 2 DTU
   - Connection string format:
   ```
   Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=EVRentalSystemDB;Persist Security Info=False;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
   ```

2. **AWS RDS** (Amazon)
   - Free tier: 750 giờ/tháng
   - Hỗ trợ SQL Server, PostgreSQL, MySQL

3. **Railway** (Dễ setup nhất - Khuyến nghị) ⭐⭐⭐
   - Free tier: $5 credit/tháng
   - Setup trong 5 phút (đơn giản hơn Azure rất nhiều!)
   - Connection string tự động generate
   - Không cần cấu hình firewall
   - 📖 **Xem hướng dẫn chi tiết:** [RAILWAY_DEPLOYMENT_GUIDE.md](./RAILWAY_DEPLOYMENT_GUIDE.md)
   - 🚀 **Quick Start:** Chạy script `.\scripts\setup-railway-database.ps1`

4. **Supabase** (PostgreSQL) ⭐
   - Free tier: 500MB database
   - Setup rất dễ
   - Cần đổi sang PostgreSQL

5. **Neon** (PostgreSQL)
   - Free tier: 0.5GB storage
   - Serverless PostgreSQL

#### Option C: Docker SQL Server (Local nhưng dễ share)

**Setup:**
1. Một thành viên chạy SQL Server trong Docker:
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

2. Connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=EVRentalSystemDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
  }
}
```

---

## 📌 Phương án 3: Hybrid - SQLite (Dev) + Cloud (Shared) 🔄

### Khi nào dùng:
- ✅ Development cá nhân dùng SQLite
- ✅ Frontend test với Cloud Database
- ✅ Linh hoạt nhất

### Setup:

1. **Cấu hình nhiều connection strings:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=EVRentalSystem.db",
    "SharedConnection": "Server=xxx.railway.app;Database=railway;User Id=postgres;Password=xxx;"
  }
}
```

2. **Sửa `Program.cs`:**
```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Nếu có biến môi trường USE_SHARED_DB, dùng database chung
if (Environment.GetEnvironmentVariable("USE_SHARED_DB") == "true")
{
    connectionString = builder.Configuration.GetConnectionString("SharedConnection");
    options.UseSqlServer(connectionString); // hoặc UseNpgsql nếu dùng PostgreSQL
}
else
{
    options.UseSqlite(connectionString);
}
```

3. **Chạy với database chung:**
```bash
# Windows PowerShell
$env:USE_SHARED_DB="true"
dotnet run --project src/EVRentalSystem.API

# Windows CMD
set USE_SHARED_DB=true
dotnet run --project src/EVRentalSystem.API

# Linux/Mac
USE_SHARED_DB=true dotnet run --project src/EVRentalSystem.API
```

---

## 🎯 Khuyến nghị cho Team

### Scenario 1: Development cá nhân
→ **Dùng SQLite** (Phương án 1)
- Setup nhanh, không cần cấu hình gì
- Mỗi người có database riêng để test

### Scenario 2: Frontend cần test với Backend
→ **Deploy Cloud Database** (Phương án 2 - Option B)
- Railway hoặc Supabase (dễ setup)
- Tất cả thành viên cùng kết nối đến 1 database
- Frontend có thể test với dữ liệu thống nhất

### Scenario 3: Cần linh hoạt
→ **Hybrid** (Phương án 3)
- Dev cá nhân: SQLite
- Frontend/Integration test: Cloud Database

---

## 📝 Checklist Setup Database Chung

### Bước 1: Chọn dịch vụ Cloud Database
- [ ] Đăng ký tài khoản (Railway/Supabase/Azure)
- [ ] Tạo database mới
- [ ] Copy connection string

### Bước 2: Cấu hình Backend
- [ ] Thêm connection string vào `appsettings.Development.json`
- [ ] Test kết nối database
- [ ] Chạy migrations: `dotnet ef database update`

### Bước 3: Share với Team
- [ ] Tạo file `.env.example` với connection string mẫu
- [ ] Hướng dẫn team setup connection string
- [ ] Seed data mẫu vào database chung

### Bước 4: Bảo mật
- [ ] ⚠️ **KHÔNG commit** connection string thật vào Git
- [ ] Dùng biến môi trường hoặc `.env` (và thêm vào `.gitignore`)
- [ ] Chỉ share connection string qua private channel (Slack/Discord)

---

## 🔒 Bảo mật Connection String

### ❌ KHÔNG làm:
```json
// appsettings.json - KHÔNG commit password thật
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=xxx;Password=MyRealPassword123;"
  }
}
```

### ✅ NÊN làm:

**Cách 1: Dùng User Secrets (Khuyến nghị cho .NET)**
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=xxx;Password=xxx;"
```

**Cách 2: Dùng biến môi trường**
```bash
# Windows
set ConnectionStrings__DefaultConnection="Server=xxx;Password=xxx;"

# Linux/Mac
export ConnectionStrings__DefaultConnection="Server=xxx;Password=xxx;"
```

**Cách 3: Dùng file `.env` (cần thêm package)**
```bash
dotnet add package DotNetEnv
```

```csharp
// Program.cs
DotNetEnv.Env.Load();
```

---

## 🚀 Quick Start - Setup Railway Database

1. **Đăng ký Railway:**
   - Vào https://railway.app
   - Đăng nhập bằng GitHub

2. **Tạo Database:**
   - New Project → Add Database → PostgreSQL
   - Copy connection string (dạng: `postgresql://postgres:password@host:port/railway`)

3. **Cấu hình Backend:**
   - Thêm vào `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "SharedConnection": "Host=xxx.railway.app;Database=railway;Username=postgres;Password=xxx;"
     }
   }
   ```

4. **Cài package Npgsql (nếu dùng PostgreSQL):**
   ```bash
   dotnet add src/EVRentalSystem.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
   ```

5. **Cập nhật `Program.cs`:**
   ```csharp
   options.UseNpgsql(builder.Configuration.GetConnectionString("SharedConnection"));
   ```

6. **Chạy migrations:**
   ```bash
   dotnet ef database update --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API
   ```

---

## 📊 So sánh các phương án

| Phương án | Setup | Chi phí | Đồng bộ | Phù hợp |
|-----------|-------|---------|---------|---------|
| SQLite (Local) | ⭐⭐⭐⭐⭐ Rất dễ | Free | ❌ Không | Dev cá nhân |
| SQL Server (LAN) | ⭐⭐ Khó | Free | ✅ Có | Team cùng mạng |
| Cloud Database | ⭐⭐⭐⭐ Dễ | Free tier | ✅ Có | Frontend test |
| Hybrid | ⭐⭐⭐ Trung bình | Free | ✅ Có | Linh hoạt |

---

## 🆘 Troubleshooting

### Lỗi: "Cannot connect to SQL Server"
- Kiểm tra connection string
- Kiểm tra firewall/network
- Kiểm tra SQL Server đã start chưa

### Lỗi: "Database does not exist"
- Chạy migrations: `dotnet ef database update`
- Kiểm tra database name trong connection string

### Lỗi: "Login failed for user"
- Kiểm tra username/password
- Kiểm tra SQL Server authentication mode (SQL/Windows)

---

## 📞 Support

Nếu gặp vấn đề:
1. Kiểm tra logs trong `logs/` folder
2. Xem connection string đã đúng chưa
3. Test kết nối bằng SQL Server Management Studio (nếu dùng SQL Server)
4. Hỏi team lead hoặc tạo issue trên GitHub

---

## ✅ Kết luận

**Câu trả lời: CÓ CẦN deploy database nếu:**
- ✅ Frontend cần test với backend
- ✅ Team cần dữ liệu đồng bộ
- ✅ Cần demo hoặc test integration

**KHÔNG CẦN deploy database nếu:**
- ❌ Chỉ development cá nhân
- ❌ Test tính năng độc lập
- ❌ Không cần dữ liệu đồng bộ

**Khuyến nghị:**
- 🎯 **Development cá nhân**: Dùng SQLite
- 🎯 **Frontend/Integration test**: Deploy Cloud Database (Railway/Supabase)
- 🎯 **Production**: Deploy Cloud Database (Azure/AWS)

---

**Happy Coding! 🎉**

