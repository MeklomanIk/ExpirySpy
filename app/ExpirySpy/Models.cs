using System;

namespace ExpirySpy;

public enum ResourceState
{
    Expired,
    ExpiringSoon,
    Available
}

public class ResourceCategory
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;

    /// <summary>
    /// Родительская категория. Для корневых категорий null.
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// Значения ресурса по умолчанию для упрощения ввода.
    /// </summary>
    public double? DefaultQuantity { get; set; }
    public string? DefaultUnit { get; set; }
    public double? DefaultMinQuantity { get; set; }
}

public class ResourceItem
{
    public long Id { get; set; }
    public long CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime PurchaseDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public double Quantity { get; set; }
    public string Unit { get; set; } = null!;
    public double MinQuantity { get; set; }

    public ResourceState GetState(DateTime now)
    {
        if (ExpiryDate.Date < now.Date)
            return ResourceState.Expired;

        var daysLeft = (ExpiryDate.Date - now.Date).TotalDays;
        if (daysLeft <= 7)
            return ResourceState.ExpiringSoon;

        return ResourceState.Available;
    }

    public bool IsLowOnStock() => Quantity <= MinQuantity;
}


