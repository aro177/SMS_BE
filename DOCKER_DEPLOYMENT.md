# Triển khai backend bằng Docker Desktop

## 1. Cách cấu hình được đóng gói

Image chạy bằng Linux container, .NET 9, cổng nội bộ `8080` và user không phải root.

Các cấu hình không nhạy cảm nằm trong `.env`:

- `SUPABASE_URL`
- `AUTHENTICATION_VALID_ISSUER`
- `AUTHENTICATION_VALID_AUDIENCE`
- `API_PORT`

Các giá trị nhạy cảm không được đưa vào environment hoặc image. Docker Compose mount chúng vào `/run/secrets` dưới dạng file read-only:

- `ConnectionStrings:DefaultConnection` -> `secrets/db_connection.txt`
- `SUPABASE_KEY` -> `secrets/supabase_key.txt`
- `Supabase:ApiSecretKey` -> `secrets/supabase_api_secret_key.txt`

Backend vẫn đọc User Secrets như bình thường khi chạy trực tiếp bằng `dotnet run`. Việc đọc `/run/secrets` chỉ được kích hoạt khi các file tương ứng tồn tại.

## 2. Chuẩn bị máy triển khai

1. Cài Git.
2. Cài Docker Desktop và chọn Linux containers.
3. Mở Docker Desktop, chờ Docker Engine ở trạng thái Running.
4. Bảo đảm máy có quyền truy cập PostgreSQL/Supabase qua mạng.

## 3. Lấy source code

Lần đầu:

```powershell
git clone <repository-url>
Set-Location "Student Management System"
```

Các lần cập nhật tiếp theo:

```powershell
git pull
```

Không dùng `git pull` khi máy triển khai có thay đổi source chưa commit. Chỉ nên lưu `.env` và file trong `secrets/` vì chúng đã được Git ignore.

## 4. Tạo cấu hình không nhạy cảm

```powershell
Copy-Item .env.example .env
notepad .env
```

Điền URL Supabase và JWT issuer thật. Không đặt database password hoặc Supabase service-role key trong `.env`.

## 5. Tạo Docker secrets

```powershell
New-Item -ItemType Directory -Path secrets -Force
notepad secrets\db_connection.txt
notepad secrets\supabase_key.txt
notepad secrets\supabase_api_secret_key.txt
```

Mỗi file chỉ chứa đúng một giá trị, không có dấu nháy:

- `db_connection.txt`: toàn bộ PostgreSQL connection string.
- `supabase_key.txt`: key dùng để khởi tạo Supabase SDK; ưu tiên key có quyền tối thiểu đủ dùng.
- `supabase_api_secret_key.txt`: server-side/service-role key cho Supabase Admin Auth.

Giới hạn quyền đọc các file cho tài khoản đang triển khai. Trên máy dùng chung, không cấp quyền đọc thư mục repository cho người dùng khác. Tuyệt đối không commit hai file này.

Có thể kiểm tra chúng đang được ignore mà không in nội dung:

```powershell
git status --short --ignored secrets .env
```

## 6. Cập nhật database

Chạy lần lượt các script cần thiết trong thư mục `supabase-migrations/` bằng Supabase SQL Editor hoặc công cụ PostgreSQL được quản trị viên phê duyệt. Phải hoàn thành bước này trước khi khởi động phiên bản backend dùng các cột mới.

## 7. Build và chạy

Có thể build và chạy trực tiếp bằng cấu hình production:

```powershell
docker compose -f docker-compose.yml build
docker compose -f docker-compose.yml up -d
```

`docker-compose.override.yml` trong repository được giữ ở trạng thái không thay đổi cấu hình production, vì vậy `docker compose up -d --build` cũng cho kết quả tương đương.

Kiểm tra trạng thái và log:

```powershell
docker compose -f docker-compose.yml ps
docker compose -f docker-compose.yml logs --tail 100 studentmanagementsystem
```

API mặc định có tại `http://localhost:8080`. Có thể đổi cổng host bằng `API_PORT` trong `.env`.

## 8. Cập nhật phiên bản sau này

```powershell
git pull
docker compose -f docker-compose.yml up -d --build
```

## 9. Dừng hoặc khởi động lại

```powershell
docker compose -f docker-compose.yml restart
docker compose -f docker-compose.yml down
```

`docker compose down` xóa container và network của ứng dụng nhưng không xóa dữ liệu PostgreSQL/Supabase bên ngoài.

## Lưu ý bảo mật

- Không `COPY` `secrets.json`, `.env` hoặc thư mục `secrets/` vào image.
- Không truyền secret bằng `ARG` trong Dockerfile vì nó có thể tồn tại trong build history/cache.
- Không ghi secret ra log hoặc chụp màn hình Docker Desktop.
- Chỉ dùng Supabase service-role key ở backend; không gửi nó xuống frontend.
- Giới hạn firewall/database để chỉ nhận kết nối từ các máy cần thiết.
- Rotate key/password ngay nếu một secret từng được commit hoặc chia sẻ ngoài ý muốn.
