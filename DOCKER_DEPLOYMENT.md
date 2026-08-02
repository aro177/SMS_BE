# Triển khai backend bằng Docker Desktop và HTTPS

## Kiến trúc

```text
Internet -> Router TCP 80/443 -> Caddy -> Backend:8080 (Docker network nội bộ)
```

Caddy tự động lấy và gia hạn chứng chỉ HTTPS. Backend không publish cổng `8080` ra máy host; chỉ Caddy publish `80` và `443`.

Các giá trị nhạy cảm được mount read-only qua Docker secrets:

- `ConnectionStrings:DefaultConnection` -> `secrets/db_connection.txt`
- `SUPABASE_KEY` -> `secrets/supabase_key.txt`
- `Supabase:ApiSecretKey` -> `secrets/supabase_api_secret_key.txt`

Không đưa các giá trị này vào `.env`, Dockerfile, build arguments hoặc Git.

## 1. Chuẩn bị

1. Cài Git và Docker Desktop.
2. Chọn Linux containers và chờ Docker Engine ở trạng thái Running.
3. Có một domain hoặc subdomain, ví dụ `api.example.com`.
4. Tạo DNS `A` record trỏ domain tới public IPv4 của router.
5. Nếu ISP dùng CGNAT, yêu cầu public IPv4 hoặc dùng một giải pháp tunnel/VPN; router port forwarding thông thường sẽ không hoạt động sau CGNAT.

## 2. Lấy source code

Lần đầu:

```powershell
git clone <repository-url> sms-backend
Set-Location sms-backend
```

Các lần cập nhật sau:

```powershell
git pull
```

Các lệnh Docker phải chạy tại thư mục chứa `docker-compose.yml`.

## 3. Tạo `.env`

```powershell
Copy-Item .env.example .env
notepad .env
```

Ví dụ:

```env
API_DOMAIN=api.example.com
CORS_ALLOWED_ORIGINS=https://app.example.com
SUPABASE_URL=https://your-project.supabase.co
AUTHENTICATION_VALID_ISSUER=https://your-project.supabase.co/auth/v1
AUTHENTICATION_VALID_AUDIENCE=authenticated
```

Quy tắc:

- `API_DOMAIN` chỉ chứa hostname, không có `https://`, path, port hoặc dấu `/` cuối.
- `CORS_ALLOWED_ORIGINS` chứa origin đầy đủ của frontend, không có dấu `/` cuối.
- Nếu có nhiều frontend origin, phân cách bằng dấu phẩy.
- `AllowedHosts` của ASP.NET Core tự động nhận giá trị từ `API_DOMAIN`.

## 4. Tạo Docker secrets

```powershell
New-Item -ItemType Directory -Path secrets -Force
notepad secrets\db_connection.txt
notepad secrets\supabase_key.txt
notepad secrets\supabase_api_secret_key.txt
```

Mỗi file chỉ chứa đúng một giá trị, không có dấu nháy. Các file này đã được Git ignore và bị loại khỏi Docker build context.

Kiểm tra trạng thái ignore mà không in nội dung secret:

```powershell
git status --short --ignored secrets .env
```

## 5. Cập nhật database

Chạy lần lượt các script cần thiết trong `supabase-migrations/` bằng Supabase SQL Editor hoặc công cụ PostgreSQL được quản trị viên cho phép.

## 6. Cấu hình router và firewall

Đặt DHCP reservation/static LAN IP cho máy chạy Docker Desktop. Trên router, forward:

- TCP `80` -> LAN IP máy Docker, port `80`.
- TCP `443` -> LAN IP máy Docker, port `443`.

Không forward port `8080`.

Windows Firewall chỉ cần cho phép inbound TCP `80` và `443`. Backend `8080` không được mở ra host hoặc Internet.

## 7. Build và chạy

```powershell
docker compose -f docker-compose.yml config
docker compose -f docker-compose.yml up -d --build
```

Kiểm tra:

```powershell
docker compose -f docker-compose.yml ps
docker compose -f docker-compose.yml logs --tail 100 caddy
docker compose -f docker-compose.yml logs --tail 100 studentmanagementsystem
```

API được truy cập tại:

```text
https://api.example.com
```

Chứng chỉ chỉ được cấp khi domain trỏ đúng public IP và Caddy nhận được kết nối từ Internet trên port `80` hoặc `443`.

## 8. Cập nhật phiên bản

```powershell
git pull
docker compose -f docker-compose.yml up -d --build
```

Volume `caddy_data` lưu certificate và khóa TLS nên không xóa volume này khi cập nhật.

## 9. Dừng hoặc khởi động lại

```powershell
docker compose -f docker-compose.yml restart
docker compose -f docker-compose.yml down
```

Không dùng `docker compose down -v` trừ khi chủ động muốn xóa dữ liệu certificate của Caddy.

## Lưu ý bảo mật

- Không public trực tiếp port backend `8080`.
- Không commit `.env` hoặc file trong `secrets/`.
- Chỉ dùng Supabase service-role key ở backend.
- CORS không thay thế authentication/authorization.
- Trusted proxy được giới hạn ở IP cố định của Caddy (`172.30.0.2`) và chỉ xử lý một proxy hop.
- Cần hoàn thiện `[Authorize]` cho các endpoint nghiệp vụ trước khi public API.
- Rotate key/password ngay nếu secret từng bị commit hoặc chia sẻ ngoài ý muốn.
