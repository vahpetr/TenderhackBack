using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using Tenderhack.Core.Data.TenderhackDbContext.Models;

namespace Tenderhack.ProductLoader
{
  public class ProductParser
  {
    public IEnumerable<Product> Parse(
      string filePath,
      HashSet<int> externalIds,
      Dictionary<string, Category> categories,
      Dictionary<string, ProductAttribute> attributes,
      Dictionary<string, ProductValue> values
      )
    {
      var options = new JsonSerializerOptions
      {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true
      };
      var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
      {
        Delimiter = ";"
      };
      using var reader = new StreamReader(filePath);
      using var csv = new CsvReader(reader, csvConfiguration);

      csv.Read();
      csv.ReadHeader();
      while (csv.Read())
      {
        var externalId = csv.GetField<int>(0);
        if (externalId <= 0 || externalIds.Contains(externalId))
        {
          continue;
        }

        var name = csv.GetField<string>(1)?.Trim().Trim('\"');
        if (string.IsNullOrWhiteSpace(name) || name.Length > 511)
        {
          continue;
        }

        var categoryTitle = csv.GetField<string>(2)?.Trim().Trim('\"');
        if (string.IsNullOrWhiteSpace(categoryTitle))
        {
          continue;
        }

        var categoryKpgz = csv.GetField<string>(3)?.Trim().Trim('\"');
        if (string.IsNullOrWhiteSpace(categoryKpgz) || categoryKpgz.Length > 32)
        {
          continue;
        }

        if (!categories.TryGetValue(categoryKpgz, out var category))
        {
          category = new Category()
          {
            Title = categoryTitle,
            Kpgz = categoryKpgz
          };
          categories.Add(categoryKpgz, category);
        }

        var json = csv.GetField<string>(4);
        var rawProperties = Enumerable.Empty<RawProperty>();
        try
        {
          if (!string.IsNullOrWhiteSpace(json))
          {
            rawProperties = JsonSerializer.Deserialize<IList<RawProperty>>(json, options);
          }
        }
        catch (Exception)
        {
          continue;
        }

        var product = new Product()
        {
          ExternalId = externalId,
          Name = name,
          Category = category,
          Properties = rawProperties
            .Where(p =>  p.Name.Length <= 511 && p.Value != null && p.Value.Length <= 255 && p.Id > 0)
            .Select(p =>
            {
              p.Name = p.Name.Trim().Trim('\"');
              p.Value = p.Value.Trim().Trim('\"');

              var attributeKey = GetFixedPropertyName(p.Id, p.Name);
              if (!attributes.TryGetValue(attributeKey, out var attribute))
              {
                attribute = new ProductAttribute()
                {
                    Name = p.Name
                };
                attributes.Add(attributeKey, attribute);
              }

              var valueKey = $"{attributeKey}_{p.Value}";
              if (!values.TryGetValue(valueKey, out var value))
              {
                value = new ProductValue()
                {
                  Name = p.Value
                };
                values.Add(valueKey, value);
              }

              var property = new ProductProperty()
              {
                Attribute = attribute,
                Value = value
              };

              return property;
            })
            .DistinctBy(p => new {p.ProductId, p.AttributeId, p.ValueId})
            .ToList()
        };

        product.Attributes = product.Properties.Select(p => p.Attribute).ToList();
        product.Values = product.Properties.Select(p => p.Value).ToList();

        yield return product;
      }
    }

    private static string GetFixedPropertyName(int id, string name)
    {
      return id switch
      {
        284858006 => "Длина",
        284858014 => "Длина",
        _ => name
      };
    }
  }
}
