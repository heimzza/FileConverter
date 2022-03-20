using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _04_FileConverter
{
    public static class FileConversionManager
    {
        // registered converters list
        public static List<IFileConverter> ConverterList { get; private set; }

        // registering converters
        public static void RegisterFileConverter(IFileConverter fileConverter)
        {
            if (ConverterList == null)
                ConverterList = new List<IFileConverter>();

            ConverterList.Add(fileConverter);
        }

        // converting format a to b
        public static object Convert(object input, string inputFileFormat, string outputFileFormat)
        {
            if (ConverterList == null)
                return new NullReferenceException("No converter has registered. Please register a converter.");

            // if there is a perfect match converter 
            var idealConverter = ConverterList.Where(c => c.InputFileFormat == inputFileFormat && c.OutputFileFormat == outputFileFormat).FirstOrDefault();
            if (idealConverter != null)
                return idealConverter.Convert(input);

            // while there is no perfect match and we have only 1 converter
            if(ConverterList.Count < 2)
            {
                return new NotSupportedException(String.Format("Converting {0} to {1} is not supported.", inputFileFormat, outputFileFormat));
            }

            // when we don't have any converter with given input or output
            if (ConverterList.Where(c => c.InputFileFormat == inputFileFormat).FirstOrDefault() == null ||
                    ConverterList.Where(c => c.OutputFileFormat == outputFileFormat).FirstOrDefault() == null)
                return new NotSupportedException(String.Format("Converting {0} to {1} is not supported.", inputFileFormat, outputFileFormat));

            // if we can make a 2 converter long route
            foreach (var converter in ConverterList)
            {
                if (converter.InputFileFormat == inputFileFormat)
                {
                    var secondConverter = ConverterList.Where(c => c.InputFileFormat == inputFileFormat).FirstOrDefault();
                    if (secondConverter != null)
                        return secondConverter.Convert(converter.Convert(input));
                }
            }

            // when we can't find perfect match or 2 converter long chain
            ConverterList = FindBestChain(inputFileFormat, outputFileFormat);

            if(ConverterList.Count > 0)
            {
                return InitChains(input, ConverterList);
            }

            // we didn't find any solution
            return new NotSupportedException(String.Format("Converting {0} to {1} is not supported.", inputFileFormat, outputFileFormat));
        }

        // finding best possible chained converters
        private static List<IFileConverter> FindBestChain(string inputFileFormat, string outputFileFormat)
        {
            var tempListInput = new List<IFileConverter>();
            foreach (var converter in ConverterList) // find all possible chains
            {
                if (converter.InputFileFormat == inputFileFormat)
                {
                    tempListInput.Add(converter);
                }
            }

            var tempListOutput = new List<IFileConverter>();

            var converterOutput = tempListInput.Where(c => c.OutputFileFormat == outputFileFormat).FirstOrDefault(); // find if there is a final chain
            if (converterOutput != null)
            {
                tempListOutput.Add(converterOutput);
                return tempListOutput; // return the last chain
            }

            List<List<IFileConverter>> fileConverters = new List<List<IFileConverter>>(); // to find best route

            foreach (var converter in tempListInput)
            {
                var result = FindBestChain(converter.OutputFileFormat, outputFileFormat);
                if (result.Count == 1) // if it is the last chain return it
                {
                    List<IFileConverter> fileConvertersLoop = new List<IFileConverter>();
                    fileConvertersLoop.Add(converter);
                    fileConvertersLoop.AddRange(result);
                    return fileConvertersLoop;
                }
                else if (result.Count > 1) // if not the last chain add it to list of list
                {
                    List<IFileConverter> fileConvertersLoop = new List<IFileConverter>();
                    fileConvertersLoop.Add(converter);
                    fileConvertersLoop.AddRange(result);
                    fileConverters.Add(fileConvertersLoop);
                }
            }

            int minCount = -1;
            int minCountListIndex = -1;
            for (int i = 0; i < fileConverters.Count; i++) // find shortest route
            {
                if (minCount == -1)
                {
                    minCount = fileConverters[i].Count;
                    minCountListIndex = i;
                }
                else if (fileConverters[i].Count < minCount)
                {
                    minCount = fileConverters[i].Count;
                    minCountListIndex = i;
                }
            }

            if (minCountListIndex == -1) // if there is no route
                return new List<IFileConverter>();

            return fileConverters[minCountListIndex]; // return shortest route
        }

        // convert with given list of chained converters
        private static object InitChains(object input, List<IFileConverter> fileConverters)
        {
            var converter = fileConverters.LastOrDefault();
            if (fileConverters.Count == 1)
                return converter.Convert(input);

            fileConverters.Remove(converter);

            return converter.Convert(InitChains(input, fileConverters));
        }
    }
}
