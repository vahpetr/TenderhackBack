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
    public IEnumerable<Contract> Parse(string filePath, Dictionary<string, Organization> organizations, Dictionary<int, int> products)
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
        var number = csv.GetField<string>(0)?.Trim().Trim('\"');
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

        var customerInn = csv.GetField<string>(4)?.Trim().Trim('\"');
        if (string.IsNullOrWhiteSpace(customerInn) || customerInn.Length > 12)
        {
          continue;
        }

        var customerKpp = csv.GetField<string>(5)?.Trim().Trim('\"');
        if (string.IsNullOrWhiteSpace(customerKpp) || customerKpp.Length > 9)
        {
          continue;
        }

        var customerName = csv.GetField<string>(6)?.Trim().Trim('\"');
        if (string.IsNullOrWhiteSpace(customerName) || customerName.Length > 511)
        {
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

        var producerInn = csv.GetField<string>(7)?.Trim().Trim('\"');
        if (string.IsNullOrWhiteSpace(producerInn) || producerInn.Length > 12)
        {
          continue;
        }

        var producerKpp = csv.GetField<string>(8)?.Trim().Trim('\"');
        if (string.IsNullOrWhiteSpace(producerKpp) || producerKpp.Length > 9)
        {
          continue;
        }

        var producerName = csv.GetField<string>(9)?.Trim().Trim('\"');
        if (string.IsNullOrWhiteSpace(producerName) || producerName.Length > 511)
        {
          continue;
        }

        var producerKey = $"{producerInn}_{producerKpp}";
        if (!organizations.TryGetValue(producerKey, out var producer))
        {
          producer = new Organization()
          {
            Name = producerName,
            Inn = producerInn,
            Kpp = producerKpp
          };
          organizations.Add(producerKey, producer);
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
          continue;
        }

        rawOrder = rawOrder
          .Where(p => p.Id.HasValue && p.Id > 0 && p.Amount > 0 && p.Quantity > 0 && products.ContainsKey(p.Id.Value))
          .ToList();
        if (!rawOrder.Any())
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
          Producer = producer,
          Orders = rawOrder.Select(p => new Order()
          {
            ProductId = products[p.Id.Value], // product must be database
            Quantity = p.Quantity,
            Amount = p.Amount
          }).ToList()
        };
      }
    }
  }
}
