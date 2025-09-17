# API – MoiFood

API backend cho ứng dụng đặt món **MoiFood**

## Tính năng chính
```bash
1. **Gửi Email bằng Gmail**  
   Hỗ trợ gửi email xác thực/tương tác qua Gmail trong ASP.NET Core.
2. **JWT Authentication**  
   Bảo mật API bằng JSON Web Token (JWT).
3. **BCrypt.Net**  
   Mã hóa mật khẩu bằng thuật toán BCrypt.
4. **AutoMapper**  
   Tự động ánh xạ (mapping) giữa Entity và DTO.
5. **Thanh toán MoMo**  
   Tích hợp cổng thanh toán MoMo.
6. **Thanh toán VNPAY**  
   Tích hợp cổng thanh toán VNPAY (thông tin test bên dưới).
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
  `POST http://0.0.0.0:5046/moifood/auth/login`
  `POST http://0.0.0.0:5046/moifood/auth/register`
```
### Food
