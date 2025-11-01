# Hướng dẫn Frontend kết nối API

## 📋 Thông tin API

**Base URL**: `http://localhost:5085`  
**Swagger UI**: `http://localhost:5085/swagger`  
**Format**: JSON  
**Authentication**: JWT Bearer Token

---

## 🔧 Cấu hình Environment Variables

### 1. Copy file `.env.example` thành `.env`
```bash
cp .env.example .env
```

### 2. Sử dụng trong code (React/Vue/Angular)

#### React (Vite):
```javascript
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
const API_AUTH_URL = import.meta.env.VITE_API_AUTH_URL;
```

#### React (Create React App):
Đổi `VITE_` thành `REACT_APP_` trong file `.env`:
```javascript
const API_BASE_URL = process.env.REACT_APP_API_BASE_URL;
```

#### Vue.js:
```javascript
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
```

#### Angular:
Sử dụng `environment.ts`:
```typescript
export const environment = {
  apiBaseUrl: 'http://localhost:5085',
  apiAuthUrl: 'http://localhost:5085/api/Auth'
};
```

---

## 🔐 Authentication Flow

### 1. Đăng ký (Register)
```javascript
POST /api/Auth/register
Content-Type: application/json

{
  "fullName": "Nguyen Van A",
  "email": "user@example.com",
  "phoneNumber": "0901234567",
  "password": "Test@123",
  "driverLicenseNumber": "B123456",
  "idCardNumber": "123456789"
}

// Response:
{
  "success": true,
  "message": "Đăng ký thành công",
  "data": {
    "id": 1,
    "fullName": "Nguyen Van A",
    "email": "user@example.com",
    "role": 1,
    "isVerified": false
  }
}
```

### 2. Đăng nhập (Login)
```javascript
POST /api/Auth/login
Content-Type: application/json

{
  "email": "renter1@example.com",
  "password": "Test@123"
}

// Response:
{
  "success": true,
  "message": "Đăng nhập thành công",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": 1,
      "fullName": "Nguyen Van A",
      "email": "renter1@example.com",
      "role": 1
    }
  }
}
```

### 3. Lưu token và sử dụng cho các request tiếp theo
```javascript
// Lưu token vào localStorage
localStorage.setItem('token', response.data.token);

// Thêm token vào header của mọi request
headers: {
  'Authorization': `Bearer ${localStorage.getItem('token')}`,
  'Content-Type': 'application/json'
}
```

---

## 📡 Ví dụ API Calls

### Axios (React/Vue)

#### Setup Axios Instance:
```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
});

// Interceptor để tự động thêm token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor để xử lý response
api.interceptors.response.use(
  (response) => response.data,
  (error) => {
    if (error.response?.status === 401) {
      // Token hết hạn, redirect về login
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
```

#### Sử dụng:
```javascript
import api from './api';

// Login
const login = async (email, password) => {
  const response = await api.post('/api/Auth/login', { email, password });
  if (response.success) {
    localStorage.setItem('token', response.data.token);
    return response.data;
  }
};

// Get Stations
const getStations = async () => {
  const response = await api.get('/api/Stations');
  return response.data;
};

// Create Booking
const createBooking = async (bookingData) => {
  const response = await api.post('/api/Bookings/create', bookingData);
  return response.data;
};
```

### Fetch API (Vanilla JS)

```javascript
// Login
async function login(email, password) {
  const response = await fetch('http://localhost:5085/api/Auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ email, password })
  });
  
  const data = await response.json();
  if (data.success) {
    localStorage.setItem('token', data.data.token);
  }
  return data;
}

// Get Stations (với token)
async function getStations() {
  const token = localStorage.getItem('token');
  const response = await fetch('http://localhost:5085/api/Stations', {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });
  
  return await response.json();
}
```

---

## 🎯 Các API Endpoints chính

### Auth
- `POST /api/Auth/register` - Đăng ký
- `POST /api/Auth/login` - Đăng nhập
- `GET /api/Auth/profile` - Lấy thông tin user (cần token)
- `POST /api/Auth/verify/{userId}` - Xác thực user (Staff/Admin)

### Stations
- `GET /api/Stations` - Danh sách điểm thuê
- `GET /api/Stations/{id}` - Chi tiết điểm thuê
- `GET /api/Stations/nearby?latitude={lat}&longitude={lng}&radius={km}` - Tìm điểm gần

### Vehicles
- `GET /api/Vehicles` - Danh sách xe
- `GET /api/Vehicles/{id}` - Chi tiết xe
- `GET /api/Vehicles/available?stationId={id}` - Xe có sẵn tại điểm

### Bookings
- `POST /api/Bookings/create` - Đặt xe (Renter)
- `GET /api/Bookings/my-bookings` - Booking của tôi (Renter)
- `POST /api/Bookings/{id}/confirm` - Xác nhận booking (Staff)
- `POST /api/Bookings/{id}/cancel` - Hủy booking

### Rentals
- `POST /api/Rentals/create` - Giao xe (Staff)
- `POST /api/Rentals/complete` - Nhận xe trả (Staff)
- `GET /api/Rentals/active` - Giao dịch đang hoạt động
- `GET /api/Rentals/{id}` - Chi tiết giao dịch

### Payments
- `POST /api/Payments/create` - Tạo thanh toán (Staff)
- `GET /api/Payments/rental/{rentalId}` - Lịch sử thanh toán

---

## 🔑 User Roles

| Role | Value | Quyền |
|------|-------|-------|
| Renter | 1 | Đặt xe, xem booking của mình |
| StationStaff | 2 | Giao/nhận xe, xác thực user, tạo thanh toán |
| Admin | 3 | Toàn quyền |

---

## 🧪 Test Accounts

| Email | Password | Role |
|-------|----------|------|
| renter1@example.com | Test@123 | Renter |
| staff1@example.com | Test@123 | StationStaff |
| admin@example.com | Test@123 | Admin |

---

## ⚠️ Validation Rules

### Register:
- **Email**: Phải đúng format email
- **Password**: Tối thiểu 6 ký tự, có chữ hoa, chữ thường, số, ký tự đặc biệt
- **PhoneNumber**: Số Việt Nam (0xxxxxxxxx hoặc +84xxxxxxxxx)
- **IdCardNumber**: 9-12 chữ số

### Booking:
- **VehicleId**: > 0
- **StationId**: > 0
- **ScheduledPickupTime**: Bắt buộc
- **Notes**: Tối đa 500 ký tự

### Rental:
- **PickupBatteryLevel**: 0-100
- **ReturnBatteryLevel**: 0-100
- **TotalDistance**: 0-10000 km
- **AdditionalFees**: 0-100,000,000 VNĐ

---

## 📝 Response Format

### Success Response:
```json
{
  "success": true,
  "message": "Thành công",
  "data": { ... },
  "errors": null
}
```

### Error Response:
```json
{
  "success": false,
  "message": "Dữ liệu không hợp lệ",
  "data": null,
  "errors": [
    "Email không hợp lệ",
    "Mật khẩu phải có ít nhất 6 ký tự"
  ]
}
```

---

## 🚀 Quick Start

1. **Khởi động Backend**:
```bash
dotnet run --project src/EVRentalSystem.API
```

2. **Kiểm tra Swagger**: http://localhost:5085/swagger

3. **Test API bằng Postman/Insomnia** hoặc trực tiếp trên Swagger

4. **Tích hợp vào Frontend** sử dụng các ví dụ code ở trên

---

## 📞 Support

Nếu có vấn đề, kiểm tra:
1. Backend có đang chạy không? (http://localhost:5085)
2. CORS đã được enable (đã config sẵn)
3. Token có đúng format không? (`Bearer {token}`)
4. Xem Swagger để biết chính xác request/response format

**Swagger UI**: http://localhost:5085/swagger - Tài liệu đầy đủ nhất!

