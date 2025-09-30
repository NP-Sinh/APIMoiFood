# API – MoiFood

API backend cho ứng dụng đặt món **MoiFood**

## 📌Tính năng chính

1. Gửi Email bằng Gmail
2. JWT Authentication
3. BCrypt.Net
4. AutoMapper(Tự động ánh xạ (mapping) giữa Entity và DTO)
5. Thanh toán MoMo
6. Thanh toán VNPAY
7.[AspNetCoreRateLimit](https://github.com/stefanprodan/AspNetCoreRateLimit)

## 📌Thông tin test Payment
> **Lưu ý:** Đây chỉ là tài khoản sandbox để kiểm thử, **không sử dụng cho giao dịch thật**.

### **VNPAY**
| Trường         | Giá trị                          |
|----------------|----------------------------------|
| Ngân hàng      | NCB                              |
| Số thẻ         | 9704198526191432198              |
| Tên chủ thẻ    | NGUYEN VAN A                     |
| Ngày phát hành | 07/15                            |
| Mật khẩu OTP   | 123456                           |

### **MOMO**
| Trường         | Giá trị                          |
|----------------|----------------------------------|
| Ngân hàng      |                                  |
| Số thẻ         | 9704000000000018                 |
| Tên chủ thẻ    | NGUYEN VAN A                     |
| Ngày phát hành | 03/07                            |
| Mật khẩu OTP   | OTP                              |

## 📌Danh sách API
### Auth
```bash
POST http://0.0.0.0:5046/moifood/auth/login
POST http://0.0.0.0:5046/moifood/auth/register
```
### Food
