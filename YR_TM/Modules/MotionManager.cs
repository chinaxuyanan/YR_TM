using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YR_TM.Modules
{
    public enum HomeMode
    {
        /// <summary>
        /// 原点信号+正方向
        /// </summary>
        ORG_P = 0,

        /// <summary>
        /// 原点信号+负方向
        /// </summary>
        ORG_N,

        /// <summary>
        /// 正限位信号为原点，正方向回原点
        /// </summary>
        PEL,

        /// <summary>
        /// 负限位信号为原点，负方向回原点
        /// </summary>
        MEL,


        /// <summary>
        /// EZ信号为原点信号，正方向回原点
        /// </summary>
        EZ_PEL,

        /// <summary>
        /// EZ信号为原点信号，负方向回原点
        /// </summary>
        EZ_MEL,

        /// <summary>
        /// 原点信号+正方向+EZ
        /// </summary>
        ORG_P_EZ,

        /// <summary>
        /// 原点信号+负方向+EZ
        /// </summary>
        ORG_N_EZ,

        /// <summary>
        /// 正限位信号为原点+EZ
        /// </summary>
        PEL_EZ,

        /// <summary>
        /// 负限位信号为原点+EZ
        /// </summary>
        MEL_EZ,

        /// <summary>
        /// 其他模式，直接从卡中读取回原点模式，无需指定参数
        /// </summary>
        CARD = 999,

        /// <summary>
        /// 其他总线型，不能从卡中读取回原点方式，基于此值累加，例如汇川总线型，选择回原点方式1，则实际传入的参数为1000 + 1 = 1001
        /// </summary>
        BUS_BASE = 1000,
    }
}
