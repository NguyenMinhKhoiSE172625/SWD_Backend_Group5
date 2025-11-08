# 🗄️ Chỉ Deploy Database lên Railway (Không cần Backend)

## ✅ Trả lời ngắn gọn

**CHỈ CẦN DATABASE THÔI!** ✅

- ✅ **Database trên Railway**: Lưu trữ dữ liệu
- ❌ **Backend trên Railway**: KHÔNG CẦN (trừ khi bạn muốn deploy cả backend)
- ✅ **Backend local**: Chạy trên máy bạn, kết nối đến database trên Railway

## 🎯 Kiến trúc đơn giản

```
┌─────────────────────┐         ┌──────────────────────┐
│   Backend Local     │────────▶│  Database Railway    │
│   (Máy của bạn)     │         │  (PostgreSQL)        │
│   localhost:5085    │         │  railway.app         │
└─────────────────────┘         └──────────────────────┘
        │
        │ API calls
        ▼
┌─────────────────────┐
│   Frontend          │
│   (React/Vue)       │
└─────────────────────┘
```

## 🚀 Các bước (CHỈ DATABASE)

### Bước 1: Xóa Backend Service (nếu đã tạo nhầm)

1. Vào Railway Dashboard
2. Click vào service **"SWD_Backend_Group5"** (service đang bị lỗi)
3. Vào tab **"Settings"**
4. Scroll xuống cuối
5. Click **"Delete Service"** hoặc **"Delete"**
6. Xác nhận xóa

**Lưu ý**: Xóa service này KHÔNG ảnh hưởng đến database!

### Bước 2: Tạo CHỈ Database

1. Trong Railway Dashboard, đảm bảo bạn đang ở **Project** level (không phải service)
2. Click **"+ New"** → **"Database"** → **"Add PostgreSQL"**
3. Đợi 1-2 phút để database được tạo
4. Click vào database vừa tạo
5. Vào tab **"Variables"**
6. Copy **`DATABASE_URL`** hoặc **`POSTGRES_URL`**

### Bước 3: Cấu hình Backend Local

**3.1. Cài package PostgreSQL:**

```bash
dotnet add src/EVRentalSystem.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
```

**3.2. Set connection string:**

```powershell
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="postgresql://user:password@host:port/database"
```

**Hoặc dùng User Secrets:**

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "postgresql://user:password@host:port/database"
```

**3.3. Chạy migrations:**

```bash
dotnet ef database update --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API
```

**3.4. Chạy backend:**

```bash
dotnet run --project src/EVRentalSystem.API
```

Backend sẽ chạy trên `localhost:5085` và kết nối đến database trên Railway!

## ✅ Kết quả

- ✅ Database trên Railway: Đang chạy và lưu trữ dữ liệu
- ✅ Backend trên máy local: Chạy và kết nối đến database Railway
- ✅ Frontend: Kết nối đến backend local (`http://localhost:5085`)

## 🔍 Kiểm tra

1. **Database trên Railway:**
   - Vào Railway Dashboard
   - Click vào database
   - Tab **"Metrics"** → Xem connections, queries, etc.

2. **Backend local:**
   - Chạy: `dotnet run --project src/EVRentalSystem.API`
   - Mở: `http://localhost:5085/swagger`
   - Test API → Dữ liệu sẽ được lưu vào database trên Railway!

## ❓ Khi nào cần deploy Backend lên Railway?

**Chỉ khi:**
- ✅ Muốn backend chạy 24/7 (không cần mở máy local)
- ✅ Muốn có domain/public URL cho backend
- ✅ Muốn auto-deploy từ GitHub
- ✅ Muốn scale backend (multiple instances)

**Nếu chỉ cần database chung cho team:**
- ✅ **KHÔNG CẦN** deploy backend
- ✅ Chỉ cần database trên Railway
- ✅ Mỗi người chạy backend local, cùng kết nối đến 1 database

## 🎯 Use Cases

### Use Case 1: Development/Testing (Khuyến nghị)

```
Backend: Local (mỗi developer)
Database: Railway (chung cho team)
Frontend: Local hoặc Netlify/Vercel
```

**Ưu điểm:**
- ✅ Database chung, dữ liệu đồng bộ
- ✅ Backend chạy local, debug dễ
- ✅ Không tốn chi phí deploy backend

### Use Case 2: Production

```
Backend: Railway/Azure App Service (24/7)
Database: Railway/Azure SQL (chung)
Frontend: Netlify/Vercel (static hosting)
```

**Ưu điểm:**
- ✅ Backend chạy 24/7
- ✅ Có public URL
- ✅ Auto-deploy từ GitHub

## 📋 Checklist

- [ ] Đã xóa backend service (nếu đã tạo nhầm)
- [ ] Đã tạo PostgreSQL database trên Railway
- [ ] Đã copy connection string
- [ ] Đã cài package `Npgsql.EntityFrameworkCore.PostgreSQL`
- [ ] Đã set connection string (environment variable hoặc User Secrets)
- [ ] Đã chạy migrations
- [ ] Đã test backend local kết nối đến database Railway
- [ ] Đã verify dữ liệu được lưu vào database

## 🔒 Lưu ý

- ⚠️ **KHÔNG commit** connection string vào Git
- ✅ Share connection string qua **private channel** (Slack/Discord)
- ✅ Database trên Railway có thể truy cập từ bất kỳ đâu (nếu có connection string)
- ✅ Backend local chỉ cần internet để kết nối đến database Railway

---

## ✅ Tóm tắt

**CHỈ CẦN DATABASE THÔI!** ✅

1. Xóa backend service (nếu có)
2. Tạo PostgreSQL database
3. Copy connection string
4. Cấu hình backend local
5. Chạy backend local → Kết nối đến database Railway

**Xong!** 🎉

---

**Xem thêm:**
- [RAILWAY_DEPLOYMENT_GUIDE.md](./RAILWAY_DEPLOYMENT_GUIDE.md) - Hướng dẫn chi tiết
- [RAILWAY_QUICK_START.md](./RAILWAY_QUICK_START.md) - Quick Start

