USE [GORDAK]
GO

IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Products]') 
    AND name = 'InstructionVideo'
)
BEGIN
    ALTER TABLE Products ADD InstructionVideo NVARCHAR(MAX) NULL;
END
GO

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
    @Accessories NVARCHAR(500) = NULL,
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
    @InstructionVideo NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(200)
AS
BEGIN
    INSERT INTO Products(
        CategoryIds, SKU, Name, Slug, ShortDescription, DetailDescription,
        Price, OldPrice, PurchasePrice, Stock, Unit, Weight,
        MainImage, ImageGallery, YoutubeVideo, Tags, RelatedProducts, Accessories,
        IsHot, IsNew, IsBestSeller, Status,
        SeoTitle, SeoDescription, SeoKeywords, 
        ShopeeLink, LazadaLink, TechnicalSpecs, InstructionVideo, CreatedBy
    )
    VALUES(
        @CategoryIds, @SKU, @Name, @Slug, @ShortDescription, @DetailDescription,
        @Price, @OldPrice, @PurchasePrice, @Stock, @Unit, @Weight,
        @MainImage, @ImageGallery, @YoutubeVideo, @Tags, @RelatedProducts, @Accessories,
        @IsHot, @IsNew, @IsBestSeller, @Status,
        @SeoTitle, @SeoDescription, @SeoKeywords,
        @ShopeeLink, @LazadaLink, @TechnicalSpecs, @InstructionVideo, @CreatedBy
    );
    SELECT SCOPE_IDENTITY();
END
GO

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
    @Accessories NVARCHAR(500) = NULL,
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
    @InstructionVideo NVARCHAR(MAX) = NULL,
    @UpdatedBy NVARCHAR(200)
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
        Accessories = @Accessories,
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
        InstructionVideo = @InstructionVideo,
        UpdatedBy = @UpdatedBy,
        UpdatedAt = GETDATE()
    WHERE ProductId = @ProductId;
END
GO
