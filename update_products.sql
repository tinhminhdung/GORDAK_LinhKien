USE [GORDAK]
GO

-- 1. Add Columns to Table
IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'ShopeeLink' AND Object_ID = Object_ID(N'Products'))
BEGIN
    ALTER TABLE Products ADD ShopeeLink NVARCHAR(500) NULL
END
GO

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'LazadaLink' AND Object_ID = Object_ID(N'Products'))
BEGIN
    ALTER TABLE Products ADD LazadaLink NVARCHAR(500) NULL
END
GO

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'TechnicalSpecs' AND Object_ID = Object_ID(N'Products'))
BEGIN
    ALTER TABLE Products ADD TechnicalSpecs NVARCHAR(MAX) NULL
END
GO

-- 2. Alter Insert Procedure
ALTER PROCEDURE [dbo].[SP_Products_Insert]
    @CategoryIds NVARCHAR(255),
    @SKU NVARCHAR(50),
    @Name NVARCHAR(255),
    @Slug NVARCHAR(255),
    @ShortDescription NVARCHAR(500),
    @DetailDescription NVARCHAR(MAX),
    @Price DECIMAL(18,0),
    @OldPrice DECIMAL(18,0),
    @PurchasePrice DECIMAL(18,0),
    @Stock INT,
    @Unit NVARCHAR(50),
    @Weight FLOAT,
    @MainImage NVARCHAR(255),
    @ImageGallery NVARCHAR(MAX),
    @YoutubeVideo NVARCHAR(1000),
    @Tags NVARCHAR(500),
    @RelatedProducts NVARCHAR(500),
    @IsHot BIT,
    @IsNew BIT,
    @IsBestSeller BIT,
    @Status TINYINT,
    @SeoTitle NVARCHAR(255),
    @SeoDescription NVARCHAR(500),
    @SeoKeywords NVARCHAR(255),
    @ShopeeLink NVARCHAR(500) = NULL,
    @LazadaLink NVARCHAR(500) = NULL,
    @TechnicalSpecs NVARCHAR(MAX) = NULL,
    @CreatedBy INT
AS
BEGIN
    INSERT INTO Products(
        CategoryIds, SKU, Name, Slug, ShortDescription, DetailDescription,
        Price, OldPrice, PurchasePrice, Stock, Unit, Weight,
        MainImage, ImageGallery, YoutubeVideo, Tags, RelatedProducts,
        IsHot, IsNew, IsBestSeller, Status,
        SeoTitle, SeoDescription, SeoKeywords, 
        ShopeeLink, LazadaLink, TechnicalSpecs, CreatedBy
    )
    VALUES(
        @CategoryIds, @SKU, @Name, @Slug, @ShortDescription, @DetailDescription,
        @Price, @OldPrice, @PurchasePrice, @Stock, @Unit, @Weight,
        @MainImage, @ImageGallery, @YoutubeVideo, @Tags, @RelatedProducts,
        @IsHot, @IsNew, @IsBestSeller, @Status,
        @SeoTitle, @SeoDescription, @SeoKeywords,
        @ShopeeLink, @LazadaLink, @TechnicalSpecs, @CreatedBy
    );
    SELECT SCOPE_IDENTITY();
END
GO

-- 3. Alter Update Procedure
ALTER PROCEDURE [dbo].[SP_Products_Update]
    @ProductId INT,
    @CategoryIds NVARCHAR(255),
    @SKU NVARCHAR(50),
    @Name NVARCHAR(255),
    @Slug NVARCHAR(255),
    @ShortDescription NVARCHAR(500),
    @DetailDescription NVARCHAR(MAX),
    @Price DECIMAL(18,0),
    @OldPrice DECIMAL(18,0),
    @PurchasePrice DECIMAL(18,0),
    @Stock INT,
    @Unit NVARCHAR(50),
    @Weight FLOAT,
    @MainImage NVARCHAR(255),
    @ImageGallery NVARCHAR(MAX),
    @YoutubeVideo NVARCHAR(1000),
    @Tags NVARCHAR(500),
    @RelatedProducts NVARCHAR(500),
    @IsHot BIT,
    @IsNew BIT,
    @IsBestSeller BIT,
    @Status TINYINT,
    @SeoTitle NVARCHAR(255),
    @SeoDescription NVARCHAR(500),
    @SeoKeywords NVARCHAR(255),
    @ShopeeLink NVARCHAR(500) = NULL,
    @LazadaLink NVARCHAR(500) = NULL,
    @TechnicalSpecs NVARCHAR(MAX) = NULL,
    @UpdatedBy INT
AS
BEGIN
    UPDATE Products
    SET CategoryIds = @CategoryIds,
        SKU = @SKU,
        Name = @Name,
        Slug = @Slug,
        ShortDescription = @ShortDescription,
        DetailDescription = @DetailDescription,
        Price = @Price,
        OldPrice = @OldPrice,
        PurchasePrice = @PurchasePrice,
        Stock = @Stock,
        Unit = @Unit,
        Weight = @Weight,
        MainImage = @MainImage,
        ImageGallery = @ImageGallery,
        YoutubeVideo = @YoutubeVideo,
        Tags = @Tags,
        RelatedProducts = @RelatedProducts,
        IsHot = @IsHot,
        IsNew = @IsNew,
        IsBestSeller = @IsBestSeller,
        Status = @Status,
        SeoTitle = @SeoTitle,
        SeoDescription = @SeoDescription,
        SeoKeywords = @SeoKeywords,
        ShopeeLink = @ShopeeLink,
        LazadaLink = @LazadaLink,
        TechnicalSpecs = @TechnicalSpecs,
        UpdatedBy = @UpdatedBy,
        UpdatedAt = GETDATE()
    WHERE ProductId = @ProductId;
END
GO
