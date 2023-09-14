using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace PanelLayout_App
{
    internal sealed class Identifier
    {
        private const string FILE_PATH = @"C:\Serhiy\Projects\Real\Data\Info\identify.txt";

        internal static List<string> UniqueData { get; set; } = new List<string>();

        internal static void AddResult(string moduleName, string methodName, string itemCode, string description, string elevation)
        {
            return;

            string data = moduleName + methodName + itemCode + elevation;

            if (UniqueData.Contains(data))
                return;

            UniqueData.Add(data);

            List<string> results = new List<string>() {
                $"Module: {moduleName}",
                $"Method: {methodName}",
                $"Item code: {itemCode}",
                //$"Description: {description}",
                $"Elevation: {elevation}",
                string.Empty
            };

            File.AppendAllLines(
                    FILE_PATH,
                    results
            );
        }

        internal static void ShowData(string layer, string elevation)
        {
            string data = layer + elevation;

            if (UniqueData.Contains(data))
                return;

            UniqueData.Add(data);

            MessageBox.Show(
                $"Layer: {layer}" +
                $"Elevation: {elevation}"
            );
        }
    }
}
