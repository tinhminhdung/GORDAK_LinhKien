USE Antigravity_ECommerce;
GO

-- 1. CLEANUP ALL DATA
DELETE FROM OrderItems;
DELETE FROM Orders;
DELETE FROM Products;
DELETE FROM News;
DELETE FROM Advertisings;
DELETE FROM Videos;
DELETE FROM Galleries;
DELETE FROM FAQs;
DELETE FROM Documents;
DELETE FROM Customers;
DELETE FROM Categories;
DELETE FROM Settings;

-- Reset Identites
DBCC CHECKIDENT ('OrderItems', RESEED, 0);
DBCC CHECKIDENT ('Orders', RESEED, 0);
DBCC CHECKIDENT ('Products', RESEED, 0);
DBCC CHECKIDENT ('News', RESEED, 0);
DBCC CHECKIDENT ('Advertisings', RESEED, 0);
DBCC CHECKIDENT ('Videos', RESEED, 0);
DBCC CHECKIDENT ('Galleries', RESEED, 0);
DBCC CHECKIDENT ('FAQs', RESEED, 0);
DBCC CHECKIDENT ('Documents', RESEED, 0);
DBCC CHECKIDENT ('Customers', RESEED, 0);
DBCC CHECKIDENT ('Categories', RESEED, 0);
DBCC CHECKIDENT ('Settings', RESEED, 0);

-- 2. SEED SETTINGS
INSERT INTO Settings (SettingKey, SettingValue, GroupName, Description) VALUES
('SiteTitle', N'Antigravity High-Tech Store', 'General', N'Tiêu đề trang web'),
('SiteDescription', N'Chuyên Laptop & Phụ kiện gaming cao cấp', 'General', N'Mô tả trang web'),
('Hotline', N'0123 456 789', 'Contact', N'Số điện thoại nóng'),
('Email', N'contact@antigravity.vn', 'Contact', N'Email hỗ trợ'),
('Address', N'Số 123 Đường Công Nghệ, Quận Cầu Giấy, Hà Nội', 'Contact', N'Địa chỉ trụ sở'),
('Facebook', N'https://facebook.com/antigravity', 'Social', N'Trang fanpage'),
('YouTube', N'https://youtube.com/c/antigravity', 'Social', N'Kênh YouTube'),
('FooterInfo', N'Hệ thống bán lẻ thiết bị công nghệ hàng đầu Việt Nam. Cam kết hàng chính hãng 100%.', 'UI', N'Thông tin chân trang'),
('InvoiceHeader', N'HÓA ĐƠN BÁN LẺ', 'Invoice', N'Tiêu đề hóa đơn'),
('InvoiceFooter', N'Cảm ơn quý khách đã mua hàng tại Antigravity Store. Hẹn gặp lại!', 'Invoice', N'Chân trang hóa đơn'),
('Logo', '/uploads/sss/638750745645893841.png', 'General', N'Logo website');

-- 3. SEED CATEGORIES
-- Type 1: Product Categories
INSERT INTO Categories (Name, Slug, CategoryType, ParentId, SortOrder, Status, CreatedAt) VALUES
(N'Laptop & Macbook', 'laptop-macbook', 1, 0, 1, 1, GETDATE()), -- Id 1
(N'Phụ kiện gaming', 'phu-kien-gaming', 1, 0, 2, 1, GETDATE()), -- Id 2
(N'Linh kiện PC', 'linh-kien-pc', 1, 0, 3, 1, GETDATE()), -- Id 3
(N'Bàn phím cơ', 'ban-phim-co', 1, 2, 1, 1, GETDATE()), -- Id 4 (Con của 2)
(N'Chuột Gaming', 'chuột-gaming', 1, 2, 2, 1, GETDATE()); -- Id 5 (Con của 2)

-- Type 2: News Categories
INSERT INTO Categories (Name, Slug, CategoryType, ParentId, SortOrder, Status, CreatedAt) VALUES
(N'Tin tức Công nghệ', 'tin-tuc-cong-nghe', 2, 0, 1, 1, GETDATE()), -- Id 6
(N'Đánh giá Sản phẩm', 'danh-gia-san-pham', 2, 0, 2, 1, GETDATE()), -- Id 7
(N'Thủ thuật Máy tính', 'thu-thuat-may-tinh', 2, 0, 3, 1, GETDATE()); -- Id 8

-- Type 0: Menu Items
INSERT INTO Categories (Name, Slug, CategoryType, ParentId, SortOrder, Status, MenuPosition, CreatedAt) VALUES
(N'Trang chủ', '/', 0, 0, 1, 1, 'Top,Mobile', GETDATE()), -- Id 9
(N'Sản phẩm', '/san-pham', 0, 0, 2, 1, 'Top,Mobile', GETDATE()), -- Id 10
(N'Tin tức', '/tin-tuc', 0, 0, 3, 1, 'Top,Mobile,Footer', GETDATE()), -- Id 11
(N'Tuyển dụng', '/tuyen-dung', 0, 0, 4, 1, 'Footer', GETDATE()), -- Id 12
(N'Liên hệ', '/lien-he', 0, 0, 5, 1, 'Top,Mobile', GETDATE()); -- Id 13

-- 4. SEED PRODUCTS
INSERT INTO Products (Name, Slug, CategoryIds, Price, OldPrice, MainImage, ShortDescription, Status, IsHot, CreatedAt) VALUES
(N'Macbook Pro M3 2024 - RAM 16GB, SSD 512GB', 'macbook-pro-m3-2024', '1', 45000000, 48000000, '/uploads/SanPham/product1.jpg', N'Dòng Laptop mạnh mẽ nhất thế giới với chip M3 Pro mới nhất.', 1, 1, GETDATE()),
(N'Bàn phím Akko 3098B Multi-mode Dracula Castle', 'akko-3098b-dracula', '2,4', 2500000, 2800000, '/uploads/SanPham/product2.jpg', N'Bàn phím cơ hotswap với thiết kế cực đẹp lấy cảm hứng từ Dracula.', 1, 1, GETDATE()),
(N'Chuột Logitech G502 X Plus Wireless RGB', 'logitech-g502-x-plus', '2,5', 3200000, 3500000, '/uploads/SanPham/product3.jpg', N'Phiên bản nâng cấp của dòng chuột gaming huyền thoại G502.', 1, 0, GETDATE()),
(N'Laptop Gaming ASUS ROG Zephyrus G14', 'rog-zephyrus-g14', '1', 35900000, 39000000, '/uploads/SanPham/product4.jpg', N'Laptop gaming mỏng nhẹ, hiệu năng cực khủng với card rời RTX 4060.', 1, 1, GETDATE()),
(N'Tai nghe SteelSeries Arctis Nova Pro', 'arctis-nova-pro', '2', 6500000, NULL, '/uploads/SanPham/product5.jpg', N'Trải nghiệm âm thanh gaming Hi-Res chân thực nhất.', 1, 0, GETDATE());

-- 5. SEED NEWS
INSERT INTO News (CategoryId, Title, Slug, Image, ShortDescription, DetailDescription, Status, IsHot, SortOrder, CreatedAt) VALUES
(6, N'Lộ diện thông tin Chip Intel thế hệ 15 Arrow Lake', 'lo-dien-chip-intel-gen-15', '/uploads/Banner/638750744835477947.png', N'Những rò rỉ mới nhất cho thấy hiệu năng vượt trội của CPU Intel mới.', N'<p>Nội dung chi tiết về chip Intel thế hệ 15...</p>', 1, 1, 1, GETDATE()),
(7, N'Đánh giá bàn phím cơ Custom cực "ngon" tầm giá 2 triệu', 'danh-gia-ban-phim-co-custom', '/uploads/Banner/638752431143745238.jpg', N'Chiếc bàn phím không chỉ đẹp mà còn có âm thanh gõ cực kỳ ấn tượng.', N'<p>Chi tiết đánh giá trải nghiệm thực tế...</p>', 1, 0, 2, GETDATE()),
(8, N'5 Cách tối ưu hóa Windows 11 để chơi game mượt mà hơn', '5-cach-toi-uu-windows-11', '/uploads/Banner/636330478278386928.png', N'Hướng dẫn chi tiết giúp bạn tăng FPS và giảm giật lag khi chơi game.', N'<p>Các bước tối ưu hệ thống...</p>', 1, 1, 3, GETDATE());

-- 6. SEED ADVERTISINGS (Banners)
INSERT INTO Advertisings (Title, Image, Position, Link, Status, Target, SortOrder, CreatedAt) VALUES
(N'Big Sale Mùa Hè - Giảm giá tới 50%', '/uploads/Banner/638750745645893841.png', 'Home_Slide', '/san-pham', 1, '_self', 1, GETDATE()),
(N'Macbook Air M2 Cực Ưu Đãi', '/uploads/Banner/638750744835477947.png', 'Home_Slide', '/laptop-macbook', 1, '_self', 2, GETDATE()),
(N'Cộng đồng Gaming Gear Việt Nam', '/uploads/Banner/636330482126038319.jpg', 'Side_Right', 'https://facebook.com', 1, '_blank', 1, GETDATE());

-- 7. SEED OTHERS
INSERT INTO Videos (Title, YoutubeId, ThumbnailUrl, Status, SortOrder, CreatedAt) VALUES
(N'Review Macbook Pro M3 chi tiết', 'v-video1', 'https://img.youtube.com/vi/v-video1/maxresdefault.jpg', 1, 1, GETDATE()),
(N'Trên tay bàn phím Custom MonsGeek', 'v-video2', 'https://img.youtube.com/vi/v-video2/maxresdefault.jpg', 1, 2, GETDATE());

INSERT INTO FAQs (Question, Answer, Status, SortOrder, CreatedAt) VALUES
(N'Chỗ cửa hàng có hỗ trợ trả góp không?', N'Dạ, Antigravity hỗ trợ trả góp qua thẻ tín dụng và hồ sơ duyệt nhanh trong 15 phút.', 1, 1, GETDATE()),
(N'Chính sách bảo hành tại đây như thế nào?', N'Tất cả sản phẩm chính hãng được bảo hành 12-24 tháng theo tiêu chuẩn nhà sản xuất.', 1, 2, GETDATE());

INSERT INTO Documents (Title, FilePath, FileSize, Status, SortOrder, CreatedAt) VALUES
(N'Bảng báo giá Laptop Gaming 2026', '/uploads/docs/bao-gia.pdf', '1.2 MB', 1, 1, GETDATE()),
(N'Hướng dẫn sử dụng bàn phím Akko', '/uploads/docs/manual-akko.pdf', '2.5 MB', 1, 2, GETDATE());

-- 8. SEED SAMPLE CUSTOMERS & ORDERS
INSERT INTO Customers (FullName, Phone, Email, Status, CreatedAt, MemberRank) VALUES
(N'Nguyễn Văn A', '0987654321', 'vana@gmail.com', 1, GETDATE(), 1),
(N'Trần Thị B', '0912345678', 'thib@yahoo.com', 1, GETDATE(), 0);

INSERT INTO Orders (OrderCode, CustomerName, CustomerPhone, ShippingAddress, TotalAmount, OrderStatus, CreatedAt) VALUES
('ORD001', N'Nguyễn Văn A', '0987654321', N'Hà Nội', 45000000, 1, GETDATE()),
('ORD002', N'Trần Thị B', '0912345678', N'TP. HCM', 2500000, 0, GETDATE());

INSERT INTO OrderItems (OrderId, ProductId, ProductName, Price, Quantity, TotalPrice) VALUES
(1, 1, N'Macbook Pro M3 2024', 45000000, 1, 45000000),
(2, 2, N'Bàn phím Akko 3098B', 2500000, 1, 2500000);
