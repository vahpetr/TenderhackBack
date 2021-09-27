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
    public IEnumerable<Product> Parse(string filePath, HashSet<int> externalIds, Dictionary<string, Category> categories, Dictionary<string, Characteristic> characteristics)
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

        var name = csv.GetField<string>(1);
        if (string.IsNullOrWhiteSpace(name) || name.Length > 511)
        {
          continue;
        }

        var categoryTitle = csv.GetField<string>(2);
        if (string.IsNullOrWhiteSpace(categoryTitle))
        {
          continue;
        }

        var categoryKpgz = csv.GetField<string>(3);
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
        var rawCharacteristic = Enumerable.Empty<RawCharacteristic>();
        try
        {
          if (!string.IsNullOrWhiteSpace(json))
          {
            rawCharacteristic = JsonSerializer.Deserialize<IList<RawCharacteristic>>(json, options);
          }
        }
        catch (Exception)
        {
          continue;
        }

        yield return new Product()
        {
          ExternalId = externalId,
          Name = name,
          Category = category,
          Characteristics = rawCharacteristic
            .Where(p =>  p.Name.Length <= 511 && p.Value != null && p.Value.Length <= 255 && p.Id > 0)
            .Select(p => {
              var characteristicKey = $"{p.Name}_{p.Value}";
              if (!characteristics.TryGetValue(characteristicKey, out var characteristic))
              {
                characteristic = new Characteristic()
                {
                  ExternalId = p.Id,
                  Name = p.Name,
                  Value = p.Value,
                };
                characteristics.Add(characteristicKey, characteristic);
              }
              return characteristic;
            })
            .ToList()
        };
      }
    }
  }
}
