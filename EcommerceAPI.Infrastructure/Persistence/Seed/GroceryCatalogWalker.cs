using System.Text.Json;

namespace EcommerceAPI.Infrastructure.Persistence.Seed
{
    internal static class GroceryCatalogWalker
    {
        public sealed record CatalogLeaf(string[] Path, string Name, string Description, string ImageRelativePath);

        public static IEnumerable<CatalogLeaf> WalkCatalog(string catalogJsonPath)
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(catalogJsonPath));
            foreach (var topLevel in doc.RootElement.EnumerateObject())
            {
                foreach (var leaf in WalkNode(topLevel.Value, new[] { topLevel.Name }))
                {
                    yield return leaf;
                }
            }
        }

        private static IEnumerable<CatalogLeaf> WalkNode(JsonElement node, string[] path)
        {
            if (node.TryGetProperty("image", out var imageProp))
            {
                var name = node.TryGetProperty("information", out var info) &&
                           info.TryGetProperty("Title", out var titleProp) &&
                           titleProp.GetString() is { Length: > 0 } title
                    ? title
                    : path[^1].Replace('-', ' ');

                var description = node.TryGetProperty("description", out var descProp)
                    ? descProp.GetString() ?? string.Empty
                    : string.Empty;

                yield return new CatalogLeaf(path, name, description, imageProp.GetString()!);
                yield break;
            }

            foreach (var child in node.EnumerateObject())
            {
                foreach (var leaf in WalkNode(child.Value, [.. path, child.Name]))
                {
                    yield return leaf;
                }
            }
        }
    }
}
