# Triển khai frontend và backend trên cùng một máy

## Kiến trúc

```text
Internet
   |
   | HTTPS :443
   v
Caddy
   |-- /*       -> Next.js frontend:3000
   `-- /api/*   -> ASP.NET Core backend:8080
```

Chỉ frontend domain được công bố. Frontend và backend không publish cổng riêng ra máy host. Trình duyệt gọi API bằng URL cùng origin, ví dụ `https://sms.example.com/api/classes`; Caddy chuyển các request `/api/*` vào backend qua Docker network.

## 1. Bố trí hai repository

Khuyến nghị clone hai repository cạnh nhau:

```text
deployment/
|-- sms-backend/    # chứa docker-compose.yml và Caddyfile
`-- sms-frontend/   # chứa Dockerfile của Next.js
```

Ví dụ:

```powershell
New-Item -ItemType Directory deployment -Force
Set-Location deployment
git clone <backend-repository-url> sms-backend
git clone <frontend-repository-url> sms-frontend
Set-Location sms-backend
```

Mọi lệnh `docker compose` được chạy trong `sms-backend`.

## 2. Cấu hình `.env`

```powershell
Copy-Item .env.example .env
notepad .env
```

Ví dụ:

```env
FRONTEND_BUILD_CONTEXT=../sms-frontend
PUBLIC_DOMAIN=sms.example.ddns.net
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_ANON_KEY=your-supabase-anon-key
SUPABASE_AUTH_COOKIE_NAME=
AUTHENTICATION_VALID_ISSUER=https://your-project.supabase.co/auth/v1
AUTHENTICATION_VALID_AUDIENCE=authenticated
```

`PUBLIC_DOMAIN` chỉ chứa hostname, không có `https://`, path, port hoặc dấu `/` cuối. `SUPABASE_ANON_KEY` được nhúng vào frontend và theo thiết kế là public; tuyệt đối không dùng service-role key tại đây.

Nếu hai repository không nằm cạnh nhau, đặt `FRONTEND_BUILD_CONTEXT` thành đường dẫn tuyệt đối tới frontend, ví dụ trên Windows:

```env
FRONTEND_BUILD_CONTEXT=D:/Java in ur area/SMS_FE
```

## 3. Docker secrets của backend

```powershell
New-Item -ItemType Directory -Path secrets -Force
notepad secrets\db_connection.txt
notepad secrets\supabase_key.txt
notepad secrets\supabase_api_secret_key.txt
```

Mỗi file chỉ chứa đúng một giá trị. Các file này được Git ignore và không được đưa vào Docker image.

## 4. Supabase Auth

Trong Supabase Dashboard, thêm frontend URL vào Site URL và Redirect URLs, ví dụ:

```text
https://sms.example.ddns.net
https://sms.example.ddns.net/**
```

Không thêm URL nội bộ như `http://frontend:3000` hoặc `http://studentmanagementsystem:8080`.

## 5. DNS, modem và firewall

Trỏ hostname/domain tới public IPv4 của modem. Đặt IP LAN cố định cho máy chạy Docker Desktop.

Forward trên modem:

- TCP `80` -> máy Docker port `80`.
- TCP `443` -> máy Docker port `443`.

Chỉ mở inbound TCP `80/443` trong Windows Firewall. Không forward hoặc mở `3000`, `8080`.

## 6. Build và chạy

```powershell
docker compose -f docker-compose.yml config
docker compose -f docker-compose.yml up -d --build
```

Kiểm tra:

```powershell
docker compose -f docker-compose.yml ps
docker compose -f docker-compose.yml logs --tail 100 caddy
docker compose -f docker-compose.yml logs --tail 100 frontend
docker compose -f docker-compose.yml logs --tail 100 studentmanagementsystem
```

Kết quả đúng:

- Caddy publish `80/443`.
- Frontend chỉ hiển thị `3000/tcp`, không có host mapping.
- Backend chỉ hiển thị `8080/tcp`, không có host mapping.

Truy cập:

```text
https://sms.example.ddns.net
```

## 7. Cập nhật cả hai ứng dụng

```powershell
Set-Location ../sms-frontend
git pull
Set-Location ../sms-backend
git pull
docker compose -f docker-compose.yml up -d --build
```

Các biến `NEXT_PUBLIC_*` được nhúng trong lúc build Next.js. Khi thay đổi domain, Supabase URL hoặc anon key, phải build lại frontend image.

## 8. Luồng request

- Trình duyệt gọi `/api/...` tương đối, nên request có cùng origin với frontend.
- Next.js server-side gọi backend qua `http://studentmanagementsystem:8080` trong Docker network.
- Caddy là trusted proxy duy nhất tại `172.30.0.2`.
- Backend chỉ chấp nhận host khớp `PUBLIC_DOMAIN`.

Việc backend không có port/hostname riêng không làm API trở thành private đối với người dùng Internet: các endpoint `/api/*` vẫn có thể được gọi qua frontend domain. Authentication và authorization vẫn bắt buộc.

## Lưu ý bảo mật

- Không public `3000` hoặc `8080`.
- Không đưa Supabase service-role key vào frontend hoặc build arguments.
- Không commit `.env` và thư mục `secrets/`.
- Không xóa volume `caddy_data` khi cập nhật vì volume này lưu certificate và khóa TLS.
- CORS cùng origin không thay thế `[Authorize]`.
