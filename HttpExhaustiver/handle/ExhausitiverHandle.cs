using HttpExhaustiver.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HttpExhaustiver.handle
{
    class ExhausitiverHandle
    {
        private static Dictionary<Int32, HttpHandle> httpHandles = new Dictionary<int, HttpHandle>(){
        {0,new HttpHandle(0)},{1,new HttpHandle(1)},{2,new HttpHandle(2)},{3,new HttpHandle(2)}
        };
        public static ExhaustiverResult doExhausitiver(ExhausitiverEntity general, ExhaustiverVerification verification)
        {
            try
            {
                HttpExhaustiver.handle.HttpHandle.HttpResult httpResult;
                if (general.Protocol.ToLower().Equals("http"))
                {
                    httpResult = httpHandles[verification.VerificationType].httpSendData(general.Host, general.Port, general.TimeOut, general.Encode, general.Data);
                }
                else
                {
                    httpResult = httpHandles[verification.VerificationType].httpsSendData(general.Host, general.Port, general.TimeOut, general.Encode, general.Data);
                }
                ExhaustiverResult result = new ExhaustiverResult();
                result.Code = httpResult.Code;
                result.Result = httpResult.Header + "\r\n" + httpResult.Body;
                result.Length = result.Result.Length;
                result.UnionId = Guid.NewGuid().ToString("N");
                if (verification.VerificationType == 0)
                {
                    if (httpResult.Code==-1) {
                        return result;
                    }
                    if (httpResult.Code == Convert.ToInt32(verification.Value.Trim()))
                    {
                        result.Success = true;
                        return result;
                    }
                    return result;
                }
                String verificationBody = httpResult.Header;
                if (verification.VerificationType==2) {
                    if (String.IsNullOrEmpty(httpResult.Body))
                    {
                        return result;
                    }
                    verificationBody = httpResult.Body;
                }
                else if (verification.VerificationType == 3) {
                    if (String.IsNullOrEmpty(httpResult.Body))
                    {
                        return result;
                    }
                    verificationBody = httpResult.Header + "\n" + httpResult.Body;
                }
                if (verification.CalcType == 0)
                {
                    if (verificationBody.Equals(verification.Value))
                    {
                        result.Success = true;
                        return result;
                    }
                    return result;
                }
                if (verification.CalcType == 1)
                {
                    if (!verificationBody.Equals(verification.Value))
                    {
                        result.Success = true;
                        return result;
                    }
                    return result;
                }
                if (verification.CalcType == 2)
                {
                    if (verificationBody.Contains(verification.Value))
                    {
                        result.Success = true;
                        return result;
                    }
                    return result;
                }
                if (verification.CalcType == 3)
                {
                    if (!verificationBody.Contains(verification.Value))
                    {
                        result.Success = true;
                        return result;
                    }
                    return result;
                }
                if (verification.CalcType == 4)
                {
                    List<String> matchResult = matchExport(verificationBody, new Regex(verification.Value));
                    if (matchResult != null && matchResult.Count > 0)
                    {
                        result.Success = true;
                        return result;
                    }
                    return result;
                }
                if (verification.CalcType == 5)
                {
                    List<String> matchResult = matchExport(verificationBody, new Regex(verification.Value));
                    if (matchResult != null && matchResult.Count > 0)
                    {
                        result.Success = true;
                        return result;
                    }
                    return result;
                }
                return result;
            }
            catch
            {
                return new ExhaustiverResult();
            }

        }
        public static List<String> matchExport(String context, Regex reg)
        {
            try
            {
                MatchCollection result = reg.Matches(context);
                List<String> results = new List<string>();
                foreach (Match m in result)
                {
                    if (String.IsNullOrEmpty(m.Value))
                    {
                        continue;
                    }
                    results.Add(m.Value);
                }
                return results;
            }
            catch
            {
                return null;
            }
        }
    }
}
