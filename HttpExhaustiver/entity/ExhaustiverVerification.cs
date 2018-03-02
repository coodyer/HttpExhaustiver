using System;
using System.Collections.Generic;
using System.Text;

namespace HttpExhaustiver.entity
{
    class ExhaustiverVerification
    {
        /// <summary>
        /// 0根据响应码 1根据header头 2根据返回内容
        /// </summary>
        private int verificationType;

        public int VerificationType
        {
            get { return verificationType; }
            set { verificationType = value; }
        }
        /// <summary>
        /// 0等于 1不等于 2包含 3不包含  4正则匹配 5正则不匹配
        /// </summary>
        private int calcType;

        public int CalcType
        {
            get { return calcType; }
            set { calcType = value; }
        }

        /// <summary>
        /// 校验值
        /// </summary>
        private String value;

        public String Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        private Boolean successThenStop=false;

        public Boolean SuccessThenStop
        {
            get { return successThenStop; }
            set { successThenStop = value; }
        }


    }
}
