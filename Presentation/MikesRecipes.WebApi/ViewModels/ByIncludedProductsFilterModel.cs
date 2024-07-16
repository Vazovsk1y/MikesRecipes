using System.ComponentModel.DataAnnotations;
using Workleap.ComponentModel.DataAnnotations;

namespace MikesRecipes.WebApi.ViewModels;

public record ByIncludedProductsFilterModel(
    [Required]
    [NotEmpty]
    IEnumerable<Guid> Products,
    [Range(0, int.MaxValue)]
    int OtherProductsCount
    );
