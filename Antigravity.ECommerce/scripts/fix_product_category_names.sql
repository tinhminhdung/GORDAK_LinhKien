-- Fix Product Stored Procedures to return CategoryNames
-- Using STRING_AGG to combine multiple category names based on CategoryIds string

GO
CREATE OR ALTER PROCEDURE SP_Products_Search
    @Keyword NVARCHAR(200) = NULL,
    @CategoryIds NVARCHAR(500) = NULL,
    @Status INT = NULL,
    @IsHot BIT = NULL,
    @PriceMin DECIMAL(18,0) = NULL,
    @PriceMax DECIMAL(18,0) = NULL,
    @Ngay_Min DATETIME = NULL,
    @Ngay_Max DATETIME = NULL,
    @SortColumn NVARCHAR(50) = 'CreatedAt',
    @SortOrder NVARCHAR(10) = 'DESC',
    @PageIndex INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;
  
    ;WITH FilteredData AS (
        SELECT p.*, 
               (SELECT STRING_AGG(c.Name, ', ') 
                FROM STRING_SPLIT(p.CategoryIds, ',') s 
                JOIN Categories c ON LTRIM(RTRIM(s.value)) = CAST(c.CategoryId AS NVARCHAR(20))
               ) as CategoryNames,
               COUNT(*) OVER() as TotalCount
        FROM Products p
        WHERE (@Keyword IS NULL OR @Keyword = '' OR p.Name LIKE '%' + @Keyword + '%' OR p.SKU LIKE '%' + @Keyword + '%')
          AND (@CategoryIds IS NULL OR EXISTS (
              SELECT 1 
              FROM STRING_SPLIT(p.CategoryIds, ',') ps
              INNER JOIN STRING_SPLIT(@CategoryIds, ',') cs 
                  ON LTRIM(RTRIM(ps.value)) = LTRIM(RTRIM(cs.value))
          ))
          AND (@Status IS NULL OR p.Status = @Status)
          AND (@IsHot IS NULL OR p.IsHot = @IsHot)
          AND (@PriceMin IS NULL OR p.Price >= @PriceMin)
          AND (@PriceMax IS NULL OR p.Price <= @PriceMax)
          AND (@Ngay_Min IS NULL OR p.CreatedAt >= @Ngay_Min)
          AND (@Ngay_Max IS NULL OR p.CreatedAt <= @Ngay_Max)
    )
    SELECT * FROM FilteredData
    ORDER BY 
        CASE WHEN @SortOrder = 'ASC' THEN
            CASE 
                WHEN @SortColumn = 'Name' THEN Name
                WHEN @SortColumn = 'Price' THEN CAST(Price AS NVARCHAR(50))
                WHEN @SortColumn = 'CreatedAt' THEN CONVERT(NVARCHAR(50), CreatedAt, 126)
                WHEN @SortColumn = 'Stock' THEN CAST(Stock AS NVARCHAR(50))
                WHEN @SortColumn = 'Views' THEN CAST(Views AS NVARCHAR(50))
                WHEN @SortColumn = 'Sales' THEN CAST(Sales AS NVARCHAR(50))
            END
        END ASC,
        CASE WHEN @SortOrder = 'DESC' THEN
            CASE 
                WHEN @SortColumn = 'Name' THEN Name
                WHEN @SortColumn = 'Price' THEN CAST(Price AS NVARCHAR(50))
                WHEN @SortColumn = 'CreatedAt' THEN CONVERT(NVARCHAR(50), CreatedAt, 126)
                WHEN @SortColumn = 'Stock' THEN CAST(Stock AS NVARCHAR(50))
                WHEN @SortColumn = 'Views' THEN CAST(Views AS NVARCHAR(50))
                WHEN @SortColumn = 'Sales' THEN CAST(Sales AS NVARCHAR(50))
            END
        END DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

GO
CREATE OR ALTER PROCEDURE SP_Products_GetById
    @ProductId INT
AS
BEGIN
    SELECT p.*, 
           (SELECT STRING_AGG(c.Name, ', ') 
            FROM STRING_SPLIT(p.CategoryIds, ',') s 
            JOIN Categories c ON LTRIM(RTRIM(s.value)) = CAST(c.CategoryId AS NVARCHAR(20))
           ) as CategoryNames
    FROM Products p 
    WHERE p.ProductId = @ProductId;
END
GO

GO
CREATE OR ALTER PROCEDURE SP_Products_GetBySlug
    @Slug NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.*, 
           (SELECT STRING_AGG(c.Name, ', ') 
            FROM STRING_SPLIT(p.CategoryIds, ',') s 
            JOIN Categories c ON LTRIM(RTRIM(s.value)) = CAST(c.CategoryId AS NVARCHAR(20))
           ) as CategoryNames
    FROM Products p 
    WHERE p.Slug = @Slug;
END
GO
