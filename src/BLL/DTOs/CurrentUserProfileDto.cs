namespace BLL.DTOs;

public record CurrentUserProfileDto(
    Guid Id,
    string FullName,
    string UserName,
    string Email,
    string PhoneNumber,
    string Address,
    string Role,
    string Status,
    string? AvatarUrl,
    Guid? WarehouseId,
    string? WarehouseName,
    string? WarehouseAddress,
    bool EmailConfirmed,
    bool PhoneNumberConfirmed,
    DateTime? CreateAt);
