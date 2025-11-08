# 🔧 Fix Migrations cho PostgreSQL

## Vấn đề

Migrations hiện tại được tạo cho SQL Server (có `nvarchar`, `datetime2`, etc.), nhưng PostgreSQL dùng `varchar`, `timestamp`, etc.

## Giải pháp

Cần xóa migrations cũ và tạo migrations mới cho PostgreSQL.

### Bước 1: Backup Migrations (Optional)

Nếu muốn giữ migrations cũ (để tham khảo):

```bash
# Tạo backup
cp -r src/EVRentalSystem.Infrastructure/Migrations src/EVRentalSystem.Infrastructure/Migrations_SQLServer_Backup
```

### Bước 2: Xóa Migrations cũ

```bash
# Xóa folder Migrations
rm -rf src/EVRentalSystem.Infrastructure/Migrations
```

**Hoặc trên Windows PowerShell:**
```powershell
Remove-Item -Recurse -Force src/EVRentalSystem.Infrastructure/Migrations
```

### Bước 3: Tạo Migrations mới cho PostgreSQL

Đảm bảo connection string đã được set (User Secrets):

```bash
dotnet ef migrations add InitialCreatePostgreSQL --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API
```

### Bước 4: Chạy Migrations

```bash
dotnet ef database update --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API
```

### Bước 5: Verify

Kiểm tra database đã có các bảng:
- Users
- Stations
- Vehicles
- Bookings
- Rentals
- Payments
- MaintenanceSchedules
- MaintenanceRecords
- VehicleInspections

## Lưu ý

- ⚠️ Database trên Railway sẽ bị xóa và tạo lại (nếu có dữ liệu, sẽ mất!)
- ✅ Migrations mới sẽ tự động dùng PostgreSQL syntax
- ✅ Entity Framework Core sẽ tự động map kiểu dữ liệu đúng

