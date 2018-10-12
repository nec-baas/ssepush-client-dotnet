namespace SsePushClient
{
    /// <summary>
    /// SSE Pushサーバから受信した情報
    /// </summary>
    public class SseMessage
    {
        /// <summary>
        /// イベントタイプ
        /// </summary>
        public string EventType;

        /// <summary>
        /// データ
        /// </summary>
        public string Data;

        /// <summary>
        /// イベントID
        /// </summary>
        public string LastEventId;

        /// <summary>
        /// リトライ値
        /// </summary>
        public string Retry;
    }
}
