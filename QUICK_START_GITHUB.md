# 🚀 Quick Start - Đưa dự án lên GitHub

## ⚡ Cách nhanh nhất (Dùng script tự động)

### Bước 1: Tạo repository trên GitHub
1. Vào https://github.com/new
2. Điền:
   - **Repository name**: `ev-rental-system`
   - **Description**: `EV Station-based Rental System - Backend API`
   - **Public** hoặc **Private**
   - ❌ KHÔNG tick "Add a README file"
3. Click **"Create repository"**

### Bước 2: Chạy script
```powershell
.\push-to-github.ps1
```

Script sẽ tự động:
- ✅ Khởi tạo Git (nếu chưa có)
- ✅ Add remote origin
- ✅ Add và commit files
- ✅ Push lên GitHub
- ✅ Mở browser để xem repository

**Xong!** 🎉

---

## 📝 Cách thủ công (Nếu muốn kiểm soát từng bước)

### 1. Tạo repository trên GitHub (như trên)

### 2. Khởi tạo Git local
```bash
git init
```

### 3. Add files
```bash
git add .
```

### 4. Commit
```bash
git commit -m "Initial commit: EV Rental System Backend API"
```

### 5. Add remote
Thay `your-username` bằng username GitHub của bạn:
```bash
git remote add origin https://github.com/your-username/ev-rental-system.git
```

### 6. Push
```bash
git branch -M main
git push -u origin main
```

### 7. Nhập credentials
- **Username**: GitHub username
- **Password**: Personal Access Token (không phải password GitHub!)

**Tạo token tại**: https://github.com/settings/tokens

---

## 🔑 Tạo Personal Access Token

1. Vào https://github.com/settings/tokens
2. Click **"Generate new token"** → **"Generate new token (classic)"**
3. Điền:
   - **Note**: `EV Rental System`
   - **Expiration**: `90 days`
   - **Scopes**: Tick ✅ **repo**
4. Click **"Generate token"**
5. **Copy token ngay!** (Bạn sẽ không thấy lại)

---

## ✅ Kiểm tra sau khi push

1. Vào https://github.com/your-username/ev-rental-system
2. Kiểm tra:
   - ✅ README.md hiển thị đẹp
   - ✅ Files đã được push đầy đủ
   - ✅ `.env` KHÔNG có (chỉ có `.env.example`)
   - ✅ `*.db` KHÔNG có

---

## 🎨 Tùy chỉnh Repository

### Thêm Topics
Click ⚙️ bên cạnh "About" → Thêm topics:
```
dotnet, csharp, api, ev-rental, clean-architecture, swagger, jwt, entity-framework
```

### Thêm Description
```
EV Station-based Rental System - Backend API built with .NET Core 8, Clean Architecture, JWT Authentication
```

---

## 🔄 Cập nhật code sau này

```bash
# Xem thay đổi
git status

# Add files
git add .

# Commit
git commit -m "feat: thêm tính năng mới"

# Push
git push
```

---

## 📚 Tài liệu chi tiết

Xem file **GITHUB_SETUP.md** để biết:
- Hướng dẫn chi tiết từng bước
- Troubleshooting
- Best practices
- Security guidelines

---

## 🆘 Gặp vấn đề?

### Lỗi: Authentication failed
→ Dùng **Personal Access Token**, không phải password

### Lỗi: remote origin already exists
```bash
git remote remove origin
git remote add origin https://github.com/your-username/ev-rental-system.git
```

### Lỗi: failed to push
```bash
git pull origin main --rebase
git push
```

---

## 🎉 Hoàn thành!

Repository của bạn đã sẵn sàng trên GitHub!

**Next steps:**
- ⭐ Star repository của bạn
- 📝 Cập nhật README.md với link đúng
- 🤝 Mời collaborators (nếu làm team)
- 📊 Setup GitHub Actions (CI/CD) nếu muốn

---

**Chúc bạn thành công!** 🚀

