using Microsoft.EntityFrameworkCore;
using zuHause.Data.Configurations;
using zuHause.Models;
using zuHause.Enums;

namespace zuHause.Tests.Data
{
    public class ImageConfigurationTests
    {
        private DbContextOptions<ZuHauseContext> GetInMemoryDbOptions()
        {
            return new DbContextOptionsBuilder<ZuHauseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void ImageConfiguration_TableName_IsLowerCase()
        {
            // Arrange
            var options = GetInMemoryDbOptions();

            // Act & Assert
            using (var context = new ZuHauseContext(options))
            {
                var entityType = context.Model.FindEntityType(typeof(Image));
                Assert.Equal("images", entityType.GetTableName());
            }
        }

        [Fact]
        public void ImageConfiguration_PrimaryKey_IsImageId()
        {
            // Arrange
            var options = GetInMemoryDbOptions();

            // Act & Assert
            using (var context = new ZuHauseContext(options))
            {
                var entityType = context.Model.FindEntityType(typeof(Image));
                var primaryKey = entityType.FindPrimaryKey();
                Assert.Equal("ImageId", primaryKey.Properties.First().Name);
            }
        }

        [Fact]
        public void ImageConfiguration_ImageGuid_HasUniqueIndex()
        {
            // Arrange
            var options = GetInMemoryDbOptions();

            // Act & Assert
            using (var context = new ZuHauseContext(options))
            {
                var entityType = context.Model.FindEntityType(typeof(Image));
                var uniqueIndex = entityType?.GetIndexes()
                    .FirstOrDefault(i => i.Name == "UQ_images_ImageGuid");
                
                // InMemory provider 可能不支援命名索引，所以檢查是否有 ImageGuid 的唯一索引
                if (uniqueIndex == null)
                {
                    uniqueIndex = entityType?.GetIndexes()
                        .FirstOrDefault(i => i.IsUnique && i.Properties.First().Name == "ImageGuid");
                }
                
                Assert.NotNull(uniqueIndex);
                Assert.True(uniqueIndex.IsUnique);
                Assert.Equal("ImageGuid", uniqueIndex.Properties.First().Name);
            }
        }

        [Fact]
        public void ImageConfiguration_EntityType_HasMaxLength50()
        {
            // Arrange
            var options = GetInMemoryDbOptions();

            // Act & Assert
            using (var context = new ZuHauseContext(options))
            {
                var entityType = context.Model.FindEntityType(typeof(Image));
                var entityTypeProperty = entityType.FindProperty("EntityType");
                Assert.Equal(50, entityTypeProperty.GetMaxLength());
            }
        }

        [Fact]
        public void ImageConfiguration_Category_HasMaxLength50()
        {
            // Arrange
            var options = GetInMemoryDbOptions();

            // Act & Assert
            using (var context = new ZuHauseContext(options))
            {
                var entityType = context.Model.FindEntityType(typeof(Image));
                var categoryProperty = entityType.FindProperty("Category");
                Assert.Equal(50, categoryProperty.GetMaxLength());
            }
        }

        [Fact]
        public void ImageConfiguration_MimeType_HasMaxLength50()
        {
            // Arrange
            var options = GetInMemoryDbOptions();

            // Act & Assert
            using (var context = new ZuHauseContext(options))
            {
                var entityType = context.Model.FindEntityType(typeof(Image));
                var mimeTypeProperty = entityType.FindProperty("MimeType");
                Assert.Equal(50, mimeTypeProperty.GetMaxLength());
            }
        }

        [Fact]
        public void ImageConfiguration_OriginalFileName_HasMaxLength255()
        {
            // Arrange
            var options = GetInMemoryDbOptions();

            // Act & Assert
            using (var context = new ZuHauseContext(options))
            {
                var entityType = context.Model.FindEntityType(typeof(Image));
                var originalFileNameProperty = entityType.FindProperty("OriginalFileName");
                Assert.Equal(255, originalFileNameProperty.GetMaxLength());
            }
        }

        [Fact]
        public void ImageConfiguration_StoredFileName_HasMaxLength50()
        {
            // Arrange
            var options = GetInMemoryDbOptions();

            // Act & Assert
            using (var context = new ZuHauseContext(options))
            {
                var entityType = context.Model.FindEntityType(typeof(Image));
                var storedFileNameProperty = entityType.FindProperty("StoredFileName");
                Assert.Equal(50, storedFileNameProperty.GetMaxLength());
            }
        }

        [Fact]
        public void ImageConfiguration_IsActive_HasDefaultValue()
        {
            // Arrange
            var options = GetInMemoryDbOptions();

            // Act & Assert
            using (var context = new ZuHauseContext(options))
            {
                var entityType = context.Model.FindEntityType(typeof(Image));
                var isActiveProperty = entityType.FindProperty("IsActive");
                Assert.Equal(true, isActiveProperty.GetDefaultValue());
            }
        }

        [Fact]
        public void ImageConfiguration_EnumConversion_WorksCorrectly()
        {
            // Arrange
            var options = GetInMemoryDbOptions();

            // Act & Assert
            using (var context = new ZuHauseContext(options))
            {
                var image = new Image
                {
                    EntityType = EntityType.Property,
                    EntityId = 1,
                    Category = ImageCategory.Gallery,
                    MimeType = "image/webp",
                    OriginalFileName = "test.jpg",
                    FileSizeBytes = 1024,
                    Width = 800,
                    Height = 600
                };

                context.Images.Add(image);
                context.SaveChanges();

                var savedImage = context.Images.First();
                Assert.Equal(EntityType.Property, savedImage.EntityType);
                Assert.Equal(ImageCategory.Gallery, savedImage.Category);
            }
        }

        [Fact]
        public void ImageConfiguration_CompositeIndexes_AreCreated()
        {
            // Arrange
            var options = GetInMemoryDbOptions();

            // Act & Assert
            using (var context = new ZuHauseContext(options))
            {
                var entityType = context.Model.FindEntityType(typeof(Image));
                var indexes = entityType?.GetIndexes();

                // InMemory provider 可能不支援命名索引，所以檢查是否有對應的複合索引
                var displayOrderIndex = indexes?.FirstOrDefault(i => i.Name == "IX_images_Entity_DisplayOrder");
                if (displayOrderIndex == null)
                {
                    displayOrderIndex = indexes?.FirstOrDefault(i => 
                        i.Properties.Count == 3 && 
                        i.Properties.Any(p => p.Name == "EntityType") &&
                        i.Properties.Any(p => p.Name == "EntityId") &&
                        i.Properties.Any(p => p.Name == "DisplayOrder"));
                }

                var categoryIndex = indexes?.FirstOrDefault(i => i.Name == "IX_images_Entity_Category");
                if (categoryIndex == null)
                {
                    categoryIndex = indexes?.FirstOrDefault(i => 
                        i.Properties.Count == 3 && 
                        i.Properties.Any(p => p.Name == "EntityType") &&
                        i.Properties.Any(p => p.Name == "EntityId") &&
                        i.Properties.Any(p => p.Name == "Category"));
                }

                Assert.NotNull(displayOrderIndex);
                Assert.Equal(3, displayOrderIndex.Properties.Count);

                Assert.NotNull(categoryIndex);
                Assert.Equal(3, categoryIndex.Properties.Count);
            }
        }
    }
}