# Tenderhack

```sh
dotnet ef migrations add Tenderhack_Initial \
  -c Tenderhack.Core.Data.TenderhackDbContext.TenderhackDbContext \
  -p Tenderhack.Core/Tenderhack.Core.csproj \
  -o Migrations/Tenderhack

dotnet ef database update --project Tenderhack.Core/Tenderhack.Core.csproj

# dotnet ef database drop --project Tenderhack.Core/Tenderhack.Core.csproj

dotnet run --project Tenderhack.ProductLoader/Tenderhack.ProductLoader.csproj -c Release /Users/vahpetr/Hackathon/Tenderhack/Tenderhack.ProductLoader/Products.csv


dotnet run --project Tenderhack.ContractLoader/Tenderhack.ContractLoader.csproj -c Release /Users/vahpetr/Hackathon/Tenderhack/Tenderhack.ContractLoader/Contracts.csv


mlnet regression --dataset ./testdataset_full_bez_price.csv --label-col quantity  --has-header true --train-time 300 --name Tenderhack.PredictQuantity
```

## TODO

1. Add Unit
