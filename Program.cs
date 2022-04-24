using System.Collections.Concurrent;
/// <summary>
/// This program answers to :
/// Write a method that will produce three (3) different strings (different string values) that have the same hash code.
/// 
/// This console app .Net 6 so the program main is implicit.
/// </summary>

//Custom range of char valid in generated string (only ASCII symbol/letter/number displayable in the console).
const int MinCharAcceptedValue = 32; 
const int MaxCharAcceptedValue = 126;

//Custom range of length of generated string (in order to limit the length of stored object in memory).
const int MinLengthValue = 2;
const int MaxLengthValue = 30;

/// <summary>
/// Keep the generated strings by hash code.
/// Common to all the threads.
/// </summary>
ConcurrentDictionary<int, ConcurrentBag<string>> _generatedStringsByHashCode = new ConcurrentDictionary<int, ConcurrentBag<string>>();

/// <summary>
/// This variable will be assign when the problem is solved, it keeps the solution hash code.
/// </summary>
int? hashcodeWith3strings = null;

DateTime startInstant = DateTime.Now;
try
{
    // Generate strings of differents sizes in parallel.
    Parallel.For(MinLengthValue, MaxLengthValue, (length, state) =>
    {
        //For each size we will genrate new string utils we have 3 string we same hash code
        while (hashcodeWith3strings == null)
        {
            var newGeneratedString = GenerateString(length);
            //keep the generated string in the dictionary in order to find another string with the same hash code
            _generatedStringsByHashCode.AddOrUpdate(
                newGeneratedString.GetHashCode(),
                new ConcurrentBag<string>(new[] { newGeneratedString }),
                //Update if there is already an entry for this hashcode
                (key, value) =>
                {
                    if (!value.Contains(newGeneratedString))
                    {
                     //Add the value only if it is different.
                     value.Add(newGeneratedString);
                     //Test the end condition in the update (thread safe)
                     if (value.Count == 3)
                        {
                         //End condition validated, assign the hascode variable
                         hashcodeWith3strings = newGeneratedString.GetHashCode();
                         //stop the parallel executions
                         state.Stop();
                        }
                    }
                 //Always return the updated concurrentBag.
                 return value;
                });
        }
    });

    if (_generatedStringsByHashCode.TryGetValue(hashcodeWith3strings.GetValueOrDefault(), out var strings))
    {
        //Here we have a successful result
        foreach (var s in strings)
            Console.WriteLine($"Hashcode : {s.GetHashCode()} for string (length={s.Length}) \"{s}\"");
    }
    else
    {
        //Here all iterations are ended but not result was found, maybe increase the max length.
        Console.WriteLine($"No luck this time.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error : {ex.Message}");
}
finally
{
    DateTime endInstant = DateTime.Now;
    Console.WriteLine($"Total duration : {endInstant - startInstant}");
}

/// <summary>
/// Generate a string of specify length (or random length if not specify)
/// </summary>
string GenerateString(int? length = null)
{
    var random = new Random();
    length ??= random.Next(MinLengthValue, MaxLengthValue);
    return string.Concat(new char[length.Value].Select(c => (char)random.Next(MinCharAcceptedValue, MaxCharAcceptedValue)));
}