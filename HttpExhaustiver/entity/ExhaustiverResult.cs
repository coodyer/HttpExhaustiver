using System;
using System.Collections.Generic;
using System.Text;

namespace HttpExhaustiver.entity
{
    class ExhaustiverResult
    {

        private Dictionary<String, String> param=new Dictionary<string,string>();

        public Dictionary<String, String> Param
        {
            get { return param; }
            set { param = value; }
        }

        private Int32 code=-1;

        public Int32 Code
        {
            get { return code; }
            set { code = value; }
        }

        private String result="";

        public String Result
        {
            get { return result; }
            set { result = value; }
        }

        private Int32 length=0;

        public Int32 Length
        {
            get { return length; }
            set { length = value; }
        }

        private String unionId;

        public String UnionId
        {
            get { return unionId; }
            set { unionId = value; }
        }
        private bool success = false;

        public bool Success
        {
            get { return success; }
            set { success = value; }
        }

    }
}
