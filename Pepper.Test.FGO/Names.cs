using System;
using System.IO;

namespace Pepper.Test.FGO
{
    public static class Names
    {
        public static string[] Versions;
        public static string BasePath;

        public static string MstFunc = "mstFunc";
        public static string MstBuff = "mstBuff";
        public static string MstSvt = "mstSvt";
        public static string MstSvtTreasureDevice = "mstSvtTreasureDevice";
        public static string MstTreasureDevice = "mstTreasureDevice";
        public static string MstTreasureDeviceLv = "mstTreasureDeviceLv";

        static Names()
        {
            BasePath = Path.Combine(Environment.CurrentDirectory, "master");
            Versions = Directory.GetDirectories(BasePath);
            
        }
    }
}