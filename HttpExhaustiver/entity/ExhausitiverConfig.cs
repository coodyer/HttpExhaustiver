using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpExhaustiver.entity
{
    class ExhausitiverConfig
    {
        ExhausitiverEntity general;

        public ExhausitiverEntity General
        {
            get { return general; }
            set { general = value; }
        }


        ExhaustiverVerification verification;

        public ExhaustiverVerification Verification
        {
            get { return verification; }
            set { verification = value; }
        }


        List<ExhausitiverDic> dics;

        public List<ExhausitiverDic> Dics
        {
            get { return dics; }
            set { dics = value; }
        }
    }
}
