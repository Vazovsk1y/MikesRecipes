using RandomRecipes.Domain.Models;

namespace RandomRecipes.Services.DTOs;

public record ProductDTO(ProductId ProductId, string Title);