USE Antigravity_ECommerce;
GO

-- RE-DEFINE SPs to be more flexible with extra parameters passed from C#

-- ADVERTISING
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Advertisings_Insert') DROP PROCEDURE SP_Advertisings_Insert;
GO
CREATE PROCEDURE SP_Advertisings_Insert
    @AdvertisingId INT = 0, -- Accept but ignore
    @Title NVARCHAR(250), @Image NVARCHAR(500) = NULL, @VideoUrl NVARCHAR(500) = NULL, @Link NVARCHAR(500) = NULL,
    @Position NVARCHAR(100) = NULL, @SortOrder INT = 0, @Status INT = 1, @Target NVARCHAR(50) = '_self',
    @StartDate DATETIME = NULL, @EndDate DATETIME = NULL, @CreatedBy NVARCHAR(100) = NULL, @UpdatedBy NVARCHAR(100) = NULL -- Accept but ignore
AS
BEGIN
    INSERT INTO Advertisings (Title, Image, VideoUrl, Link, Position, SortOrder, Status, Target, StartDate, EndDate, CreatedBy, CreatedAt)
    VALUES (@Title, @Image, @VideoUrl, @Link, @Position, @SortOrder, @Status, @Target, @StartDate, @EndDate, @CreatedBy, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

-- NEWS
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_News_Insert') DROP PROCEDURE SP_News_Insert;
GO
CREATE PROCEDURE SP_News_Insert
    @NewsId INT = 0, -- Ignore
    @CategoryId INT, @Title NVARCHAR(250), @Slug NVARCHAR(250), @Image NVARCHAR(500) = NULL, 
    @ShortDescription NVARCHAR(MAX) = NULL, @DetailDescription NVARCHAR(MAX) = NULL, @Tags NVARCHAR(500) = NULL, 
    @SortOrder INT = 0, @Status INT = 1, @IsHot BIT = 0, @SeoTitle NVARCHAR(250) = NULL, 
    @SeoDescription NVARCHAR(500) = NULL, @SeoKeywords NVARCHAR(500) = NULL, 
    @CreatedBy NVARCHAR(100) = NULL, @UpdatedBy NVARCHAR(100) = NULL -- Ignore
AS
BEGIN
    INSERT INTO News (CategoryId, Title, Slug, Image, ShortDescription, DetailDescription, Tags, SortOrder, Status, IsHot, SeoTitle, SeoDescription, SeoKeywords, CreatedBy, CreatedAt)
    VALUES (@CategoryId, @Title, @Slug, @Image, @ShortDescription, @DetailDescription, @Tags, @SortOrder, @Status, @IsHot, @SeoTitle, @SeoDescription, @SeoKeywords, @CreatedBy, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

-- VIDEOS
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Videos_Insert') DROP PROCEDURE SP_Videos_Insert;
GO
CREATE PROCEDURE SP_Videos_Insert
    @VideoId INT = 0,
    @Title NVARCHAR(250), @YoutubeId NVARCHAR(100), @ThumbnailUrl NVARCHAR(500) = NULL,
    @SortOrder INT = 0, @Status INT = 1, @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    INSERT INTO Videos (Title, YoutubeId, ThumbnailUrl, SortOrder, Status, CreatedBy, CreatedAt)
    VALUES (@Title, @YoutubeId, @ThumbnailUrl, @SortOrder, @Status, @CreatedBy, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

-- GALLERIES
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Galleries_Insert') DROP PROCEDURE SP_Galleries_Insert;
GO
CREATE PROCEDURE SP_Galleries_Insert
    @GalleryId INT = 0,
    @AlbumName NVARCHAR(250), @CoverImage NVARCHAR(500) = NULL, @Images NVARCHAR(MAX) = NULL,
    @SortOrder INT = 0, @Status INT = 1, @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    INSERT INTO Galleries (AlbumName, CoverImage, Images, SortOrder, Status, CreatedBy, CreatedAt)
    VALUES (@AlbumName, @CoverImage, @Images, @SortOrder, @Status, @CreatedBy, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

-- FAQS
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_FAQs_Insert') DROP PROCEDURE SP_FAQs_Insert;
GO
CREATE PROCEDURE SP_FAQs_Insert
    @FAQId INT = 0,
    @Question NVARCHAR(MAX), @Answer NVARCHAR(MAX), @SortOrder INT = 0, @Status INT = 1
AS
BEGIN
    INSERT INTO FAQs (Question, Answer, SortOrder, Status, CreatedAt)
    VALUES (@Question, @Answer, @SortOrder, @Status, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

-- DOCUMENTS
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Documents_Insert') DROP PROCEDURE SP_Documents_Insert;
GO
CREATE PROCEDURE SP_Documents_Insert
    @DocumentId INT = 0,
    @Title NVARCHAR(500), @FilePath NVARCHAR(500), @FileSize NVARCHAR(50) = NULL, 
    @SortOrder INT = 0, @Status INT = 1
AS
BEGIN
    INSERT INTO Documents (Title, FilePath, FileSize, SortOrder, Status, CreatedAt)
    VALUES (@Title, @FilePath, @FileSize, @SortOrder, @Status, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO
