namespace gsnet_sharp
{
    // 行分割数据包解码器，最基础的一种解码器
    public class LineBasedFrameDecoder
    {

    }

    // 固定长度数据包解码器
    public class FixedLengthFrameDecoder
    {

    }

    // 自定义分隔符数据包解码器
    public class DelimiterBasedFrameDecoder
    {

    }


    // 自定义长度数据包解码器
    public class LengthFieldBasedFrameDecoder
    {
        public static int HeaderLength = 4 + 1 + 1 + 1 + 1 + 4 + 4;
        public static int MagicNumber = 0xf3e2;
        public static int Version = 0;
    }
}
