using RandomRecipes.Domain.Enums;

namespace RandomRecipes.Domain.ValueObjects;

public record IngridientAmount(double Count, AmountType AmountType, string? ExtraInfo);
