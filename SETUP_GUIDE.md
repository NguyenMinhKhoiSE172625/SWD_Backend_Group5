# 🚀 Hướng dẫn Setup dự án cho Team Members

## 📋 Yêu cầu hệ thống

Trước khi bắt đầu, đảm bảo máy đã cài:

- ✅ [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) - **BẮT BUỘC**
- ✅ [Git](https://git-scm.com/download/win) - **BẮT BUỘC**
- ✅ [Visual Studio 2022](https://visualstudio.microsoft.com/) hoặc [VS Code](https://code.visualstudio.com/) - Khuyên dùng
- ✅ [Postman](https://www.postman.com/downloads/) hoặc browser để test API - Tùy chọn

---

## 📥 Bước 1: Clone Repository

### Cách 1: Dùng Git Command Line
```bash
git clone https://github.com/NguyenMinhKhoiSE172625/SWD_Backend_Group5.git
cd SWD_Backend_Group5
```

### Cách 2: Dùng Visual Studio
1. Mở Visual Studio 2022
2. Click **"Clone a repository"**
3. Paste URL: `https://github.com/NguyenMinhKhoiSE172625/SWD_Backend_Group5.git`
4. Click **"Clone"**

### Cách 3: Dùng VS Code
1. Mở VS Code
2. Nhấn `Ctrl + Shift + P`
3. Gõ `Git: Clone`
4. Paste URL: `https://github.com/NguyenMinhKhoiSE172625/SWD_Backend_Group5.git`

---

## 📦 Bước 2: Restore NuGet Packages

Mở terminal trong thư mục dự án và chạy:

```bash
dotnet restore
```

Lệnh này sẽ tải về tất cả các packages cần thiết:
- Entity Framework Core 9.0.10
- JWT Bearer Authentication 8.0.11
- BCrypt.Net-Next 4.0.3
- Swashbuckle (Swagger) 6.6.2
- SQLite

**Kết quả mong đợi:**
```
Restore completed in 5.2 sec for EVRentalSystem.Domain.csproj.
Restore completed in 5.3 sec for EVRentalSystem.Application.csproj.
Restore completed in 5.4 sec for EVRentalSystem.Infrastructure.csproj.
Restore completed in 5.5 sec for EVRentalSystem.API.csproj.
```

---

## 🗄️ Bước 3: Tạo Database

Database sẽ được tạo **TỰ ĐỘNG** khi chạy ứng dụng lần đầu!

Ứng dụng sử dụng **SQLite** nên không cần cài database server.

File database sẽ được tạo tại: `src/EVRentalSystem.API/evrentalsystem.db`

### Seed Data tự động

Khi chạy lần đầu, hệ thống sẽ tự động tạo:
- ✅ 5 users (Admin, Staff, Renters)
- ✅ 3 stations (Hà Nội, TP.HCM, Đà Nẵng)
- ✅ 6 vehicles (VinFast VF8, VF9, VFe34)

---

## ▶️ Bước 4: Chạy ứng dụng

### Cách 1: Dùng Command Line (Khuyên dùng)
```bash
dotnet run --project src/EVRentalSystem.API
```

### Cách 2: Dùng Visual Studio
1. Mở file `EVRentalSystem.sln`
2. Nhấn `F5` hoặc click **"Start"**

### Cách 3: Dùng VS Code
1. Mở thư mục dự án
2. Nhấn `F5`
3. Chọn `.NET Core` nếu được hỏi

---

## ✅ Bước 5: Kiểm tra ứng dụng đã chạy

Khi chạy thành công, bạn sẽ thấy:

```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5085
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.

========================================
🚀 EV Rental System API đã khởi động!
========================================
📖 Swagger UI: http://localhost:5085/swagger
🌐 API Base URL: http://localhost:5085
========================================

✅ Đã tự động mở Swagger UI trong trình duyệt!
```

**Swagger UI sẽ tự động mở** trong trình duyệt tại: http://localhost:5085/swagger

---

## 🧪 Bước 6: Test API

### Test với Swagger UI (Dễ nhất)

1. **Mở Swagger**: http://localhost:5085/swagger

2. **Đăng nhập để lấy token**:
   - Mở endpoint `POST /api/Auth/login`
   - Click **"Try it out"**
   - Nhập:
   ```json
   {
     "email": "renter1@example.com",
     "password": "Test@123"
   }
   ```
   - Click **"Execute"**
   - Copy `token` từ response

3. **Authorize với token**:
   - Click nút **"Authorize"** 🔒 ở đầu trang
   - Nhập: `Bearer {token-vừa-copy}`
   - Click **"Authorize"**

4. **Test các API khác**:
   - Bây giờ bạn có thể test tất cả các API!

### Test Accounts

| Email | Password | Role |
|-------|----------|------|
| admin@example.com | Test@123 | Admin |
| staff1@example.com | Test@123 | StationStaff |
| staff2@example.com | Test@123 | StationStaff |
| renter1@example.com | Test@123 | Renter |
| renter2@example.com | Test@123 | Renter |

---

## 🔧 Troubleshooting

### ❌ Lỗi: "The command could not be loaded"
**Nguyên nhân**: Chưa cài .NET 8 SDK

**Giải pháp**:
```bash
# Kiểm tra version
dotnet --version

# Nếu không phải 8.x.x, tải về:
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### ❌ Lỗi: "Port 5085 already in use"
**Nguyên nhân**: Port đang được sử dụng

**Giải pháp**:
```bash
# Tìm process đang dùng port
netstat -ano | findstr :5085

# Kill process (thay PID)
taskkill /PID <PID> /F
```

### ❌ Lỗi: "Unable to resolve service for type ApplicationDbContext"
**Nguyên nhân**: Chưa restore packages

**Giải pháp**:
```bash
dotnet restore
dotnet build
```

### ❌ Lỗi: "Database operation failed"
**Nguyên nhân**: Database bị lỗi

**Giải pháp**:
```bash
# Xóa database cũ
rm src/EVRentalSystem.API/evrentalsystem.db

# Chạy lại ứng dụng (sẽ tạo database mới)
dotnet run --project src/EVRentalSystem.API
```

### ❌ Swagger không tự động mở
**Giải pháp**: Mở thủ công tại http://localhost:5085/swagger

---

## 📁 Cấu trúc dự án

```
SWD_Backend_Group5/
├── src/
│   ├── EVRentalSystem.API/              # API Layer (Controllers, Program.cs)
│   ├── EVRentalSystem.Application/      # Application Layer (DTOs, Interfaces)
│   ├── EVRentalSystem.Domain/           # Domain Layer (Entities, Enums)
│   └── EVRentalSystem.Infrastructure/   # Infrastructure Layer (Services, DbContext)
├── .env.example                         # Environment variables template
├── README.md                            # Tài liệu chính
├── FRONTEND_GUIDE.md                    # Hướng dẫn cho Frontend
└── EVRentalSystem.sln                   # Solution file
```

---

## 🔄 Workflow khi làm việc

### 1. Pull code mới nhất
```bash
git pull origin main
```

### 2. Tạo branch mới cho feature
```bash
git checkout -b feature/ten-feature
```

### 3. Code và test

### 4. Commit và push
```bash
git add .
git commit -m "feat: mô tả feature"
git push origin feature/ten-feature
```

### 5. Tạo Pull Request trên GitHub

---

## 📚 Tài liệu tham khảo

- 📖 **README.md** - Tổng quan dự án
- 📖 **FRONTEND_GUIDE.md** - Hướng dẫn tích hợp Frontend
- 📖 **manuals/** - Thư mục chứa các hướng dẫn chi tiết
- 🌐 **Swagger UI** - http://localhost:5085/swagger (khi app đang chạy)

---

## 🆘 Cần giúp đỡ?

- 💬 Hỏi trong group chat
- 📝 Tạo Issue trên GitHub
- 📧 Liên hệ team lead

---

## ✅ Checklist Setup

- [ ] Đã cài .NET 8 SDK
- [ ] Đã clone repository
- [ ] Đã chạy `dotnet restore`
- [ ] Đã chạy ứng dụng thành công
- [ ] Đã mở được Swagger UI
- [ ] Đã test login API thành công
- [ ] Đã authorize và test các API khác

**Nếu tất cả đều ✅, bạn đã sẵn sàng code!** 🎉

---

## 🎯 Quick Start (TL;DR)

```bash
# 1. Clone
git clone https://github.com/NguyenMinhKhoiSE172625/SWD_Backend_Group5.git
cd SWD_Backend_Group5

# 2. Restore packages
dotnet restore

# 3. Chạy
dotnet run --project src/EVRentalSystem.API

# 4. Mở Swagger
# http://localhost:5085/swagger

# 5. Login với:
# Email: renter1@example.com
# Password: Test@123
```

**Xong!** 🚀

