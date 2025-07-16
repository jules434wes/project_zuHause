using zuHause.Models;
using zuHause.Enums;

namespace zuHause.Tests.Models
{
    public class ImageTests
    {
        [Fact]
        public void Image_Constructor_SetsDefaultValues()
        {
            // Act
            var image = new Image();

            // Assert
            Assert.True(image.IsActive);
            Assert.NotEqual(DateTime.MinValue, image.UploadedAt);
            Assert.Equal(string.Empty, image.MimeType);
            Assert.Equal(string.Empty, image.OriginalFileName);
            Assert.Equal(string.Empty, image.StoredFileName);
        }

        [Fact]
        public void Image_SetProperties_ReturnsCorrectValues()
        {
            // Arrange
            var imageId = 1L;
            var imageGuid = Guid.NewGuid();
            var entityType = EntityType.Property;
            var entityId = 123;
            var category = ImageCategory.Gallery;
            var mimeType = "image/webp";
            var originalFileName = "test.jpg";
            var storedFileName = "abc123.webp";
            var fileSizeBytes = 1024L;
            var width = 800;
            var height = 600;
            var displayOrder = 1;
            var uploadedByMemberId = 456;
            var uploadedAt = DateTime.UtcNow;

            // Act
            var image = new Image
            {
                ImageId = imageId,
                ImageGuid = imageGuid,
                EntityType = entityType,
                EntityId = entityId,
                Category = category,
                MimeType = mimeType,
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                FileSizeBytes = fileSizeBytes,
                Width = width,
                Height = height,
                DisplayOrder = displayOrder,
                UploadedByMemberId = uploadedByMemberId,
                UploadedAt = uploadedAt
            };

            // Assert
            Assert.Equal(imageId, image.ImageId);
            Assert.Equal(imageGuid, image.ImageGuid);
            Assert.Equal(entityType, image.EntityType);
            Assert.Equal(entityId, image.EntityId);
            Assert.Equal(category, image.Category);
            Assert.Equal(mimeType, image.MimeType);
            Assert.Equal(originalFileName, image.OriginalFileName);
            Assert.Equal(storedFileName, image.StoredFileName);
            Assert.Equal(fileSizeBytes, image.FileSizeBytes);
            Assert.Equal(width, image.Width);
            Assert.Equal(height, image.Height);
            Assert.Equal(displayOrder, image.DisplayOrder);
            Assert.Equal(uploadedByMemberId, image.UploadedByMemberId);
            Assert.Equal(uploadedAt, image.UploadedAt);
        }

        [Fact]
        public void Image_EntityType_SupportsAllValues()
        {
            // Arrange & Act & Assert
            var propertyImage = new Image { EntityType = EntityType.Property };
            Assert.Equal(EntityType.Property, propertyImage.EntityType);

            var memberImage = new Image { EntityType = EntityType.Member };
            Assert.Equal(EntityType.Member, memberImage.EntityType);

            var furnitureImage = new Image { EntityType = EntityType.Furniture };
            Assert.Equal(EntityType.Furniture, furnitureImage.EntityType);

            var announcementImage = new Image { EntityType = EntityType.Announcement };
            Assert.Equal(EntityType.Announcement, announcementImage.EntityType);
        }

        [Fact]
        public void Image_Category_SupportsAllValues()
        {
            // Arrange & Act & Assert
            var bedroomImage = new Image { Category = ImageCategory.BedRoom };
            Assert.Equal(ImageCategory.BedRoom, bedroomImage.Category);

            var livingImage = new Image { Category = ImageCategory.Living };
            Assert.Equal(ImageCategory.Living, livingImage.Category);

            var kitchenImage = new Image { Category = ImageCategory.Kitchen };
            Assert.Equal(ImageCategory.Kitchen, kitchenImage.Category);

            var balconyImage = new Image { Category = ImageCategory.Balcony };
            Assert.Equal(ImageCategory.Balcony, balconyImage.Category);

            var galleryImage = new Image { Category = ImageCategory.Gallery };
            Assert.Equal(ImageCategory.Gallery, galleryImage.Category);

            var avatarImage = new Image { Category = ImageCategory.Avatar };
            Assert.Equal(ImageCategory.Avatar, avatarImage.Category);

            var productImage = new Image { Category = ImageCategory.Product };
            Assert.Equal(ImageCategory.Product, productImage.Category);
        }

        [Fact]
        public void Image_NullableProperties_CanBeNull()
        {
            // Arrange & Act
            var image = new Image
            {
                DisplayOrder = null,
                UploadedByMemberId = null
            };

            // Assert
            Assert.Null(image.DisplayOrder);
            Assert.Null(image.UploadedByMemberId);
        }
    }
}