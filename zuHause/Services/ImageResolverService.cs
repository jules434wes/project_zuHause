using zuHause.Models;
using zuHause.Enums;
using Microsoft.EntityFrameworkCore;

namespace zuHause.Services
{
    public class ImageResolverService
    {
        private readonly ZuHauseContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ImageResolverService(ZuHauseContext context,IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetImageUrl(int propertyId, string entityType, string category, string size)
        {

            var request = _httpContextAccessor.HttpContext!.Request;
            var fallbackImageUrl = $"{request.Scheme}://{request.Host}/images/default-picture.png";

            bool parsedEntityTypeSuccess = Enum.TryParse<EntityType>(entityType,ignoreCase:true, out var entityTypeEnum);
            bool parsedCategorySuccess = Enum.TryParse<ImageCategory>(category, ignoreCase:true, out var ImageCategoryEnum);



            if (!parsedEntityTypeSuccess || !parsedCategorySuccess)
            {
                return fallbackImageUrl;
            }
                var image =  await _context.Images.Where(img=> (img.EntityId == propertyId) && (img.EntityType == entityTypeEnum) && img.Category == ImageCategoryEnum && img.DisplayOrder == 1).FirstOrDefaultAsync();
            string fileName = "";
            if(image != null)
            {
                fileName = image.StoredFileName.Replace("-", "").ToLower();
            }
            else
            {
                return fallbackImageUrl;
            }

                string imageUrl = $"https://zuhauseimg.blob.core.windows.net/zuhaus-images/{category}/{propertyId}/{size}/{fileName}";

            return imageUrl;
        }
    }
}
