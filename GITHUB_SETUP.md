# 🚀 Hướng dẫn đưa dự án lên GitHub

## 📋 Chuẩn bị

### ✅ Checklist trước khi push
- [x] File `.gitignore` đã có
- [x] File `README.md` đã có
- [x] File `LICENSE` đã có
- [x] File `.env.example` đã có (không push `.env` thật)
- [x] Database file (*.db) sẽ không được push (đã có trong .gitignore)
- [x] Build artifacts sẽ không được push (bin/, obj/)

---

## 🌐 Bước 1: Tạo Repository trên GitHub

### Cách 1: Tạo trên GitHub Web
1. Đăng nhập vào https://github.com
2. Click nút **"+"** ở góc trên bên phải → **"New repository"**
3. Điền thông tin:
   - **Repository name**: `ev-rental-system` (hoặc tên bạn muốn)
   - **Description**: `EV Station-based Rental System - Backend API`
   - **Public** hoặc **Private** (tùy chọn)
   - ❌ **KHÔNG** tick "Add a README file" (vì đã có sẵn)
   - ❌ **KHÔNG** tick "Add .gitignore" (vì đã có sẵn)
   - ✅ **Có thể** chọn "Choose a license" → MIT License
4. Click **"Create repository"**

### Cách 2: Tạo bằng GitHub CLI (nếu đã cài gh)
```bash
gh repo create ev-rental-system --public --source=. --remote=origin
```

---

## 💻 Bước 2: Khởi tạo Git Local (nếu chưa có)

### Kiểm tra xem đã có Git chưa
```bash
git status
```

### Nếu chưa có Git, khởi tạo:
```bash
git init
```

---

## 📦 Bước 3: Add và Commit files

### 1. Kiểm tra files sẽ được commit
```bash
git status
```

### 2. Add tất cả files
```bash
git add .
```

### 3. Kiểm tra lại files đã add
```bash
git status
```

Đảm bảo các file sau **KHÔNG** xuất hiện:
- ❌ `*.db` (database files)
- ❌ `bin/`, `obj/` (build artifacts)
- ❌ `.env` (environment variables - chỉ push `.env.example`)
- ❌ `.vs/`, `.vscode/` (IDE settings)

### 4. Commit
```bash
git commit -m "Initial commit: EV Rental System Backend API"
```

---

## 🔗 Bước 4: Kết nối với GitHub Remote

### Thêm remote origin
Thay `your-username` bằng username GitHub của bạn:

```bash
git remote add origin https://github.com/your-username/ev-rental-system.git
```

### Kiểm tra remote
```bash
git remote -v
```

Kết quả:
```
origin  https://github.com/your-username/ev-rental-system.git (fetch)
origin  https://github.com/your-username/ev-rental-system.git (push)
```

---

## 🚀 Bước 5: Push lên GitHub

### Đổi tên branch thành main (nếu đang là master)
```bash
git branch -M main
```

### Push lần đầu
```bash
git push -u origin main
```

### Nhập credentials
- **Username**: GitHub username của bạn
- **Password**: **KHÔNG PHẢI** password GitHub!
  - Phải dùng **Personal Access Token** (PAT)

---

## 🔑 Tạo Personal Access Token (PAT)

Nếu GitHub yêu cầu password và bị lỗi, làm theo:

### 1. Vào GitHub Settings
1. Click avatar → **Settings**
2. Scroll xuống → **Developer settings** (cuối cùng bên trái)
3. Click **Personal access tokens** → **Tokens (classic)**
4. Click **Generate new token** → **Generate new token (classic)**

### 2. Cấu hình Token
- **Note**: `EV Rental System`
- **Expiration**: `90 days` (hoặc tùy chọn)
- **Select scopes**: Tick ✅ **repo** (toàn bộ)
- Click **Generate token**

### 3. Copy Token
⚠️ **QUAN TRỌNG**: Copy token ngay! Bạn sẽ không thấy lại nó!

### 4. Sử dụng Token
Khi Git yêu cầu password, paste **token** vào (không phải password GitHub).

---

## 🔄 Các lệnh Git thường dùng

### Sau khi sửa code
```bash
# Xem files đã thay đổi
git status

# Add files
git add .

# Commit
git commit -m "feat: thêm tính năng ABC"

# Push lên GitHub
git push
```

### Tạo branch mới
```bash
# Tạo và chuyển sang branch mới
git checkout -b feature/new-feature

# Push branch mới lên GitHub
git push -u origin feature/new-feature
```

### Pull code mới nhất
```bash
git pull origin main
```

---

## 📝 Cập nhật README.md

Sau khi push lên GitHub, cập nhật các link trong `README.md`:

1. Mở file `README.md`
2. Thay `your-username` bằng username GitHub thật:
   ```markdown
   git clone https://github.com/your-username/ev-rental-system.git
   ```

3. Commit và push:
   ```bash
   git add README.md
   git commit -m "docs: update GitHub links"
   git push
   ```

---

## 🎨 Tùy chỉnh Repository trên GitHub

### 1. Thêm Topics
1. Vào repository trên GitHub
2. Click ⚙️ bên cạnh "About"
3. Thêm topics: `dotnet`, `csharp`, `api`, `ev-rental`, `clean-architecture`, `swagger`

### 2. Thêm Description
```
EV Station-based Rental System - Backend API built with .NET Core 8
```

### 3. Thêm Website (nếu có)
```
https://your-username.github.io/ev-rental-system
```

---

## ✅ Kiểm tra

Sau khi push xong:

1. ✅ Vào https://github.com/your-username/ev-rental-system
2. ✅ Kiểm tra README.md hiển thị đẹp
3. ✅ Kiểm tra files đã được push đầy đủ
4. ✅ Kiểm tra `.env` **KHÔNG** có trong repo (chỉ có `.env.example`)
5. ✅ Kiểm tra `*.db` **KHÔNG** có trong repo

---

## 🔒 Bảo mật

### ⚠️ KHÔNG BAO GIỜ push các file sau:
- ❌ `.env` (chứa thông tin nhạy cảm)
- ❌ `*.db` (database với dữ liệu thật)
- ❌ `appsettings.Production.json` (nếu có thông tin nhạy cảm)
- ❌ API keys, passwords, secrets

### ✅ Nên push:
- ✅ `.env.example` (template không có giá trị thật)
- ✅ Source code
- ✅ Documentation
- ✅ Configuration templates

---

## 🆘 Troubleshooting

### Lỗi: "remote origin already exists"
```bash
git remote remove origin
git remote add origin https://github.com/your-username/ev-rental-system.git
```

### Lỗi: "failed to push some refs"
```bash
git pull origin main --rebase
git push -u origin main
```

### Lỗi: Authentication failed
- Đảm bảo dùng **Personal Access Token**, không phải password
- Tạo token mới nếu cần

### Muốn xóa file đã commit nhầm
```bash
# Xóa file khỏi Git nhưng giữ lại local
git rm --cached filename

# Commit
git commit -m "Remove sensitive file"

# Push
git push
```

---

## 🎉 Hoàn thành!

Repository của bạn đã sẵn sàng trên GitHub! 🚀

**Next steps:**
- ⭐ Thêm star cho repo của bạn
- 📝 Viết Wiki nếu cần
- 🐛 Tạo Issues cho bugs/features
- 🤝 Mời collaborators
- 📊 Setup GitHub Actions (CI/CD) nếu muốn

---

## 📞 Cần giúp đỡ?

Nếu gặp vấn đề, tham khảo:
- [GitHub Docs](https://docs.github.com)
- [Git Documentation](https://git-scm.com/doc)

