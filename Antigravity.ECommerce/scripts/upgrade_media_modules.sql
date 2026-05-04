USE Antigravity_ECommerce;
GO

-- 1. NÂNG CẤP CẤU TRÚC BẢNG (Bổ sung các trường SEO và Audit nếu chưa có)

-- Bảng Videos
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Videos') AND name = 'Slug')
    ALTER TABLE Videos ADD Slug NVARCHAR(250);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Videos') AND name = 'CategoryId')
    ALTER TABLE Videos ADD CategoryId INT;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Videos') AND name = 'SeoTitle')
    ALTER TABLE Videos ADD SeoTitle NVARCHAR(250);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Videos') AND name = 'SeoDescription')
    ALTER TABLE Videos ADD SeoDescription NVARCHAR(500);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Videos') AND name = 'SeoKeywords')
    ALTER TABLE Videos ADD SeoKeywords NVARCHAR(500);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Videos') AND name = 'UpdatedBy')
    ALTER TABLE Videos ADD UpdatedBy NVARCHAR(100);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Videos') AND name = 'UpdatedAt')
    ALTER TABLE Videos ADD UpdatedAt DATETIME;
GO

-- Bảng Galleries
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Galleries') AND name = 'Description')
    ALTER TABLE Galleries ADD Description NVARCHAR(MAX);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Galleries') AND name = 'Slug')
    ALTER TABLE Galleries ADD Slug NVARCHAR(250);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Galleries') AND name = 'CategoryId')
    ALTER TABLE Galleries ADD CategoryId INT;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Galleries') AND name = 'SeoTitle')
    ALTER TABLE Galleries ADD SeoTitle NVARCHAR(250);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Galleries') AND name = 'SeoDescription')
    ALTER TABLE Galleries ADD SeoDescription NVARCHAR(500);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Galleries') AND name = 'SeoKeywords')
    ALTER TABLE Galleries ADD SeoKeywords NVARCHAR(500);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Galleries') AND name = 'UpdatedBy')
    ALTER TABLE Galleries ADD UpdatedBy NVARCHAR(100);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Galleries') AND name = 'UpdatedAt')
    ALTER TABLE Galleries ADD UpdatedAt DATETIME;
GO

-- Bảng FAQs
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FAQs') AND name = 'Slug')
    ALTER TABLE FAQs ADD Slug NVARCHAR(250);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FAQs') AND name = 'CategoryId')
    ALTER TABLE FAQs ADD CategoryId INT;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FAQs') AND name = 'SeoTitle')
    ALTER TABLE FAQs ADD SeoTitle NVARCHAR(250);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FAQs') AND name = 'SeoDescription')
    ALTER TABLE FAQs ADD SeoDescription NVARCHAR(500);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FAQs') AND name = 'SeoKeywords')
    ALTER TABLE FAQs ADD SeoKeywords NVARCHAR(500);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FAQs') AND name = 'CreatedBy')
    ALTER TABLE FAQs ADD CreatedBy NVARCHAR(100);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FAQs') AND name = 'UpdatedBy')
    ALTER TABLE FAQs ADD UpdatedBy NVARCHAR(100);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FAQs') AND name = 'UpdatedAt')
    ALTER TABLE FAQs ADD UpdatedAt DATETIME;
GO

-- Bảng Documents
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'Slug')
    ALTER TABLE Documents ADD Slug NVARCHAR(250);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'CategoryId')
    ALTER TABLE Documents ADD CategoryId INT;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'SeoTitle')
    ALTER TABLE Documents ADD SeoTitle NVARCHAR(250);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'SeoDescription')
    ALTER TABLE Documents ADD SeoDescription NVARCHAR(500);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'SeoKeywords')
    ALTER TABLE Documents ADD SeoKeywords NVARCHAR(500);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'CreatedBy')
    ALTER TABLE Documents ADD CreatedBy NVARCHAR(100);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'UpdatedBy')
    ALTER TABLE Documents ADD UpdatedBy NVARCHAR(100);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'UpdatedAt')
    ALTER TABLE Documents ADD UpdatedAt DATETIME;
GO

-- 2. CẬP NHẬT STORED PROCEDURES (Đồng bộ tham số với Service)

-- VIDEOS
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Videos_Insert') DROP PROCEDURE SP_Videos_Insert;
GO
CREATE PROCEDURE SP_Videos_Insert
    @Title NVARCHAR(250), @YoutubeId NVARCHAR(100), @Slug NVARCHAR(250), @ThumbnailUrl NVARCHAR(500) = NULL,
    @CategoryId INT, @SortOrder INT, @Status INT, @SeoTitle NVARCHAR(250) = NULL,
    @SeoDescription NVARCHAR(500) = NULL, @SeoKeywords NVARCHAR(500) = NULL, @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    INSERT INTO Videos (Title, YoutubeId, Slug, ThumbnailUrl, CategoryId, SortOrder, Status, SeoTitle, SeoDescription, SeoKeywords, CreatedBy, CreatedAt)
    VALUES (@Title, @YoutubeId, @Slug, @ThumbnailUrl, @CategoryId, @SortOrder, @Status, @SeoTitle, @SeoDescription, @SeoKeywords, @CreatedBy, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Videos_Update') DROP PROCEDURE SP_Videos_Update;
GO
CREATE PROCEDURE SP_Videos_Update
    @VideoId INT, @Title NVARCHAR(250), @YoutubeId NVARCHAR(100), @Slug NVARCHAR(250), @ThumbnailUrl NVARCHAR(500) = NULL,
    @CategoryId INT, @SortOrder INT, @Status INT, @SeoTitle NVARCHAR(250) = NULL,
    @SeoDescription NVARCHAR(500) = NULL, @SeoKeywords NVARCHAR(500) = NULL, @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    UPDATE Videos SET Title=@Title, YoutubeId=@YoutubeId, Slug=@Slug, ThumbnailUrl=@ThumbnailUrl, 
           CategoryId=@CategoryId, SortOrder=@SortOrder, Status=@Status, SeoTitle=@SeoTitle, 
           SeoDescription=@SeoDescription, SeoKeywords=@SeoKeywords, UpdatedBy=@UpdatedBy, UpdatedAt=GETDATE()
    WHERE VideoId=@VideoId;
END;
GO

-- GALLERIES
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Galleries_Insert') DROP PROCEDURE SP_Galleries_Insert;
GO
CREATE PROCEDURE SP_Galleries_Insert
    @AlbumName NVARCHAR(250), @Slug NVARCHAR(250), @CoverImage NVARCHAR(500) = NULL, @Description NVARCHAR(MAX) = NULL,
    @Images NVARCHAR(MAX) = NULL, @CategoryId INT, @SortOrder INT, @Status INT, @SeoTitle NVARCHAR(250) = NULL,
    @SeoDescription NVARCHAR(500) = NULL, @SeoKeywords NVARCHAR(500) = NULL, @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    INSERT INTO Galleries (AlbumName, Slug, CoverImage, [Description], Images, CategoryId, SortOrder, Status, SeoTitle, SeoDescription, SeoKeywords, CreatedBy, CreatedAt)
    VALUES (@AlbumName, @Slug, @CoverImage, @Description, @Images, @CategoryId, @SortOrder, @Status, @SeoTitle, @SeoDescription, @SeoKeywords, @CreatedBy, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Galleries_Update') DROP PROCEDURE SP_Galleries_Update;
GO
CREATE PROCEDURE SP_Galleries_Update
    @GalleryId INT, @AlbumName NVARCHAR(250), @Slug NVARCHAR(250), @CoverImage NVARCHAR(500) = NULL, @Description NVARCHAR(MAX) = NULL,
    @Images NVARCHAR(MAX) = NULL, @CategoryId INT, @SortOrder INT, @Status INT, @SeoTitle NVARCHAR(250) = NULL,
    @SeoDescription NVARCHAR(500) = NULL, @SeoKeywords NVARCHAR(500) = NULL, @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    UPDATE Galleries SET AlbumName=@AlbumName, Slug=@Slug, CoverImage=@CoverImage, [Description]=@Description, 
           Images=@Images, CategoryId=@CategoryId, SortOrder=@SortOrder, Status=@Status, SeoTitle=@SeoTitle, 
           SeoDescription=@SeoDescription, SeoKeywords=@SeoKeywords, UpdatedBy=@UpdatedBy, UpdatedAt=GETDATE()
    WHERE GalleryId=@GalleryId;
END;
GO

-- FAQs
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_FAQs_Insert') DROP PROCEDURE SP_FAQs_Insert;
GO
CREATE PROCEDURE SP_FAQs_Insert
    @Question NVARCHAR(MAX), @Answer NVARCHAR(MAX), @Slug NVARCHAR(250), @CategoryId INT, 
    @SortOrder INT, @Status INT, @SeoTitle NVARCHAR(250) = NULL,
    @SeoDescription NVARCHAR(500) = NULL, @SeoKeywords NVARCHAR(500) = NULL, @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    INSERT INTO FAQs (Question, Answer, Slug, CategoryId, SortOrder, Status, SeoTitle, SeoDescription, SeoKeywords, CreatedBy, CreatedAt)
    VALUES (@Question, @Answer, @Slug, @CategoryId, @SortOrder, @Status, @SeoTitle, @SeoDescription, @SeoKeywords, @CreatedBy, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_FAQs_Update') DROP PROCEDURE SP_FAQs_Update;
GO
CREATE PROCEDURE SP_FAQs_Update
    @FAQId INT, @Question NVARCHAR(MAX), @Answer NVARCHAR(MAX), @Slug NVARCHAR(250), @CategoryId INT, 
    @SortOrder INT, @Status INT, @SeoTitle NVARCHAR(250) = NULL,
    @SeoDescription NVARCHAR(500) = NULL, @SeoKeywords NVARCHAR(500) = NULL, @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    UPDATE FAQs SET Question=@Question, Answer=@Answer, Slug=@Slug, CategoryId=@CategoryId, 
           SortOrder=@SortOrder, Status=@Status, SeoTitle=@SeoTitle, SeoDescription=@SeoDescription, 
           SeoKeywords=@SeoKeywords, UpdatedBy=@UpdatedBy, UpdatedAt=GETDATE()
    WHERE FAQId=@FAQId;
END;
GO

-- DOCUMENTS
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Documents_Insert') DROP PROCEDURE SP_Documents_Insert;
GO
CREATE PROCEDURE SP_Documents_Insert
    @Title NVARCHAR(500), @FilePath NVARCHAR(500), @FileSize NVARCHAR(50) = NULL,
    @Slug NVARCHAR(250), @CategoryId INT, @SortOrder INT, @Status INT, @SeoTitle NVARCHAR(250) = NULL,
    @SeoDescription NVARCHAR(500) = NULL, @SeoKeywords NVARCHAR(500) = NULL, @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    INSERT INTO Documents (Title, FilePath, FileSize, Slug, CategoryId, SortOrder, Status, SeoTitle, SeoDescription, SeoKeywords, CreatedBy, CreatedAt)
    VALUES (@Title, @FilePath, @FileSize, @Slug, @CategoryId, @SortOrder, @Status, @SeoTitle, @SeoDescription, @SeoKeywords, @CreatedBy, GETDATE());
    SELECT SCOPE_IDENTITY();
END;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Documents_Update') DROP PROCEDURE SP_Documents_Update;
GO
CREATE PROCEDURE SP_Documents_Update
    @DocumentId INT, @Title NVARCHAR(500), @FilePath NVARCHAR(500), @FileSize NVARCHAR(50) = NULL,
    @Slug NVARCHAR(250), @CategoryId INT, @SortOrder INT, @Status INT, @SeoTitle NVARCHAR(250) = NULL,
    @SeoDescription NVARCHAR(500) = NULL, @SeoKeywords NVARCHAR(500) = NULL, @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    UPDATE Documents SET Title=@Title, FilePath=@FilePath, FileSize=@FileSize, Slug=@Slug, 
           CategoryId=@CategoryId, SortOrder=@SortOrder, Status=@Status, SeoTitle=@SeoTitle, 
           SeoDescription=@SeoDescription, SeoKeywords=@SeoKeywords, UpdatedBy=@UpdatedBy, UpdatedAt=GETDATE()
    WHERE DocumentId=@DocumentId;
END;
GO
