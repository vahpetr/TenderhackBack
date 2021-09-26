//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System;
using System.IO;
using Microsoft.ML;

namespace Tenderhack.PredictQuantity.Model
{
    public class ConsumeModel
    {
        private static Lazy<PredictionEngine<PredictQuantityInput, PredictQuantityOutput>> PredictionEngine = new Lazy<PredictionEngine<PredictQuantityInput, PredictQuantityOutput>>(CreatePredictionEngine);

        public static string MLNetModelPath = Path.GetFullPath("MLModel.zip");

        // For more info on consuming ML.NET models, visit https://aka.ms/mlnet-consume
        // Method for consuming model in your app
        public static PredictQuantityOutput Predict(PredictQuantityInput input)
        {
            PredictQuantityOutput result = PredictionEngine.Value.Predict(input);
            return result;
        }

        public static PredictionEngine<PredictQuantityInput, PredictQuantityOutput> CreatePredictionEngine()
        {
            // Create new MLContext
            MLContext mlContext = new MLContext();

            // Load model & create prediction engine
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<PredictQuantityInput, PredictQuantityOutput>(mlModel);

            return predEngine;
        }
    }
}
