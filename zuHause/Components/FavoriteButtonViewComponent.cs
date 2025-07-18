using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using System.Security.Claims;

namespace zuHause.Components
{
    public class FavoriteButtonViewComponent : ViewComponent
    {
        private readonly ZuHauseContext _context;

        public FavoriteButtonViewComponent(ZuHauseContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int propertyId)
        {
            var viewModel = new FavoriteButtonViewModel
            {
                PropertyId = propertyId,
                IsFavorite = false,
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false
            };

            if (viewModel.IsAuthenticated)
            {
                var claimsIdentity = User.Identity as ClaimsIdentity;
                var memberIdClaim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(memberIdClaim, out int memberId))
                {
                    viewModel.IsFavorite = await _context.Favorites
                        .AnyAsync(f => f.MemberId == memberId && f.PropertyId == propertyId);
                }
            }

            return View(viewModel);
        }
    }

    public class FavoriteButtonViewModel
    {
        public int PropertyId { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}