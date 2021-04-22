namespace SiamCross
{
    public static class Constants
    {
        public static readonly int DaySeconds = 86400;
#if DEBUG
        public static readonly int ConnectTimeout = 100000;
#else
        public static readonly int ConnectTimeout = 20000;
#endif


        public static readonly ushort[] Periods = new ushort[]
            {1,2,3,4,5,7,10,15,20,30,40,60,90,120,180,240,300,420,600,720 };
        public static readonly short[] Quantitys = new short[]
            {0,1,2,3,4,5,7,10,15,20,30,40,50,70,100,150,200,300,400,500,600,700,800,900,-1 };

        public static readonly double DefaultSoundSpeedFixed = 320;

        public const int BTLE_PKG_HDR_SIZE = 3;
        public const int BTLE_PKG_MAX_SIZE = 247;

        public const int SIAM_PKG_CRC_SIZE = 2;
        public const int SIAM_PKG_HDR_SIZE = 12;
        public const int SIAM_PKG_DEFAULT_DATA_SIZE = 40;

        public const int MIN_PKG_SIZE = 12;
        public const int MAX_PKG_SIZE = 256;

        public const int ShortDelay = 50;

        public const int LongDelay = 300;

        public const int SecondDelay = 1000;

        public const int DefaultWell = 0;

        public const int DefaultBush = 0;

        public const int DefaultShop = 0;

        public const float DefaultBufferPressure = 0f;

        public const string DefaultComment = "Комментарий отсутствует";

        public const float DefaultRod = 32;

        public const int DefaultDynPeriod = 10;

        public const double DefaultPumpRate = 60.0 / DefaultDynPeriod;

        public const float DefaultImtravel = 4;

        public const int DefaultApertNumber = 5;

        public const int ConnectTimeOut = 10000;

        public const string DefaultModelPump = "Балансирный";

        public const float EhoFixedSoundSpeed = 341.33f;

        public const int DefaultUmtMeasurementInterval = 5;
    }
}
