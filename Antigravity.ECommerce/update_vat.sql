USE GORDAK;
GO

-- 1. Add VAT columns to Orders table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'RequiresVAT')
BEGIN
    ALTER TABLE [dbo].[Orders] ADD RequiresVAT BIT NOT NULL DEFAULT(0);
    ALTER TABLE [dbo].[Orders] ADD VATCompanyName NVARCHAR(250) NULL;
    ALTER TABLE [dbo].[Orders] ADD VATTaxCode NVARCHAR(50) NULL;
    ALTER TABLE [dbo].[Orders] ADD VATCompanyAddress NVARCHAR(500) NULL;
    ALTER TABLE [dbo].[Orders] ADD VATInvoiceEmail NVARCHAR(250) NULL;
END
GO

-- 2. Update SP_Orders_Insert
IF OBJECT_ID('SP_Orders_Insert', 'P') IS NOT NULL
BEGIN
    EXEC('
    ALTER PROCEDURE [dbo].[SP_Orders_Insert]
        @CustomerId INT = NULL,
        @CustomerName NVARCHAR(250),
        @CustomerPhone NVARCHAR(50),
        @CustomerEmail NVARCHAR(250) = NULL,
        @ShippingAddress NVARCHAR(500),
        @WardId INT = NULL,
        @ProvinceId INT = NULL,
        @SubTotal DECIMAL(18,2),
        @ShippingFee DECIMAL(18,2),
        @Discount DECIMAL(18,2),
        @TotalAmount DECIMAL(18,2),
        @OrderStatus INT = 0,
        @PaymentMethod NVARCHAR(50) = ''COD'',
        @PaymentStatus INT = 0,
        @CustomerNote NVARCHAR(MAX) = NULL,
        @CreatedBy NVARCHAR(250) = NULL,
        @RequiresVAT BIT = 0,
        @VATCompanyName NVARCHAR(250) = NULL,
        @VATTaxCode NVARCHAR(50) = NULL,
        @VATCompanyAddress NVARCHAR(500) = NULL,
        @VATInvoiceEmail NVARCHAR(250) = NULL
    AS
    BEGIN
        SET NOCOUNT ON;

        DECLARE @OrderCode NVARCHAR(50);
        SET @OrderCode = ''ORD'' + FORMAT(GETDATE(), ''yyMMdd'') + CAST(NEXT VALUE FOR OrderCodeSeq AS NVARCHAR(10));

        INSERT INTO Orders (
            OrderCode, CustomerId, CustomerName, CustomerPhone, CustomerEmail,
            ShippingAddress, WardId, ProvinceId, SubTotal, ShippingFee, Discount,
            TotalAmount, OrderStatus, PaymentMethod, PaymentStatus, CustomerNote,
            CreatedAt, UpdatedAt, UpdatedBy, RequiresVAT, VATCompanyName, VATTaxCode, VATCompanyAddress, VATInvoiceEmail
        )
        VALUES (
            @OrderCode, @CustomerId, @CustomerName, @CustomerPhone, @CustomerEmail,
            @ShippingAddress, @WardId, @ProvinceId, @SubTotal, @ShippingFee, @Discount,
            @TotalAmount, @OrderStatus, @PaymentMethod, @PaymentStatus, @CustomerNote,
            GETDATE(), GETDATE(), @CreatedBy, @RequiresVAT, @VATCompanyName, @VATTaxCode, @VATCompanyAddress, @VATInvoiceEmail
        );

        SELECT SCOPE_IDENTITY();
    END
    ');
END
GO

-- 3. Update SP_Orders_GetById
IF OBJECT_ID('SP_Orders_GetById', 'P') IS NOT NULL
BEGIN
    EXEC('
    ALTER PROCEDURE [dbo].[SP_Orders_GetById]
        @OrderId INT
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT 
            O.*,
            P.ten_Tinh AS ProvinceName,
            W.ten_Xa AS WardName,
            C.RankName AS MemberRankName
        FROM Orders O
        LEFT JOIN Tinh_ThanhPho P ON O.ProvinceId = P.ma_Tinh
        LEFT JOIN Phuong_Xa W ON O.WardId = W.ma_Xa
        LEFT JOIN Customers C ON O.CustomerId = C.CustomerId
        WHERE O.OrderId = @OrderId;
    END
    ');
END
GO

-- 4. Update SP_Orders_Search
IF OBJECT_ID('SP_Orders_Search', 'P') IS NOT NULL
BEGIN
    EXEC('
    ALTER PROCEDURE [dbo].[SP_Orders_Search]
        @Keyword NVARCHAR(250) = NULL,
        @OrderStatus INT = NULL,
        @PaymentStatus INT = NULL,
        @ProvinceId INT = NULL,
        @WardId INT = NULL,
        @Ngay_Min DATETIME = NULL,
        @Ngay_Max DATETIME = NULL,
        @SortColumn NVARCHAR(50) = ''CreatedAt'',
        @SortOrder NVARCHAR(4) = ''DESC'',
        @PageIndex INT = 1,
        @PageSize INT = 20
    AS
    BEGIN
        SET NOCOUNT ON;

        DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;

        SELECT 
            O.*,
            P.ten_Tinh AS ProvinceName,
            W.ten_Xa AS WardName,
            C.RankName AS MemberRankName,
            (SELECT COUNT(*) FROM OrderItems WHERE OrderId = O.OrderId) AS ItemCount,
            COUNT(*) OVER() AS TotalRows
        FROM Orders O
        LEFT JOIN Tinh_ThanhPho P ON O.ProvinceId = P.ma_Tinh
        LEFT JOIN Phuong_Xa W ON O.WardId = W.ma_Xa
        LEFT JOIN Customers C ON O.CustomerId = C.CustomerId
        WHERE 
            (@Keyword IS NULL OR O.OrderCode LIKE N''%'' + @Keyword + ''%'' OR O.CustomerName LIKE N''%'' + @Keyword + ''%'' OR O.CustomerPhone LIKE N''%'' + @Keyword + ''%'')
            AND (@OrderStatus IS NULL OR O.OrderStatus = @OrderStatus)
            AND (@PaymentStatus IS NULL OR O.PaymentStatus = @PaymentStatus)
            AND (@ProvinceId IS NULL OR O.ProvinceId = @ProvinceId)
            AND (@WardId IS NULL OR O.WardId = @WardId)
            AND (@Ngay_Min IS NULL OR CAST(O.CreatedAt AS DATE) >= CAST(@Ngay_Min AS DATE))
            AND (@Ngay_Max IS NULL OR CAST(O.CreatedAt AS DATE) <= CAST(@Ngay_Max AS DATE))
        ORDER BY
            CASE WHEN @SortColumn = ''CreatedAt'' AND @SortOrder = ''ASC'' THEN O.CreatedAt END ASC,
            CASE WHEN @SortColumn = ''CreatedAt'' AND @SortOrder = ''DESC'' THEN O.CreatedAt END DESC,
            CASE WHEN @SortColumn = ''TotalAmount'' AND @SortOrder = ''ASC'' THEN O.TotalAmount END ASC,
            CASE WHEN @SortColumn = ''TotalAmount'' AND @SortOrder = ''DESC'' THEN O.TotalAmount END DESC,
            CASE WHEN @SortColumn = ''OrderCode'' AND @SortOrder = ''ASC'' THEN O.OrderCode END ASC,
            CASE WHEN @SortColumn = ''OrderCode'' AND @SortOrder = ''DESC'' THEN O.OrderCode END DESC
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END
    ');
END
GO
