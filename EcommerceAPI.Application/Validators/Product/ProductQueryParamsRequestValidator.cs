using EcommerceAPI.Application.DTOs.Product;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.Product
{
    public class ProductQueryParamsRequestValidator : AbstractValidator<ProductQueryParamsRequest>
    {
        private static readonly string[] AllowedSortBy = { "name", "price", "newest", "stock" };
        private static readonly string[] AllowedSortDir = { "asc", "desc" };
        private const int MaxSearchLength = 150;
        private const int MaxListItemLength = 100;
        private const int MaxListItems = 25;
        private const decimal MaxPriceValue = 1_000_000m;

        public ProductQueryParamsRequestValidator()
        {
            // --- search ---
            RuleFor(x => x.Search)
                .MaximumLength(MaxSearchLength)
                .WithMessage($"Search text cannot exceed {MaxSearchLength} characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Search));

            // --- categorySlug ---
            RuleFor(x => x.CategorySlug)
                .MaximumLength(MaxListItemLength)
                .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage("categorySlug must be a valid slug (lowercase letters, numbers, hyphens).")
                .When(x => !string.IsNullOrWhiteSpace(x.CategorySlug));

            // --- tags ---
            RuleFor(x => x.Tags)
                .Must(t => t!.Count <= MaxListItems)
                .WithMessage($"tags cannot contain more than {MaxListItems} values.")
                .Must(t => t!.All(v => !string.IsNullOrWhiteSpace(v) && v.Length <= MaxListItemLength))
                .WithMessage("tags cannot contain empty or overly long values.")
                .When(x => x.Tags is { Count: > 0 });

            // --- brand ---
            RuleFor(x => x.Brand)
                .Must(b => b!.Count <= MaxListItems)
                .WithMessage($"brand cannot contain more than {MaxListItems} values.")
                .Must(b => b!.All(v => !string.IsNullOrWhiteSpace(v) && v.Length <= MaxListItemLength))
                .WithMessage("brand cannot contain empty or overly long values.")
                .When(x => x.Brand is { Count: > 0 });

            // --- minPrice / maxPrice: individual bounds ---
            RuleFor(x => x.MinPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("minPrice cannot be negative.")
                .LessThanOrEqualTo(MaxPriceValue)
                .WithMessage($"minPrice cannot exceed {MaxPriceValue:N0}.")
                .When(x => x.MinPrice.HasValue);

            RuleFor(x => x.MaxPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("maxPrice cannot be negative.")
                .LessThanOrEqualTo(MaxPriceValue)
                .WithMessage($"maxPrice cannot exceed {MaxPriceValue:N0}.")
                .When(x => x.MaxPrice.HasValue);

            // --- minPrice / maxPrice: relationship ---
            RuleFor(x => x)
                .Must(x => x.MinPrice!.Value <= x.MaxPrice!.Value)
                .WithMessage("minPrice cannot be greater than maxPrice.")
                .WithName("minPrice")
                .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);

            // --- sortBy ---
            RuleFor(x => x.SortBy)
                .Must(v => AllowedSortBy.Contains(v, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"sortBy must be one of: {string.Join(", ", AllowedSortBy)}.")
                .When(x => !string.IsNullOrWhiteSpace(x.SortBy));

            // --- sortDir ---
            RuleFor(x => x.SortDir)
                .Must(v => AllowedSortDir.Contains(v, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"sortDir must be one of: {string.Join(", ", AllowedSortDir)}.")
                .When(x => !string.IsNullOrWhiteSpace(x.SortDir));

            // --- limit ---
            RuleFor(x => x.Limit)
                .InclusiveBetween(1, 100)
                .WithMessage("limit must be between 1 and 100.");

            // --- cursor: format only, semantic validation (sort/filter match) happens in ElasticSearchService ---
            RuleFor(x => x.Cursor)
                .Must(BeValidBase64)
                .WithMessage("cursor is not a valid pagination token.")
                .When(x => !string.IsNullOrWhiteSpace(x.Cursor));
        }

        private static bool BeValidBase64(string? cursor)
        {
            if (string.IsNullOrWhiteSpace(cursor)) return true;
            Span<byte> buffer = new byte[cursor.Length];
            return Convert.TryFromBase64String(cursor, buffer, out _);
        }
    }
}