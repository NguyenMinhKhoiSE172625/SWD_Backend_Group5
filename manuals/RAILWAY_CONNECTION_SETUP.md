# 🔗 Setup Connection String cho Railway Database

## 📋 Connection Strings bạn có

Bạn có 2 connection strings từ Railway:

1. **`DATABASE_URL`** (Internal):
   ```
   postgresql://postgres:FmVQjXuyvmrKPRijVspZeuyWSoWcuXIG@postgres.railway.internal:5432/railway
   ```
   - ❌ **KHÔNG DÙNG** cho backend local
   - Chỉ dùng cho services chạy TRONG Railway network

2. **`DATABASE_PUBLIC_URL`** (Public - Dùng cái này! ✅):
   ```
   postgresql://postgres:FmVQjXuyvmrKPRijVspZeuyWSoWcuXIG@gondola.proxy.rlwy.net:26018/railway
   ```
   - ✅ **DÙNG CÁI NÀY** cho backend local
   - Có thể kết nối từ bên ngoài Railway

## 🚀 Cách Setup (Chọn 1 trong 3 cách)

### Cách 1: Environment Variable (Đơn giản nhất - Khuyến nghị)

**Windows PowerShell:**

```powershell
$env:ConnectionStrings__DefaultConnection="postgresql://postgres:FmVQjXuyvmrKPRijVspZeuyWSoWcuXIG@gondola.proxy.rlwy.net:26018/railway"
```

**Windows CMD:**

```cmd
set ConnectionStrings__DefaultConnection=postgresql://postgres:FmVQjXuyvmrKPRijVspZeuyWSoWcuXIG@gondola.proxy.rlwy.net:26018/railway
```

**Linux/Mac:**

```bash
export ConnectionStrings__DefaultConnection="postgresql://postgres:FmVQjXuyvmrKPRijVspZeuyWSoWcuXIG@gondola.proxy.rlwy.net:26018/railway"
```

**Sau đó chạy backend:**

```bash
dotnet run --project src/EVRentalSystem.API
```

⚠️ **Lưu ý**: Environment variable chỉ tồn tại trong session hiện tại. Nếu đóng terminal, cần set lại.

**Để set vĩnh viễn (Windows):**

```powershell
[System.Environment]::SetEnvironmentVariable('ConnectionStrings__DefaultConnection', 'postgresql://postgres:FmVQjXuyvmrKPRijVspZeuyWSoWcuXIG@gondola.proxy.rlwy.net:26018/railway', 'User')
```

### Cách 2: User Secrets (Bảo mật hơn)

**Bước 1: Thêm UserSecretsId vào project**

Sửa file `src/EVRentalSystem.API/EVRentalSystem.API.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UserSecretsId>ev-rental-system-api-secrets</UserSecretsId>
  </PropertyGroup>
  <!-- ... rest of the file ... -->
</Project>
```

**Bước 2: Set connection string**

```bash
cd src/EVRentalSystem.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "postgresql://postgres:FmVQjXuyvmrKPRijVspZeuyWSoWcuXIG@gondola.proxy.rlwy.net:26018/railway"
```

### Cách 3: appsettings.Development.json (Không khuyến nghị)

⚠️ **CẢNH BÁO**: Chỉ dùng nếu bạn chắc chắn file này KHÔNG bị commit vào Git!

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "postgresql://postgres:FmVQjXuyvmrKPRijVspZeuyWSoWcuXIG@gondola.proxy.rlwy.net:26018/railway"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## 🔧 Chạy Migrations

Sau khi set connection string, chạy migrations:

```bash
dotnet ef database update --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API
```

Hoặc ứng dụng sẽ tự động chạy migrations khi khởi động (nếu đã cấu hình trong `Program.cs`).

## ✅ Test kết nối

```bash
dotnet run --project src/EVRentalSystem.API
```

Kiểm tra logs để đảm bảo:
- ✅ "📊 Sử dụng PostgreSQL database"
- ✅ "✅ Database 'railway' đã sẵn sàng!"
- ✅ "✅ Tất cả migrations đã được áp dụng thành công!"
- ✅ "✅ Dữ liệu mẫu đã được khởi tạo thành công!"

## 🔒 Bảo mật

- ⚠️ **KHÔNG commit** connection string vào Git
- ✅ **KHÔNG share** password trong public channels
- ✅ Share connection string qua **private channel** (Slack/Discord) với team
- ✅ Sử dụng **environment variables** hoặc **User Secrets**

## 🐛 Troubleshooting

### Lỗi: "No connection could be made"

**Giải pháp:**
1. Kiểm tra đang dùng `DATABASE_PUBLIC_URL` (không phải `DATABASE_URL`)
2. Kiểm tra internet connection
3. Kiểm tra database đã được tạo trên Railway (đợi 1-2 phút)

### Lỗi: "SSL connection required"

**Giải pháp:**
- Code đã tự động thêm `SSL Mode=Require;` khi convert URL format
- Nếu vẫn lỗi, connection string đã đúng format

### Lỗi: "Package Npgsql.EntityFrameworkCore.PostgreSQL chưa được cài đặt"

**Giải pháp:**
```bash
dotnet add src/EVRentalSystem.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
```

## 📋 Checklist

- [ ] Đã cài package `Npgsql.EntityFrameworkCore.PostgreSQL`
- [ ] Đã set connection string (dùng `DATABASE_PUBLIC_URL`)
- [ ] Đã chạy migrations
- [ ] Đã test kết nối (backend chạy thành công)
- [ ] Đã test API (dữ liệu được lưu vào database)
- [ ] Đã verify database trong Railway Dashboard

## 🎉 Hoàn tất!

Bây giờ backend local của bạn đã kết nối đến database trên Railway!

---

**Xem thêm:**
- [RAILWAY_DATABASE_ONLY.md](./RAILWAY_DATABASE_ONLY.md) - Hướng dẫn chỉ deploy database
- [RAILWAY_DEPLOYMENT_GUIDE.md](./RAILWAY_DEPLOYMENT_GUIDE.md) - Hướng dẫn chi tiết
- [RAILWAY_NEXT_STEPS.md](./RAILWAY_NEXT_STEPS.md) - Next steps

