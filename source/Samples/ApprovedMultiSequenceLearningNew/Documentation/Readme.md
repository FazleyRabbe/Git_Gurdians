# ML23/24-09   Approve Prediction of Multisequence Learning 

## Introduction
Multi-sequence learning in Hierarchical Temporal Memory (HTM) is concerned with making a computational model learn and predict patterns from many sequences of time-series data. In this paper, we present a new extension to the HTM pipeline that directly reads training sequences from JSON files, making it easy to ingest large-scale data. The data is encoded into Sparse Distributed Representations (SDRs) using a scalar encoder, which preserves both Character and numerical range, but also the temporal sequence ordering of every sequence. The SDRs are passed to the Spatial Pooler (SP) for learning stable representations of input patterns, which are passed to the Temporal Memory (TM) for encoding longer-term sequential dependencies.
After the model finishes its repeated training process, it will automatically start loading test sequences from separate datasets. This setup lets the system predict the next element in each sequence and calculate an overall accuracy. We evaluate this multi-sequence approach in Two different fields - cancer prediction and power consumption prediction with HTM showing the patterns and trends within each dataset. By combining an HTM-based process with simple JSON data handling, the project provides a strong and easy-to-use platform for sequence modeling in many different areas.

## System Requirements

- **Operating System**: Windows 10+ or macOS
- **Processor**: Multi-core processor recommended
- **Memory**: 8GB RAM minimum, 16GB recommended
- **Storage**: 2GB for installation, additional space for processing
- **Dependencies**: .NET 8.0 SDK and runtime
  
## Implementation

### Prerequisites

Ensure you have the following installed on your system:

- .NET SDK (8.0 or later)** – [Download Here](https://dotnet.microsoft.com/en-us/download)

- For code debugging, we recommend using Visual Studio 2022/visual studio code IDE.

### 1. fork from

```sh
https://github.com/ddobric/neocortexapi.git
```
### 2. Clone the repository
```sh
https://github.com/FazleyRabbe/Git_Gurdians.git
```
To clone the repository at first you have to create a folder in your local drive. Then you have to open visual studio 2022 and select the `Clone a repository`. After that, you have to put the `repository location` and the `folder path` and clone it.

### 3. Open Project
Go to your project folder, then inside the `source folder` you will get the `NeoCortexApi.sln`, then open it.

### 4. Run the project
First, Build your project, then select the `ApprovedMultiSequenceLearningNew ` and run the project. 

### 5. Install Required NuGet Packages
You have to install every required package from Visual Studio 2022.

## Workflow Diagram

![image](https://github.com/FazleyRabbe/Git_Gurdians/blob/master/source/Samples/ApprovedMultiSequenceLearningNew/Documentation/Image%20and%20Result/flowchart%20diagram.png)

Fig 1: Architecture of Approve Prediction of Multisequence Learning

Above is our project's implementation flow.

At first, the model gets the dataset as a sequence from our JSON dataset folder by `sequence.cs`

For power consumption prediction:
```csharp
public class Sequence
{
    public String name { get; set; }
    public int[] data { get; set; }
}
```
For Cancer prediction:
```csharp
public class Sequence
{
    public string name { get; set; }
    public char[] data { get; set; }
}
```


- Sample Dataset:

For power consumption prediction:
```json
[
  {
    "name": "S1",
    "data": [ 0, 5, 6, 7, 8, 10 ]
  },
  {
    "name": "S2",
    "data": [ 4, 6, 11, 12, 13 ]
  },
  {
    "name": "S3",
    "data": [ 8, 1, 2, 3, 4 ]
  },
  {
    "name": "S4",
    "data": [ 2, 3, 4, 7, 8, 10, 11 ]
  }
]
```
For Cancer prediction:
```json
[
  {
    "name": "S1",
    "data": [ "E", "F", "G", "H", "I", "J", "K", "L", "M" ]
  },
  {
    "name": "S2",
    "data": [ "P", "Q", "R", "S" ]
  },
  {
    "name": "S3",
    "data": [ "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" ]
  },
  {
    "name": "S4",
    "data": [ "C", "D", "G", "H", "J", "K", "L", "M" ]
  }
]
```


- Test Dataset:

For power consumption prediction:
```json
[
  {
    "name": "Test1",
    "data": [ 5, 6, 7, 8 ]
  },
  {
    "name": "Test2",
    "data": [ 6, 11, 12, 13 ]
  },
  {
    "name": "Test3",
    "data": [ 1, 2, 3, 4 ]
  },
  {
    "name": "Test4",
    "data": [ 3, 4, 7, 8, 10 ]
  }

]
```
For Cancer prediction:
```json
[
  {
    "name": "Test1",
    "data": [ "E", "F", "G", "H" ]
  },
  {
    "name": "Test2",
    "data": [ "P", "Q", "R", "S" ]
  },
  {
    "name": "Test3",
    "data": [ "A", "B", "C", "D" ]
  },
  {
    "name": "Test4",
    "data": [ "C", "D", "G", "H" ]
  }
]
```


- Our implemented methods are in `HelperMethod.cs` and can be found [here](https://github.com/FazleyRabbe/Git_Gurdians/blob/master/source/Samples/ApprovedMultiSequenceLearningNew/HelperMethods.cs):

### 1. FetchHTMConfig()

We save the HTMConfig, which is utilized for Hierarchical Temporal Memory, to 'Connections'. In two different cases, we have to change some values in variables. Also, used different methods for better prediction.


For power consumption prediction:
```csharp
  public static HtmConfig FetchEnhancedHTMConfig(int inputBits, int numColumns)
 {
     return new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
     {
         Random = new ThreadSafeRandom(42),
         CellsPerColumn = 32,
         GlobalInhibition = false,
         LocalAreaDensity = 0.03,
         NumActiveColumnsPerInhArea = 0.02 * numColumns,
         PotentialRadius = (int)(0.5 * inputBits),
         MaxBoost = 10.0,
         DutyCyclePeriod = 25,
         MinPctOverlapDutyCycles = 0.75,
         MaxSynapsesPerSegment = (int)(0.02 * numColumns),
         ActivationThreshold = 10,
         ConnectedPermanence = 0.2,
         PermanenceDecrement = 0.01,
         PermanenceIncrement = 0.03,
         PredictedSegmentDecrement = 0.005,
     };

 }
```

For  prediction:
```csharp
  private static HtmConfig FetchHTMConfig(int inputBits, int numColumns)
 {
     return new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
     {
         Random = new ThreadSafeRandom(42),
         CellsPerColumn = 25,
         GlobalInhibition = true,
         LocalAreaDensity = -1,
         NumActiveColumnsPerInhArea = 0.02 * numColumns,
         PotentialRadius = (int)(0.15 * inputBits),
         MaxBoost = 10.0,
         DutyCyclePeriod = 25,
         MinPctOverlapDutyCycles = 0.75,
         MaxSynapsesPerSegment = (int)(0.02 * numColumns),
         ActivationThreshold = 15,
         ConnectedPermanence = 0.5,
         PermanenceDecrement = 0.25,
         PermanenceIncrement = 0.15,
         PredictedSegmentDecrement = 0.1
     };
 }
```
All of the fields are self-explanatory according to HTM theory.

### 2. getEncoder()

We used 'ScalarEncoder' because the scalar encoder, which is the primary encoder of the HTM system, accomplishes this by mapping continuous scalar inputs into a fixed-length binary vector through a process of dividing the input range into overlapping buckets, with each bucket setting a specific set of bits. Remember, 'inputBits' is the same as 'HTMConfig'.


```csharp
 public static EncoderBase GetEnhancedEncoder(int inputBits)  
 {
     var settings = new Dictionary<string, object>
     {
         { "W", 21 },
         { "N", inputBits },
         { "Radius", -1.0 },
         { "MinVal", 0.0 },
         { "Periodic", false },
         { "Name", "scalar" },
         { "ClipInput", false },
         { "MaxVal", 20.0 }
     };

     return new ScalarEncoder(settings);
 }
```
Keep in mind that the `MaxValue` for the encoder is set to `20`, which can be changed but must be matched when producing the synthetic dataset. For Cancer Prediction, the `MaxValue` will be `26` because of 'A' => 0, 'Z' => 25. 

### 3. ReadDataset()

When supplied as a whole path, this function reads the JSON file and returns an object from the `Sequence` list.

```csharp
public static List<Sequence> ReadDataset(string path)
{
    Console.WriteLine("Reading Sequence...");
    try
    {
        string fileContent = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<List<Sequence>>(fileContent);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to read the dataset: {ex.Message}");
        return new List<Sequence>(); // Return an empty list in case of failure
    }
}
```

### 4. GenerateSequence()

By automating the process, we have improved our approach to creating datasets by doing away with the necessity for laborious manual involvement. With this enhanced method, datasets are created according to predefined criteria. These are size, which establishes the length of each sequence, startVal and endVal, which set the beginning and finishing range values for the sequences, and numberOfSequence, which establishes the number of sequences to be constructed. This simplified process improves dataset-generating accuracy and efficiency. We have done the process by two different methods. 

```csharp
public static List<Sequence> CreateSequences(int count, int size, int startVal, int stopVal)
{
    return Enumerable.Range(1, count).Select(i =>
        new Sequence
        {
            name = $"S{i}",
            data = GenerateRandomSequence(size, startVal, stopVal)
        })
        .ToList();
}

private static int[] GenerateRandomSequence(int size, int startVal, int stopVal)
{
    var rnd = new Random();
    var sequence = new HashSet<int>();

    while (sequence.Count < size)
    {
        int number = rnd.Next(startVal, stopVal + 1);
        sequence.Add(number);
    }

    return sequence.OrderBy(n => n).ToArray();
}
```
Keep in mind that {endVal} needs to be smaller than {MaxVal} of the `ScalarEncoder` that was used previously.

### 5. SaveDataset()

Stores the dataset in the application's running application's `dataset` directory under the `BasePath`.


```csharp
 public static string SaveDataset(List<Sequence> sequences)
 {
     string basePath = AppDomain.CurrentDomain.BaseDirectory;
     string datasetFolder = Path.Combine(basePath, "dataset");
     Directory.CreateDirectory(datasetFolder); // CreateDirectory is safe to call if directory exists
     string datasetPath = Path.Combine(datasetFolder, $"dataset_{DateTime.Now.Ticks}.json");

     Console.WriteLine("Saving dataset...");
     File.WriteAllText(datasetPath, JsonConvert.SerializeObject(sequences));
     return datasetPath;
 }
```

- Our Changed and added methods are in `Program.cs` are given below and can be found [here](https://github.com/FazleyRabbe/Git_Gurdians/blob/master/source/Samples/ApprovedMultiSequenceLearningNew/Program.cs):

### 6. Calculating accuracy in PredictNextElement() 

```csharp
int matchCount = 0;
int predictions = 0;
 private static double CalculateAccuracy(int matchCount, int predictions)
 {
     double accuracy = 0.0;
     accuracy = (double)matchCount / predictions * 100;
     Console.WriteLine("*******#######*******");

     return accuracy;
 }
```
### 7. main() 
We have changed the main method for choosing different cases. Users can choose one option for their prediction. 


```csharp
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
```
- Our implemented methods in `CancerPrediction.cs` and can be found [here](https://github.com/FazleyRabbe/Git_Gurdians/blob/master/source/Samples/ApprovedMultiSequenceLearningNew/CancerPrediction.cs). We Added this class for predicting cancer sequence in the character data set. We have use all functions from other classes with different data types for characters. We have added one indexing method for characters to Index.
### 8. CharToIndex()

```csharp
private static int CharToIndex(char c)
{
    c = char.ToUpperInvariant(c);
    return c - 'A';  // 'A' => 0, 'Z' => 25
}
```

## How to run the project


### To run the experiment

1. Select `ApprovedMultiSequenceLearningNew` as the startup project when you open [NeoCortexApi.sln](https://github.com/FazleyRabbe/Git_Gurdians/blob/master/source/NeoCortexApi.sln).

2. The `Main()` is located in `Program.cs`. As you can see below, comment-out the `dataset` file that was saved from the last run. Select one Data set:

```csharp
private const string DatasetFileName = "dataset_01.json";
//private const string DatasetFileName = "dataset_03.json";
//private const string DatasetFileName = "dataset_02.json";
//private const string DatasetFileName = "dataset_04.json";
private const string TestsetFileName = "test_01.json";
//private const string TestsetFileName = "test_02.json";
```
3. Now Start Debugging `ApprovedMultiSequenceLearningNew`. You will be see in console there have 2 option for predicton as you can see in given image:

   ![image](https://github.com/FazleyRabbe/Git_Gurdians/blob/master/source/Samples/ApprovedMultiSequenceLearningNew/Documentation/Image%20and%20Result/User%20Select%20white%20background.jpg)
   
Fig. 2: User choosen options for prediction.

Press `1` for Cancer Prediction then `Enter`

Press `2` for Power_Consumption_Prediction then `Enter`
       

## Results
We have used a range of datasets to do as much experimentation as is practical. We purposely kept the dataset sizes small and the sequence lengths short in order to account for the significant execution time.

- Here the given figure shown the prediction of next element for the Cancer Prediction scenario. For example, the sequence `CDGH` predicted the next elements with 100% accuracy. 

![results](https://github.com/FazleyRabbe/Git_Gurdians/blob/master/source/Samples/ApprovedMultiSequenceLearningNew/Documentation/Image%20and%20Result/Cancer%20Prediction%20white%20Background.jpg)
    
Fig. 3: Prediction of next elements with accuracy for Cancer Prediction scenario

- The figure shows the Prediction of the next elements with accuracy for the Power Consumption scenario. For example the sequence `3, 4, 7, 8, 9` predicted next elements with 100% accuracy.

![results](https://github.com/FazleyRabbe/Git_Gurdians/blob/master/source/Samples/ApprovedMultiSequenceLearningNew/Documentation/Image%20and%20Result/Power_Consumption%20white%20Background.jpg)
    
Fig. 4: Prediction of next elements with accuracy for Power_Consumption_Prediction scenario

## Conclusion
In conclusion, our research emphasizes the importance of Multisequence Learning in enhancing predictive analytics across multiple domains. We demonstrated the potential of a novel strategy for automating sequence extraction and prediction testing from .JSON files to increase efficiency and accuracy when compared to manual approaches. Our findings demonstrate the technique's application across a wide range of businesses, from power consumption forecasting to cancer prediction. Through testing, we proved the effectiveness of our technique in properly predicting sequences, confirming its value for a wide range of applications. In essence, our study not only explains the principles of Multisequence Learning, but it also pioneers a streamlined way for propelling sequence prediction toward speedier and more reliable outcomes, supporting innovation and development in predictive modeling paradigms.

## Reference

- [Hawkins, J., & Blakeslee, S. (2004). On Intelligence. Times Books](https://doi.org/10.2514/1.18111) 

