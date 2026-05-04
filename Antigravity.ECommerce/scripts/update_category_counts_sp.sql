-- Update Stored Procedures to include ItemCount (ProductCount or NewsCount)
-- Products uses CategoryIds (comma-separated), News uses CategoryId (int)

ALTER PROCEDURE SP_Categories_GetAll
AS
BEGIN
    SELECT c.*,
           (CASE 
               WHEN CategoryType = 1 THEN (SELECT COUNT(*) FROM Products p WHERE ',' + ISNULL(p.CategoryIds, '') + ',' LIKE '%,' + CAST(c.CategoryId AS NVARCHAR(10)) + ',%')
               WHEN CategoryType = 2 THEN (SELECT COUNT(*) FROM News n WHERE n.CategoryId = c.CategoryId)
               ELSE 0 
           END) AS ItemCount
    FROM Categories c
    ORDER BY SortOrder ASC, CreatedAt DESC;
END;
GO

ALTER PROCEDURE SP_Categories_Search
    @Keyword NVARCHAR(250) = NULL,
    @ParentId INT = NULL,
    @Status INT = NULL,
    @CategoryType INT = NULL,
    @SortColumn NVARCHAR(50) = 'SortOrder',
    @SortOrder NVARCHAR(10) = 'ASC',
    @PageIndex INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;

    SELECT c.*, 
           COUNT(*) OVER() as TotalCount,
           (CASE 
               WHEN c.CategoryType = 1 THEN (SELECT COUNT(*) FROM Products p WHERE ',' + ISNULL(p.CategoryIds, '') + ',' LIKE '%,' + CAST(c.CategoryId AS NVARCHAR(10)) + ',%')
               WHEN c.CategoryType = 2 THEN (SELECT COUNT(*) FROM News n WHERE n.CategoryId = c.CategoryId)
               ELSE 0 
           END) AS ItemCount
    FROM Categories c
    WHERE (@Keyword IS NULL OR c.Name LIKE '%' + @Keyword + '%' OR c.Slug LIKE '%' + @Keyword + '%')
      AND (@ParentId IS NULL OR c.ParentId = @ParentId)
      AND (@Status IS NULL OR c.Status = @Status)
      AND (@CategoryType IS NULL OR c.CategoryType = @CategoryType)
    ORDER BY 
        CASE WHEN @SortColumn = 'SortOrder' AND @SortOrder = 'ASC' THEN c.SortOrder END ASC,
        CASE WHEN @SortColumn = 'SortOrder' AND @SortOrder = 'DESC' THEN c.SortOrder END DESC,
        CASE WHEN @SortColumn = 'Name' AND @SortOrder = 'ASC' THEN c.Name END ASC,
        CASE WHEN @SortColumn = 'Name' AND @SortOrder = 'DESC' THEN c.Name END DESC,
        CASE WHEN @SortColumn = 'CreatedAt' AND @SortOrder = 'ASC' THEN c.CreatedAt END ASC,
        CASE WHEN @SortColumn = 'CreatedAt' AND @SortOrder = 'DESC' THEN c.CreatedAt END DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO
