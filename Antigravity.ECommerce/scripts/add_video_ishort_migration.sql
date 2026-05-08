USE Antigravity_ECommerce;
GO

-- ============================================================
-- MIGRATION: Thêm cột IsHot vào bảng Videos
-- Ngày: 2026-05-08
-- ============================================================

-- 1. Thêm cột IsHot nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Videos' AND COLUMN_NAME = 'IsHot')
BEGIN
    ALTER TABLE Videos ADD IsHot BIT NOT NULL DEFAULT 0;
    PRINT 'Da them cot IsHot vao bang Videos.';
END
ELSE
BEGIN
    PRINT 'Cot IsHot da ton tai trong bang Videos.';
END
GO

-- 2. Thêm cột CategoryId nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Videos' AND COLUMN_NAME = 'CategoryId')
BEGIN
    ALTER TABLE Videos ADD CategoryId INT NOT NULL DEFAULT 0;
    PRINT 'Da them cot CategoryId vao bang Videos.';
END
GO

-- 3. Thêm cột Slug nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Videos' AND COLUMN_NAME = 'Slug')
BEGIN
    ALTER TABLE Videos ADD Slug NVARCHAR(250) NULL;
    PRINT 'Da them cot Slug vao bang Videos.';
END
GO

-- 4. Thêm cột UpdatedBy nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Videos' AND COLUMN_NAME = 'UpdatedBy')
BEGIN
    ALTER TABLE Videos ADD UpdatedBy NVARCHAR(100) NULL;
    PRINT 'Da them cot UpdatedBy vao bang Videos.';
END
GO

-- 5. Thêm cột UpdatedAt nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Videos' AND COLUMN_NAME = 'UpdatedAt')
BEGIN
    ALTER TABLE Videos ADD UpdatedAt DATETIME NULL;
    PRINT 'Da them cot UpdatedAt vao bang Videos.';
END
GO

-- 6. Thêm cột SeoTitle nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Videos' AND COLUMN_NAME = 'SeoTitle')
BEGIN
    ALTER TABLE Videos ADD SeoTitle NVARCHAR(250) NULL;
END
GO

-- 7. Thêm cột SeoDescription nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Videos' AND COLUMN_NAME = 'SeoDescription')
BEGIN
    ALTER TABLE Videos ADD SeoDescription NVARCHAR(500) NULL;
END
GO

-- 8. Thêm cột SeoKeywords nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Videos' AND COLUMN_NAME = 'SeoKeywords')
BEGIN
    ALTER TABLE Videos ADD SeoKeywords NVARCHAR(500) NULL;
END
GO

-- ============================================================
-- CẬP NHẬT STORED PROCEDURES
-- ============================================================

-- SP_Videos_GetAll: JOIN CategoryName + trả về IsHot
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Videos_GetAll') DROP PROCEDURE SP_Videos_GetAll;
GO
CREATE PROCEDURE SP_Videos_GetAll AS
BEGIN
    SELECT v.*, c.Name as CategoryName
    FROM Videos v
    LEFT JOIN Categories c ON v.CategoryId = c.CategoryId
    ORDER BY v.SortOrder ASC, v.VideoId DESC;
END;
GO

-- SP_Videos_GetById: JOIN CategoryName
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Videos_GetById') DROP PROCEDURE SP_Videos_GetById;
GO
CREATE PROCEDURE SP_Videos_GetById @Id INT AS
BEGIN
    SELECT v.*, c.Name as CategoryName
    FROM Videos v
    LEFT JOIN Categories c ON v.CategoryId = c.CategoryId
    WHERE v.VideoId = @Id;
END;
GO

-- SP_Videos_GetBySlug: JOIN CategoryName
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Videos_GetBySlug') DROP PROCEDURE SP_Videos_GetBySlug;
GO
CREATE PROCEDURE SP_Videos_GetBySlug @Slug NVARCHAR(250) AS
BEGIN
    SELECT v.*, c.Name as CategoryName
    FROM Videos v
    LEFT JOIN Categories c ON v.CategoryId = c.CategoryId
    WHERE v.Slug = @Slug;
END;
GO

-- SP_Videos_Insert: Thêm CategoryId, Slug, IsHot, SeoTitle/Desc/Keywords
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Videos_Insert') DROP PROCEDURE SP_Videos_Insert;
GO
CREATE PROCEDURE SP_Videos_Insert
    @Title NVARCHAR(250),
    @YoutubeId NVARCHAR(100),
    @Slug NVARCHAR(250) = NULL,
    @ThumbnailUrl NVARCHAR(500) = NULL,
    @CategoryId INT = 0,
    @SortOrder INT = 0,
    @Status INT = 1,
    @IsHot BIT = 0,
    @SeoTitle NVARCHAR(250) = NULL,
    @SeoDescription NVARCHAR(500) = NULL,
    @SeoKeywords NVARCHAR(500) = NULL,
    @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    INSERT INTO Videos (Title, YoutubeId, Slug, ThumbnailUrl, CategoryId, SortOrder, Status, IsHot, SeoTitle, SeoDescription, SeoKeywords, CreatedBy, CreatedAt)
    VALUES (@Title, @YoutubeId, @Slug, @ThumbnailUrl, @CategoryId, @SortOrder, @Status, @IsHot, @SeoTitle, @SeoDescription, @SeoKeywords, @CreatedBy, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

-- SP_Videos_Update: Thêm CategoryId, Slug, IsHot, SeoTitle/Desc/Keywords, UpdatedBy
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Videos_Update') DROP PROCEDURE SP_Videos_Update;
GO
CREATE PROCEDURE SP_Videos_Update
    @VideoId INT,
    @Title NVARCHAR(250),
    @YoutubeId NVARCHAR(100),
    @Slug NVARCHAR(250) = NULL,
    @ThumbnailUrl NVARCHAR(500) = NULL,
    @CategoryId INT = 0,
    @SortOrder INT = 0,
    @Status INT = 1,
    @IsHot BIT = 0,
    @SeoTitle NVARCHAR(250) = NULL,
    @SeoDescription NVARCHAR(500) = NULL,
    @SeoKeywords NVARCHAR(500) = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    UPDATE Videos
    SET Title = @Title,
        YoutubeId = @YoutubeId,
        Slug = @Slug,
        ThumbnailUrl = @ThumbnailUrl,
        CategoryId = @CategoryId,
        SortOrder = @SortOrder,
        Status = @Status,
        IsHot = @IsHot,
        SeoTitle = @SeoTitle,
        SeoDescription = @SeoDescription,
        SeoKeywords = @SeoKeywords,
        UpdatedBy = @UpdatedBy,
        UpdatedAt = GETDATE()
    WHERE VideoId = @VideoId;
END;
GO

-- SP_Videos_Search: Thêm SortColumn/SortOrder parameter, trả về IsHot + CategoryName
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Videos_Search') DROP PROCEDURE SP_Videos_Search;
GO
CREATE PROCEDURE SP_Videos_Search
    @Keyword NVARCHAR(250) = NULL,
    @Status INT = NULL,
    @CategoryId INT = NULL,
    @SortColumn NVARCHAR(50) = 'CreatedAt',
    @SortOrder NVARCHAR(10) = 'DESC',
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
    ORDER BY
        CASE WHEN @SortColumn = 'Title'     AND @SortOrder = 'ASC'  THEN v.Title     END ASC,
        CASE WHEN @SortColumn = 'Title'     AND @SortOrder = 'DESC' THEN v.Title     END DESC,
        CASE WHEN @SortColumn = 'SortOrder' AND @SortOrder = 'ASC'  THEN v.SortOrder END ASC,
        CASE WHEN @SortColumn = 'SortOrder' AND @SortOrder = 'DESC' THEN v.SortOrder END DESC,
        CASE WHEN @SortColumn = 'Status'    AND @SortOrder = 'ASC'  THEN v.Status    END ASC,
        CASE WHEN @SortColumn = 'Status'    AND @SortOrder = 'DESC' THEN v.Status    END DESC,
        CASE WHEN @SortColumn = 'CreatedAt' AND @SortOrder = 'ASC'  THEN v.CreatedAt END ASC,
        v.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

PRINT 'Migration hoan thanh. Da them IsHot vao Videos va cap nhat tat ca Stored Procedures.';
GO
