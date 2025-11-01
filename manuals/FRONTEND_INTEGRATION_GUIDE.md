# Frontend Integration Guide - EV Rental System API

Hướng dẫn tích hợp Frontend với Backend API.

## 🌐 Base URL

```
Development: http://localhost:5085
Production: https://your-domain.com
```

## 🔑 Authentication

### 1. Login Flow

```javascript
// Login request
const loginResponse = await fetch('http://localhost:5085/api/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    email: 'nguyenvana@gmail.com',
    password: 'User@123'
  })
});

const loginData = await loginResponse.json();

if (loginData.success) {
  // Lưu token vào localStorage hoặc sessionStorage
  localStorage.setItem('token', loginData.data.token);
  localStorage.setItem('user', JSON.stringify(loginData.data.user));
}
```

### 2. Sử dụng Token cho các request tiếp theo

```javascript
const token = localStorage.getItem('token');

const response = await fetch('http://localhost:5085/api/vehicles/available?stationId=1', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

const data = await response.json();
```

### 3. Logout

```javascript
// Xóa token khỏi storage
localStorage.removeItem('token');
localStorage.removeItem('user');
// Redirect về trang login
```

## 📦 Response Format

Tất cả API đều trả về format chuẩn:

```typescript
interface ApiResponse<T> {
  success: boolean;
  message: string | null;
  data: T | null;
  errors: string[] | null;
}
```

**Success Response:**
```json
{
  "success": true,
  "message": "Success message",
  "data": { ... },
  "errors": null
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Error message",
  "data": null,
  "errors": ["Error detail 1", "Error detail 2"]
}
```

## 🎯 Common Use Cases

### Use Case 1: Hiển thị danh sách xe có sẵn

```javascript
async function getAvailableVehicles(stationId) {
  const token = localStorage.getItem('token');
  
  const response = await fetch(
    `http://localhost:5085/api/vehicles/available?stationId=${stationId}`,
    {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }
  );
  
  const result = await response.json();
  
  if (result.success) {
    return result.data; // Array of vehicles
  } else {
    throw new Error(result.message);
  }
}

// Usage
try {
  const vehicles = await getAvailableVehicles(1);
  // Display vehicles in UI
  vehicles.forEach(vehicle => {
    console.log(`${vehicle.model} - ${vehicle.pricePerHour}đ/giờ`);
  });
} catch (error) {
  console.error('Error:', error.message);
}
```

### Use Case 2: Đặt xe

```javascript
async function createBooking(vehicleId, pickupTime, returnTime) {
  const token = localStorage.getItem('token');
  
  const response = await fetch('http://localhost:5085/api/bookings/create', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      vehicleId: vehicleId,
      scheduledPickupTime: pickupTime, // ISO 8601 format
      scheduledReturnTime: returnTime,
      notes: 'Optional notes'
    })
  });
  
  const result = await response.json();
  
  if (result.success) {
    return result.data; // Booking object
  } else {
    throw new Error(result.message);
  }
}

// Usage
try {
  const booking = await createBooking(
    1,
    '2025-11-02T10:00:00',
    '2025-11-02T18:00:00'
  );
  console.log('Booking created:', booking.bookingCode);
} catch (error) {
  alert('Đặt xe thất bại: ' + error.message);
}
```

### Use Case 3: Tìm điểm thuê gần

```javascript
async function findNearbyStations(latitude, longitude, radiusKm = 5) {
  const response = await fetch(
    `http://localhost:5085/api/stations/nearby?latitude=${latitude}&longitude=${longitude}&radiusKm=${radiusKm}`
  );
  
  const result = await response.json();
  
  if (result.success) {
    return result.data;
  } else {
    throw new Error(result.message);
  }
}

// Usage with Geolocation API
if (navigator.geolocation) {
  navigator.geolocation.getCurrentPosition(async (position) => {
    const stations = await findNearbyStations(
      position.coords.latitude,
      position.coords.longitude,
      10 // 10km radius
    );
    // Display stations on map
  });
}
```

## 🔄 Complete Rental Flow (For Staff)

```javascript
// Step 1: Staff login
const staffLogin = await fetch('http://localhost:5085/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'staff1@evrentalsystem.com',
    password: 'Staff@123'
  })
});

const staffData = await staffLogin.json();
const staffToken = staffData.data.token;

// Step 2: Get pending bookings at station
const bookingsResponse = await fetch(
  'http://localhost:5085/api/bookings/station/1',
  {
    headers: { 'Authorization': `Bearer ${staffToken}` }
  }
);

const bookingsData = await bookingsResponse.json();
const pendingBookings = bookingsData.data.filter(b => b.status === 'Pending');

// Step 3: Confirm booking
const confirmResponse = await fetch(
  `http://localhost:5085/api/bookings/${pendingBookings[0].id}/confirm`,
  {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${staffToken}` }
  }
);

// Step 4: Create rental (pickup vehicle)
const rentalResponse = await fetch(
  'http://localhost:5085/api/rentals/create',
  {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${staffToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      bookingId: pendingBookings[0].id,
      vehicleId: pendingBookings[0].vehicleId,
      pickupBatteryLevel: 100,
      pickupInspection: {
        imageUrls: ['url1', 'url2'],
        notes: 'Xe trong tình trạng tốt',
        damageReport: null
      }
    })
  }
);

const rentalData = await rentalResponse.json();
console.log('Rental created:', rentalData.data.rentalCode);

// ... Later when customer returns ...

// Step 5: Complete rental (return vehicle)
const completeResponse = await fetch(
  'http://localhost:5085/api/rentals/complete',
  {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${staffToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      rentalId: rentalData.data.id,
      returnBatteryLevel: 45,
      totalDistance: 50.5,
      additionalFees: 0,
      returnInspection: {
        imageUrls: ['url3', 'url4'],
        notes: 'Xe trả về bình thường',
        damageReport: null
      }
    })
  }
);

const completedRental = await completeResponse.json();
console.log('Total amount:', completedRental.data.totalAmount);

// Step 6: Create payment
const paymentResponse = await fetch(
  'http://localhost:5085/api/payments/create',
  {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${staffToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      rentalId: completedRental.data.id,
      amount: completedRental.data.totalAmount,
      type: 1, // RentalFee
      paymentMethod: 'Cash'
    })
  }
);
```

## 📱 React Example (Hooks)

```jsx
import { useState, useEffect } from 'react';

// Custom hook for API calls
function useApi() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  
  const callApi = async (url, options = {}) => {
    setLoading(true);
    setError(null);
    
    try {
      const token = localStorage.getItem('token');
      const headers = {
        'Content-Type': 'application/json',
        ...options.headers
      };
      
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }
      
      const response = await fetch(url, {
        ...options,
        headers
      });
      
      const result = await response.json();
      
      if (!result.success) {
        throw new Error(result.message || 'API call failed');
      }
      
      return result.data;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setLoading(false);
    }
  };
  
  return { callApi, loading, error };
}

// Component example
function VehicleList({ stationId }) {
  const [vehicles, setVehicles] = useState([]);
  const { callApi, loading, error } = useApi();
  
  useEffect(() => {
    async function fetchVehicles() {
      try {
        const data = await callApi(
          `http://localhost:5085/api/vehicles/available?stationId=${stationId}`
        );
        setVehicles(data);
      } catch (err) {
        console.error('Failed to fetch vehicles:', err);
      }
    }
    
    fetchVehicles();
  }, [stationId]);
  
  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;
  
  return (
    <div>
      {vehicles.map(vehicle => (
        <div key={vehicle.id}>
          <h3>{vehicle.model}</h3>
          <p>Giá: {vehicle.pricePerHour.toLocaleString()}đ/giờ</p>
          <p>Pin: {vehicle.batteryCapacity}%</p>
        </div>
      ))}
    </div>
  );
}
```

## 🎨 Vue.js Example

```vue
<template>
  <div>
    <div v-if="loading">Loading...</div>
    <div v-else-if="error">Error: {{ error }}</div>
    <div v-else>
      <div v-for="vehicle in vehicles" :key="vehicle.id" class="vehicle-card">
        <h3>{{ vehicle.model }}</h3>
        <p>Giá: {{ formatPrice(vehicle.pricePerHour) }}đ/giờ</p>
        <p>Pin: {{ vehicle.batteryCapacity }}%</p>
        <button @click="bookVehicle(vehicle.id)">Đặt xe</button>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      vehicles: [],
      loading: false,
      error: null
    };
  },
  
  async mounted() {
    await this.fetchVehicles();
  },
  
  methods: {
    async fetchVehicles() {
      this.loading = true;
      this.error = null;
      
      try {
        const token = localStorage.getItem('token');
        const response = await fetch(
          'http://localhost:5085/api/vehicles/available?stationId=1',
          {
            headers: {
              'Authorization': `Bearer ${token}`
            }
          }
        );
        
        const result = await response.json();
        
        if (result.success) {
          this.vehicles = result.data;
        } else {
          this.error = result.message;
        }
      } catch (err) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },
    
    formatPrice(price) {
      return price.toLocaleString('vi-VN');
    },
    
    async bookVehicle(vehicleId) {
      // Implement booking logic
    }
  }
};
</script>
```

## 🛠️ Error Handling

```javascript
async function apiCall(url, options = {}) {
  try {
    const response = await fetch(url, options);
    const result = await response.json();
    
    if (!result.success) {
      // Handle API error
      if (result.errors && result.errors.length > 0) {
        // Show validation errors
        result.errors.forEach(error => console.error(error));
      }
      throw new Error(result.message);
    }
    
    return result.data;
  } catch (error) {
    // Handle network error
    if (error.name === 'TypeError') {
      console.error('Network error:', error);
      throw new Error('Không thể kết nối đến server');
    }
    throw error;
  }
}
```

## 📅 Date Handling

API sử dụng ISO 8601 format cho datetime.

```javascript
// Convert Date to ISO string
const pickupTime = new Date('2025-11-02 10:00:00');
const isoString = pickupTime.toISOString(); // "2025-11-02T10:00:00.000Z"

// Parse ISO string to Date
const dateFromApi = new Date('2025-11-02T10:00:00');

// Format for display (Vietnamese)
const displayDate = dateFromApi.toLocaleString('vi-VN', {
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit'
});
```

## 🔐 Role-based UI

```javascript
// Get user role from localStorage
const user = JSON.parse(localStorage.getItem('user'));
const userRole = user?.role; // "Renter", "StationStaff", or "Admin"

// Show/hide UI based on role
if (userRole === 'StationStaff' || userRole === 'Admin') {
  // Show staff-only features
  showVehicleManagement();
  showRentalManagement();
} else if (userRole === 'Renter') {
  // Show renter features
  showBookingForm();
  showMyBookings();
}
```

## 📊 Enums Reference

```javascript
const UserRole = {
  Renter: 1,
  StationStaff: 2,
  Admin: 3
};

const VehicleStatus = {
  Available: 0,
  Booked: 1,
  InUse: 2,
  Maintenance: 3,
  Damaged: 4
};

const BookingStatus = {
  Pending: 0,
  Confirmed: 1,
  Cancelled: 2,
  Completed: 3
};

const RentalStatus = {
  Active: 0,
  Completed: 1,
  Cancelled: 2
};

const PaymentType = {
  Deposit: 0,
  RentalFee: 1,
  AdditionalFee: 2,
  Refund: 3
};

const PaymentStatus = {
  Pending: 0,
  Completed: 1,
  Failed: 2,
  Refunded: 3
};
```

## 🚀 Quick Start Checklist

- [ ] Cài đặt và chạy Backend API
- [ ] Test API qua Swagger UI
- [ ] Implement login/logout
- [ ] Lưu token vào localStorage
- [ ] Thêm Authorization header cho các request
- [ ] Handle API response format
- [ ] Implement error handling
- [ ] Test với các role khác nhau
- [ ] Implement date formatting
- [ ] Test complete user flows

## 📞 Support

Nếu gặp vấn đề:
1. Kiểm tra Swagger UI: `http://localhost:5085`
2. Xem API_EXAMPLES.md để có ví dụ chi tiết
3. Kiểm tra console log để xem response từ API
4. Đảm bảo token được gửi đúng format: `Bearer {token}`

---

**Happy Coding! 🎉**

