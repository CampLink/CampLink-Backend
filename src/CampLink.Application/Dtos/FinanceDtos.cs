namespace CampLink.Application.Dtos;

/// <summary>Строка финансового отчёта (см. README: ФИН. ОТЧЁТ — запасы в статусе «расход + куплено» за период).</summary>
public class FinanceReportLineDto
{
    public long ResourceId { get; set; }

    public string ResourceName { get; set; } = string.Empty;

    /// <summary>Количество проданных единиц за период.</summary>
    public int Quantity { get; set; }

    /// <summary>Выручка (сумма Quantity * Price) за период.</summary>
    public decimal Amount { get; set; }
}

/// <summary>Результат финансового отчёта за период.</summary>
public class FinanceReportDto
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public List<FinanceReportLineDto> Lines { get; set; } = new();

    /// <summary>Итоговая выручка за период.</summary>
    public decimal TotalAmount { get; set; }
}
