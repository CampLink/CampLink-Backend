namespace CampLink.Application.Dtos;

/// <summary>Запрос на создание клиента.</summary>
public class CreateClientDto
{
    /// <summary>Название.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Описание.</summary>
    public string? Description { get; set; }

    /// <summary>Контакты.</summary>
    public string? Contacts { get; set; }
}

/// <summary>Данные клиента.</summary>
public class ClientDto
{
    public long ClientId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Contacts { get; set; }
}
