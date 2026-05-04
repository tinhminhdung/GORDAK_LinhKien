USE Antigravity_ECommerce;
GO

-- 1. SP_Categories_Search (Updated with CategoryType)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Categories_Search')
    DROP PROCEDURE SP_Categories_Search;
GO
CREATE PROCEDURE SP_Categories_Search
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

    SELECT *, COUNT(*) OVER() as TotalCount
    FROM Categories
    WHERE (@Keyword IS NULL OR Name LIKE '%' + @Keyword + '%' OR Slug LIKE '%' + @Keyword + '%')
      AND (@ParentId IS NULL OR ParentId = @ParentId)
      AND (@Status IS NULL OR Status = @Status)
      AND (@CategoryType IS NULL OR CategoryType = @CategoryType)
    ORDER BY 
        CASE WHEN @SortColumn = 'SortOrder' AND @SortOrder = 'ASC' THEN SortOrder END ASC,
        CASE WHEN @SortColumn = 'SortOrder' AND @SortOrder = 'DESC' THEN SortOrder END DESC,
        CASE WHEN @SortColumn = 'Name' AND @SortOrder = 'ASC' THEN Name END ASC,
        CASE WHEN @SortColumn = 'Name' AND @SortOrder = 'DESC' THEN Name END DESC,
        CASE WHEN @SortColumn = 'CreatedAt' AND @SortOrder = 'ASC' THEN CreatedAt END ASC,
        CASE WHEN @SortColumn = 'CreatedAt' AND @SortOrder = 'DESC' THEN CreatedAt END DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- 2. SP_Advertisings_Search
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Advertisings_Search')
    DROP PROCEDURE SP_Advertisings_Search;
GO
CREATE PROCEDURE SP_Advertisings_Search
    @Keyword NVARCHAR(250) = NULL,
    @Position NVARCHAR(100) = NULL,
    @Status INT = NULL,
    @PageIndex INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;

    SELECT *, COUNT(*) OVER() as TotalCount
    FROM Advertisings
    WHERE (@Keyword IS NULL OR Title LIKE '%' + @Keyword + '%')
      AND (@Position IS NULL OR Position = @Position)
      AND (@Status IS NULL OR Status = @Status)
    ORDER BY Position, SortOrder
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- 3. SP_Videos_Search
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Videos_Search')
    DROP PROCEDURE SP_Videos_Search;
GO
CREATE PROCEDURE SP_Videos_Search
    @Keyword NVARCHAR(250) = NULL,
    @Status INT = NULL,
    @CategoryId INT = NULL,
    @PageIndex INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;

    SELECT v.*, c.Name as CategoryName, COUNT(*) OVER() as TotalCount
    FROM Videos v
    LEFT JOIN Categories c ON v.CategoryId = c.CategoryId
    WHERE (@Keyword IS NULL OR v.Title LIKE '%' + @Keyword + '%')
      AND (@Status IS NULL OR v.Status = @Status)
      AND (@CategoryId IS NULL OR @CategoryId = 0 OR v.CategoryId = @CategoryId)
    ORDER BY v.SortOrder ASC, v.VideoId DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- 4. SP_Galleries_Search
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Galleries_Search')
    DROP PROCEDURE SP_Galleries_Search;
GO
CREATE PROCEDURE SP_Galleries_Search
    @Keyword NVARCHAR(250) = NULL,
    @Status INT = NULL,
    @CategoryId INT = NULL,
    @PageIndex INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;

    SELECT g.*, c.Name as CategoryName, COUNT(*) OVER() as TotalCount
    FROM Galleries g
    LEFT JOIN Categories c ON g.CategoryId = c.CategoryId
    WHERE (@Keyword IS NULL OR g.AlbumName LIKE '%' + @Keyword + '%')
      AND (@Status IS NULL OR g.Status = @Status)
      AND (@CategoryId IS NULL OR @CategoryId = 0 OR g.CategoryId = @CategoryId)
    ORDER BY g.SortOrder ASC, g.GalleryId DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- 5. SP_FAQs_Search
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_FAQs_Search')
    DROP PROCEDURE SP_FAQs_Search;
GO
CREATE PROCEDURE SP_FAQs_Search
    @Keyword NVARCHAR(250) = NULL,
    @Status INT = NULL,
    @CategoryId INT = NULL,
    @PageIndex INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;

    SELECT f.*, c.Name as CategoryName, COUNT(*) OVER() as TotalCount
    FROM FAQs f
    LEFT JOIN Categories c ON f.CategoryId = c.CategoryId
    WHERE (@Keyword IS NULL OR f.Question LIKE '%' + @Keyword + '%' OR f.Answer LIKE '%' + @Keyword + '%')
      AND (@Status IS NULL OR f.Status = @Status)
      AND (@CategoryId IS NULL OR @CategoryId = 0 OR f.CategoryId = @CategoryId)
    ORDER BY f.SortOrder ASC, f.FAQId DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- 6. SP_Documents_Search
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Documents_Search')
    DROP PROCEDURE SP_Documents_Search;
GO
CREATE PROCEDURE SP_Documents_Search
    @Keyword NVARCHAR(250) = NULL,
    @Status INT = NULL,
    @CategoryId INT = NULL,
    @PageIndex INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;

    SELECT d.*, c.Name as CategoryName, COUNT(*) OVER() as TotalCount
    FROM Documents d
    LEFT JOIN Categories c ON d.CategoryId = c.CategoryId
    WHERE (@Keyword IS NULL OR d.Title LIKE '%' + @Keyword + '%')
      AND (@Status IS NULL OR d.Status = @Status)
      AND (@CategoryId IS NULL OR @CategoryId = 0 OR d.CategoryId = @CategoryId)
    ORDER BY d.SortOrder, d.DocumentId DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- 7. SP_News_Search (Updated)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_News_Search')
    DROP PROCEDURE SP_News_Search;
GO
CREATE PROCEDURE SP_News_Search
    @Keyword NVARCHAR(250) = NULL,
    @CategoryId INT = NULL,
    @Status INT = NULL,
    @PageIndex INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;

    SELECT n.*, c.Name as CategoryName, COUNT(*) OVER() as TotalCount 
    FROM News n 
    LEFT JOIN Categories c ON n.CategoryId = c.CategoryId 
    WHERE (@Keyword IS NULL OR n.Title LIKE '%' + @Keyword + '%' OR n.ShortDescription LIKE '%' + @Keyword + '%')
      AND (@CategoryId IS NULL OR @CategoryId = 0 OR n.CategoryId = @CategoryId)
      AND (@Status IS NULL OR n.Status = @Status)
    ORDER BY n.SortOrder, n.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO
