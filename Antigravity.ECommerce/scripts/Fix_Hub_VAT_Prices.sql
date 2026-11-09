-- ==============================================================================
-- KỊCH BẢN CẬP NHẬT DATABASE NGÀY 09/11/2026 (CHỈ CÁC MỤC SỬA HÔM NAY)
-- Áp dụng cho: CSDL LIVE CẢ 2 TRANG UNI-TREND.VN & GORDAK.VN
-- Nội dung sửa:
--   1. Cập nhật lại thủ tục [dbo].[SP_Hub_SyncProduct]:
--      Dữ liệu @Price do HUB đẩy xuống là Giá CHƯA VAT -> Lưu vào [OldPrice],
--      tự động tính [Price] = Giá sau VAT 8% (ROUND(@Price * 1.08, 0)).
--   2. Chuẩn hóa dữ liệu cũ: Cập nhật lại Giá sau VAT 8% cho các sản phẩm
--      đang bị trùng giá (Price = OldPrice) do lần đồng bộ trước từ HUB.
-- ==============================================================================

PRINT N'==============================================================================';
PRINT N'1. CẬP NHẬT LẠI STORED PROCEDURE: [dbo].[SP_Hub_SyncProduct] (NGÀY 09/11/2026)';
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

-- 2.1 Cập nhật Price = OldPrice * 1.08 cho các sản phẩm đang bị Price = OldPrice
UPDATE [dbo].[Products]
SET [Price] = ROUND([OldPrice] * 1.08, 0),
    [UpdatedAt] = GETDATE()
WHERE [OldPrice] > 0 AND ([Price] = [OldPrice] OR [Price] = 0);

PRINT N'-> Đã tính lại Price (Sau VAT 8%) cho ' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + N' sản phẩm bị trùng giá.';
GO

-- 2.2 Bổ sung OldPrice = Price / 1.08 cho các sản phẩm có Price nhưng thiếu OldPrice
UPDATE [dbo].[Products]
SET [OldPrice] = ROUND([Price] / 1.08, 0),
    [UpdatedAt] = GETDATE()
WHERE [Price] > 0 AND ([OldPrice] IS NULL OR [OldPrice] = 0);

PRINT N'-> Đã tính lại OldPrice (Chưa VAT) cho ' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + N' sản phẩm thiếu giá chưa VAT.';
GO

PRINT N'==============================================================================';
PRINT N'3. KIỂM TRA MẪU CÁC SẢN PHẨM SAU KHI CẬP NHẬT GIÁ';
PRINT N'==============================================================================';
GO

-- Hiển thị kiểm tra các sản phẩm tiêu biểu (ví dụ NM00115 và 10 SP khác)
SELECT TOP 10 
    [SKU], 
    [Name] AS [Tên sản phẩm], 
    [OldPrice] AS [Giá (Chưa VAT)], 
    [Price] AS [Giá bán (Sau VAT 8%)],
    CAST(ROUND([Price] - [OldPrice], 0) AS DECIMAL(18,0)) AS [Tiền thuế VAT 8%],
    [Stock] AS [Tồn kho],
    [Unit] AS [ĐVT]
FROM [dbo].[Products]
WHERE [OldPrice] > 0
ORDER BY [UpdatedAt] DESC, [ProductId] DESC;
GO

PRINT N'==============================================================================';
PRINT N'✅ HOÀN TẤT CẬP NHẬT CSDL HÔM NAY (09/11/2026)!';
PRINT N'==============================================================================';
GO
