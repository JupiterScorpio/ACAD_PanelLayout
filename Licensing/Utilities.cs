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
        public static T Deserialize<T>(string filePath, bool decrypt = false)
        {
            if (!File.Exists(filePath))
                return default(T);

            try
            {
                var reader = new StreamReader(filePath);
                var content = reader.ReadToEnd();
                if(decrypt)
                {
                    content = StringCipher.Decrypt(content);
                }
                object obj = DeserializeString<T>(content);
                reader.Close();

                return (T)obj;
            }
            catch (Exception ex)
            {
#if _LICVALIDATE_DEBUG_MSGSON_
                System.Windows.Forms.MessageBox.Show("Exception occurred in deserializing object - " + ex.Message);
#else
                System.Diagnostics.Debug.Print("Exception occurred - " + ex.Message);
#endif
                //throw new Exception("An error occurred", ex);
            }
            return default(T);
        }

        public static T DeserializeString<T>(string content)
        {
            try
            {
                var xmlserializer = new XmlSerializer(typeof(T));
                var reader = new StringReader(content);
                object obj;
                using (var writer = XmlReader.Create(reader))
                {
                    obj = xmlserializer.Deserialize(writer);
                }
                reader.Close();

                return (T)obj;
            }
            catch (Exception ex)
            {
#if _LICVALIDATE_DEBUG_MSGSON_
                System.Windows.Forms.MessageBox.Show("Exception occurred in deserializing content - " + ex.Message);
#else
                System.Diagnostics.Debug.Print("Exception occurred - " + ex.Message);
#endif
            }
            return default(T);
        }
    }
}