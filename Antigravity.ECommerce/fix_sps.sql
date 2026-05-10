USE GORDAK;
GO

-- Fix SP_Orders_GetById
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
            P.Name AS ProvinceName,
            W.Name AS WardName,
            C.MemberRank AS MemberRankName
        FROM Orders O
        LEFT JOIN Provinces P ON O.ProvinceId = P.ProvinceId
        LEFT JOIN Wards W ON O.WardId = W.WardId
        LEFT JOIN Customers C ON O.CustomerId = C.CustomerId
        WHERE O.OrderId = @OrderId;
    END
    ');
END
GO

-- Fix SP_Orders_Search
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
            P.Name AS ProvinceName,
            W.Name AS WardName,
            C.MemberRank AS MemberRankName,
            (SELECT COUNT(*) FROM OrderItems WHERE OrderId = O.OrderId) AS ItemCount,
            COUNT(*) OVER() AS TotalRows
        FROM Orders O
        LEFT JOIN Provinces P ON O.ProvinceId = P.ProvinceId
        LEFT JOIN Wards W ON O.WardId = W.WardId
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
