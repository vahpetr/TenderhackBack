//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System.Text.Json.Serialization;
using Microsoft.ML.Data;

namespace Tenderhack.PredictQuantity.Model
{
    public class PredictQuantityInput
    {
        [ColumnName("customer_region"), LoadColumn(0)]
        public float CustomerRegion { get; set; }


        [ColumnName("kpgz"), LoadColumn(1)]
        public string CpgzCode { get; set; }


        [ColumnName("season"), LoadColumn(2)]
        public string Season { get; set; }


        [JsonIgnore]
        [ColumnName("quantity"), LoadColumn(3)]
        public float Quantity { get; set; }
    }
}
