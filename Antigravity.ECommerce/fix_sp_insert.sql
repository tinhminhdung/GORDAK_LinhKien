USE GORDAK;
GO

IF OBJECT_ID('SP_Orders_Insert', 'P') IS NOT NULL
BEGIN
    EXEC('
    ALTER PROCEDURE [dbo].[SP_Orders_Insert]
        @OrderCode NVARCHAR(50),
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

        IF @OrderCode IS NULL OR @OrderCode = ''''
        BEGIN
            SET @OrderCode = ''ORD'' + FORMAT(GETDATE(), ''yyMMdd'') + CAST(CAST(RAND()*10000 AS INT) AS NVARCHAR(10));
        END

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
