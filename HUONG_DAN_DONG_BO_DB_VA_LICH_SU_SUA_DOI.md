# HƯỚNG DẪN ĐỒNG BỘ CSDL & NHẬT KÝ THAY ĐỔI MÃ NGUỒN (04/11 - 06/11/2026)
> **Dự án**: Gordak Việt Nam (`gordak.vn`) - Mã viết tắt: `go`  
> **Database**: `[GORDAK]`  
> **Nền tảng**: ASP.NET Core (.NET 8), Entity Framework Core / Dapper / Stored Procedures  
> **Thời điểm lập tài liệu**: 06/11/2026  

---

## I. TỔNG HỢP CÁC NỘI DUNG ĐÃ SỬA ĐỔI (04/11 - 06/11/2026)

### 1. Thu gọn phân trang danh mục & sản phẩm (Không hiển thị dàn trải)
- **Vấn đề trước đây**: Danh mục sản phẩm khi có nhiều trang hiển thị toàn bộ tất cả số trang kéo dài hàng mét làm xô lệch khung layout website.
- **Giải pháp**:
  - Tối ưu thuật toán sinh phân trang: Thu gọn thông minh dưới dạng `1 2 3 4 5 ... 35 36` hoặc khi người dùng ở trang bất kỳ (ví dụ trang 10): `1 ... 8 9 10 11 12 ... 36`.
  - Giữ lại nút điều hướng `Đầu`, `Trước`, `Tiếp`, `Cuối`.
- **File mã nguồn thay đổi**:
  - `Antigravity.ECommerce/Views/Product/ProductList.cshtml`
  - `Antigravity.ECommerce/Views/Shared/_Pagination.cshtml`

### 2. Bổ sung Bộ lọc Trạng thái Bắn Hub trong Quản trị Đơn hàng Admin
- **Mục đích**: Cho phép admin lọc nhanh đơn hàng theo tình trạng đồng bộ sang Hub.
- **Các giá trị lọc (`ddlHubStatus`)**:
  - `-1` / `null`: Tất cả đơn hàng
  - `1`: HUB đã nhận (`HubSent = 1`)
  - `0`: HUB chưa nhận (`ISNULL(HubSent, 0) = 0`)
  - `2`: Bắn lỗi (`HubSent = 2`)
  - `3`: Chưa gửi (`HubSent = 3` hoặc `HubSent IS NULL`)
- **Tối ưu CSDL**: Tạo chỉ mục `IX_Orders_HubSent` trên bảng `Orders` và tinh chỉnh Stored Procedure `SP_Orders_Search` để lọc theo `@HubStatus` với điều kiện `ISNULL(o.HubSent, 0)`.
- **File mã nguồn thay đổi**:
  - `Antigravity.ECommerce/Areas/Admin/Views/Orders/Index.cshtml`
  - `Antigravity.ECommerce/Areas/Admin/Controllers/OrdersController.cs`
  - `Antigravity.ECommerce/Services/OrderService.cs`

### 3. Tích hợp Hệ thống Hub Toàn diện (Luồng A & Luồng B)
- **Luồng A (Webhook tiếp nhận tồn kho & giá từ Hub)**:
  - Đồng bộ tự động tồn kho theo SKU, cập nhật ngày hàng về dự kiến (`ExpectedDate`).
  - Hỗ trợ `JsonSerializerOptions.PropertyNameCaseInsensitive` để nhận JSON linh hoạt.
- **Luồng B (Bắn đơn sang Hub)**:
  - Chuẩn hóa 8 phương thức giao hàng Hub trên trang Checkout.
  - Cơ chế phòng vệ: Kiểm tra định dạng email công ty xuất hóa đơn và giới hạn độ dài `<= 100` ký tự (tránh HTTP 400 từ Hub).
  - Chuẩn hóa Source định danh hệ thống là `gordak`.
- **File mã nguồn thay đổi**:
  - `Antigravity.ECommerce/Controllers/CheckoutController.cs`
  - `Antigravity.ECommerce/Services/HubSyncService.cs`
  - `Antigravity.ECommerce/Handlers/HubSyncHandler.cs`

---

## II. DANH MỤC THAY ĐỔI CSDL (DATABASE MIGRATION)
Khi triển khai mã nguồn mới sang **máy tính khác** hoặc **máy chủ LIVE**, bắt buộc phải chạy script SQL dưới đây trên Database `[GORDAK]`.

### File SQL có sẵn trong mã nguồn:
- `Antigravity.ECommerce/scripts/Hub_Integration_Migration.sql`
- `TAILIEU_HUB/Hub_SQL_Migrations/02_Migration_Gordak.sql`

---

## III. SCRIPT SQL CẦN CHẠY TRÊN DATABASE `[GORDAK]`

```sql
USE [GORDAK];
GO

PRINT N'==============================================================================';
PRINT N'BẮT ĐẦU CẬP NHẬT DATABASE CHO GORDAK.VN TÍCH HỢP HUB & TỐI ƯU BỘ LỌC ĐƠN';
PRINT N'==============================================================================';
GO

-- ------------------------------------------------------------------------------
-- 1. BẢNG Products: Bổ sung cột ExpectedDate & Chỉ mục SKU
-- ------------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Products]') 
      AND name = 'ExpectedDate'
)
BEGIN
    ALTER TABLE [dbo].[Products]
    ADD [ExpectedDate] DATE NULL;
    PRINT N'-> Đã thêm cột [ExpectedDate] vào bảng Products.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Products_SKU' AND object_id = OBJECT_ID(N'[dbo].[Products]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Products_SKU] ON [dbo].[Products] ([SKU]);
    PRINT N'-> Đã tạo chỉ mục tìm kiếm [IX_Products_SKU] trên bảng Products.';
END
GO

-- ------------------------------------------------------------------------------
-- 2. BẢNG Orders: Bổ sung các cột theo dõi đồng bộ HUB & Phương thức giao hàng
-- ------------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') 
      AND name = 'HubSent'
)
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [HubSent] BIT NOT NULL CONSTRAINT DF_Orders_HubSent DEFAULT 0;
    PRINT N'-> Đã thêm cột [HubSent] vào bảng Orders.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') 
      AND name = 'HubTries'
)
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [HubTries] INT NOT NULL CONSTRAINT DF_Orders_HubTries DEFAULT 0;
    PRINT N'-> Đã thêm cột [HubTries] vào bảng Orders.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') 
      AND name = 'HubLastError'
)
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [HubLastError] NVARCHAR(500) NULL;
    PRINT N'-> Đã thêm cột [HubLastError] vào bảng Orders.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') 
      AND name = 'HubSentAt'
)
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [HubSentAt] DATETIME NULL;
    PRINT N'-> Đã thêm cột [HubSentAt] vào bảng Orders.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') 
      AND name = 'DeliveryMethod'
)
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [DeliveryMethod] NVARCHAR(255) NULL;
    PRINT N'-> Đã thêm cột [DeliveryMethod] vào bảng Orders.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') 
      AND name = 'DeliveryNote'
)
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [DeliveryNote] NVARCHAR(255) NULL;
    PRINT N'-> Đã thêm cột [DeliveryNote] vào bảng Orders.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') 
      AND name = 'DeliveryChosen'
)
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [DeliveryChosen] BIT NOT NULL CONSTRAINT DF_Orders_DeliveryChosen DEFAULT 0;
    PRINT N'-> Đã thêm cột [DeliveryChosen] vào bảng Orders.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') 
      AND name = 'HubOrderCode'
)
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [HubOrderCode] NVARCHAR(100) NULL;
    PRINT N'-> Đã thêm cột [HubOrderCode] vào bảng Orders.';
END
GO

-- Chỉ mục lọc trạng thái Hub trên Admin
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Orders_HubSent' AND object_id = OBJECT_ID(N'[dbo].[Orders]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Orders_HubSent] 
    ON [dbo].[Orders] ([HubSent])
    INCLUDE ([Id], [OrderCode], [CustomerName], [TotalAmount], [CreatedAt]);
    PRINT N'-> Đã tạo chỉ mục tối ưu lọc đơn [IX_Orders_HubSent] trên bảng Orders.';
END
GO

-- ------------------------------------------------------------------------------
-- 3. TẠO BẢNG HÀNG ĐỢI HubNotifyQueue
-- ------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HubNotifyQueue')
BEGIN
    CREATE TABLE [dbo].[HubNotifyQueue] (
        [QueueId] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [OrderId] INT NOT NULL,
        [Action] NVARCHAR(50) NOT NULL,
        [PayloadJson] NVARCHAR(MAX) NULL,
        [Status] NVARCHAR(20) NOT NULL CONSTRAINT DF_HubNotifyQueue_Status DEFAULT N'Pending',
        [Tries] INT NOT NULL CONSTRAINT DF_HubNotifyQueue_Tries DEFAULT 0,
        [MaxTries] INT NOT NULL CONSTRAINT DF_HubNotifyQueue_MaxTries DEFAULT 5,
        [LastError] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME NOT NULL CONSTRAINT DF_HubNotifyQueue_CreatedAt DEFAULT GETDATE(),
        [NextRunAt] DATETIME NOT NULL CONSTRAINT DF_HubNotifyQueue_NextRunAt DEFAULT GETDATE(),
        [SentAt] DATETIME NULL
    );
    CREATE NONCLUSTERED INDEX [IX_HubNotifyQueue_Status_NextRun] 
    ON [dbo].[HubNotifyQueue] ([Status], [NextRunAt])
    INCLUDE ([QueueId], [OrderId], [Action], [Tries]);
    PRINT N'-> Đã tạo bảng [HubNotifyQueue].';
END
GO

-- ------------------------------------------------------------------------------
-- 4. STORED PROCEDURE ĐỒNG BỘ SẢN PHẨM TỪ HUB: SP_Hub_SyncProduct
-- ------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE [dbo].[SP_Hub_SyncProduct]
    @SKU NVARCHAR(100),
    @StockQuantity INT = NULL,
    @Price DECIMAL(18,2) = NULL,
    @ExpectedDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Products]
    SET 
        [StockQuantity] = CASE WHEN @StockQuantity IS NOT NULL THEN @StockQuantity ELSE [StockQuantity] END,
        [Price] = CASE WHEN @Price IS NOT NULL AND @Price > 0 THEN @Price ELSE [Price] END,
        [ExpectedDate] = CASE WHEN @ExpectedDate IS NOT NULL THEN @ExpectedDate ELSE [ExpectedDate] END,
        [UpdatedAt] = GETDATE()
    WHERE [SKU] = @SKU;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ------------------------------------------------------------------------------
-- 5. STORED PROCEDURE CẬP NHẬT TRẠNG THÁI HUB CHO ĐƠN HÀNG: SP_Orders_UpdateHubStatus
-- ------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE [dbo].[SP_Orders_UpdateHubStatus]
    @OrderId INT,
    @HubSent BIT,
    @HubLastError NVARCHAR(500) = NULL,
    @HubOrderCode NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Orders]
    SET 
        [HubSent] = @HubSent,
        [HubTries] = ISNULL([HubTries], 0) + 1,
        [HubLastError] = @HubLastError,
        [HubSentAt] = CASE WHEN @HubSent = 1 THEN GETDATE() ELSE [HubSentAt] END,
        [HubOrderCode] = CASE WHEN @HubOrderCode IS NOT NULL AND RTRIM(@HubOrderCode) <> '' THEN @HubOrderCode ELSE [HubOrderCode] END,
        [UpdatedAt] = GETDATE()
    WHERE [Id] = @OrderId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ------------------------------------------------------------------------------
-- 6. CÁC STORED PROCEDURE HÀNG ĐỢI THÔNG BÁO HUB
-- ------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE [dbo].[SP_HubNotifyQueue_Enqueue]
    @OrderId INT,
    @Action NVARCHAR(50),
    @PayloadJson NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [dbo].[HubNotifyQueue] ([OrderId], [Action], [PayloadJson], [Status], [Tries], [CreatedAt], [NextRunAt])
    VALUES (@OrderId, @Action, @PayloadJson, N'Pending', 0, GETDATE(), GETDATE());
    SELECT SCOPE_IDENTITY() AS QueueId;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_HubNotifyQueue_GetPending]
    @BatchSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@BatchSize)
        [QueueId], [OrderId], [Action], [PayloadJson], [Tries], [MaxTries]
    FROM [dbo].[HubNotifyQueue]
    WHERE [Status] = N'Pending' AND [NextRunAt] <= GETDATE()
    ORDER BY [QueueId] ASC;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_HubNotifyQueue_MarkSent]
    @QueueId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[HubNotifyQueue]
    SET [Status] = N'Sent', [SentAt] = GETDATE()
    WHERE [QueueId] = @QueueId;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_HubNotifyQueue_MarkFailed]
    @QueueId BIGINT,
    @ErrorMessage NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[HubNotifyQueue]
    SET 
        [Tries] = [Tries] + 1,
        [LastError] = @ErrorMessage,
        [Status] = CASE WHEN [Tries] + 1 >= [MaxTries] THEN N'Failed' ELSE N'Pending' END,
        [NextRunAt] = DATEADD(SECOND, POWER(2, [Tries] + 1) * 30, GETDATE())
    WHERE [QueueId] = @QueueId;
END
GO

-- ------------------------------------------------------------------------------
-- 7. STORED PROCEDURE TÌM KIẾM ĐƠN HÀNG ADMIN (CÓ THAM SỐ BỘ LỌC @HubStatus)
-- ------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE [dbo].[SP_Orders_Search]
    @Keyword NVARCHAR(250) = NULL,
    @Status INT = NULL,
    @HubStatus INT = NULL, -- -1 hoặc NULL: Tất cả; 1: Đã nhận; 0: Chưa nhận; 2: Lỗi
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @PageIndex INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    SET @Keyword = NULLIF(RTRIM(LTRIM(@Keyword)), '');
    IF @Status = -1 SET @Status = NULL;
    IF @HubStatus = -1 SET @HubStatus = NULL;

    DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;

    ;WITH FilteredOrders AS (
        SELECT 
            o.*,
            COUNT(*) OVER() AS TotalCount
        FROM [dbo].[Orders] o
        WHERE 
            (@Keyword IS NULL OR (o.[OrderCode] LIKE N'%' + @Keyword + N'%' OR o.[CustomerName] LIKE N'%' + @Keyword + N'%' OR o.[PhoneNumber] LIKE N'%' + @Keyword + N'%'))
            AND (@Status IS NULL OR o.[Status] = @Status)
            AND (@HubStatus IS NULL 
                 OR (@HubStatus = 1 AND ISNULL(o.[HubSent], 0) = 1)
                 OR (@HubStatus = 0 AND ISNULL(o.[HubSent], 0) = 0)
                 OR (@HubStatus = 2 AND ISNULL(o.[HubSent], 0) = 2)
                 OR (@HubStatus = 3 AND o.[HubSent] IS NULL))
            AND (@FromDate IS NULL OR o.[CreatedAt] >= @FromDate)
            AND (@ToDate IS NULL OR o.[CreatedAt] <= @ToDate)
    )
    SELECT * FROM FilteredOrders
    ORDER BY [CreatedAt] DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

PRINT N'==============================================================================';
PRINT N'HOÀN TẤT ĐỒNG BỘ CSDL CHO GORDAK VIỆT NAM (GORDAK)!';
PRINT N'==============================================================================';
GO
```

