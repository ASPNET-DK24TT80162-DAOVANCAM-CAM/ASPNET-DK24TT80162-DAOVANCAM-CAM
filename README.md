# HUỚNG DẪN CHẠY ĐỒ ÁN: WEBSITE BÁN GIÀY (ASP.NET MVC VÀ C#)

Đây là đồ án xây dựng trang web bán giày hoàn chỉnh bằng ngôn ngữ C# trên nền tảng ASP.NET MVC. 

ĐIỂM TIỆN LỢI CỦA CODE NÀY: Bạn KHÔNG CẦN biết dùng SQL Server để tạo database, không cần chạy file .sql thủ công. Khi bạn bấm chạy web lần đầu, hệ thống sẽ tự động tạo Database và nạp sẵn toàn bộ dữ liệu mẫu bao gồm danh sách giày, hãng sản xuất và tài khoản thử nghiệm vào máy của bạn.

## Các Chức Năng Chính Của Đồ Án

### 1. Giao diện Khách hàng
* Trang chủ: Hiển thị banner slider, danh sách giày mới nhất, giày bán chạy.
* Bộ lọc và Tìm kiếm: Phân loại giày theo hãng (Nike, Adidas, Jordan...), theo loại, tìm kiếm theo tên sản phẩm.
* Chi tiết sản phẩm: Xem thông tin chi tiết, chọn Size, chọn Màu sắc và số lượng.
* Giỏ hàng: Thêm, sửa, xóa sản phẩm trong giỏ, tự động tính tổng tiền.
* Đặt hàng và Quản lý: Form điền thông tin đặt hàng, lưu danh sách sản phẩm yêu thích (Wishlist).

### 2. Giao diện Quản trị (Admin)
* Quản lý sản phẩm: Thêm, sửa, xóa thông tin giày, giá bán, giá khuyến mãi.
* Quản lý kho: Quản lý chi tiết từng size, từng màu và số lượng tồn kho tương ứng của mỗi mẫu giày.
* Quản lý đơn hàng: Tiếp nhận đơn hàng từ khách, cập nhật trạng thái đơn hàng (Chờ duyệt, Đang giao, Đã giao).

---

## Hướng Dẫn Các Bước Chạy Web Chi Tiết

Để chạy được dự án này, máy tính của bạn cần cài đặt sẵn 2 phần mềm sau:
1. Visual Studio (Khuyến khích phiên bản 2019 hoặc 2022).
2. SQL Server (Phiên bản Express hoặc LocalDB).

### Bước 1: Mở dự án bằng Visual Studio
1. Tải thư mục mã nguồn về máy và giải nén.
2. Tìm file có tên là ShopManagement.slnx (hoặc ShopManagement.sln).
3. Click đúp chuột vào file đó để mở dự án bằng phần mềm Visual Studio.

### Bước 2: Cấu hình kết nối SQL Server (Bắt buộc)
Vì mỗi máy tính có một tên SQL Server riêng, bạn cần sửa lại dòng này thì website mới có thể kết nối dữ liệu và chạy được.

1. Trong cột bên phải (Solution Explorer), tìm và mở file Web.config (nằm ở thư mục ngoài cùng của dự án).
2. Tìm đến dòng số 11, bạn sẽ thấy đoạn mã kết nối như sau:
   ```xml
   <connectionStrings>
     <add name="ShopDbContext" connectionString="Data Source=DESKTOP-RAH6IHC\SQL2K21;Initial Catalog=ShopBanGiay;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True" providerName="System.Data.SqlClient" />
   </connectionStrings>