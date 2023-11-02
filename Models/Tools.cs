using System.Collections.Generic;

namespace EcTools.Models
{
    public class Tools
    {
        public static List<string> ReplaceSpecialCharacters(string[] inputStrings)
        {
            List<string> outputStrings = new List<string>();

            foreach (var str in inputStrings)
            {
                string modifiedStr = str
                    .Replace('é', 'e')
                    .Replace('è', 'e')
                    .Replace('ê', 'e')
                    .Replace('ë', 'e')
                    .Replace('à', 'a')
                    .Replace('â', 'a')
                    .Replace('ä', 'a')
                    .Replace('ç', 'c')
                    .Replace('î', 'i')
                    .Replace('ï', 'i')
                    .Replace('ô', 'o')
                    .Replace('ö', 'o')
                    .Replace('ù', 'u')
                    .Replace('û', 'u')
                    .Replace('ü', 'u')
                    .Replace('ÿ', 'y');
                outputStrings.Add(modifiedStr);
            }

            return outputStrings;

        }

        public static string[] UpdateDuplicates(string[] array)
        {
            Dictionary<string, int> valueCounts = new Dictionary<string, int>();
            string[] result = new string[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                string value = array[i];

                if (!valueCounts.ContainsKey(value))
                {
                    valueCounts[value] = 1;
                }
                else
                {
                    valueCounts[value]++;
                    value = $"{value}_{valueCounts[value]}";
                }

                result[i] = value;
            }

            return result;
        }
    }
}
