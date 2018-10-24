using System;

namespace Panaroma.OKC.Integration.Library
{
    public class PairResult
    {
        public string Df02TraceNo { get; set; }
        public string Df02TranDate { get; set; }
        public string Df02TranTime { get; set; }

        public string Df6FErrRespCode { get; set; }
        public string ErrRespCodeResult { get; set; }
        public string Df6FExtDevIndex { get; set; }
        public string Df5KeyInvalidationCnt { get; set; }
        public string KencKcv { get; set; }
    }
}
