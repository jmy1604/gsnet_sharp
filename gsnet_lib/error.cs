namespace gsnet_sharp
{
    internal class ErrorCode
    {
        #region 内部错误
        // 处理长度错误
        public static int ProcessedLengthError = -100000;
        #endregion

        // 超出接收缓冲区长度
        public static int RecvLengthTooLong = -200000;
        
        // netty包格式非法
        public static int NettyPacketFormatInvalid = -300000;
    }
}
