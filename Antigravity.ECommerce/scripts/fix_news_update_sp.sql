USE Antigravity_ECommerce;
GO

-- Force update SP_News_Update to accept all potential parameters from SNews.GetParameters
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_News_Update') DROP PROCEDURE SP_News_Update;
GO
CREATE PROCEDURE SP_News_Update
    @NewsId INT,
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
    @CreatedBy NVARCHAR(100) = NULL, -- Accept but ignore in update
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    UPDATE News SET 
        CategoryId=@CategoryId, Title=@Title, Slug=@Slug, Image=@Image, 
        ShortDescription=@ShortDescription, DetailDescription=@DetailDescription, 
        Tags=@Tags, SortOrder=@SortOrder, Status=@Status, IsHot=@IsHot, 
        SeoTitle=@SeoTitle, SeoDescription=@SeoDescription, SeoKeywords=@SeoKeywords, 
        UpdatedBy=@UpdatedBy, UpdatedAt=GETDATE()
    WHERE NewsId=@NewsId;
END;
GO
