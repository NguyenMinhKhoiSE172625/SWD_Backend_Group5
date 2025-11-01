# 🚗 EV Rental System - Hệ thống thuê xe điện

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

Hệ thống quản lý thuê xe điện tại các điểm thuê (EV Station-based Rental System) - Backend API được xây dựng bằng .NET Core 8 với Clean Architecture.

## 📋 Mục lục

- [Tính năng](#-tính-năng)
- [Công nghệ](#-công-nghệ)
- [Cài đặt](#-cài-đặt)
- [Sử dụng](#-sử-dụng)
- [API Documentation](#-api-documentation)
- [Kiến trúc](#-kiến-trúc)
- [Hướng dẫn Frontend](#-hướng-dẫn-frontend)
- [Contributing](#-contributing)
- [License](#-license)

## ✨ Tính năng

### 👤 Người thuê (Renter)
- ✅ Đăng ký & xác thực tài khoản
- ✅ Tìm điểm thuê trên bản đồ (tìm điểm gần nhất)
- ✅ Xem danh sách xe có sẵn theo điểm thuê
- ✅ Đặt xe trước
- ✅ Xem lịch sử thuê xe

### 👨‍💼 Nhân viên điểm thuê (Station Staff)
- ✅ Xác thực khách hàng mới
- ✅ **Giao xe** (Vehicle Pickup Inspection)
  - Kiểm tra tình trạng xe
  - Chụp ảnh xe
  - Ghi nhận mức pin
- ✅ **Nhận xe trả** (Vehicle Return Inspection)
  - Kiểm tra tình trạng xe khi trả
  - Tính toán phí tự động
  - Ghi nhận hư hỏng (nếu có)
- ✅ Quản lý thanh toán (đặt cọc, phí thuê, phí phát sinh)

### 🔧 Quản trị viên (Admin)
- ✅ Quản lý điểm thuê
- ✅ Quản lý xe (thêm, sửa, xóa)
- ✅ Quản lý nhân viên
- ✅ Báo cáo & phân tích
  - Doanh thu theo điểm thuê
  - Tỷ lệ sử dụng xe
  - Giờ cao điểm

## 🛠 Công nghệ

### Backend
- **.NET Core 8** - Framework chính
- **Entity Framework Core 9** - ORM
- **SQLite** - Database (dễ dàng chuyển sang SQL Server/PostgreSQL)
- **JWT Bearer Authentication** - Xác thực
- **Swagger/OpenAPI** - API Documentation
- **BCrypt.Net** - Mã hóa mật khẩu

### Architecture
- **Clean Architecture** (Domain, Application, Infrastructure, API)
- **Repository Pattern**
- **Dependency Injection**
- **Data Annotations Validation**

## 🚀 Cài đặt

### Yêu cầu
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git

### Bước 1: Clone repository
```bash
git clone https://github.com/your-username/ev-rental-system.git
cd ev-rental-system
```

### Bước 2: Restore packages
```bash
dotnet restore
```

### Bước 3: Chạy ứng dụng
```bash
dotnet run --project src/EVRentalSystem.API
```

Hoặc đơn giản hơn:
```bash
cd src/EVRentalSystem.API
dotnet run
```

### Bước 4: Mở Swagger UI
Ứng dụng sẽ tự động mở Swagger UI trong trình duyệt tại:
```
http://localhost:5085/swagger
```

## 📖 Sử dụng

### Test Accounts

| Email | Password | Role |
|-------|----------|------|
| renter1@example.com | Test@123 | Renter (Người thuê) |
| staff1@example.com | Test@123 | StationStaff (Nhân viên) |
| admin@example.com | Test@123 | Admin |

### Quick Start

1. **Đăng nhập**
```bash
POST /api/Auth/login
{
  "email": "renter1@example.com",
  "password": "Test@123"
}
```

2. **Lấy token từ response và thêm vào header**
```
Authorization: Bearer {your-token}
```

3. **Gọi các API khác**
```bash
GET /api/Stations
GET /api/Vehicles/available?stationId=1
POST /api/Bookings/create
```

## 📚 API Documentation

### Swagger UI
Truy cập http://localhost:5085/swagger để xem tài liệu API đầy đủ với khả năng test trực tiếp.

### API Endpoints

#### 🔐 Authentication
- `POST /api/Auth/register` - Đăng ký tài khoản
- `POST /api/Auth/login` - Đăng nhập
- `GET /api/Auth/profile` - Lấy thông tin user
- `POST /api/Auth/verify/{userId}` - Xác thực user (Staff/Admin)

#### 📍 Stations
- `GET /api/Stations` - Danh sách điểm thuê
- `GET /api/Stations/{id}` - Chi tiết điểm thuê
- `GET /api/Stations/nearby` - Tìm điểm thuê gần nhất

#### 🚗 Vehicles
- `GET /api/Vehicles` - Danh sách xe
- `GET /api/Vehicles/{id}` - Chi tiết xe
- `GET /api/Vehicles/available` - Xe có sẵn

#### 📅 Bookings
- `POST /api/Bookings/create` - Đặt xe
- `GET /api/Bookings/my-bookings` - Booking của tôi
- `POST /api/Bookings/{id}/confirm` - Xác nhận booking
- `POST /api/Bookings/{id}/cancel` - Hủy booking

#### 🔑 Rentals (Giao/Nhận xe)
- `POST /api/Rentals/create` - **Giao xe**
- `POST /api/Rentals/complete` - **Nhận xe trả**
- `GET /api/Rentals/active` - Giao dịch đang hoạt động
- `GET /api/Rentals/{id}` - Chi tiết giao dịch

#### 💳 Payments
- `POST /api/Payments/create` - Tạo thanh toán
- `GET /api/Payments/rental/{rentalId}` - Lịch sử thanh toán

## 🏗 Kiến trúc

```
EVRentalSystem/
├── src/
│   ├── EVRentalSystem.Domain/          # Entities, Enums
│   ├── EVRentalSystem.Application/     # DTOs, Interfaces
│   ├── EVRentalSystem.Infrastructure/  # Services, DbContext
│   └── EVRentalSystem.API/             # Controllers, Program.cs
├── .env                                # Environment variables
├── .env.example                        # Environment template
├── FRONTEND_GUIDE.md                   # Hướng dẫn cho Frontend
├── VALIDATION_TEST_CASES.md            # Test cases validation
└── README.md
```

### Clean Architecture Layers

```
┌─────────────────────────────────────┐
│         API Layer                   │  Controllers, Filters
├─────────────────────────────────────┤
│    Infrastructure Layer             │  Services, DbContext
├─────────────────────────────────────┤
│    Application Layer                │  DTOs, Interfaces
├─────────────────────────────────────┤
│       Domain Layer                  │  Entities, Enums
└─────────────────────────────────────┘
```

## 🎨 Hướng dẫn Frontend

### Environment Variables
Copy file `.env.example` thành `.env`:
```bash
cp .env.example .env
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

### Xem thêm
Đọc file [FRONTEND_GUIDE.md](FRONTEND_GUIDE.md) để biết chi tiết về:
- Authentication flow
- API examples
- Validation rules
- Response format

## 🧪 Testing

### Test với Swagger UI
1. Mở http://localhost:5085/swagger
2. Click "Authorize" và nhập token
3. Test các endpoints

### Test Cases
Xem file [VALIDATION_TEST_CASES.md](VALIDATION_TEST_CASES.md) để biết các test cases chi tiết.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Your Name** - *Initial work*

## 🙏 Acknowledgments

- .NET Core Team
- Entity Framework Core Team
- Swagger/OpenAPI

## 📞 Support

Nếu có vấn đề, vui lòng tạo [Issue](https://github.com/your-username/ev-rental-system/issues) trên GitHub.

---

⭐ **Nếu project hữu ích, hãy cho một star!** ⭐

