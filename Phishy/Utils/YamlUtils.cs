using Phishy.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Phishy.Utils
{
    public class YamlUtils
    {
        public static void GenerateSampleRunConfig(string fileName)
        {
            Properties properties = new();

            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();
            string yaml = serializer.Serialize(properties);

            FileUtils.SaveFileInCurrentDirectory(fileName, yaml);

            Console.WriteLine($"[YamlUtils]: Generated sample YAML config with name: {fileName}.");
        }

        public static Properties? ReadPropertiesFromCurrentDirectory(string fileName)
        {
            string yamlContent = FileUtils.ReadFileFromCurrentDirectory(fileName);

            IDeserializer deserializer = new DeserializerBuilder()
                .WithDuplicateKeyChecking()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();

            try
            {
                return deserializer.Deserialize<Properties>(yamlContent);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"[YamlUtils]: The file '{fileName}' does not exist.");
            }
            catch (YamlDotNet.Core.YamlException ex)
            {
                string errorMsg = ex.Message;
                string errorToMatch = " not found";
                if (errorMsg.Contains(errorToMatch))
                {
                    var index = errorMsg.IndexOf(errorToMatch, StringComparison.Ordinal);
                    errorMsg = errorMsg.Substring(0, index) + " is invalid!";
                }
                Console.WriteLine($"[YamlUtils]: Error while deserializing YAML: {errorMsg}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[YamlUtils]: An error occurred: {ex.Message}");
            }

            return null;
        }
    }
}
