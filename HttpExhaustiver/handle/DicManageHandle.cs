using HttpExhaustiver.entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;

namespace HttpExhaustiver.handle
{
    class DicManageHandle
    {


        /// <summary>
        /// 参数名、字典对象
        /// </summary>
        private Dictionary<String, ExhausitiverDic> dics = new Dictionary<string, ExhausitiverDic>();

        private Dictionary<String, StreamReader> readers = new Dictionary<string, StreamReader>();

        public void closeReaders()
        {
            if (readers.Count==0) {
                return;
            }
            foreach (String key in readers.Keys)
            {
                try { readers[key].Close(); }
                catch { }

            }
        }

        public long totalNums = 0;

        public DicManageHandle(Dictionary<String, ExhausitiverDic> dics)
        {
            totalNums = 0;
            this.dics = dics;
            foreach (ExhausitiverDic dic in dics.Values)
            {
                StreamReader sr = new StreamReader(dic.Path, Encoding.Default);
                readers.Add(dic.Path, sr);
            }
            int i = 0;
            foreach (ExhausitiverDic dic in dics.Values)
            {
                currentParams.Add(dic.ParamName, null);
                if (i >= dics.Count - 1)
                {
                    continue;
                }
                try
                {
                    readLine(dic);
                }
                catch { }
                i++;
            }
            foreach (ExhausitiverDic dic in dics.Values)
            {
                if (totalNums == 0)
                {
                    totalNums = getLineNums(dic.Path);
                    continue;
                }
                totalNums = totalNums * getLineNums(dic.Path);
            }
            while (dicpool.Count < 10000)
            {
                try
                {
                    Dictionary<String, String> param = next();
                    if (param == null)
                    {
                        break;
                    }
                    dicpool.Enqueue(param);
                }
                catch
                {
                    Thread.Sleep(10);
                }
            }
            Thread initPoolThread = new Thread(initPool);
            initPoolThread.Start();
        }
        private long getLineNums(String path)
        {

            int lineCount = 0;
            try
            {

                using (StreamReader sr = new StreamReader(path))
                {
                    while (sr.Peek() >= 0)
                    {
                        sr.ReadLine();
                        lineCount++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            return lineCount;
        }


        private Dictionary<String, String> currentParams = new Dictionary<string, string>();


        Queue<Dictionary<String, String>> dicpool = new Queue<Dictionary<string, string>>();


        private void initPool()
        {
                while (true)
                {
                    while (dicpool.Count < 10000)
                    {
                        try
                        {
                            Dictionary<String, String> param = next();
                            if (param == null)
                            {
                                break;
                            }
                            dicpool.Enqueue(param);
                        }
                        catch {
                            Thread.Sleep(1);
                        }
                    }
                    Thread.Sleep(1);
                }
        }

        public Dictionary<String, String> nextParam()
        {
            try
            {
                Dictionary<String, String> param = dicpool.Dequeue();
                if (param != null)
                {
                    return param;
                }
                return next();
            }
            catch { 
                return next(); 
            }
        }

        private Dictionary<String, String> next()
        {
            lock (this)
            {
                int currentIndex = currentParams.Count - 1;
                String line = null;
                while (line == null && currentIndex > -1)
                {
                    var element = dics.ElementAt(currentIndex);
                    ExhausitiverDic dic = element.Value;
                    line = readLine(dic);
                    if (line == null)
                    {
                        if (currentIndex <= 0)
                        {
                            return null;
                        }
                        currentIndex--;
                    }

                }
                foreach (ExhausitiverDic dic in dics.Values)
                {
                    if (currentParams[dic.ParamName] != null)
                    {
                        continue;
                    }
                    readLine(dic);
                }
                Dictionary<String, String> paramsTemp = new Dictionary<string, string>();
                foreach (String key in currentParams.Keys)
                {
                    try
                    {
                        paramsTemp.Add(key, currentParams[key]);
                    }
                    catch { }
                }
                return paramsTemp;
            }
        }

        private String readLine(ExhausitiverDic dic)
        {
            String line = null;
            try
            {
                line = readers[dic.Path].ReadLine();
                if (line == null)
                {
                    readers[dic.Path].Close();
                }
            }
            catch { }
            currentParams[dic.ParamName] = line;
            return line;
        }
    }
}
