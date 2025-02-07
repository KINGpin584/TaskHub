// DTOs/CategoryDTOs.cs

namespace TaskManagementSystem.Core.DTOs
{
public class CreateCategoryDTO
{
    public required string Name { get; set; }
    public int Priority { get; set; }
}

public class CategoryResponseDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int TaskCount { get; set; }
}
}