# 🚗 EV Rental System - Backend API

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Hệ thống quản lý thuê xe điện tại các điểm thuê (EV Station-based Rental System) - Backend API được xây dựng bằng .NET Core 8 với Clean Architecture.

---

## 🚀 Quick Start cho Team Members

### Bạn mới pull dự án về?

👉 **Đọc ngay**: [SETUP_GUIDE.md](SETUP_GUIDE.md) - Hướng dẫn setup từ A-Z

### TL;DR - Chạy nhanh trong 3 bước:

```bash
# 1. Clone repository
git clone https://github.com/NguyenMinhKhoiSE172625/SWD_Backend_Group5.git
cd SWD_Backend_Group5

# 2. Restore packages
dotnet restore

# 3. Chạy ứng dụng
dotnet run --project src/EVRentalSystem.API
```

Swagger UI sẽ tự động mở tại: **http://localhost:5085/swagger** 🎉

---

## 📋 Mục lục

- [Tính năng](#-tính-năng)
- [Công nghệ](#-công-nghệ)
- [Kiến trúc](#-kiến-trúc)
- [API Documentation](#-api-documentation)
- [Test Accounts](#-test-accounts)
- [Hướng dẫn Frontend](#-hướng-dẫn-frontend)

---

## ✨ Tính năng

### 👤 Người thuê (Renter)
- ✅ Đăng ký & đăng nhập
- ✅ Tìm điểm thuê gần nhất
- ✅ Xem xe có sẵn
- ✅ Đặt xe trước
- ✅ Xem lịch sử thuê xe

### 👨‍💼 Nhân viên điểm thuê (Station Staff)
- ✅ Xác thực khách hàng
- ✅ **Giao xe** (Vehicle Pickup)
  - Kiểm tra tình trạng xe
  - Chụp ảnh xe
  - Ghi nhận mức pin
- ✅ **Nhận xe trả** (Vehicle Return)
  - Kiểm tra tình trạng xe
  - Tính toán phí tự động
  - Ghi nhận hư hỏng (nếu có)
- ✅ Quản lý thanh toán

### 🔧 Quản trị viên (Admin)
- ✅ Quản lý điểm thuê
- ✅ Quản lý xe
- ✅ Quản lý nhân viên
- ✅ Báo cáo & phân tích

---

## 🛠 Công nghệ

- **.NET Core 8** - Web API Framework
- **Entity Framework Core 9** - ORM
- **SQLite** - Database (dễ dàng chuyển sang SQL Server/PostgreSQL)
- **JWT Bearer Authentication** - Xác thực
- **Swagger/OpenAPI** - API Documentation
- **BCrypt.Net** - Mã hóa mật khẩu

### Architecture Pattern
- **Clean Architecture** (Domain, Application, Infrastructure, API)
- **Repository Pattern**
- **Dependency Injection**

---

## 🏗 Kiến trúc

```
SWD_Backend_Group5/
├── src/
│   ├── EVRentalSystem.API/              # Controllers, Program.cs
│   ├── EVRentalSystem.Application/      # DTOs, Interfaces
│   ├── EVRentalSystem.Domain/           # Entities, Enums
│   └── EVRentalSystem.Infrastructure/   # Services, DbContext
├── SETUP_GUIDE.md                       # 👈 Hướng dẫn setup
├── README.md                            # 👈 Bạn đang đọc
└── EVRentalSystem.sln                   # Solution file
```

---

## 📚 API Documentation

### Swagger UI
Khi ứng dụng đang chạy, truy cập: **http://localhost:5085/swagger**

### API Endpoints Summary

#### 🔐 Authentication (`/api/Auth`)
- `POST /api/Auth/register` - Đăng ký
- `POST /api/Auth/login` - Đăng nhập
- `GET /api/Auth/profile` - Thông tin user
- `POST /api/Auth/verify/{userId}` - Xác thực user (Staff/Admin)

#### 📍 Stations (`/api/Stations`)
- `GET /api/Stations` - Danh sách điểm thuê
- `GET /api/Stations/{id}` - Chi tiết điểm thuê
- `GET /api/Stations/nearby` - Tìm điểm gần nhất

#### 🚗 Vehicles (`/api/Vehicles`)
- `GET /api/Vehicles` - Danh sách xe
- `GET /api/Vehicles/{id}` - Chi tiết xe
- `GET /api/Vehicles/available` - Xe có sẵn

#### 📅 Bookings (`/api/Bookings`)
- `POST /api/Bookings/create` - Đặt xe
- `GET /api/Bookings/my-bookings` - Booking của tôi
- `POST /api/Bookings/{id}/confirm` - Xác nhận booking
- `POST /api/Bookings/{id}/cancel` - Hủy booking

#### 🔑 Rentals (`/api/Rentals`) - **Giao/Nhận xe**
- `POST /api/Rentals/create` - **Giao xe**
- `POST /api/Rentals/complete` - **Nhận xe trả**
- `GET /api/Rentals/active` - Giao dịch đang hoạt động
- `GET /api/Rentals/{id}` - Chi tiết giao dịch

#### 💳 Payments (`/api/Payments`)
- `POST /api/Payments/create` - Tạo thanh toán
- `GET /api/Payments/rental/{rentalId}` - Lịch sử thanh toán

---

## 🔑 Test Accounts

| Email | Password | Role |
|-------|----------|------|
| admin@example.com | Test@123 | Admin |
| staff1@example.com | Test@123 | StationStaff |
| staff2@example.com | Test@123 | StationStaff |
| renter1@example.com | Test@123 | Renter |
| renter2@example.com | Test@123 | Renter |

### Cách sử dụng trong Swagger:

1. Gọi `POST /api/Auth/login` với email và password
2. Copy `token` từ response
3. Click nút **"Authorize"** 🔒 ở đầu trang Swagger
4. Nhập: `Bearer {token}`
5. Click **"Authorize"**
6. Bây giờ có thể test tất cả API!

---

## 🎨 Hướng dẫn Frontend

### Environment Variables

File `.env.example` đã có sẵn. Copy và đổi tên thành `.env`:

```bash
VITE_API_BASE_URL=http://localhost:5085
VITE_API_AUTH_URL=http://localhost:5085/api/Auth
VITE_SWAGGER_URL=http://localhost:5085/swagger
```

### Axios Setup (React/Vue)

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 30000
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
```

### Response Format

Tất cả API đều trả về format:

```json
{
  "success": true,
  "message": "Thành công",
  "data": { ... },
  "errors": null
}
```

---

## 🔄 Workflow

### Pull code mới nhất
```bash
git pull origin main
```

### Tạo branch mới
```bash
git checkout -b feature/ten-feature
```

### Commit và push
```bash
git add .
git commit -m "feat: mô tả feature"
git push origin feature/ten-feature
```

### Tạo Pull Request trên GitHub

---

## 🧪 Testing

### Test với Swagger UI
1. Mở http://localhost:5085/swagger
2. Login để lấy token
3. Authorize với token
4. Test các endpoints

### Test với Postman
Import Swagger JSON từ: http://localhost:5085/swagger/v1/swagger.json

---

## 🆘 Troubleshooting

### Lỗi: "The command could not be loaded"
→ Chưa cài .NET 8 SDK. Tải tại: https://dotnet.microsoft.com/download/dotnet/8.0

### Lỗi: "Port 5085 already in use"
→ Kill process đang dùng port:
```bash
netstat -ano | findstr :5085
taskkill /PID <PID> /F
```

### Lỗi: Database
→ Xóa file `evrentalsystem.db` và chạy lại ứng dụng

**Xem thêm**: [SETUP_GUIDE.md](SETUP_GUIDE.md#-troubleshooting)

---

## 📖 Tài liệu

- 📘 [SETUP_GUIDE.md](SETUP_GUIDE.md) - **Hướng dẫn setup chi tiết**
- 📗 [manuals/README.md](manuals/README.md) - Tài liệu kỹ thuật
- 📙 [.env.example](.env.example) - Environment variables template
- 🌐 [Swagger UI](http://localhost:5085/swagger) - API Documentation (khi app chạy)

---

## 🤝 Contributing

1. Fork repository
2. Tạo branch mới (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'feat: Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

---

## 📝 License

MIT License - Xem file [LICENSE](LICENSE) để biết chi tiết

---

## 👥 Team

**SWD Backend Group 5**

---

## 🙏 Acknowledgments

- .NET Core Team
- Entity Framework Core Team
- Swagger/OpenAPI

---

⭐ **Nếu project hữu ích, hãy cho một star!** ⭐

**Repository**: https://github.com/NguyenMinhKhoiSE172625/SWD_Backend_Group5

