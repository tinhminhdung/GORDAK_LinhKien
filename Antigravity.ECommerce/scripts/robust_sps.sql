USE Antigravity_ECommerce;
GO

-- 1. NEWS INSERT
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_News_Insert') DROP PROCEDURE SP_News_Insert;
GO
CREATE PROCEDURE SP_News_Insert
    @NewsId INT = 0, -- Accept but ignore
    @CategoryId INT,
    @Title NVARCHAR(250),
    @Slug NVARCHAR(250),
    @Image NVARCHAR(500) = NULL, 
    @ShortDescription NVARCHAR(MAX) = NULL,
    @DetailDescription NVARCHAR(MAX) = NULL,
    @Tags NVARCHAR(500) = NULL, 
    @SortOrder INT = 0,
    @Status INT = 1,
    @IsHot BIT = 0,
    @SeoTitle NVARCHAR(250) = NULL, 
    @SeoDescription NVARCHAR(500) = NULL,
    @SeoKeywords NVARCHAR(500) = NULL,
    @CreatedBy NVARCHAR(100) = NULL,
    @UpdatedBy NVARCHAR(100) = NULL -- Accept but ignore in insert
AS
BEGIN
    INSERT INTO News (CategoryId, Title, Slug, Image, ShortDescription, DetailDescription, Tags, SortOrder, Status, IsHot, SeoTitle, SeoDescription, SeoKeywords, CreatedBy, CreatedAt)
    VALUES (@CategoryId, @Title, @Slug, @Image, @ShortDescription, @DetailDescription, @Tags, @SortOrder, @Status, @IsHot, @SeoTitle, @SeoDescription, @SeoKeywords, @CreatedBy, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

-- 2. CATEGORY UPDATE (already has SP_Categories_Update, but let's ensure it's robust)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Categories_Update') DROP PROCEDURE SP_Categories_Update;
GO
CREATE PROCEDURE SP_Categories_Update
    @CategoryId INT,
    @ParentId INT = 0,
    @Name NVARCHAR(250),
    @Slug NVARCHAR(250),
    @Description NVARCHAR(MAX) = NULL,
    @Content NVARCHAR(MAX) = NULL,
    @Image NVARCHAR(500) = NULL,
    @ImageAlt NVARCHAR(250) = NULL,
    @SortOrder INT = 0,
    @Status INT = 1,
    @SeoTitle NVARCHAR(250) = NULL,
    @SeoDescription NVARCHAR(500) = NULL,
    @SeoKeywords NVARCHAR(500) = NULL,
    @Banner NVARCHAR(500) = NULL,
    @Icon NVARCHAR(100) = NULL,
    @CategoryType INT = 1,
    @LinkType INT = 1,
    @Url NVARCHAR(500) = NULL,
    @Target NVARCHAR(50) = '_self',
    @MenuPosition NVARCHAR(100) = NULL,
    @CreatedBy NVARCHAR(100) = NULL, -- Accept but ignore
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    UPDATE Categories SET 
        ParentId=@ParentId, Name=@Name, Slug=@Slug, Description=@Description, 
        Content=@Content, Image=@Image, ImageAlt=@ImageAlt, SortOrder=@SortOrder, 
        Status=@Status, SeoTitle=@SeoTitle, SeoDescription=@SeoDescription, 
        SeoKeywords=@SeoKeywords, Banner=@Banner, Icon=@Icon, 
        CategoryType=@CategoryType, LinkType=@LinkType, [Url]=@Url, Target=@Target, 
        MenuPosition=@MenuPosition, UpdatedBy=@UpdatedBy, UpdatedAt=GETDATE()
    WHERE CategoryId=@CategoryId;
END;
GO
