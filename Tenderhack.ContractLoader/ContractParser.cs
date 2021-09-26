using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using Tenderhack.Core.Data.TenderhackDbContext.Models;

namespace Tenderhack.ContractLoader
{
  public class ContractParser
  {
    public IEnumerable<Contract> Parse(string filePath, Dictionary<string, Organization> organizations, Dictionary<int, int> productMap)
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
      // var i = 0;
      while (csv.Read())
      {
        var number = csv.GetField<string>(0);
        if (string.IsNullOrWhiteSpace(number))
        {
          continue;
        }

        var publicAtString = csv.GetField<string>(1);
        if (!DateTime.TryParseExact(publicAtString, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var publicAt))
        {
          continue;
        }
        publicAt = publicAt.ToUniversalTime();

        // ConclusionAt
        var conclusionAtString = csv.GetField<string>(2);
        if (!DateTime.TryParseExact(conclusionAtString, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var conclusionAt))
        {
          continue;
        }

        conclusionAt = conclusionAt.ToUniversalTime();

        if (publicAt > conclusionAt)
        {
          continue;
        }

        var priceString = csv.GetField<string>(3);
        if (!decimal.TryParse(priceString, NumberStyles.Float, CultureInfo.InvariantCulture, out var price))
        {
          continue;
        }

        if (price <= 0)
        {
          continue;
        }

        var customerInn = csv.GetField<string>(4);
        if (string.IsNullOrWhiteSpace(customerInn) || customerInn.Length > 12)
        {
          // skip
          continue;
        }

        var customerKpp = csv.GetField<string>(5);
        if (string.IsNullOrWhiteSpace(customerKpp) || customerKpp.Length > 9)
        {
          // skip
          continue;
        }

        var customerName = csv.GetField<string>(6);
        if (string.IsNullOrWhiteSpace(customerName) || customerName.Length > 511)
        {
          // skip
          continue;
        }

        var customerKey = $"{customerInn}_{customerKpp}";
        if (!organizations.TryGetValue(customerKey, out var customer))
        {
          customer = new Organization()
          {
            Name = customerName,
            Inn = customerInn,
            Kpp = customerKpp
          };
          organizations.Add(customerKey, customer);
        }

        var providerInn = csv.GetField<string>(7);
        if (string.IsNullOrWhiteSpace(providerInn) || providerInn.Length > 12)
        {
          // skip
          continue;
        }

        var providerKpp = csv.GetField<string>(8);
        if (string.IsNullOrWhiteSpace(providerKpp) || providerKpp.Length > 9)
        {
          // skip
          continue;
        }

        var providerName = csv.GetField<string>(9);
        if (string.IsNullOrWhiteSpace(providerName) || providerName.Length > 511)
        {
          // skip
          continue;
        }

        var providerKey = $"{providerInn}_{providerKpp}";
        if (!organizations.TryGetValue(providerKey, out var provider))
        {
          provider = new Organization()
          {
            Name = providerName,
            Inn = providerInn,
            Kpp = providerKpp
          };
          organizations.Add(providerKey, provider);
        }

        var json = csv.GetField<string>(10);
        var rawOrder = Enumerable.Empty<RawOrder>();
        try
        {
          if (!string.IsNullOrWhiteSpace(json))
          {
            rawOrder = JsonSerializer.Deserialize<IList<RawOrder>>(json, options);
          }
        }
        catch (Exception)
        {
          // skip
          continue;
        }

        rawOrder = rawOrder
          .Where(p => p.Id > 0 && p.Amount > 0 && p.Quantity > 0 &&
                      (!p.Id.HasValue || productMap.ContainsKey(p.Id.Value)))
          .ToList();

        if (rawOrder.Count() == 0)
        {
          continue;
        }

        yield return new Contract()
        {
          Number = number,
          PublicAt = publicAt,
          ConclusionAt = conclusionAt,
          Price = price,
          Customer = customer,
          Provider = provider,
          Orders = rawOrder
            .Select(p => new Order()
          {
              ProductId = p.Id.HasValue ? productMap[p.Id.Value] : null,
              Quantity = p.Quantity,
              Amount = p.Amount
          }).ToList()
        };
      }
    }
  }
}
