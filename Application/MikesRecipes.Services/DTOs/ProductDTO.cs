using MikesRecipes.Domain.Models;

namespace MikesRecipes.Services.DTOs;

public record ProductDTO(ProductId ProductId, string Title);