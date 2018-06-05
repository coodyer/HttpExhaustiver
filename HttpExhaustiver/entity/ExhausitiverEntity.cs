using System;
using System.Collections.Generic;
using System.Text;

namespace HttpExhaustiver.entity
{
    class ExhausitiverEntity
    {

        String protocol;

        public String Protocol
        {
            get { return protocol; }
            set { protocol = value; }
        }
        String host;

        public String Host
        {
            get { return host; }
            set { host = value; }
        }

        Int32 threadNum = 1;

        public Int32 ThreadNum
        {
            get { return threadNum; }
            set { threadNum = value; }
        }


        String method;

        public String Method
        {
            get { return method; }
            set { method = value; }
        }
        
        Int32 port;

        public Int32 Port
        {
            get { return port; }
            set { port = value; }
        } 
        
        String encode;

        public String Encode
        {
            get { return encode; }
            set { encode = value; }
        } 
        
        Int32 timeOut;

        public Int32 TimeOut
        {
            get { return timeOut; }
            set { timeOut = value; }
        }

        String body;

        public String Body
        {
            get { return body; }
            set { body = value; }
        }

        byte[] data;

        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        public static ExhausitiverEntity parseExhausitiver(String protocol,String encode,Int32 timeOut,String context)
        {

            context = context.Replace("\r\n", "\n");
            if (String.IsNullOrEmpty(context))
            {
                return null;
            }
            ExhausitiverEntity entity = new ExhausitiverEntity();
            entity.body = context;
            entity.timeOut = timeOut;
            entity.protocol = protocol;
            String[] lines = context.Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            lines = lines[0].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            //解析请求方式
            entity.Method = "GET";
            if (lines[0].ToLower().StartsWith("post"))
            {
                entity.Method = "POST";
            }
            Dictionary<String, String> headers = new Dictionary<string, string>();
            for (int i = 1; i < lines.Length; i++)
            {
                try
                {
                    if (lines[i].Contains("\n\n"))
                    {
                        break;
                    }
                    Int32 splitModder = lines[i].IndexOf(":");
                    if (splitModder < 0)
                    {
                        continue;
                    }
                    String fieldName = lines[i].Substring(0, splitModder);
                    String fieldValue = lines[i].Substring(splitModder + 1, lines[i].Length - splitModder - 1).Trim();
                    headers.Add(fieldName.Trim(), fieldValue.Trim());
                }
                catch { }
            }
            //获得HOST
            if (headers.ContainsKey("Host"))
            {
                String host = headers["Host"].Trim();
                String[] tags = host.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                entity.host = tags[0];
                Int32 port = 80;
                if (protocol.ToLower().Equals("https"))
                {
                    port = 443;
                }
                if (tags.Length == 2)
                {
                    port = Convert.ToInt32(tags[1]);
                }
                entity.port = port;
            }
            encode= (encode.Equals("自动") ? "UTF-8" : encode);
            entity.encode = encode;
            //获得Body
            if (entity.Method.Equals("POST"))
            {
                Int32 splitModder = context.IndexOf("\n\n");
                String body = context.Substring(splitModder + 1, context.Length - splitModder - 1).Trim();
                Int32 length = Encoding.GetEncoding(entity.encode).GetByteCount(body);
                StringBuilder sber = new StringBuilder(lines[0]).Append("\n");
                if (headers.ContainsKey("Content-Length"))
                {
                    headers["Content-Length"] = Convert.ToString(length);
                }
                else
                {
                    headers.Add("Content-Length", Convert.ToString(length));
                }
                foreach (String key in headers.Keys)
                {
                    sber.Append(key + ": " + headers[key]).Append("\n");
                }
                sber.Append("\n");
                if (String.IsNullOrEmpty(body))
                {
                    body = "\n";
                }
                sber.Append(body);
                context = sber.ToString();
            }
            else {
                context = context.Trim() + "\n\n";
            }
            entity.body = context;
            entity.data = Encoding.GetEncoding(entity.encode).GetBytes(entity.body);
            return entity;
        }

    }
}
