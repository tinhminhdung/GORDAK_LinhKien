/* ==============================================================================
   BỔ SUNG: HUB GỬI THÊM TÊN SẢN PHẨM (18/11/2026)
   Áp dụng cho: gordak.vn (Database: [GORDAK])
   File kịch bản: scripts/Fix_Hub_ProductName.sql
   ============================================================================== */

PRINT N'-> Đang cập nhật Stored Procedure [dbo].[SP_Hub_SyncProduct]...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_Hub_SyncProduct]
    @SKU NVARCHAR(50),
    @Stock INT,
    @Price DECIMAL(18,0), /* Giá do HUB gửi xuống là GIÁ CHƯA VAT */
    @Unit NVARCHAR(50) = NULL,
    @ExpectedDate DATE = NULL,
    @Name NVARCHAR(300) = NULL /* Bổ sung ngày 18/11/2026: Tên sản phẩm trên HUB */
AS
BEGIN
    SET NOCOUNT ON;

    /* =========================================================================
       CHANGE LOG:
       Ngày sửa: 2026-11-19
       Người sửa: Coder / Antigravity
       Lý do sửa: Bổ sung cập nhật trường [Name] (Tên sản phẩm) đồng bộ từ HUB theo
                  thông báo ngày 18/11/2026. Nếu @Name rỗng hoặc quá 300 ký tự thì
                  giữ tên cũ. Giữ nguyên đường dẫn (Slug).
       Ngày sửa: 2026-11-09
       Người sửa: Coder / Antigravity
       Lý do sửa: Dữ liệu @Price từ HUB đẩy xuống là Giá CHƯA VAT. Cập nhật đúng
                  trường [OldPrice] = Giá chưa VAT, và tính [Price] = Giá sau VAT (8%)
                  để tránh lỗi giá VAT và chưa VAT bị trùng nhau trên website.
       ========================================================================= */

    /* Nếu có tồn (>0) thì ExpectedDate tự động set NULL theo quy tắc nghiệp vụ HUB */
    IF @Stock > 0
    BEGIN
        SET @ExpectedDate = NULL;
    END

    /* [START OLD CODE - 2026-11-19]
    UPDATE [dbo].[Products]
    SET [Stock] = @Stock,
        [OldPrice] = CASE WHEN @Price > 0 THEN @Price ELSE [OldPrice] END,
        [Price] = CASE WHEN @Price > 0 THEN ROUND(@Price * 1.08, 0) ELSE [Price] END,
        [Unit] = CASE WHEN @Unit IS NOT NULL AND RTRIM(@Unit) <> '' THEN @Unit ELSE [Unit] END,
        [ExpectedDate] = @ExpectedDate,
        [Name] = CASE WHEN @Name IS NOT NULL AND LTRIM(RTRIM(@Name)) <> '' AND LEN(@Name) <= 300 THEN LTRIM(RTRIM(@Name)) ELSE [Name] END,
        [UpdatedAt] = GETDATE()
    WHERE [SKU] = @SKU;
    [END OLD CODE] */

    UPDATE [dbo].[Products]
    SET [Stock] = @Stock,
        [OldPrice] = CASE WHEN @Price > 0 THEN @Price ELSE [OldPrice] END,
        [Price] = CASE WHEN @Price > 0 THEN ROUND(@Price * 1.08, 0) ELSE [Price] END,
        [Unit] = CASE WHEN @Unit IS NOT NULL AND RTRIM(@Unit) <> '' THEN @Unit ELSE [Unit] END,
        [ExpectedDate] = @ExpectedDate,
        [Name] = CASE WHEN @Name IS NOT NULL AND LTRIM(RTRIM(@Name)) <> '' AND LEN(@Name) <= 300 THEN LTRIM(RTRIM(@Name)) ELSE [Name] END,
        [UpdatedAt] = GETDATE()
    WHERE [SKU] = @SKU;

    /* Trả về số dòng cập nhật (1 = tìm thấy & cập nhật, 0 = không tìm thấy SKU) */
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT N'-> Đã cập nhật thành công [dbo].[SP_Hub_SyncProduct] tiếp nhận trường @Name!';
GO
