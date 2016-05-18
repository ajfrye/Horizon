﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class DeepCopy
    {
        //Make a deep copy of any class as long as it is marked as [Serializable]
        //http://www.codeproject.com/Articles/28952/Shallow-Copy-vs-Deep-Copy-in-NET
        public static T Copy<T>(T item)
        {
            if (typeof(T).Equals(typeof(Matrix<double>)))
            {
                Console.WriteLine("trying to copy matrix");
            }

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, item);
            stream.Seek(0, SeekOrigin.Begin);
            T result = (T)formatter.Deserialize(stream);
            stream.Close();
            return result;
        }
    }
}
