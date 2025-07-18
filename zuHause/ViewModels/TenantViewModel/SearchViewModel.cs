// ViewModels/TenantViewModel/SearchViewModel.cs
using System;
using System.Collections.Generic;

// 命名空間已更新為 zuHause.ViewModels.TenantViewModel
namespace zuHause.ViewModels.TenantViewModel
{
    public class SearchViewModel
    {
        public int PropertyId { get; set; }
        public string? Title { get; set; }
        public string? AddressLine { get; set; }
        public int RoomCount { get; set; }
        public int LivingRoomCount { get; set; }
        public int BathroomCount { get; set; }
        public int CurrentFloor { get; set; }
        public int TotalFloors { get; set; }
        public decimal Area { get; set; }
        public decimal MonthlyRent { get; set; }
        public List<string> Features { get; set; } = new List<string>();
        public string? ImagePath { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool IsFavorited { get; set; }
    }
}