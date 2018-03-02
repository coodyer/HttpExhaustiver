using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text;

namespace HttpExhaustiver.handle
{
    class JsonHandle
    {
        public static Dictionary<String, Assembly> assemblyMap = new Dictionary<string, Assembly>();
        private static Type jsonHandleType = null;
        private static byte[] resourceBytes = Properties.Resources.Newtonsoft_Json_net2_0;

        public static bool init()
        {
            if (jsonHandleType != null)
            {
                return true;
            }
            try
            {
                if (jsonHandleType == null)
                {
                    Assembly assembly = DllLoader.loadDll(resourceBytes);
                    jsonHandleType = assembly.GetType("Newtonsoft.Json.JsonConvert");
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public static string toJson(object value)
        {
            try
            {
                init();
                return (string)jsonHandleType.InvokeMember("SerializeObject", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { value });
            }
            catch
            {
                return "";
            }

        }
        public static string toObject(object value)
        {
            init();
            return (string)jsonHandleType.InvokeMember("SerializeObject", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { value });
        }

        public static object toBean<T>(string value)
        {
            init();
            return jsonHandleType.InvokeMember("DeserializeObject", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { value, typeof(T), null });
        }


        /// <summary>
        /// JSON字符串格式化
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string JsonTree(string json)
        {
            int level = 0;
            var jsonArr = json.ToCharArray();　　// Using System.Linq;
            string jsonTree = string.Empty;
            for (int i = 0; i < json.Length; i++)
            {
                char c = jsonArr[i];
                if (level > 0 && '\n' == jsonTree.ToCharArray()[jsonTree.Length - 1])
                {
                    jsonTree += TreeLevel(level);
                }
                switch (c)
                {
                    case '[':
                        jsonTree += c + "\n";
                        level++;
                        break;
                    case ',':
                        jsonTree += c + "\n";
                        break;
                    case ']':
                        jsonTree += "\n";
                        level--;
                        jsonTree += TreeLevel(level);
                        jsonTree += c;
                        break;
                    default:
                        jsonTree += c;
                        break;
                }
            }
            return jsonTree;
        }
        /// <summary>
        /// 树等级
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private static string TreeLevel(int level)
        {
            string leaf = string.Empty;
            for (int t = 0; t < level; t++)
            {
                leaf += "\t";
            }
            return leaf;
        }
        public static class DllLoader
        {
            public static Assembly loadDll(byte[] resource)
            {

                resource = Decompress(resource);
                Assembly assembly = Assembly.Load(resource);
                return assembly;
            }
            public static byte[] getFileByte(String path)
            {
                FileStream fs = new FileStream(path, FileMode.Open);
                long size = fs.Length;
                byte[] array = new byte[size];
                fs.Read(array, 0, array.Length);
                fs.Close();
                return array;
            }
            public static void writeFileByte(String path, byte[] array)
            {
                FileStream fs = new FileStream(path, FileMode.Create);
                fs.Write(array, 0, array.Length);
                fs.Close();
            }
            public static byte[] Compress(byte[] rawData)
            {
                MemoryStream ms = new MemoryStream();
                GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress, true);
                compressedzipStream.Write(rawData, 0, rawData.Length);
                compressedzipStream.Close();
                return ms.ToArray();
            }
            public static byte[] Decompress(byte[] zippedData)
            {
                MemoryStream ms = new MemoryStream(zippedData);
                GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Decompress);
                MemoryStream outBuffer = new MemoryStream();
                byte[] block = new byte[1024];
                while (true)
                {
                    int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                    if (bytesRead <= 0)
                        break;
                    else
                        outBuffer.Write(block, 0, bytesRead);
                }
                compressedzipStream.Close();
                return outBuffer.ToArray();
            }
            public static String DecompressToString(byte[] inputBytes, Encoding encoding)
            {

                using (MemoryStream ms = new MemoryStream(inputBytes))
                {
                    using (GZipStream zipStream = new GZipStream(ms, CompressionMode.Decompress))
                    using (StreamReader sr = new StreamReader(zipStream, encoding))
                        return sr.ReadToEnd();
                }
            }
        }
    }
}
