using NeoCortexApi;
using NeoCortexApi.Encoders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static ApprovedMultiSequenceLearningNew.MultiSequenceLearning;


namespace ApprovedMultiSequenceLearningNew
{
    class Program
    {
        private const string DatasetFolder = "dataset";
        private const string ReportFolder = "report";
        // Adjust these to your file names
        private const string DatasetFileName = "dataset_01.json";
        //private const string DatasetFileName = "dataset_03.json";
        //private const string DatasetFileName = "dataset_02.json";
        //private const string DatasetFileName = "dataset_04.json";
        private const string TestsetFileName = "test_01.json";
        //private const string TestsetFileName = "test_02.json";

        static void Main(string[] args)
        {
            Console.WriteLine("**********************   Welcome By Git Gurdians    ************************ \n ");
            Console.WriteLine("************   ML-23/24-09   Approve Prediction of Multi Sequence Learning    ************** \n ");
            Console.WriteLine("**************  Option - 1 - Cancer_Prediction             ************** ");
            Console.WriteLine("**************  Option - 2 - Power_Consumption_Prediction  ************** ");

            Console.WriteLine("\n");
            Console.WriteLine("Please Select a option to Continue with MultiSequence Experiment");
            string input = Console.ReadLine();      // Read user input (always returns string)
            int userInput = int.Parse(input);

            /// <summary> ///
            /// Displays a menu prompting the user to select an experiment option for the multi-sequence learning project. 
            /// <summary> ///
            switch (userInput)
            {
                case 1:
                    Console.WriteLine("User Selected MultiSequence Experiment - Cancer_Prediction\n");
                    CanPrediction();
                    break;
                case 2:
                    Console.WriteLine("User Selected MultiSequence Experiment - Power_Consumption_Prediction\n");
                    IntegerPrediction();
                    break;

                default:
                    Console.WriteLine("User Entered Invalid Option");
                    break;

            }
        }

        /// <summary> ///
        /// This method calls the RunAll method of the CancerPrediction class with the specified dataset and testset JSON files. 
        /// </summary>
        private static void CanPrediction()
        {
            CancerPrediction.RunAll("dataset_04.json", "test_02.json");

        }

        private static void IntegerPrediction()
        {
            //Reading Input Dataset
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            List<Sequence> sequences = ReadDataset(Path.Combine(basePath, DatasetFolder, DatasetFileName));
            //Reading Test  dataset
            List<Sequence> sequencesTest = ReadDataset(Path.Combine(basePath, DatasetFolder, TestsetFileName));
            //Train & test
            List<Report> reports = RunMultiSequenceLearningExperiment(sequences, sequencesTest);
            //Save the final predictions/accuracy into a text report
            WriteReport(reports, basePath);

        }

        /// <summary> ///
        /// Reads a JSON dataset from the specified file path and deserializes it into a list of Sequence objects.
        /// If an error occurs during the reading or deserialization process, it logs the error and returns an empty list. 
        /// </summary> ///
        private static List<Sequence> ReadDataset(string datasetPath)
        {
            try
            {
                Console.WriteLine($"Reading Dataset: {datasetPath}");
                return JsonConvert.DeserializeObject<List<Sequence>>(File.ReadAllText(datasetPath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading dataset: {ex.Message}");
                return new List<Sequence>();
            }
        }

        /// <summary>
        /// Saves the final predictions and accuracies to a text file.
        /// </summary>
        private static void WriteReport(List<Report> reports, string basePath)
        {
            string reportFolder = EnsureDirectory(Path.Combine(basePath, ReportFolder));
            string reportPath = Path.Combine(reportFolder, $"report_{DateTime.Now.Ticks}.txt");

            using (StreamWriter sw = File.CreateText(reportPath))
            {
                foreach (Report report in reports)
                {
                    WriteReportContent(sw, report);
                }
            }
        }

        /// <summary>
        /// Ensures that the directory at the specified path exists.
        /// If the directory does not exist, it creates the directory.
        /// Returns the provided path after confirming its existence.
        /// </summary>
        private static string EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// Write the test sequence information, including its name and the sequence data
        /// Iterate over each prediction log entry and write it as an indented line for clarity
        /// Write the overall accuracy of the predictions for this test sequence
        /// </summary>
        
        private static void WriteReportContent(StreamWriter sw, Report report)
        {
            sw.WriteLine("**************");
            sw.WriteLine($"Using test sequence: {report.SequenceName} -> {string.Join("-", report.SequenceData)}");
            foreach (string log in report.PredictionLog)
            {
                sw.WriteLine($"\t{log}");   
            }
            sw.WriteLine($"\tAccuracy: {report.Accuracy}%");
            sw.WriteLine("**************");
        }

        /// <summary>
        /// Iterate through each test sequence.
        /// Set the report's sequence name and data
        /// Calculate prediction accuracy for the current test sequence.
        /// Store the calculated accuracy in the report.
        /// </summary>
        private static List<Report> RunMultiSequenceLearningExperiment(List<Sequence> sequences, List<Sequence> sequencesTest)
        {
            var reports = new List<Report>();
            var experiment = new MultiSequenceLearning();
            var predictor = experiment.Run(sequences);

            foreach (Sequence item in sequencesTest)
            {
                var report = new Report
                {
                    SequenceName = item.name,
                    SequenceData = item.data
                };
                Console.WriteLine("*******#######*******");
                Console.WriteLine($"Test Sequence  {item.name}: {string.Join(", ", item.data)}");
                double accuracy = PredictNextElement(predictor, item.data, report);
                report.Accuracy = accuracy;
                reports.Add(report);

                Console.WriteLine($"Accuracy for {item.name} sequence: {accuracy}%");
            }

            return reports;
        }

        /// <summary>
        /// Runs the predictor on a test sequence to measure accuracy.
        /// </summary>
        private static double PredictNextElement(Predictor predictor, int[] list, Report report)
        {
            int matchCount = 0, predictions = 0;
            double accuracy = 0.0;
            List<string> logs = new List<string>();

            // Reset the predictor's internal state between sequences
            predictor.Reset();

            // For each pair of (current, next) in the test data
            for (int i = 0; i < list.Length - 1; i++)
            {
                int current = list[i];
                int next = list[i + 1];

                logs.Add(PredictElement(predictor, current, next, ref matchCount));
                predictions++;
            }

            report.PredictionLog = logs;
            return CalculateAccuracy(matchCount, predictions);
        }

        /// <summary>
        /// Predicts the next element for a given input value using the provided predictor.
        /// It logs the input and the predicted sequence, determines the predicted next element
        /// increments the match count if the prediction is correct.
        /// </summary>
        private static string PredictElement(Predictor predictor, int current, int next, ref int matchCount)
        {
            Console.WriteLine($"Input: {current}");
            var predictions = predictor.Predict(current);
            if (predictions.Any())
            {
                // Sort by highest similarity
                var highestPrediction = predictions.OrderByDescending(p => p.Similarity).First();
                string predictedSequence = highestPrediction.PredictedInput.Split('-').First();
                int predictedNext = int.Parse(highestPrediction.PredictedInput.Split('-').Last());

                Console.WriteLine($"Predicted Sequence: {predictedSequence} - Predicted next element: {predictedNext}");
                if (predictedNext == next)
                    matchCount++;

                return $"Input: {current}, Predicted Sequence: {predictedSequence}, Predicted next element: {predictedNext}";
            }
            else
            {
                Console.WriteLine("No Prediction");
                return $"Input: {current}, No Prediction";
            }
        }

        //Summary
        //Accuracy is calculated as number of matching predictions made 
        //divided by total number of prediction made for an element in subsequence
        //accuracy = number of matching predictions/total number of prediction * 100
        //Summary
        private static double CalculateAccuracy(int matchCount, int predictions)
        {
            double accuracy = 0.0;
            accuracy = (double)matchCount / predictions * 100;
            Console.WriteLine("*******#######*******");

            return accuracy;
        }
    }
}
