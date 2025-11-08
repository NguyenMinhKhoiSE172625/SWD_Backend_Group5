# ⚡ Railway Quick Start - Deploy Database trong 5 phút

## 🎯 Tại sao Railway?

- ⚡ **Đơn giản nhất**: Setup chỉ 5 phút (vs 15-20 phút của Azure)
- 🔥 **Không cần firewall**: Railway tự động xử lý
- 🆓 **Free tier**: $5 credit/tháng (đủ cho development/staging)
- 🔗 **Connection string tự động**: Copy-paste là xong

## 🚀 3 Bước đơn giản

### Bước 1: Tạo Database trên Railway (2 phút)

1. Đăng ký: https://railway.app (dùng GitHub)
2. **New Project** → **Empty Project**
3. **+ New** → **Database** → **Add PostgreSQL**
4. Đợi 1-2 phút để database được tạo
5. Click vào database → Tab **Variables**
6. Copy **`DATABASE_URL`** hoặc **`POSTGRES_URL`**

### Bước 2: Chạy Script Setup (2 phút)

```powershell
.\scripts\setup-railway-database.ps1
```

Script sẽ:
- ✅ Cài package `Npgsql.EntityFrameworkCore.PostgreSQL`
- ✅ Hướng dẫn set connection string
- ✅ Chạy migrations (nếu muốn)

### Bước 3: Test (1 phút)

```powershell
dotnet run --project src/EVRentalSystem.API
```

Kiểm tra logs:
- ✅ Database connection thành công
- ✅ Migrations đã được áp dụng

## 📋 Hoặc làm thủ công

### 1. Cài package

```bash
dotnet add src/EVRentalSystem.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
```

### 2. Set connection string

**Option A: Environment variable (Khuyến nghị)**

```powershell
$env:ConnectionStrings__DefaultConnection="postgresql://user:password@host:port/database"
```

**Option B: User Secrets**

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "postgresql://user:password@host:port/database"
```

### 3. Chạy migrations

```bash
dotnet ef database update --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API
```

## ✅ Xong!

Database đã được deploy lên Railway! 🎉

## 🔒 Lưu ý

- ⚠️ **KHÔNG commit** connection string vào Git
- ✅ Share connection string qua **private channel** (Slack/Discord)
- ✅ Sử dụng **environment variables** hoặc **User Secrets**

## 📖 Xem thêm

- [RAILWAY_DEPLOYMENT_GUIDE.md](./RAILWAY_DEPLOYMENT_GUIDE.md) - Hướng dẫn chi tiết
- [DATABASE_DEPLOYMENT_GUIDE.md](./DATABASE_DEPLOYMENT_GUIDE.md) - So sánh các phương án

---

**Happy Deploying! 🚂🎉**

