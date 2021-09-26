//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System;
using Tenderhack.PredictQuantity.Model;

namespace Tenderhack.PredictQuantity.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create single instance of sample data from first line of dataset for model input
            PredictQuantityInput sampleData = new PredictQuantityInput()
            {
                CustomerRegion = 77F,
                Kpgz = @"01.15.01.01.01.01",
                Season = @"3Лето",
            };

            // Make a single prediction on the sample data and print results
            var predictionResult = ConsumeModel.Predict(sampleData);

            Console.WriteLine("Using model to make single prediction -- Comparing actual Quantity with predicted Quantity from sample data...\n\n");
            Console.WriteLine($"Customer_region: {sampleData.CustomerRegion}");
            Console.WriteLine($"Kpgz: {sampleData.Kpgz}");
            Console.WriteLine($"Season: {sampleData.Season}");
            Console.WriteLine($"\n\nPredicted Quantity: {predictionResult.Score}\n\n");
            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }
    }
}
