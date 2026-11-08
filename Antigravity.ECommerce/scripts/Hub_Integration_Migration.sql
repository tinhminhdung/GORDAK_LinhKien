-- ==============================================================================
-- SCRIPT CẬP NHẬT CƠ SỞ DỮ LIỆU ĐỂ TÍCH HỢP HỆ THỐNG HUB BÁN HÀNG
-- Áp dụng cho website: gordak.vn (Dự án: go)
-- Database: [GORDAK]
-- 
-- NHẬT KÝ CẬP NHẬT / CHANGE LOG:
-- ------------------------------------------------------------------------------
-- [06/11/2026] - BỔ SUNG BỘ LỌC TRẠNG THÁI HUB TRÊN ADMIN QUẢN LÝ ĐƠN HÀNG:
--   + Thêm mục 2.1: Tạo chỉ mục [IX_Orders_HubSent] trên bảng Orders để tối ưu tốc độ lọc đơn theo trạng thái Hub.
--   + Thêm mục 5b: Stored Procedure [dbo].[SP_Orders_Search] hỗ trợ tham số @HubStatus (INT = NULL)
--     Giá trị: 1 (HUB đã nhận), 0 (HUB chưa nhận), 2 (Bắn lỗi), 3 (Chưa gửi), NULL (Tất cả).
-- [01/11/2026] - TÍCH HỢP HUB BAN ĐẦU:
--   + Bảng Products (ExpectedDate), Orders (HubSent, HubTries, HubLastError, HubSentAt, DeliveryMethod...).
--   + Bảng hàng đợi HubNotifyQueue và các Stored Procedure đồng bộ.
-- ==============================================================================

USE [GORDAK];
GO

PRINT N'==============================================================================';
PRINT N'BẮT ĐẦU CẬP NHẬT DATABASE CHO GORDAK.VN TÍCH HỢP HUB';
PRINT N'==============================================================================';
GO

-- ------------------------------------------------------------------------------
-- 1. BẢNG Products: Bổ sung cột ExpectedDate (Ngày hàng về dự kiến từ HUB - Luồng A)
-- ------------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Products]') 
      AND name = 'ExpectedDate'
)
BEGIN
    ALTER TABLE [dbo].[Products]
    ADD [ExpectedDate] DATE NULL;
    PRINT N'-> Đã thêm cột [ExpectedDate] (DATE, NULL) vào bảng Products thành công.';
END
ELSE
BEGIN
    PRINT N'-> Cột [ExpectedDate] đã tồn tại trong bảng Products.';
END
GO

-- Tạo chỉ mục IX_Products_SKU để tăng tốc tối đa khi HUB đồng bộ 1.000 mã/lần (Index Seek)
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
-- 2. BẢNG Orders: Bổ sung các cột theo dõi đồng bộ HUB và Phương thức giao hàng (Luồng B)
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

-- ------------------------------------------------------------------------------
-- 2.1 [BỔ SUNG MỚI - NGÀY 06/11/2026]
-- TẠO CHỈ MỤC TỐI ƯU BỘ LỌC TRẠNG THÁI HUB: IX_Orders_HubSent
-- Tối ưu hóa truy vấn tìm kiếm lọc đơn hàng theo HubSent trên Admin
-- ------------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Orders_HubSent' AND object_id = OBJECT_ID(N'[dbo].[Orders]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Orders_HubSent] 
    ON [dbo].[Orders] ([HubSent]) 
    INCLUDE ([OrderId], [OrderCode], [HubLastError], [CreatedAt]);
    PRINT N'-> Đã tạo chỉ mục tìm kiếm [IX_Orders_HubSent] trên bảng Orders (Bổ sung 06/11/2026).';
END
GO

-- ------------------------------------------------------------------------------
-- 3. TẠO BẢNG HubNotifyQueue: Quản lý danh sách đăng ký "Báo cho tôi" (kind: notify)
-- ------------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.tables 
    WHERE object_id = OBJECT_ID(N'[dbo].[HubNotifyQueue]')
)
BEGIN
    CREATE TABLE [dbo].[HubNotifyQueue] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [OrderCode] VARCHAR(50) NOT NULL UNIQUE,
        [Json] NVARCHAR(MAX) NOT NULL,
        [CreatedAt] DATETIME NOT NULL CONSTRAINT DF_HubNotifyQueue_CreatedAt DEFAULT GETDATE(),
        [HubTries] INT NOT NULL CONSTRAINT DF_HubNotifyQueue_HubTries DEFAULT 0,
        [HubLastError] NVARCHAR(500) NULL,
        [CustomerName] NVARCHAR(200) NULL,
        [CustomerPhone] NVARCHAR(50) NULL,
        [CustomerAddress] NVARCHAR(500) NULL,
        [ProductSKU] NVARCHAR(100) NULL,
        [ProductName] NVARCHAR(500) NULL,
        [Quantity] INT NOT NULL CONSTRAINT DF_HubNotifyQueue_Quantity DEFAULT 1,
        [Unit] NVARCHAR(50) NULL,
        [Price] DECIMAL(18,2) NOT NULL CONSTRAINT DF_HubNotifyQueue_Price DEFAULT 0,
        [ExpectedDate] DATE NULL,
        [Note] NVARCHAR(500) NULL,
        [HubSent] BIT NOT NULL CONSTRAINT DF_HubNotifyQueue_HubSent DEFAULT 0,
        [HubSentAt] DATETIME NULL,
        [Status] INT NOT NULL CONSTRAINT DF_HubNotifyQueue_Status DEFAULT 0
    );
    PRINT N'-> Đã tạo bảng [HubNotifyQueue] thành công.';
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HubNotifyQueue]') AND name = 'CustomerName')
        ALTER TABLE [dbo].[HubNotifyQueue] ADD [CustomerName] NVARCHAR(200) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HubNotifyQueue]') AND name = 'CustomerPhone')
        ALTER TABLE [dbo].[HubNotifyQueue] ADD [CustomerPhone] NVARCHAR(50) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HubNotifyQueue]') AND name = 'CustomerAddress')
        ALTER TABLE [dbo].[HubNotifyQueue] ADD [CustomerAddress] NVARCHAR(500) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HubNotifyQueue]') AND name = 'ProductSKU')
        ALTER TABLE [dbo].[HubNotifyQueue] ADD [ProductSKU] NVARCHAR(100) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HubNotifyQueue]') AND name = 'ProductName')
        ALTER TABLE [dbo].[HubNotifyQueue] ADD [ProductName] NVARCHAR(500) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HubNotifyQueue]') AND name = 'Quantity')
        ALTER TABLE [dbo].[HubNotifyQueue] ADD [Quantity] INT NOT NULL CONSTRAINT DF_HubNotifyQueue_Quantity DEFAULT 1;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HubNotifyQueue]') AND name = 'Unit')
        ALTER TABLE [dbo].[HubNotifyQueue] ADD [Unit] NVARCHAR(50) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HubNotifyQueue]') AND name = 'Price')
        ALTER TABLE [dbo].[HubNotifyQueue] ADD [Price] DECIMAL(18,2) NOT NULL CONSTRAINT DF_HubNotifyQueue_Price DEFAULT 0;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HubNotifyQueue]') AND name = 'ExpectedDate')
        ALTER TABLE [dbo].[HubNotifyQueue] ADD [ExpectedDate] DATE NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HubNotifyQueue]') AND name = 'Note')
        ALTER TABLE [dbo].[HubNotifyQueue] ADD [Note] NVARCHAR(500) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HubNotifyQueue]') AND name = 'HubSent')
        ALTER TABLE [dbo].[HubNotifyQueue] ADD [HubSent] BIT NOT NULL CONSTRAINT DF_HubNotifyQueue_HubSent DEFAULT 0;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HubNotifyQueue]') AND name = 'HubSentAt')
        ALTER TABLE [dbo].[HubNotifyQueue] ADD [HubSentAt] DATETIME NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HubNotifyQueue]') AND name = 'Status')
        ALTER TABLE [dbo].[HubNotifyQueue] ADD [Status] INT NOT NULL CONSTRAINT DF_HubNotifyQueue_Status DEFAULT 0;
    PRINT N'-> Bảng [HubNotifyQueue] đã tồn tại và được cập nhật đầy đủ các cột.';
END
GO

-- ------------------------------------------------------------------------------
-- 4. STORED PROCEDURE ĐỒNG BỘ TỪ HUB (LUỒNG A): SP_Hub_SyncProduct
-- Cập nhật tồn kho, giá bán lẻ, đơn vị tính, ngày dự kiến có hàng theo SKU
-- ------------------------------------------------------------------------------
PRINT N'-> Đang tạo/cập nhật thủ tục [dbo].[SP_Hub_SyncProduct]...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_Hub_SyncProduct]
    @SKU NVARCHAR(50),
    @Stock INT,
    @Price DECIMAL(18,0),
    @Unit NVARCHAR(50) = NULL,
    @ExpectedDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Nếu có tồn (>0) thì ExpectedDate tự động set NULL theo quy tắc nghiệp vụ HUB
    IF @Stock > 0
    BEGIN
        SET @ExpectedDate = NULL;
    END

    UPDATE [dbo].[Products]
    SET [Stock] = @Stock,
        [Price] = CASE WHEN @Price > 0 THEN @Price ELSE [Price] END,
        [Unit] = CASE WHEN @Unit IS NOT NULL AND RTRIM(@Unit) <> '' THEN @Unit ELSE [Unit] END,
        [ExpectedDate] = @ExpectedDate,
        [UpdatedAt] = GETDATE()
    WHERE [SKU] = @SKU;

    -- Trả về số dòng cập nhật (1 = tìm thấy & cập nhật, 0 = không tìm thấy SKU)
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT N'-> Đã hoàn thành thủ tục [dbo].[SP_Hub_SyncProduct].';
GO

-- ------------------------------------------------------------------------------
-- 5. STORED PROCEDURE CẬP NHẬT TRẠNG THÁI GỬI ĐƠN HÀNG SANG HUB: SP_Orders_UpdateHubStatus
-- ------------------------------------------------------------------------------
PRINT N'-> Đang tạo/cập nhật thủ tục [dbo].[SP_Orders_UpdateHubStatus]...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_Orders_UpdateHubStatus]
    @OrderId INT,
    @HubSent BIT,
    @HubLastError NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[Orders]
    SET [HubSent] = @HubSent,
        [HubTries] = [HubTries] + 1,
        [HubLastError] = @HubLastError,
        [HubSentAt] = CASE WHEN @HubSent = 1 THEN GETDATE() ELSE [HubSentAt] END,
        [UpdatedAt] = GETDATE()
    WHERE [OrderId] = @OrderId;
END
GO

PRINT N'-> Đã hoàn thành thủ tục [dbo].[SP_Orders_UpdateHubStatus].';
GO

-- ------------------------------------------------------------------------------
-- 5b. [BỔ SUNG MỚI - NGÀY 06/11/2026]
-- STORED PROCEDURE TÌM KIẾM ĐƠN HÀNG KÈM BỘ LỌC TRẠNG THÁI HUB: SP_Orders_Search
-- Nghiệp vụ: Lọc đơn hàng theo trạng thái gửi HUB (Đã nhận / Chưa nhận / Bị lỗi / Chưa gửi)
-- Ngày cập nhật: 06/11/2026
-- ------------------------------------------------------------------------------
PRINT N'-> Đang tạo/cập nhật thủ tục [dbo].[SP_Orders_Search] (Bổ sung 06/11/2026)...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_Orders_Search]
    @Keyword NVARCHAR(250) = NULL,
    @OrderStatus INT = NULL,
    @PaymentStatus INT = NULL,
    @ProvinceId INT = NULL,
    @WardId INT = NULL,
    @RequiresVAT BIT = NULL,
    @Ngay_Min DATETIME = NULL,
    @Ngay_Max DATETIME = NULL,
    @SortColumn NVARCHAR(50) = 'CreatedAt',
    @SortOrder NVARCHAR(10) = 'DESC',
    @PageIndex INT = 1,
    @PageSize INT = 20,
    @HubStatus INT = NULL -- [BỔ SUNG 06/11/2026] 1: Đã nhận, 0: Chưa nhận, 2: Lỗi, 3: Chưa gửi
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;

    SELECT 
        o.*,
        p.Name AS ProvinceName,
        w.Name AS WardName,
        c.MemberRank AS MemberRankName,
        (SELECT COUNT(*) FROM OrderItems WHERE OrderId = o.OrderId) AS ItemCount,
        COUNT(*) OVER() as TotalCount
    FROM Orders o
    LEFT JOIN Provinces p ON o.ProvinceId = p.ProvinceId
    LEFT JOIN Wards w ON o.WardId = w.WardId
    LEFT JOIN Customers c ON o.CustomerId = c.CustomerId
    WHERE (@Keyword IS NULL OR o.OrderCode LIKE N'%' + @Keyword + '%' OR o.CustomerName LIKE N'%' + @Keyword + '%' OR o.CustomerPhone LIKE N'%' + @Keyword + '%')
      AND (@OrderStatus IS NULL OR o.OrderStatus = @OrderStatus)
      AND (@PaymentStatus IS NULL OR o.PaymentStatus = @PaymentStatus)
      AND (@ProvinceId IS NULL OR o.ProvinceId = @ProvinceId)
      AND (@WardId IS NULL OR o.WardId = @WardId)
      AND (@RequiresVAT IS NULL OR o.RequiresVAT = @RequiresVAT)
      AND (@Ngay_Min IS NULL OR o.CreatedAt >= @Ngay_Min)
      AND (@Ngay_Max IS NULL OR o.CreatedAt <= @Ngay_Max)
      AND (
          @HubStatus IS NULL 
          OR (@HubStatus = 1 AND o.HubSent = 1)
          OR (@HubStatus = 0 AND ISNULL(o.HubSent, 0) = 0)
          OR (@HubStatus = 2 AND ISNULL(o.HubSent, 0) = 0 AND o.HubLastError IS NOT NULL AND o.HubLastError <> '')
          OR (@HubStatus = 3 AND ISNULL(o.HubSent, 0) = 0 AND (o.HubLastError IS NULL OR o.HubLastError = ''))
      )
    ORDER BY 
        CASE WHEN @SortColumn = 'CreatedAt' AND @SortOrder = 'ASC' THEN o.CreatedAt END ASC,
        CASE WHEN @SortColumn = 'CreatedAt' AND @SortOrder = 'DESC' THEN o.CreatedAt END DESC,
        CASE WHEN @SortColumn = 'TotalAmount' AND @SortOrder = 'ASC' THEN o.TotalAmount END ASC,
        CASE WHEN @SortColumn = 'TotalAmount' AND @SortOrder = 'DESC' THEN o.TotalAmount END DESC,
        CASE WHEN @SortColumn = 'OrderCode' AND @SortOrder = 'ASC' THEN o.OrderCode END ASC,
        CASE WHEN @SortColumn = 'OrderCode' AND @SortOrder = 'DESC' THEN o.OrderCode END DESC,
        CASE WHEN @SortColumn = 'OrderId' AND @SortOrder = 'ASC' THEN o.OrderId END ASC,
        CASE WHEN @SortColumn = 'OrderId' AND @SortOrder = 'DESC' THEN o.OrderId END DESC,
        o.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

PRINT N'-> Đã hoàn thành thủ tục [dbo].[SP_Orders_Search].';
GO

-- ------------------------------------------------------------------------------
-- 6. CÁC STORED PROCEDURE QUẢN LÝ HÀNG ĐỢI HubNotifyQueue
-- ------------------------------------------------------------------------------
PRINT N'-> Đang tạo các thủ tục cho [dbo].[HubNotifyQueue]...';
GO

-- 6.1 Thêm vào danh sách đăng ký báo có hàng
CREATE OR ALTER PROCEDURE [dbo].[SP_HubNotifyQueue_Enqueue]
    @OrderCode VARCHAR(50),
    @Json NVARCHAR(MAX),
    @CustomerName NVARCHAR(200) = NULL,
    @CustomerPhone NVARCHAR(50) = NULL,
    @CustomerAddress NVARCHAR(500) = NULL,
    @ProductSKU NVARCHAR(100) = NULL,
    @ProductName NVARCHAR(500) = NULL,
    @Quantity INT = 1,
    @Unit NVARCHAR(50) = NULL,
    @Price DECIMAL(18,2) = 0,
    @ExpectedDate DATE = NULL,
    @Note NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @CustomerName IS NULL AND ISJSON(@Json) = 1
    BEGIN
        SET @CustomerName = JSON_VALUE(@Json, '$.customer.name');
        SET @CustomerPhone = JSON_VALUE(@Json, '$.customer.phone');
        SET @CustomerAddress = JSON_VALUE(@Json, '$.customer.address');
        SET @ProductSKU = JSON_VALUE(@Json, '$.items[0].sku');
        SET @ProductName = JSON_VALUE(@Json, '$.items[0].name');
        SET @Quantity = ISNULL(TRY_CAST(JSON_VALUE(@Json, '$.items[0].qty') AS INT), 1);
        SET @Unit = JSON_VALUE(@Json, '$.items[0].unit');
        SET @Price = ISNULL(TRY_CAST(JSON_VALUE(@Json, '$.items[0].price') AS DECIMAL(18,2)), 0);
        SET @ExpectedDate = TRY_CAST(JSON_VALUE(@Json, '$.items[0].expected') AS DATE);
        SET @Note = JSON_VALUE(@Json, '$.note');
    END

    IF NOT EXISTS (SELECT 1 FROM [dbo].[HubNotifyQueue] WHERE [OrderCode] = @OrderCode)
    BEGIN
        INSERT INTO [dbo].[HubNotifyQueue] (
            [OrderCode], [Json], [CreatedAt], [HubTries], [HubSent], [Status],
            [CustomerName], [CustomerPhone], [CustomerAddress],
            [ProductSKU], [ProductName], [Quantity], [Unit], [Price], [ExpectedDate], [Note]
        )
        VALUES (
            @OrderCode, @Json, GETDATE(), 0, 0, 0,
            @CustomerName, @CustomerPhone, @CustomerAddress,
            @ProductSKU, @ProductName, ISNULL(@Quantity, 1), @Unit, ISNULL(@Price, 0), @ExpectedDate, @Note
        );
    END
END
GO

-- 6.2 Xóa theo OrderCode
CREATE OR ALTER PROCEDURE [dbo].[SP_HubNotifyQueue_Delete]
    @OrderCode VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [dbo].[HubNotifyQueue] WHERE [OrderCode] = @OrderCode;
END
GO

-- 6.2b Cập nhật trạng thái gửi HUB cho phiếu đăng ký báo có hàng
CREATE OR ALTER PROCEDURE [dbo].[SP_HubNotifyQueue_UpdateHubStatus]
    @OrderCode VARCHAR(50),
    @HubSent BIT,
    @HubLastError NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[HubNotifyQueue]
    SET [HubSent] = @HubSent,
        [HubTries] = [HubTries] + 1,
        [HubLastError] = @HubLastError,
        [HubSentAt] = CASE WHEN @HubSent = 1 THEN GETDATE() ELSE [HubSentAt] END
    WHERE [OrderCode] = @OrderCode;
END
GO

-- 6.3 Cập nhật lỗi và tăng số lần thử khi gửi thất bại
CREATE OR ALTER PROCEDURE [dbo].[SP_HubNotifyQueue_UpdateError]
    @OrderCode VARCHAR(50),
    @HubLastError NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[HubNotifyQueue]
    SET [HubSent] = 0,
        [HubTries] = [HubTries] + 1,
        [HubLastError] = @HubLastError
    WHERE [OrderCode] = @OrderCode;
END
GO

-- 6.4 Lấy danh sách cần gửi lại (tối đa 20 bản ghi chưa vượt quá 20 lần thử)
CREATE OR ALTER PROCEDURE [dbo].[SP_HubNotifyQueue_GetPending]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 20 [Id], [OrderCode], [Json], [HubTries]
    FROM [dbo].[HubNotifyQueue]
    WHERE [HubSent] = 0 AND [HubTries] < 20
    ORDER BY [Id] ASC;
END
GO

-- 6.5 Dọn dẹp hàng đợi quá hạn
CREATE OR ALTER PROCEDURE [dbo].[SP_HubNotifyQueue_Cleanup]
AS
BEGIN
    SET NOCOUNT ON;
END
GO

-- 6.6 Tìm kiếm & Phân trang danh sách đăng ký báo có hàng cho Admin
CREATE OR ALTER PROCEDURE [dbo].[SP_HubNotifyQueue_Search]
    @Keyword NVARCHAR(200) = NULL,
    @Status INT = NULL,
    @HubStatus INT = NULL,
    @Ngay_Min DATETIME = NULL,
    @Ngay_Max DATETIME = NULL,
    @PageIndex INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    SELECT q.*,
           p.[ProductId],
           p.[Slug] AS ProductSlug,
           p.[MainImage] AS ProductImage,
           p.[Stock] AS CurrentStock,
           COUNT(*) OVER() AS TotalCount
    FROM [dbo].[HubNotifyQueue] q
    LEFT JOIN [dbo].[Products] p ON p.[SKU] = q.[ProductSKU]
    WHERE (@Keyword IS NULL OR @Keyword = ''
           OR q.[OrderCode] LIKE '%' + @Keyword + '%'
           OR q.[CustomerName] LIKE '%' + @Keyword + '%'
           OR q.[CustomerPhone] LIKE '%' + @Keyword + '%'
           OR q.[ProductSKU] LIKE '%' + @Keyword + '%'
           OR q.[ProductName] LIKE '%' + @Keyword + '%')
      AND (@Status IS NULL OR q.[Status] = @Status)
      AND (@Ngay_Min IS NULL OR q.[CreatedAt] >= @Ngay_Min)
      AND (@Ngay_Max IS NULL OR q.[CreatedAt] <= DATEADD(SECOND, 86399, CAST(CAST(@Ngay_Max AS DATE) AS DATETIME)))
      AND (
          @HubStatus IS NULL
          OR (@HubStatus = 1 AND q.[HubSent] = 1)
          OR (@HubStatus = 0 AND q.[HubSent] = 0)
          OR (@HubStatus = 2 AND q.[HubSent] = 0 AND q.[HubLastError] IS NOT NULL AND q.[HubLastError] <> '')
          OR (@HubStatus = 3 AND q.[HubSent] = 0 AND (q.[HubLastError] IS NULL OR q.[HubLastError] = ''))
      )
    ORDER BY q.[CreatedAt] DESC, q.[Id] DESC
    OFFSET (@PageIndex - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- 6.7 Đếm số lượng theo trạng thái xử lý trên Admin
CREATE OR ALTER PROCEDURE [dbo].[SP_HubNotifyQueue_GetStatusCounts]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        COUNT(*) AS [All],
        ISNULL(SUM(CASE WHEN [Status] = 0 THEN 1 ELSE 0 END), 0) AS [New],
        ISNULL(SUM(CASE WHEN [Status] = 1 THEN 1 ELSE 0 END), 0) AS [Contacted],
        ISNULL(SUM(CASE WHEN [Status] = 2 THEN 1 ELSE 0 END), 0) AS [Completed],
        ISNULL(SUM(CASE WHEN [Status] = 3 THEN 1 ELSE 0 END), 0) AS [Cancelled]
    FROM [dbo].[HubNotifyQueue];
END
GO

-- 6.8 Lấy chi tiết theo Id
CREATE OR ALTER PROCEDURE [dbo].[SP_HubNotifyQueue_GetById]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 q.*,
           p.[ProductId],
           p.[Slug] AS ProductSlug,
           p.[MainImage] AS ProductImage,
           p.[Stock] AS CurrentStock
    FROM [dbo].[HubNotifyQueue] q
    LEFT JOIN [dbo].[Products] p ON p.[SKU] = q.[ProductSKU]
    WHERE q.[Id] = @Id;
END
GO

-- 6.9 Cập nhật trạng thái xử lý & ghi chú
CREATE OR ALTER PROCEDURE [dbo].[SP_HubNotifyQueue_UpdateStatus]
    @Id INT,
    @Status INT,
    @Note NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[HubNotifyQueue]
    SET [Status] = @Status,
        [Note] = CASE WHEN @Note IS NOT NULL THEN @Note ELSE [Note] END
    WHERE [Id] = @Id;
END
GO

-- 6.10 Xóa bản ghi theo Id
CREATE OR ALTER PROCEDURE [dbo].[SP_HubNotifyQueue_DeleteById]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [dbo].[HubNotifyQueue] WHERE [Id] = @Id;
END
GO

PRINT N'-> Đã hoàn thành các thủ tục cho [dbo].[HubNotifyQueue].';
GO

-- ------------------------------------------------------------------------------
-- 7. THỦ TỤC LẤY ĐƠN HÀNG ĐỂ ĐỒNG BỘ SANG HUB: SP_Orders_GetForHubSync
-- ------------------------------------------------------------------------------
PRINT N'-> Đang tạo thủ tục [dbo].[SP_Orders_GetForHubSync]...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_Orders_GetForHubSync]
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Lấy thông tin đơn hàng
    SELECT TOP 1 *
    FROM [dbo].[Orders]
    WHERE [OrderId] = @OrderId;

    -- Lấy danh sách chi tiết sản phẩm kèm SKU và Unit từ bảng Products
    SELECT oi.*, p.[SKU] AS ProductSKU, p.[Unit] AS ProductUnit, p.[OldPrice] AS ProdOldPrice
    FROM [dbo].[OrderItems] oi
    LEFT JOIN [dbo].[Products] p ON oi.[ProductId] = p.[ProductId]
    WHERE oi.[OrderId] = @OrderId;
END
GO

-- 10. THỦ TỤC TRUY VẤN SẢN PHẨM THEO SKU (HỖ TRỢ BÁO CÓ HÀNG & SYNC)
IF OBJECT_ID(N'[dbo].[SP_Products_GetBySku]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_Products_GetBySku];
GO

CREATE PROCEDURE [dbo].[SP_Products_GetBySku]
    @SKU NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 * FROM [dbo].[Products] WHERE [SKU] = @SKU;
END
GO

-- 11. THỦ TỤC CẬP NHẬT CẤU HÌNH HỆ THỐNG (UPSERT BẢNG Settings)
IF OBJECT_ID(N'[dbo].[SP_Settings_UpdateValue]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_Settings_UpdateValue];
GO

CREATE PROCEDURE [dbo].[SP_Settings_UpdateValue]
    @SettingKey NVARCHAR(100),
    @SettingValue NVARCHAR(MAX),
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM [dbo].[Settings] WHERE [SettingKey] = @SettingKey)
    BEGIN
        UPDATE [dbo].[Settings]
        SET [SettingValue] = @SettingValue,
            [UpdatedBy] = @UpdatedBy,
            [UpdatedAt] = GETDATE()
        WHERE [SettingKey] = @SettingKey;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[Settings] ([SettingKey], [SettingValue], [GroupName], [Description], [CreatedBy], [CreatedAt])
        VALUES (@SettingKey, @SettingValue, N'HubSettings', N'Cấu hình tích hợp HUB POS', @UpdatedBy, GETDATE());
    END
END
GO

-- 12. KHỞI TẠO CÁC GIÁ TRỊ CẤU HÌNH HUB MẶC ĐỊNH VÀO BẢNG Settings (NẾU CHƯA CÓ)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Settings] WHERE [SettingKey] = 'Hub_Enable')
    INSERT INTO [dbo].[Settings] ([SettingKey], [SettingValue], [GroupName], [Description], [CreatedAt])
    VALUES ('Hub_Enable', 'true', N'HubSettings', N'Kích hoạt đồng bộ HUB POS', GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Settings] WHERE [SettingKey] = 'Hub_Source')
    INSERT INTO [dbo].[Settings] ([SettingKey], [SettingValue], [GroupName], [Description], [CreatedAt])
    VALUES ('Hub_Source', 'gordak', N'HubSettings', N'Mã định danh Website trên HUB (gordak)', GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Settings] WHERE [SettingKey] = 'Hub_SyncAllowIp')
    INSERT INTO [dbo].[Settings] ([SettingKey], [SettingValue], [GroupName], [Description], [CreatedAt])
    VALUES ('Hub_SyncAllowIp', '', N'HubSettings', N'IP Whitelist máy chủ HUB', GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Settings] WHERE [SettingKey] = 'Hub_SyncKey')
    INSERT INTO [dbo].[Settings] ([SettingKey], [SettingValue], [GroupName], [Description], [CreatedAt])
    VALUES ('Hub_SyncKey', 'GORDAK_HUB_SYNC_KEY_2026', N'HubSettings', N'Khóa bí mật Webhook Sync', GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Settings] WHERE [SettingKey] = 'Hub_OrderUrl')
    INSERT INTO [dbo].[Settings] ([SettingKey], [SettingValue], [GroupName], [Description], [CreatedAt])
    VALUES ('Hub_OrderUrl', 'https://pos.nhatminhesc.com/api/web/orders', N'HubSettings', N'URL tiếp nhận đơn hàng HUB', GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Settings] WHERE [SettingKey] = 'Hub_OrderKey')
    INSERT INTO [dbo].[Settings] ([SettingKey], [SettingValue], [GroupName], [Description], [CreatedAt])
    VALUES ('Hub_OrderKey', 'GORDAK_ORDER_KEY_2026', N'HubSettings', N'Khóa bí mật gửi đơn hàng', GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Settings] WHERE [SettingKey] = 'Hub_NotifyUrl')
    INSERT INTO [dbo].[Settings] ([SettingKey], [SettingValue], [GroupName], [Description], [CreatedAt])
    VALUES ('Hub_NotifyUrl', 'https://pos.nhatminhesc.com/api/web/orders', N'HubSettings', N'URL nhận đăng ký báo có hàng HUB', GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Settings] WHERE [SettingKey] = 'Hub_NotifyKey')
    INSERT INTO [dbo].[Settings] ([SettingKey], [SettingValue], [GroupName], [Description], [CreatedAt])
    VALUES ('Hub_NotifyKey', 'GORDAK_ORDER_KEY_2026', N'HubSettings', N'Khóa bí mật gửi báo có hàng', GETDATE());
GO

-- 13. THỦ TỤC THÊM MỚI SẢN PHẨM (BỔ SUNG ExpectedDate) - GORDAK
CREATE OR ALTER PROCEDURE [dbo].[SP_Products_Insert]
    @CategoryIds NVARCHAR(500) = NULL,
    @SKU NVARCHAR(100) = NULL,
    @Name NVARCHAR(500),
    @Slug NVARCHAR(500) = NULL,
    @ShortDescription NVARCHAR(MAX) = NULL,
    @DetailDescription NVARCHAR(MAX) = NULL,
    @Price DECIMAL(18,2) = 0,
    @OldPrice DECIMAL(18,2) = 0,
    @PurchasePrice DECIMAL(18,2) = 0,
    @Stock INT = 0,
    @Unit NVARCHAR(50) = NULL,
    @Weight DECIMAL(18,2) = 0,
    @MainImage NVARCHAR(500) = NULL,
    @ImageGallery NVARCHAR(MAX) = NULL,
    @YoutubeVideo NVARCHAR(MAX) = NULL,
    @Tags NVARCHAR(500) = NULL,
    @RelatedProducts NVARCHAR(500) = NULL,
    @Accessories NVARCHAR(500) = NULL,
    @IsHot BIT = 0,
    @IsNew BIT = 0,
    @IsBestSeller BIT = 0,
    @Status INT = 1,
    @SeoTitle NVARCHAR(500) = NULL,
    @SeoDescription NVARCHAR(MAX) = NULL,
    @SeoKeywords NVARCHAR(500) = NULL,
    @ShopeeLink NVARCHAR(500) = NULL,
    @LazadaLink NVARCHAR(500) = NULL,
    @TechnicalSpecs NVARCHAR(MAX) = NULL,
    @InstructionVideo NVARCHAR(MAX) = NULL,
    @BrandId INT = NULL,
    @WarrantyId INT = NULL,
    @ConditionId INT = NULL,
    @ExpectedDate DATETIME = NULL,
    @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [dbo].[Products]
    (
        [CategoryIds], [SKU], [Name], [Slug], [ShortDescription],
        [DetailDescription], [Price], [OldPrice], [PurchasePrice],
        [Stock], [Unit], [Weight], [MainImage], [ImageGallery], [YoutubeVideo],
        [Tags], [RelatedProducts], [Accessories], [IsHot], [IsNew],
        [IsBestSeller], [Status], [SeoTitle], [SeoDescription], [SeoKeywords],
        [ShopeeLink], [LazadaLink], [TechnicalSpecs], [InstructionVideo],
        [BrandId], [WarrantyId], [ConditionId],
        [ExpectedDate], [CreatedBy], [CreatedAt]
    )
    VALUES
    (
        @CategoryIds, @SKU, @Name, @Slug, @ShortDescription,
        @DetailDescription, @Price, @OldPrice, @PurchasePrice,
        @Stock, @Unit, @Weight, @MainImage, @ImageGallery, @YoutubeVideo,
        @Tags, @RelatedProducts, @Accessories, @IsHot, @IsNew,
        @IsBestSeller, @Status, @SeoTitle, @SeoDescription, @SeoKeywords,
        @ShopeeLink, @LazadaLink, @TechnicalSpecs, @InstructionVideo,
        @BrandId, @WarrantyId, @ConditionId,
        @ExpectedDate, @CreatedBy, GETDATE()
    );

    SELECT SCOPE_IDENTITY();
END
GO

-- 14. THỦ TỤC CHỈNH SỬA SẢN PHẨM (BỔ SUNG ExpectedDate) - GORDAK
CREATE OR ALTER PROCEDURE [dbo].[SP_Products_Update]
    @ProductId INT,
    @CategoryIds NVARCHAR(500) = NULL,
    @SKU NVARCHAR(100) = NULL,
    @Name NVARCHAR(500),
    @Slug NVARCHAR(500) = NULL,
    @ShortDescription NVARCHAR(MAX) = NULL,
    @DetailDescription NVARCHAR(MAX) = NULL,
    @Price DECIMAL(18,2) = 0,
    @OldPrice DECIMAL(18,2) = 0,
    @PurchasePrice DECIMAL(18,2) = 0,
    @Stock INT = 0,
    @Unit NVARCHAR(50) = NULL,
    @Weight DECIMAL(18,2) = 0,
    @MainImage NVARCHAR(500) = NULL,
    @ImageGallery NVARCHAR(MAX) = NULL,
    @YoutubeVideo NVARCHAR(MAX) = NULL,
    @Tags NVARCHAR(500) = NULL,
    @RelatedProducts NVARCHAR(500) = NULL,
    @Accessories NVARCHAR(500) = NULL,
    @IsHot BIT = 0,
    @IsNew BIT = 0,
    @IsBestSeller BIT = 0,
    @Status INT = 1,
    @SeoTitle NVARCHAR(500) = NULL,
    @SeoDescription NVARCHAR(MAX) = NULL,
    @SeoKeywords NVARCHAR(500) = NULL,
    @ShopeeLink NVARCHAR(500) = NULL,
    @LazadaLink NVARCHAR(500) = NULL,
    @TechnicalSpecs NVARCHAR(MAX) = NULL,
    @InstructionVideo NVARCHAR(MAX) = NULL,
    @BrandId INT = NULL,
    @WarrantyId INT = NULL,
    @ConditionId INT = NULL,
    @ExpectedDate DATETIME = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Products]
    SET [CategoryIds] = @CategoryIds,
        [SKU] = @SKU,
        [Name] = @Name,
        [Slug] = @Slug,
        [ShortDescription] = @ShortDescription,
        [DetailDescription] = @DetailDescription,
        [Price] = @Price,
        [OldPrice] = @OldPrice,
        [PurchasePrice] = @PurchasePrice,
        [Stock] = @Stock,
        [Unit] = @Unit,
        [Weight] = @Weight,
        [MainImage] = @MainImage,
        [ImageGallery] = @ImageGallery,
        [YoutubeVideo] = @YoutubeVideo,
        [Tags] = @Tags,
        [RelatedProducts] = @RelatedProducts,
        [Accessories] = @Accessories,
        [IsHot] = @IsHot,
        [IsNew] = @IsNew,
        [IsBestSeller] = @IsBestSeller,
        [Status] = @Status,
        [SeoTitle] = @SeoTitle,
        [SeoDescription] = @SeoDescription,
        [SeoKeywords] = @SeoKeywords,
        [ShopeeLink] = @ShopeeLink,
        [LazadaLink] = @LazadaLink,
        [TechnicalSpecs] = @TechnicalSpecs,
        [InstructionVideo] = @InstructionVideo,
        [BrandId] = @BrandId,
        [WarrantyId] = @WarrantyId,
        [ConditionId] = @ConditionId,
        [ExpectedDate] = @ExpectedDate,
        [UpdatedBy] = @UpdatedBy,
        [UpdatedAt] = GETDATE()
    WHERE [ProductId] = @ProductId;
END
GO

-- 15. THỦ TỤC TẠO ĐƠN HÀNG (BỔ SUNG DeliveryMethod, DeliveryNote, DeliveryChosen)
CREATE OR ALTER PROCEDURE [dbo].[SP_Orders_Insert]
    @OrderCode NVARCHAR(50),
    @CustomerId INT = NULL,
    @CustomerName NVARCHAR(255),
    @CustomerPhone NVARCHAR(50),
    @CustomerEmail NVARCHAR(255) = NULL,
    @ShippingAddress NVARCHAR(500),
    @WardId INT = NULL,
    @ProvinceId INT = NULL,
    @SubTotal DECIMAL(18,2) = 0,
    @ShippingFee DECIMAL(18,2) = 0,
    @Discount DECIMAL(18,2) = 0,
    @TotalAmount DECIMAL(18,2) = 0,
    @OrderStatus INT = 0,
    @PaymentMethod NVARCHAR(100) = 'COD',
    @PaymentStatus INT = 0,
    @CustomerNote NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(250) = NULL,
    @RequiresVAT BIT = 0,
    @VATCompanyName NVARCHAR(255) = NULL,
    @VATTaxCode NVARCHAR(50) = NULL,
    @VATCompanyAddress NVARCHAR(500) = NULL,
    @VATInvoiceEmail NVARCHAR(255) = NULL,
    @DeliveryMethod NVARCHAR(255) = NULL,
    @DeliveryNote NVARCHAR(500) = NULL,
    @DeliveryChosen BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @OrderCode IS NULL OR @OrderCode = ''
    BEGIN
        SET @OrderCode = 'ORD' + FORMAT(GETDATE(), 'yyMMdd') + CAST(CAST(RAND()*10000 AS INT) AS NVARCHAR(10));
    END

    INSERT INTO [dbo].[Orders]
    (
        [OrderCode], [CustomerId], [CustomerName], [CustomerPhone], [CustomerEmail],
        [ShippingAddress], [WardId], [ProvinceId], [SubTotal], [ShippingFee],
        [Discount], [TotalAmount], [OrderStatus], [PaymentMethod], [PaymentStatus],
        [CustomerNote], [CreatedAt], [UpdatedAt], [UpdatedBy],
        [RequiresVAT], [VATCompanyName], [VATTaxCode], [VATCompanyAddress], [VATInvoiceEmail],
        [DeliveryMethod], [DeliveryNote], [DeliveryChosen],
        [HubSent], [HubTries]
    )
    VALUES
    (
        @OrderCode, @CustomerId, @CustomerName, @CustomerPhone, @CustomerEmail,
        @ShippingAddress, @WardId, @ProvinceId, @SubTotal, @ShippingFee,
        @Discount, @TotalAmount, @OrderStatus, @PaymentMethod, @PaymentStatus,
        @CustomerNote, GETDATE(), GETDATE(), @CreatedBy,
        @RequiresVAT, @VATCompanyName, @VATTaxCode, @VATCompanyAddress, @VATInvoiceEmail,
        @DeliveryMethod, @DeliveryNote, @DeliveryChosen,
        0, 0
    );

    SELECT SCOPE_IDENTITY();
END
GO

-- 16. THỦ TỤC THÊM CHI TIẾT ĐƠN HÀNG (OrderItems)
CREATE OR ALTER PROCEDURE [dbo].[SP_OrderItems_Insert]
    @OrderId INT,
    @ProductId INT,
    @ProductName NVARCHAR(500),
    @ProductSKU NVARCHAR(100) = NULL,
    @ProductImage NVARCHAR(500) = NULL,
    @Price DECIMAL(18,2) = 0,
    @Quantity INT = 1,
    @TotalPrice DECIMAL(18,2) = 0
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [dbo].[OrderItems]
    (
        [OrderId], [ProductId], [ProductName], [ProductSKU], [ProductImage],
        [Price], [Quantity], [TotalPrice]
    )
    VALUES
    (
        @OrderId, @ProductId, @ProductName, @ProductSKU, @ProductImage,
        @Price, @Quantity, @TotalPrice
    );
END
GO

PRINT N'==============================================================================';
PRINT N'HOÀN TẤT TOÀN BỘ CẬP NHẬT CSDL CHO HỆ THỐNG HUB TRÊN GORDAK.VN!';
PRINT N'==============================================================================';
GO

