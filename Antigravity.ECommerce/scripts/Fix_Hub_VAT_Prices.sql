-- ==============================================================================
-- KỊCH BẢN KHẮC PHỤC LỖI GIÁ HUB VÀ CHUẨN HÓA GIÁ VAT (8%)
-- Ngày thực hiện: 2026-11-09
-- Áp dụng cho: CSDL UNI-TREND & GORDAK
-- ==============================================================================

PRINT N'==============================================================================';
PRINT N'1. CẬP NHẬT LẠI THỦ TỤC ĐỒNG BỘ HUB: [dbo].[SP_Hub_SyncProduct]';
PRINT N'==============================================================================';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_Hub_SyncProduct]
    @SKU NVARCHAR(50),
    @Stock INT,
    @Price DECIMAL(18,0), -- Giá do HUB gửi xuống là GIÁ CHƯA VAT
    @Unit NVARCHAR(50) = NULL,
    @ExpectedDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================================
    -- CHANGE LOG:
    -- Ngày sửa: 2026-11-09
    -- Người sửa: Coder / Antigravity
    -- Lý do sửa: Dữ liệu @Price từ HUB đẩy xuống là Giá CHƯA VAT. Cập nhật đúng
    --            trường [OldPrice] = Giá chưa VAT, và tính [Price] = Giá sau VAT (8%)
    --            để tránh lỗi giá VAT và chưa VAT bị trùng nhau trên website.
    -- =========================================================================

    -- Nếu có tồn (>0) thì ExpectedDate tự động set NULL theo quy tắc nghiệp vụ HUB
    IF @Stock > 0
    BEGIN
        SET @ExpectedDate = NULL;
    END

    /* [START OLD CODE - 2026-11-09]
    UPDATE [dbo].[Products]
    SET [Stock] = @Stock,
        [Price] = CASE WHEN @Price > 0 THEN @Price ELSE [Price] END,
        [Unit] = CASE WHEN @Unit IS NOT NULL AND RTRIM(@Unit) <> '' THEN @Unit ELSE [Unit] END,
        [ExpectedDate] = @ExpectedDate,
        [UpdatedAt] = GETDATE()
    WHERE [SKU] = @SKU;
    [END OLD CODE] */

    UPDATE [dbo].[Products]
    SET [Stock] = @Stock,
        [OldPrice] = CASE WHEN @Price > 0 THEN @Price ELSE [OldPrice] END,
        [Price] = CASE WHEN @Price > 0 THEN ROUND(@Price * 1.08, 0) ELSE [Price] END,
        [Unit] = CASE WHEN @Unit IS NOT NULL AND RTRIM(@Unit) <> '' THEN @Unit ELSE [Unit] END,
        [ExpectedDate] = @ExpectedDate,
        [UpdatedAt] = GETDATE()
    WHERE [SKU] = @SKU;

    -- Trả về số dòng cập nhật (1 = tìm thấy & cập nhật, 0 = không tìm thấy SKU)
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT N'-> Đã cập nhật SP_Hub_SyncProduct thành công.';
GO

PRINT N'==============================================================================';
PRINT N'2. CHUẨN HÓA DỮ LIỆU CŨ: CẬP NHẬT GIÁ BÁN SAU VAT (8%) CHO CÁC SẢN PHẨM BỊ TRÙNG';
PRINT N'==============================================================================';
GO

-- Trường hợp 1: Sản phẩm có OldPrice > 0 và Price bị trùng đúng bằng OldPrice (hoặc Price = 0)
UPDATE [dbo].[Products]
SET [Price] = ROUND([OldPrice] * 1.08, 0),
    [UpdatedAt] = GETDATE()
WHERE [OldPrice] > 0 AND ([Price] = [OldPrice] OR [Price] = 0);

PRINT N'-> Đã cập nhật Price = ROUND(OldPrice * 1.08, 0) cho các sản phẩm bị trùng giá.';
GO

-- Trường hợp 2: Sản phẩm có Price > 0 nhưng OldPrice = 0 (bị thiếu giá chưa VAT)
UPDATE [dbo].[Products]
SET [OldPrice] = ROUND([Price] / 1.08, 0),
    [UpdatedAt] = GETDATE()
WHERE [Price] > 0 AND ([OldPrice] IS NULL OR [OldPrice] = 0);

PRINT N'-> Đã cập nhật OldPrice = ROUND(Price / 1.08, 0) cho các sản phẩm thiếu giá chưa VAT.';
GO

SELECT COUNT(1) AS TotalProducts,
       SUM(CASE WHEN [Price] > [OldPrice] AND [OldPrice] > 0 THEN 1 ELSE 0 END) AS ProductsWithCorrectVat,
       SUM(CASE WHEN [Price] = [OldPrice] AND [Price] > 0 THEN 1 ELSE 0 END) AS ProductsWithSamePrice
FROM [dbo].[Products];
GO

