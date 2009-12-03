using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace CodeGenHelpers
{
    [Serializable]
    public struct OptionsDto
    {
        public string JobType;
        public string Server;
        public string ArchPath;
    }
    public static class CodeGenHelper
    {
        public static void DumpObjectToFile(object o, StreamWriter fs) 
        {
            XmlSerializer xs = new XmlSerializer(o.GetType());
            xs.Serialize(fs, o);
        }

        public static T GetObjectFromFile<T>(StreamReader fs)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            T result = (T)xs.Deserialize(fs);
            return result;
        }
    }
}
