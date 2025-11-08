# ✅ Next Steps - Sau khi tạo Database trên Railway

## 🎯 Bạn đã làm đúng!

Bạn đã tạo PostgreSQL database trên Railway thành công! Bây giờ cần cấu hình backend local để kết nối đến database này.

## 📋 Các bước tiếp theo

### Bước 1: Lấy Connection String từ Railway

Trong Railway Dashboard (tab Variables bạn đang thấy):

1. **Tìm biến `DATABASE_URL`** hoặc **`DATABASE_PUBLIC_URL`**
2. Click vào biến đó để xem giá trị (click vào dấu sao để hiện password)
3. **Copy connection string**, có 2 format:

**Format 1 (URL format - Railway cung cấp):**
```
postgresql://postgres:password@host:port/railway
```

**Format 2 (Nếu có DATABASE_PUBLIC_URL):**
```
postgresql://postgres:password@host.railway.app:port/railway
```

**Lưu ý**: Connection string đã bao gồm:
- Host, Port, Database name
- Username, Password
- SSL (nếu cần)

### Bước 2: Cài Package PostgreSQL cho .NET

Mở terminal và chạy:

```bash
cd src/EVRentalSystem.Infrastructure
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

### Bước 3: Cấu hình Connection String

**Option A: Environment Variable (Khuyến nghị)**

```powershell
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="postgresql://postgres:password@host:port/railway"
```

**Option B: User Secrets (Khuyến nghị cho development)**

```bash
cd src/EVRentalSystem.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "postgresql://postgres:password@host:port/railway"
```

**Option C: appsettings.Development.json (Cẩn thận - có thể commit vào Git)**

Chỉ dùng nếu bạn chắc chắn file này không bị commit vào Git!

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "postgresql://postgres:password@host:port/railway"
  }
}
```

⚠️ **Lưu ý**: 
- KHÔNG commit connection string có password vào Git!
- Code đã được cấu hình để tự động convert PostgreSQL URL format sang connection string format

### Bước 4: Chạy Migrations

```bash
dotnet ef database update --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API
```

Hoặc ứng dụng sẽ tự động chạy migrations khi khởi động (nếu đã cấu hình trong `Program.cs`).

### Bước 5: Test kết nối

```bash
dotnet run --project src/EVRentalSystem.API
```

Kiểm tra logs để đảm bảo:
- ✅ "📊 Sử dụng PostgreSQL database"
- ✅ "✅ Database 'railway' đã sẵn sàng!"
- ✅ "✅ Tất cả migrations đã được áp dụng thành công!"
- ✅ "✅ Dữ liệu mẫu đã được khởi tạo thành công!"

### Bước 6: Test API

1. Mở Swagger: http://localhost:5085/swagger
2. Test các API endpoints
3. Dữ liệu sẽ được lưu vào database trên Railway!

## 🔍 Verify Database

### Kiểm tra trong Railway Dashboard:

1. Vào tab **"Metrics"** của database
2. Xem:
   - **Connections**: Số kết nối đang active
   - **Queries**: Số queries đang chạy
   - **Database Size**: Dung lượng database

### Kiểm tra bằng SQL:

1. Vào tab **"Database"** (nếu Railway có query editor)
2. Hoặc dùng tool như **pgAdmin**, **DBeaver**, hoặc **VS Code extension**
3. Kết nối bằng connection string từ Railway
4. Chạy query:

```sql
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public'
ORDER BY table_name;
```

Bạn sẽ thấy các bảng:
- Users
- Stations
- Vehicles
- Bookings
- Rentals
- Payments
- MaintenanceSchedules
- MaintenanceRecords
- VehicleInspections
- __EFMigrationsHistory

## 🐛 Troubleshooting

### Lỗi: "Package Npgsql.EntityFrameworkCore.PostgreSQL chưa được cài đặt"

**Giải pháp:**
```bash
dotnet add src/EVRentalSystem.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
```

### Lỗi: "No connection could be made"

**Giải pháp:**
1. Kiểm tra connection string đúng chưa
2. Đảm bảo database đã được tạo (đợi 1-2 phút)
3. Kiểm tra internet connection

### Lỗi: "SSL connection required"

**Giải pháp:**
- Code đã tự động thêm `SSL Mode=Require;` khi convert URL format
- Nếu vẫn lỗi, thêm thủ công: `SSL Mode=Require;` vào connection string

### Lỗi: "relation does not exist"

**Giải pháp:**
- Chạy migrations:
```bash
dotnet ef database update --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API
```

## ✅ Checklist

- [ ] Đã copy connection string từ Railway (DATABASE_URL hoặc DATABASE_PUBLIC_URL)
- [ ] Đã cài package `Npgsql.EntityFrameworkCore.PostgreSQL`
- [ ] Đã set connection string (environment variable hoặc User Secrets)
- [ ] Đã chạy migrations
- [ ] Đã test kết nối (backend chạy thành công)
- [ ] Đã test API (dữ liệu được lưu vào database)
- [ ] Đã verify database trong Railway Dashboard

## 🎉 Hoàn tất!

Bây giờ bạn có:
- ✅ Database trên Railway (PostgreSQL)
- ✅ Backend local kết nối đến database Railway
- ✅ Dữ liệu được lưu trữ trên cloud
- ✅ Team có thể cùng sử dụng 1 database

## 📖 Xem thêm

- [RAILWAY_DATABASE_ONLY.md](./RAILWAY_DATABASE_ONLY.md) - Hướng dẫn chỉ deploy database
- [RAILWAY_DEPLOYMENT_GUIDE.md](./RAILWAY_DEPLOYMENT_GUIDE.md) - Hướng dẫn chi tiết
- [RAILWAY_QUICK_START.md](./RAILWAY_QUICK_START.md) - Quick Start

---

**Happy Coding! 🚀🎉**

