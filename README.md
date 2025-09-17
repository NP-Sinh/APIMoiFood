# API – MoiFood

API backend cho ứng dụng đặt món **MoiFood**

## Tính năng chính
```bash
1. Gửi Email bằng Gmail
2. JWT Authentication
3. BCrypt.Net
4. AutoMapper(Tự động ánh xạ (mapping) giữa Entity và DTO)
5. Thanh toán MoMo
6. Thanh toán VNPAY
```
## Thông tin test VNPAY
> **Lưu ý:** Đây chỉ là tài khoản sandbox để kiểm thử, **không sử dụng cho giao dịch thật**.

| Trường         | Giá trị                          |
|----------------|----------------------------------|
| Ngân hàng      | NCB                              |
| Số thẻ         | 9704 1985 2619 1432 198          |
| Tên chủ thẻ    | NGUYEN VAN A                     |
| Ngày phát hành | 07/15                            |
| Mật khẩu OTP   | 123456                           |

## Danh sách API
### Auth
```bash
POST http://0.0.0.0:5046/moifood/auth/login
POST http://0.0.0.0:5046/moifood/auth/register
```
### Food
