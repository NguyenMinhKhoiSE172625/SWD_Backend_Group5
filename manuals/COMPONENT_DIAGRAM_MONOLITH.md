# Component Diagram - Monolith Architecture

## Mô tả
Sơ đồ kiến trúc Monolith của hệ thống EV Rental System, thể hiện cấu trúc Clean Architecture với 4 layers.

---

## Sơ đồ Mermaid

```mermaid
graph TB
    subgraph "Client Layer"
        WEB[Web Browser]
        MOBILE[Mobile App]
        POSTMAN[API Testing Tools]
    end

    subgraph "API Layer (Presentation)"
        CONTROLLERS[Controllers]
        MIDDLEWARE[Middleware]
        FILTERS[Filters & Validators]
        SWAGGER[Swagger/OpenAPI]
    end

    subgraph "Application Layer (Business Logic)"
        DTOS[DTOs]
        INTERFACES[Service Interfaces]
        VALIDATORS[Validation Logic]
    end

    subgraph "Infrastructure Layer (Data Access)"
        SERVICES[Service Implementations]
        DBCONTEXT[EF Core DbContext]
        MIGRATIONS[Database Migrations]
        JWTSERVICE[JWT Service]
    end

    subgraph "Domain Layer (Core)"
        ENTITIES[Entities]
        ENUMS[Enums]
        BUSINESSRULES[Business Rules]
    end

    subgraph "External Systems"
        SQLSERVER[(SQL Server Database)]
        LOGGING[Serilog File Logging]
    end

    %% Client to API
    WEB --> CONTROLLERS
    MOBILE --> CONTROLLERS
    POSTMAN --> CONTROLLERS

    %% API Layer connections
    CONTROLLERS --> MIDDLEWARE
    MIDDLEWARE --> FILTERS
    CONTROLLERS --> SWAGGER
    CONTROLLERS --> INTERFACES

    %% Application Layer connections
    INTERFACES --> DTOS
    INTERFACES --> VALIDATORS

    %% Infrastructure Layer connections
    SERVICES --> INTERFACES
    SERVICES --> DBCONTEXT
    SERVICES --> JWTSERVICE
    DBCONTEXT --> MIGRATIONS

    %% Domain Layer connections
    SERVICES --> ENTITIES
    SERVICES --> ENUMS
    ENTITIES --> BUSINESSRULES
    DBCONTEXT --> ENTITIES

    %% External connections
    DBCONTEXT --> SQLSERVER
    MIDDLEWARE --> LOGGING

    style WEB fill:#e1f5ff
    style MOBILE fill:#e1f5ff
    style POSTMAN fill:#e1f5ff
    style CONTROLLERS fill:#fff3e0
    style MIDDLEWARE fill:#fff3e0
    style FILTERS fill:#fff3e0
    style SWAGGER fill:#fff3e0
    style DTOS fill:#f3e5f5
    style INTERFACES fill:#f3e5f5
    style VALIDATORS fill:#f3e5f5
    style SERVICES fill:#e8f5e9
    style DBCONTEXT fill:#e8f5e9
    style MIGRATIONS fill:#e8f5e9
    style JWTSERVICE fill:#e8f5e9
    style ENTITIES fill:#fce4ec
    style ENUMS fill:#fce4ec
    style BUSINESSRULES fill:#fce4ec
    style SQLSERVER fill:#ffebee
    style LOGGING fill:#ffebee
```

---

## Chi tiết các thành phần

### **1. Client Layer**
- **Web Browser**: Giao diện web (future)
- **Mobile App**: Ứng dụng di động (future)
- **API Testing Tools**: Postman, Swagger UI

### **2. API Layer (Presentation)**
- **Controllers**: 
  - AuthController
  - BookingsController
  - RentalsController
  - VehiclesController
  - StationsController
  - PaymentsController
  - MaintenanceController
  - AdminController
  - HealthController
- **Middleware**: Exception handling, logging
- **Filters**: ValidateModelAttribute
- **Swagger**: API documentation

### **3. Application Layer (Business Logic)**
- **DTOs**: Request/Response models
  - Auth DTOs
  - Booking DTOs
  - Rental DTOs
  - Vehicle DTOs
  - Payment DTOs
  - Maintenance DTOs
  - Admin DTOs
- **Interfaces**: Service contracts
  - IAuthService
  - IBookingService
  - IRentalService
  - IVehicleService
  - IStationService
  - IPaymentService
  - IMaintenanceService
  - IAdminService
  - IJwtService

### **4. Infrastructure Layer (Data Access)**
- **Service Implementations**: Business logic implementation
- **DbContext**: Entity Framework Core context
- **Migrations**: Database schema versioning
- **JWT Service**: Token generation and validation

### **5. Domain Layer (Core)**
- **Entities**: 
  - User
  - Station
  - Vehicle
  - Booking
  - Rental
  - VehicleInspection
  - Payment
  - MaintenanceSchedule
  - MaintenanceRecord
- **Enums**: 
  - UserRole
  - VehicleStatus
  - BookingStatus
  - RentalStatus
  - PaymentStatus
  - PaymentType
  - MaintenanceType
  - MaintenanceStatus
- **Business Rules**: Domain validation logic

### **6. External Systems**
- **SQL Server**: Primary database
- **Serilog**: File-based logging system

---

## Luồng dữ liệu

1. **Client → API Layer**: HTTP Request
2. **API Layer → Application Layer**: DTO validation
3. **Application Layer → Infrastructure Layer**: Service call
4. **Infrastructure Layer → Domain Layer**: Entity manipulation
5. **Domain Layer → Database**: Data persistence
6. **Database → Client**: Response chain

---

## Ưu điểm của Monolith Architecture

✅ **Đơn giản**: Dễ phát triển, dễ deploy  
✅ **Performance**: Không có network latency giữa các services  
✅ **Transaction**: ACID transactions dễ dàng  
✅ **Debugging**: Dễ debug và trace  
✅ **Testing**: Dễ viết integration tests  

---

## Nhược điểm

⚠️ **Scalability**: Khó scale từng phần riêng lẻ  
⚠️ **Deployment**: Phải deploy toàn bộ app khi có thay đổi nhỏ  
⚠️ **Technology Lock-in**: Khó thay đổi technology stack  
⚠️ **Team Coordination**: Nhiều team làm chung 1 codebase  

---

## Khi nào nên chuyển sang Microservices?

- Khi hệ thống có > 100,000 users
- Khi cần scale riêng từng module
- Khi có nhiều team (> 5 teams)
- Khi cần deploy độc lập từng service

---

**Kết luận**: Với quy mô hiện tại (dự án học tập/startup nhỏ), **Monolith Architecture là lựa chọn phù hợp**.

