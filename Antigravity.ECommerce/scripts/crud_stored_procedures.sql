USE Antigravity_ECommerce;
GO

-- =============================================
-- MODULE: ADVERTISING
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Advertisings_GetAll') DROP PROCEDURE SP_Advertisings_GetAll;
GO
CREATE PROCEDURE SP_Advertisings_GetAll AS
BEGIN
    SELECT * FROM Advertisings ORDER BY Position, SortOrder;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Advertisings_GetById') DROP PROCEDURE SP_Advertisings_GetById;
GO
CREATE PROCEDURE SP_Advertisings_GetById @Id INT AS
BEGIN
    SELECT * FROM Advertisings WHERE AdvertisingId = @Id;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Advertisings_Insert') DROP PROCEDURE SP_Advertisings_Insert;
GO
CREATE PROCEDURE SP_Advertisings_Insert
    @Title NVARCHAR(250), @Image NVARCHAR(500), @VideoUrl NVARCHAR(500), @Link NVARCHAR(500),
    @Position NVARCHAR(100), @SortOrder INT, @Status INT, @Target NVARCHAR(50),
    @StartDate DATETIME, @EndDate DATETIME, @CreatedBy NVARCHAR(100)
AS
BEGIN
    INSERT INTO Advertisings (Title, Image, VideoUrl, Link, Position, SortOrder, Status, Target, StartDate, EndDate, CreatedBy, CreatedAt)
    VALUES (@Title, @Image, @VideoUrl, @Link, @Position, @SortOrder, @Status, @Target, @StartDate, @EndDate, @CreatedBy, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Advertisings_Update') DROP PROCEDURE SP_Advertisings_Update;
GO
CREATE PROCEDURE SP_Advertisings_Update
    @AdvertisingId INT, @Title NVARCHAR(250), @Image NVARCHAR(500), @VideoUrl NVARCHAR(500), @Link NVARCHAR(500),
    @Position NVARCHAR(100), @SortOrder INT, @Status INT, @Target NVARCHAR(50),
    @StartDate DATETIME, @EndDate DATETIME, @UpdatedBy NVARCHAR(100)
AS
BEGIN
    UPDATE Advertisings SET Title=@Title, Image=@Image, VideoUrl=@VideoUrl, Link=@Link, 
           Position=@Position, SortOrder=@SortOrder, Status=@Status, Target=@Target, 
           StartDate=@StartDate, EndDate=@EndDate, UpdatedBy=@UpdatedBy, UpdatedAt=GETDATE()
    WHERE AdvertisingId=@AdvertisingId;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Advertisings_Delete') DROP PROCEDURE SP_Advertisings_Delete;
GO
CREATE PROCEDURE SP_Advertisings_Delete @Id INT AS
BEGIN
    DELETE FROM Advertisings WHERE AdvertisingId = @Id;
END;
GO

-- =============================================
-- MODULE: NEWS
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_News_GetAll') DROP PROCEDURE SP_News_GetAll;
GO
CREATE PROCEDURE SP_News_GetAll @Top INT = 0 AS
BEGIN
    IF @Top > 0
        SELECT TOP (@Top) n.*, c.Name as CategoryName FROM News n LEFT JOIN Categories c ON n.CategoryId = c.CategoryId ORDER BY n.SortOrder, n.CreatedAt DESC;
    ELSE
        SELECT n.*, c.Name as CategoryName FROM News n LEFT JOIN Categories c ON n.CategoryId = c.CategoryId ORDER BY n.SortOrder, n.CreatedAt DESC;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_News_GetById') DROP PROCEDURE SP_News_GetById;
GO
CREATE PROCEDURE SP_News_GetById @Id INT AS
BEGIN
    SELECT n.*, c.Name as CategoryName FROM News n LEFT JOIN Categories c ON n.CategoryId = c.CategoryId WHERE n.NewsId = @Id;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_News_GetBySlug') DROP PROCEDURE SP_News_GetBySlug;
GO
CREATE PROCEDURE SP_News_GetBySlug @Slug NVARCHAR(250) AS
BEGIN
    SELECT n.*, c.Name as CategoryName FROM News n LEFT JOIN Categories c ON n.CategoryId = c.CategoryId WHERE n.Slug = @Slug;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_News_Insert') DROP PROCEDURE SP_News_Insert;
GO
CREATE PROCEDURE SP_News_Insert
    @CategoryId INT, @Title NVARCHAR(250), @Slug NVARCHAR(250), @Image NVARCHAR(500), 
    @ShortDescription NVARCHAR(MAX), @DetailDescription NVARCHAR(MAX), @Tags NVARCHAR(500), 
    @SortOrder INT, @Status INT, @IsHot BIT, @SeoTitle NVARCHAR(250), 
    @SeoDescription NVARCHAR(500), @SeoKeywords NVARCHAR(500), @CreatedBy NVARCHAR(100)
AS
BEGIN
    INSERT INTO News (CategoryId, Title, Slug, Image, ShortDescription, DetailDescription, Tags, SortOrder, Status, IsHot, SeoTitle, SeoDescription, SeoKeywords, CreatedBy, CreatedAt)
    VALUES (@CategoryId, @Title, @Slug, @Image, @ShortDescription, @DetailDescription, @Tags, @SortOrder, @Status, @IsHot, @SeoTitle, @SeoDescription, @SeoKeywords, @CreatedBy, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_News_Update') DROP PROCEDURE SP_News_Update;
GO
CREATE PROCEDURE SP_News_Update
    @NewsId INT, @CategoryId INT, @Title NVARCHAR(250), @Slug NVARCHAR(250), @Image NVARCHAR(500), 
    @ShortDescription NVARCHAR(MAX), @DetailDescription NVARCHAR(MAX), @Tags NVARCHAR(500), 
    @SortOrder INT, @Status INT, @IsHot BIT, @SeoTitle NVARCHAR(250), 
    @SeoDescription NVARCHAR(500), @SeoKeywords NVARCHAR(500), @UpdatedBy NVARCHAR(100)
AS
BEGIN
    UPDATE News SET CategoryId=@CategoryId, Title=@Title, Slug=@Slug, Image=@Image, 
           ShortDescription=@ShortDescription, DetailDescription=@DetailDescription, 
           Tags=@Tags, SortOrder=@SortOrder, Status=@Status, IsHot=@IsHot, 
           SeoTitle=@SeoTitle, SeoDescription=@SeoDescription, SeoKeywords=@SeoKeywords, 
           UpdatedBy=@UpdatedBy, UpdatedAt=GETDATE()
    WHERE NewsId=@NewsId;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_News_Delete') DROP PROCEDURE SP_News_Delete;
GO
CREATE PROCEDURE SP_News_Delete @Id INT AS
BEGIN
    DELETE FROM News WHERE NewsId = @Id;
END;
GO

-- =============================================
-- MODULE: VIDEOS
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Videos_GetAll') DROP PROCEDURE SP_Videos_GetAll;
GO
CREATE PROCEDURE SP_Videos_GetAll AS
BEGIN
    SELECT * FROM Videos ORDER BY SortOrder ASC, VideoId DESC;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Videos_GetById') DROP PROCEDURE SP_Videos_GetById;
GO
CREATE PROCEDURE SP_Videos_GetById @Id INT AS
BEGIN
    SELECT * FROM Videos WHERE VideoId = @Id;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Videos_Insert') DROP PROCEDURE SP_Videos_Insert;
GO
CREATE PROCEDURE SP_Videos_Insert
    @Title NVARCHAR(250), @YoutubeId NVARCHAR(100), @ThumbnailUrl NVARCHAR(500),
    @SortOrder INT, @Status INT, @CreatedBy NVARCHAR(100)
AS
BEGIN
    INSERT INTO Videos (Title, YoutubeId, ThumbnailUrl, SortOrder, Status, CreatedBy, CreatedAt)
    VALUES (@Title, @YoutubeId, @ThumbnailUrl, @SortOrder, @Status, @CreatedBy, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Videos_Update') DROP PROCEDURE SP_Videos_Update;
GO
CREATE PROCEDURE SP_Videos_Update
    @VideoId INT, @Title NVARCHAR(250), @YoutubeId NVARCHAR(100), @ThumbnailUrl NVARCHAR(500),
    @SortOrder INT, @Status INT
AS
BEGIN
    UPDATE Videos SET Title=@Title, YoutubeId=@YoutubeId, ThumbnailUrl=@ThumbnailUrl, 
           SortOrder=@SortOrder, Status=@Status
    WHERE VideoId=@VideoId;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Videos_Delete') DROP PROCEDURE SP_Videos_Delete;
GO
CREATE PROCEDURE SP_Videos_Delete @Id INT AS
BEGIN
    DELETE FROM Videos WHERE VideoId = @Id;
END;
GO

-- =============================================
-- MODULE: GALLERIES
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Galleries_GetAll') DROP PROCEDURE SP_Galleries_GetAll;
GO
CREATE PROCEDURE SP_Galleries_GetAll AS
BEGIN
    SELECT * FROM Galleries ORDER BY SortOrder ASC, GalleryId DESC;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Galleries_GetById') DROP PROCEDURE SP_Galleries_GetById;
GO
CREATE PROCEDURE SP_Galleries_GetById @Id INT AS
BEGIN
    SELECT * FROM Galleries WHERE GalleryId = @Id;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Galleries_Insert') DROP PROCEDURE SP_Galleries_Insert;
GO
CREATE PROCEDURE SP_Galleries_Insert
    @AlbumName NVARCHAR(250), @CoverImage NVARCHAR(500), @Images NVARCHAR(MAX),
    @SortOrder INT, @Status INT, @CreatedBy NVARCHAR(100)
AS
BEGIN
    INSERT INTO Galleries (AlbumName, CoverImage, Images, SortOrder, Status, CreatedBy, CreatedAt)
    VALUES (@AlbumName, @CoverImage, @Images, @SortOrder, @Status, @CreatedBy, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Galleries_Update') DROP PROCEDURE SP_Galleries_Update;
GO
CREATE PROCEDURE SP_Galleries_Update
    @GalleryId INT, @AlbumName NVARCHAR(250), @CoverImage NVARCHAR(500), @Images NVARCHAR(MAX),
    @SortOrder INT, @Status INT
AS
BEGIN
    UPDATE Galleries SET AlbumName=@AlbumName, CoverImage=@CoverImage, Images=@Images, 
           SortOrder=@SortOrder, Status=@Status
    WHERE GalleryId=@GalleryId;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Galleries_Delete') DROP PROCEDURE SP_Galleries_Delete;
GO
CREATE PROCEDURE SP_Galleries_Delete @Id INT AS
BEGIN
    DELETE FROM Galleries WHERE GalleryId = @Id;
END;
GO

-- =============================================
-- MODULE: FAQs
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_FAQs_GetAll') DROP PROCEDURE SP_FAQs_GetAll;
GO
CREATE PROCEDURE SP_FAQs_GetAll AS
BEGIN
    SELECT * FROM FAQs ORDER BY SortOrder ASC, FAQId DESC;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_FAQs_GetById') DROP PROCEDURE SP_FAQs_GetById;
GO
CREATE PROCEDURE SP_FAQs_GetById @Id INT AS
BEGIN
    SELECT * FROM FAQs WHERE FAQId = @Id;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_FAQs_Insert') DROP PROCEDURE SP_FAQs_Insert;
GO
CREATE PROCEDURE SP_FAQs_Insert
    @Question NVARCHAR(MAX), @Answer NVARCHAR(MAX), @SortOrder INT, @Status INT
AS
BEGIN
    INSERT INTO FAQs (Question, Answer, SortOrder, Status, CreatedAt)
    VALUES (@Question, @Answer, @SortOrder, @Status, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_FAQs_Update') DROP PROCEDURE SP_FAQs_Update;
GO
CREATE PROCEDURE SP_FAQs_Update
    @FAQId INT, @Question NVARCHAR(MAX), @Answer NVARCHAR(MAX), @SortOrder INT, @Status INT
AS
BEGIN
    UPDATE FAQs SET Question=@Question, Answer=@Answer, SortOrder=@SortOrder, Status=@Status
    WHERE FAQId=@FAQId;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_FAQs_Delete') DROP PROCEDURE SP_FAQs_Delete;
GO
CREATE PROCEDURE SP_FAQs_Delete @Id INT AS
BEGIN
    DELETE FROM FAQs WHERE FAQId = @Id;
END;
GO

-- =============================================
-- MODULE: DOCUMENTS
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Documents_GetAll') DROP PROCEDURE SP_Documents_GetAll;
GO
CREATE PROCEDURE SP_Documents_GetAll AS
BEGIN
    SELECT * FROM Documents ORDER BY SortOrder, DocumentId DESC;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Documents_GetById') DROP PROCEDURE SP_Documents_GetById;
GO
CREATE PROCEDURE SP_Documents_GetById @Id INT AS
BEGIN
    SELECT * FROM Documents WHERE DocumentId = @Id;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Documents_Insert') DROP PROCEDURE SP_Documents_Insert;
GO
CREATE PROCEDURE SP_Documents_Insert
    @Title NVARCHAR(500), @FilePath NVARCHAR(500), @FileSize NVARCHAR(50), 
    @SortOrder INT, @Status INT
AS
BEGIN
    INSERT INTO Documents (Title, FilePath, FileSize, SortOrder, Status, CreatedAt)
    VALUES (@Title, @FilePath, @FileSize, @SortOrder, @Status, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Documents_Update') DROP PROCEDURE SP_Documents_Update;
GO
CREATE PROCEDURE SP_Documents_Update
    @DocumentId INT, @Title NVARCHAR(500), @FilePath NVARCHAR(500), @FileSize NVARCHAR(50), 
    @SortOrder INT, @Status INT
AS
BEGIN
    UPDATE Documents SET Title=@Title, FilePath=@FilePath, FileSize=@FileSize, 
           SortOrder=@SortOrder, Status=@Status
    WHERE DocumentId=@DocumentId;
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Documents_Delete') DROP PROCEDURE SP_Documents_Delete;
GO
CREATE PROCEDURE SP_Documents_Delete @Id INT AS
BEGIN
    DELETE FROM Documents WHERE DocumentId = @Id;
END;
GO
