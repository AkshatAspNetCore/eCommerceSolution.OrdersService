namespace BusinessLogicLayer.DTO;

public record UserDTO(Guid UserID, string? Email, string? Username, string? Gender)
{
    public UserDTO() : this(default, default, default, default)
    {
    }
}
