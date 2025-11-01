# API Examples - EV Rental System

Tài liệu này cung cấp các ví dụ cụ thể để test API.

## 🔐 1. Authentication Flow

### 1.1. Đăng ký tài khoản mới (Renter)

**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "fullName": "Lê Văn C",
  "email": "levanc@gmail.com",
  "phoneNumber": "0901234572",
  "password": "User@123",
  "driverLicenseNumber": "B111222333",
  "idCardNumber": "079111222333"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Đăng ký thành công. Vui lòng chờ xác thực từ nhân viên.",
  "data": {
    "id": 6,
    "fullName": "Lê Văn C",
    "email": "levanc@gmail.com",
    "phoneNumber": "0901234572",
    "role": "Renter",
    "isVerified": false
  },
  "errors": null
}
```

### 1.2. Đăng nhập

**Endpoint:** `POST /api/auth/login`

**Request Body (Renter):**
```json
{
  "email": "nguyenvana@gmail.com",
  "password": "User@123"
}
```

**Request Body (Staff):**
```json
{
  "email": "staff1@evrentalsystem.com",
  "password": "Staff@123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": 4,
      "fullName": "Nguyễn Văn A",
      "email": "nguyenvana@gmail.com",
      "phoneNumber": "0901234570",
      "role": "Renter",
      "isVerified": true
    }
  },
  "errors": null
}
```

### 1.3. Xác thực khách hàng (Staff/Admin only)

**Endpoint:** `POST /api/auth/verify/6`

**Headers:**
```
Authorization: Bearer {staff_or_admin_token}
```

**Response:**
```json
{
  "success": true,
  "message": "Xác thực người dùng thành công",
  "data": true,
  "errors": null
}
```

## 🏢 2. Stations (Điểm thuê)

### 2.1. Lấy tất cả điểm thuê

**Endpoint:** `GET /api/stations`

**Response:**
```json
{
  "success": true,
  "message": null,
  "data": [
    {
      "id": 1,
      "name": "Điểm thuê Quận 1",
      "address": "123 Nguyễn Huệ, Quận 1, TP.HCM",
      "latitude": 10.7769,
      "longitude": 106.7009,
      "phoneNumber": "0281234567",
      "description": "Điểm thuê xe trung tâm Quận 1",
      "isActive": true,
      "availableVehiclesCount": 2
    }
  ],
  "errors": null
}
```

### 2.2. Tìm điểm thuê gần

**Endpoint:** `GET /api/stations/nearby?latitude=10.7769&longitude=106.7009&radiusKm=5`

**Response:** Tương tự như trên, chỉ trả về các điểm trong bán kính 5km

## 🚗 3. Vehicles (Xe)

### 3.1. Lấy xe có sẵn tại điểm

**Endpoint:** `GET /api/vehicles/available?stationId=1`

**Response:**
```json
{
  "success": true,
  "message": null,
  "data": [
    {
      "id": 1,
      "licensePlate": "59A-12345",
      "model": "VinFast Klara",
      "brand": "VinFast",
      "year": 2023,
      "color": "Đỏ",
      "batteryCapacity": 100,
      "pricePerHour": 50000,
      "pricePerDay": 300000,
      "status": "Available",
      "imageUrl": null,
      "description": "Xe máy điện VinFast Klara mới 2023",
      "stationId": 1,
      "stationName": "Điểm thuê Quận 1"
    }
  ],
  "errors": null
}
```

### 3.2. Cập nhật trạng thái xe (Staff/Admin)

**Endpoint:** `PUT /api/vehicles/1/status`

**Headers:**
```
Authorization: Bearer {staff_token}
```

**Request Body:**
```json
{
  "status": 3
}
```
*Status: 0=Available, 1=Booked, 2=InUse, 3=Maintenance, 4=Damaged*

### 3.3. Cập nhật pin xe (Staff/Admin)

**Endpoint:** `PUT /api/vehicles/1/battery`

**Request Body:**
```json
{
  "batteryLevel": 85
}
```

## 📅 4. Bookings (Đặt xe)

### 4.1. Tạo đặt xe (Renter)

**Endpoint:** `POST /api/bookings/create`

**Headers:**
```
Authorization: Bearer {renter_token}
```

**Request Body:**
```json
{
  "vehicleId": 1,
  "scheduledPickupTime": "2025-11-02T10:00:00",
  "scheduledReturnTime": "2025-11-02T18:00:00",
  "notes": "Tôi sẽ đến lúc 10h sáng"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Đặt xe thành công",
  "data": {
    "id": 1,
    "bookingCode": "BK20251101001",
    "userId": 4,
    "userName": "Nguyễn Văn A",
    "vehicleId": 1,
    "vehicleName": "VinFast Klara - 59A-12345",
    "stationId": 1,
    "stationName": "Điểm thuê Quận 1",
    "bookingDate": "2025-11-01T10:30:00",
    "scheduledPickupTime": "2025-11-02T10:00:00",
    "scheduledReturnTime": "2025-11-02T18:00:00",
    "status": "Pending",
    "notes": "Tôi sẽ đến lúc 10h sáng"
  },
  "errors": null
}
```

### 4.2. Xem đặt xe của tôi (Renter)

**Endpoint:** `GET /api/bookings/my-bookings`

**Headers:**
```
Authorization: Bearer {renter_token}
```

### 4.3. Xác nhận đặt xe (Staff)

**Endpoint:** `POST /api/bookings/1/confirm`

**Headers:**
```
Authorization: Bearer {staff_token}
```

### 4.4. Hủy đặt xe (Renter)

**Endpoint:** `POST /api/bookings/1/cancel`

**Headers:**
```
Authorization: Bearer {renter_token}
```

## 🚙 5. Rentals (Giao nhận xe) - QUAN TRỌNG

### 5.1. Giao xe cho khách (Staff)

**Endpoint:** `POST /api/rentals/create`

**Headers:**
```
Authorization: Bearer {staff_token}
```

**Request Body:**
```json
{
  "bookingId": 1,
  "vehicleId": 1,
  "pickupBatteryLevel": 100,
  "pickupInspection": {
    "imageUrls": [
      "https://example.com/images/pickup1.jpg",
      "https://example.com/images/pickup2.jpg"
    ],
    "notes": "Xe trong tình trạng tốt, không có vết xước",
    "damageReport": null
  }
}
```

**Response:**
```json
{
  "success": true,
  "message": "Giao xe thành công",
  "data": {
    "id": 1,
    "rentalCode": "RN20251101001",
    "bookingId": 1,
    "userId": 4,
    "userName": "Nguyễn Văn A",
    "vehicleId": 1,
    "vehicleName": "VinFast Klara - 59A-12345",
    "pickupTime": "2025-11-01T10:35:00",
    "returnTime": null,
    "pickupBatteryLevel": 100,
    "returnBatteryLevel": null,
    "totalDistance": null,
    "totalAmount": null,
    "additionalFees": null,
    "status": "Active",
    "pickupStaffId": 2,
    "pickupStaffName": "Nhân viên Quận 1"
  },
  "errors": null
}
```

### 5.2. Nhận xe trả từ khách (Staff)

**Endpoint:** `POST /api/rentals/complete`

**Headers:**
```
Authorization: Bearer {staff_token}
```

**Request Body (Không có hư hỏng):**
```json
{
  "rentalId": 1,
  "returnBatteryLevel": 45,
  "totalDistance": 50.5,
  "additionalFees": 0,
  "additionalFeesReason": null,
  "returnInspection": {
    "imageUrls": [
      "https://example.com/images/return1.jpg",
      "https://example.com/images/return2.jpg"
    ],
    "notes": "Xe trả về bình thường",
    "damageReport": null
  }
}
```

**Request Body (Có hư hỏng):**
```json
{
  "rentalId": 1,
  "returnBatteryLevel": 30,
  "totalDistance": 75.0,
  "additionalFees": 500000,
  "additionalFeesReason": "Xe bị xước và vỡ gương",
  "returnInspection": {
    "imageUrls": [
      "https://example.com/images/damage1.jpg",
      "https://example.com/images/damage2.jpg"
    ],
    "notes": "Phát hiện hư hỏng",
    "damageReport": "Xe bị xước bên hông trái, gương chiếu hậu bị vỡ"
  }
}
```

**Response:**
```json
{
  "success": true,
  "message": "Nhận xe trả thành công",
  "data": {
    "id": 1,
    "rentalCode": "RN20251101001",
    "bookingId": 1,
    "userId": 4,
    "userName": "Nguyễn Văn A",
    "vehicleId": 1,
    "vehicleName": "VinFast Klara - 59A-12345",
    "pickupTime": "2025-11-01T10:35:00",
    "returnTime": "2025-11-01T18:30:00",
    "pickupBatteryLevel": 100,
    "returnBatteryLevel": 45,
    "totalDistance": 50.5,
    "totalAmount": 400000,
    "additionalFees": 0,
    "status": "Completed",
    "pickupStaffId": 2,
    "pickupStaffName": "Nhân viên Quận 1",
    "returnStaffId": 2,
    "returnStaffName": "Nhân viên Quận 1"
  },
  "errors": null
}
```

*Lưu ý: `totalAmount` được tính tự động:*
- *Nếu thuê ≤ 24 giờ: Số giờ × `pricePerHour`*
- *Nếu thuê > 24 giờ: Số ngày × `pricePerDay`*

### 5.3. Xem lịch sử thuê xe (Renter)

**Endpoint:** `GET /api/rentals/my-rentals`

**Headers:**
```
Authorization: Bearer {renter_token}
```

### 5.4. Xem xe đang thuê (Staff/Admin)

**Endpoint:** `GET /api/rentals/active`

**Headers:**
```
Authorization: Bearer {staff_token}
```

## 💳 6. Payments (Thanh toán)

### 6.1. Tạo thanh toán (Staff)

**Endpoint:** `POST /api/payments/create`

**Headers:**
```
Authorization: Bearer {staff_token}
```

**Request Body:**
```json
{
  "rentalId": 1,
  "amount": 400000,
  "type": 1,
  "paymentMethod": "Cash",
  "notes": "Thanh toán tiền thuê xe"
}
```

*Payment Type: 0=Deposit, 1=RentalFee, 2=AdditionalFee, 3=Refund*

**Response:**
```json
{
  "success": true,
  "message": "Tạo thanh toán thành công",
  "data": {
    "id": 1,
    "paymentCode": "PAY20251101001",
    "userId": 4,
    "userName": "Nguyễn Văn A",
    "rentalId": 1,
    "rentalCode": "RN20251101001",
    "amount": 400000,
    "type": "RentalFee",
    "status": "Completed",
    "paymentMethod": "Cash",
    "paymentDate": "2025-11-01T18:35:00"
  },
  "errors": null
}
```

### 6.2. Xem lịch sử thanh toán (Renter)

**Endpoint:** `GET /api/payments/my-payments`

**Headers:**
```
Authorization: Bearer {renter_token}
```

## 🔄 Complete Flow Example

### Scenario: Khách hàng thuê xe trong 8 giờ

1. **Khách đăng nhập**
```
POST /api/auth/login
Body: { "email": "nguyenvana@gmail.com", "password": "User@123" }
→ Nhận token
```

2. **Khách tìm điểm thuê gần**
```
GET /api/stations/nearby?latitude=10.7769&longitude=106.7009&radiusKm=5
→ Chọn điểm thuê ID=1
```

3. **Khách xem xe có sẵn**
```
GET /api/vehicles/available?stationId=1
→ Chọn xe ID=1 (VinFast Klara - 50,000đ/giờ)
```

4. **Khách đặt xe**
```
POST /api/bookings/create
Body: {
  "vehicleId": 1,
  "scheduledPickupTime": "2025-11-02T10:00:00",
  "scheduledReturnTime": "2025-11-02T18:00:00"
}
→ Nhận booking ID=1
```

5. **Nhân viên đăng nhập**
```
POST /api/auth/login
Body: { "email": "staff1@evrentalsystem.com", "password": "Staff@123" }
→ Nhận staff token
```

6. **Nhân viên xác nhận đặt xe**
```
POST /api/bookings/1/confirm
```

7. **Nhân viên giao xe (10:00)**
```
POST /api/rentals/create
Body: {
  "bookingId": 1,
  "vehicleId": 1,
  "pickupBatteryLevel": 100,
  "pickupInspection": { ... }
}
→ Nhận rental ID=1
```

8. **Nhân viên nhận xe trả (18:00 - sau 8 giờ)**
```
POST /api/rentals/complete
Body: {
  "rentalId": 1,
  "returnBatteryLevel": 45,
  "totalDistance": 50.5,
  "additionalFees": 0,
  "returnInspection": { ... }
}
→ Tổng tiền: 8 giờ × 50,000đ = 400,000đ
```

9. **Nhân viên tạo thanh toán**
```
POST /api/payments/create
Body: {
  "rentalId": 1,
  "amount": 400000,
  "type": 1,
  "paymentMethod": "Cash"
}
```

---

**Lưu ý:** Tất cả datetime phải theo format ISO 8601: `YYYY-MM-DDTHH:mm:ss`

