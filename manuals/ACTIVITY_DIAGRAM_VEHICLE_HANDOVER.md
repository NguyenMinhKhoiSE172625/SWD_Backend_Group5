# 🔄 Activity Diagram - Quy trình Giao – Nhận xe TỔNG THỂ

## 📋 Thông tin Diagram

**Loại:** Activity Diagram (Swimlane)  
**Mục đích:** Mô tả quy trình TỔNG THỂ từ Đặt xe → Nhận xe → Trả xe → Thanh toán  
**Actors:** EV Renter, Station Staff, System  
**Vị trí:** Diagram #6 (trong 13 diagrams)

---

## 🎯 Mục đích

Diagram này mô tả **TOÀN BỘ** quy trình "Quản lý giao – nhận xe" từ đầu đến cuối, bao gồm:
1. **Đặt xe** (Booking)
2. **Nhận xe** (Pickup/Check-in)
3. **Sử dụng xe** (In-use)
4. **Trả xe** (Return/Check-out)
5. **Thanh toán** (Payment)

---

## 👥 Swimlanes (3 Actors)

### 🧑 **EV RENTER (Người thuê xe)**
- Tìm kiếm và đặt xe
- Đến trạm nhận xe
- Sử dụng xe
- Trả xe và thanh toán

### 👨‍💼 **STATION STAFF (Nhân viên trạm)**
- Xác nhận/Từ chối booking
- Kiểm tra giấy tờ khách hàng
- Kiểm tra xe trước/sau thuê
- Ghi nhận tình trạng xe
- Tính phí bồi thường (nếu có)

### ⚙️ **SYSTEM (Hệ thống)**
- Hiển thị xe khả dụng
- Tạo và cập nhật booking
- Tạo và cập nhật rental
- Tính toán chi phí
- Xử lý thanh toán
- Tạo hóa đơn

---

## 🔄 Quy trình Chi tiết

### **PHASE 1: ĐẶT XE (BOOKING)**

#### **Bước 1-5: Tìm kiếm và Đặt xe**

| Bước | Actor | Hành động | Kết quả |
|------|-------|-----------|---------|
| 1 | Renter | Tìm kiếm xe khả dụng | - |
| 2 | System | Hiển thị danh sách xe khả dụng | Danh sách xe |
| 3 | Renter | Chọn xe và thời gian thuê | - |
| 4 | Renter | Xác nhận đặt xe | - |
| 5 | System | Tạo booking mới (trạng thái: **Pending**) | Booking ID |

#### **Bước 6-10: Staff Xác nhận**

| Bước | Actor | Hành động | Kết quả |
|------|-------|-----------|---------|
| 6 | Staff | Nhận yêu cầu đặt xe | - |
| 7 | Staff | Kiểm tra xe khả dụng | - |
| 8 | Staff | **Decision:** Xe sẵn sàng? | Có/Không |
| 9a | Staff | **[Có]** Xác nhận booking | - |
| 9b | Staff | **[Không]** Từ chối booking | - |

#### **Bước 11-14: System Cập nhật**

**Nếu XÁC NHẬN:**
| Bước | Actor | Hành động | Kết quả |
|------|-------|-----------|---------|
| 10 | System | Cập nhật booking: **Confirmed** | - |
| 11 | System | Cập nhật xe: **Reserved** | - |
| 12 | System | Gửi thông báo xác nhận | Email/SMS |
| 13 | Renter | Nhận thông báo đặt xe thành công | ✅ Success |

**Nếu TỪ CHỐI:**
| Bước | Actor | Hành động | Kết quả |
|------|-------|-----------|---------|
| 10 | System | Cập nhật booking: **Cancelled** | - |
| 11 | Renter | **Decision:** Đặt xe thành công? | ❌ Không |
| 12 | Renter | Hủy đặt xe | ❌ End |

---

### **PHASE 2: NHẬN XE (PICKUP/CHECK-IN)**

#### **Bước 15-20: Khách đến trạm**

| Bước | Actor | Hành động | Kết quả |
|------|-------|-----------|---------|
| 14 | Renter | Đến trạm nhận xe | - |
| 15 | Staff | Đón khách hàng | - |
| 16 | Renter | Xác nhận thông tin cá nhân | CMND/CCCD |
| 17 | System | Xác thực thông tin khách hàng | - |
| 18 | Renter | **Decision:** Thông tin hợp lệ? | Có/Không |

**Nếu KHÔNG hợp lệ:**
| Bước | Actor | Hành động | Kết quả |
|------|-------|-----------|---------|
| 19 | Renter | Hủy đặt xe | ❌ End |

**Nếu HỢP LỆ:**
| Bước | Actor | Hành động | Kết quả |
|------|-------|-----------|---------|
| 19 | Staff | Kiểm tra giấy tờ khách hàng | ✅ OK |

#### **Bước 21-27: Kiểm tra và Giao xe**

| Bước | Actor | Hành động | Kết quả |
|------|-------|-----------|---------|
| 20 | Staff | Kiểm tra xe trước giao | Checklist |
| 21 | Staff | Ghi nhận tình trạng xe ban đầu | VehicleInspection (Pickup) |
| 22 | System | Tạo rental mới (trạng thái: **Active**) | Rental ID |
| 23 | System | Cập nhật xe: **InUse** | - |
| 24 | System | Ghi nhận thời gian nhận xe | PickupTime |
| 25 | Renter | Ký xác nhận nhận xe | Signature |
| 26 | Staff | Giao chìa khóa cho khách | 🔑 Key |

---

### **PHASE 3: SỬ DỤNG XE (IN-USE)**

| Bước | Actor | Hành động | Kết quả |
|------|-------|-----------|---------|
| 27 | Renter | Sử dụng xe | 🚗 Driving |

---

### **PHASE 4: TRẢ XE (RETURN/CHECK-OUT)**

#### **Bước 28-33: Khách trả xe**

| Bước | Actor | Hành động | Kết quả |
|------|-------|-----------|---------|
| 28 | Renter | Đưa xe về trạm | - |
| 29 | Staff | Nhận xe từ khách | 🔑 Key |
| 30 | Staff | Kiểm tra tình trạng xe sau thuê | Checklist |
| 31 | System | Ghi nhận thời gian trả xe | ReturnTime |
| 32 | Renter | Xác nhận trả xe | - |
| 33 | Staff | Ghi nhận hư hỏng (nếu có) | VehicleInspection (Return) |

#### **Bước 34-36: Kiểm tra hư hỏng**

| Bước | Actor | Hành động | Kết quả |
|------|-------|-----------|---------|
| 34 | Renter | **Decision:** Có hư hỏng? | Có/Không |
| 35a | Staff | **[Có]** Tính phí bồi thường | AdditionalFee |
| 35b | System | **[Không]** Tính toán tổng chi phí | TotalAmount |

---

### **PHASE 5: THANH TOÁN (PAYMENT)**

#### **Bước 37-42: Xử lý thanh toán**

| Bước | Actor | Hành động | Kết quả |
|------|-------|-----------|---------|
| 36 | System | Tính toán tổng chi phí | TotalAmount |
| 37 | System | Tạo payment (trạng thái: **Pending**) | Payment ID |
| 38 | Renter | Thanh toán phí thuê | Cash/Card/E-wallet |
| 39 | System | Xử lý thanh toán | Processing... |
| 40 | Renter | **Decision:** Thanh toán thành công? | Có/Không |

**Nếu THẤT BẠI:**
| Bước | Actor | Hành động | Kết quả |
|------|-------|-----------|---------|
| 41 | System | Cập nhật payment: **Failed** | - |
| 42 | Renter | Thử lại thanh toán | Retry |
| 43 | System | Xử lý thanh toán (lại) | Processing... |

**Nếu THÀNH CÔNG:**
| Bước | Actor | Hành động | Kết quả |
|------|-------|-----------|---------|
| 41 | System | Cập nhật payment: **Completed** | ✅ |
| 42 | System | Cập nhật rental: **Completed** | ✅ |
| 43 | System | Cập nhật xe: **Available** | ✅ |
| 44 | System | Tạo hóa đơn | Invoice |
| 45 | Renter | Nhận hóa đơn & Hoàn tất | ✅ Success |
| 46 | Staff | Xác nhận hoàn tất thuê xe | ✅ End |

---

## 🎨 Màu sắc trong Diagram

| Màu | Ý nghĩa | Ví dụ |
|-----|---------|-------|
| 🔵 **Xanh nhạt** | Bắt đầu | Tìm kiếm xe |
| 🟢 **Xanh lá** | Thành công | Hoàn tất, Xác nhận |
| 🔴 **Đỏ nhạt** | Thất bại/Hủy | Hủy đặt xe, Từ chối |
| 🟡 **Vàng** | Cập nhật trạng thái | System updates |

---

## 🔑 Các Trạng thái Quan trọng

### **Booking Status:**
1. **Pending** - Chờ xác nhận
2. **Confirmed** - Đã xác nhận
3. **Cancelled** - Đã hủy

### **Vehicle Status:**
1. **Available** - Sẵn sàng
2. **Reserved** - Đã đặt
3. **InUse** - Đang sử dụng

### **Rental Status:**
1. **Active** - Đang thuê
2. **Completed** - Hoàn tất

### **Payment Status:**
1. **Pending** - Chờ thanh toán
2. **Completed** - Đã thanh toán
3. **Failed** - Thất bại

---

## ✅ Điểm Khác biệt với Sequence Diagrams

| Aspect | Sequence Diagrams (#3, #4, #5) | Activity Diagram (#6) |
|--------|--------------------------------|----------------------|
| **Focus** | Chi tiết từng bước (API calls) | Quy trình tổng thể |
| **Actors** | API, Services, Database | Renter, Staff, System |
| **Level** | Technical (code level) | Business (process level) |
| **Database** | ✅ Show DB operations | ❌ Hide DB operations |
| **Scope** | 1 use case (Đặt/Nhận/Trả) | Toàn bộ flow (Đặt→Nhận→Trả) |

---

## 📊 Liên kết với Diagrams khác

| Diagram | Mối liên hệ |
|---------|-------------|
| **#1: Use Case Diagram** | Tổng quan → Activity Diagram chi tiết hóa use case "Quản lý giao – nhận xe" |
| **#3: Sequence - Tìm kiếm & Đặt xe** | Chi tiết kỹ thuật của PHASE 1 (Đặt xe) |
| **#4: Sequence - Nhận xe** | Chi tiết kỹ thuật của PHASE 2 (Nhận xe) |
| **#5: Sequence - Trả xe & Thanh toán** | Chi tiết kỹ thuật của PHASE 4-5 (Trả xe + Thanh toán) |
| **#8: State Diagram - Booking** | Trạng thái Booking (Pending → Confirmed → Cancelled) |
| **#12: Class Diagram** | Entities: Booking, Rental, Payment, VehicleInspection |

---

## 🎯 Sử dụng trong Deliverables

### **1. Code: Flow "Quản lý giao – nhận xe"**
- ✅ Diagram này là **CORE** của deliverable này
- ✅ Mô tả **TOÀN BỘ** quy trình từ đầu đến cuối
- ✅ Kết hợp với Sequence Diagrams (#3, #4, #5) để giải thích chi tiết

### **2. System Design Document**
- ✅ Section "Business Logic Flows"
- ✅ Giải thích quy trình nghiệp vụ
- ✅ Decision points và error handling

### **3. Installation Manual**
- ❌ Không liên quan

### **4. Performance Testing**
- ⚠️ Có thể dùng để identify **critical paths** cần test performance

---

## 💡 Lưu ý

### **✅ ĐÃ XÓA:**
- ❌ Không show database operations (INSERT, UPDATE, SELECT)
- ❌ Không show table names (Users, Bookings, Rentals, Payments)
- ❌ Không show SQL queries

### **✅ ĐÃ TÁCH NHỎ:**
- ✅ 5 Phases rõ ràng (Đặt → Nhận → Sử dụng → Trả → Thanh toán)
- ✅ 3 Swimlanes (Renter, Staff, System)
- ✅ Decision points rõ ràng (◇)
- ✅ Error handling (Hủy, Thử lại)

### **✅ FOCUS VÀO:**
- ✅ Business process (quy trình nghiệp vụ)
- ✅ Actor interactions (tương tác giữa các actors)
- ✅ Status transitions (chuyển đổi trạng thái)
- ✅ Decision points (điểm quyết định)

---

## 📍 Vị trí trong 13 Diagrams

**Thứ tự đề xuất:**

1. Use Case Diagram - Tổng quan Hệ thống
2. Sequence Diagram - Đăng ký & Xác thực Tài khoản
3. Sequence Diagram - Tìm kiếm & Đặt xe
4. Sequence Diagram - Nhận xe (Check-in)
5. Sequence Diagram - Trả xe & Thanh toán
6. **Activity Diagram - Quy trình Giao – Nhận xe TỔNG THỂ** ← **DIAGRAM NÀY**
7. Activity Diagram - Quy trình Bảo trì Xe
8. State Diagram - Trạng thái Booking
9. Component Diagram - Kiến trúc Microservices
10. Deployment Diagram - Kiến trúc Triển khai
11. Entity Relationship Diagram (ERD)
12. Sequence Diagram - Quản lý Báo cáo & Analytics (Admin)
13. Class Diagram - Core Domain Models

**Lý do đặt ở vị trí #6:**
- ✅ Sau Sequence Diagrams (chi tiết) → Activity Diagram (tổng thể)
- ✅ Trước State Diagram (trạng thái) → Logic flow
- ✅ Nhóm với Activity Diagram khác (#7: Bảo trì Xe)

---

**Developed with ❤️ for SWD Project**

