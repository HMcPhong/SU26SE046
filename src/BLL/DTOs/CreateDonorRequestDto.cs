namespace BLL.DTOs
{
    public class CreateDonorRequestDto
    {
        public DateTime? PickupDate { get; set; }

        public string ContactName { get; set; } = string.Empty;

        public string ContactPhoneNumber { get; set; } = string.Empty;

        public string DeliveryMethod { get; set; } = "StaffPickup";

        public string Description { get; set; }

        public List<string>? ImageUrls { get; set; }

        public decimal EstimateWeight { get; set; }

        public string? PickupAddress { get; set; }

        public Guid WarehouseId { get; set; }
    }
}
