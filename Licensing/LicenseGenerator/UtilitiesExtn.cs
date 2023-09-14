using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace CommonUtilities
{
    internal static partial class Utilities
    {
        public static void Serialize<T>(this T value, string filePath, bool encrypt = false)
        {
            try
            {
                var folder = Path.GetDirectoryName(filePath);
                Debug.Assert(folder != null, "folder != null");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var writer = new StreamWriter(filePath);
                var xmlData = Serialize<T>(value);
                if(encrypt)
                {
                    xmlData = StringCipher.Encrypt(xmlData);
                }
                writer.Write(xmlData);
                writer.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }
        public static string Serialize<T>(this T value)
        {
            try
            {
                var xmlserializer = new XmlSerializer(typeof(T));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, value);
                }
                stringWriter.Close();
                return stringWriter.GetStringBuilder().ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }
    }
}