using NUnit.Framework;
using SsePushClient;
using System;

namespace SsePushClientTest
{
    [TestFixture]
    public class SsePushReceiveClientTest
    {
        [SetUp]
        public void SetUp()
        {
        }

        /// <summary>
        /// コンストラクタ(異常)
        /// SSEサーバのURIがNullの場合、InvalidOperationExceptionを発行すること
        /// </summary>
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void TestSsePushReceiveClientNormal()
        {
            SsePushReceiveClient client = new SsePushReceiveClient(null);
        }

        /// <summary>
        /// Open(SSE Pushサーバと接続する)
        /// </summary>
        /// EventSource.Start()Mock化できないため、ITにてテストする

        /// <summary>
        /// Close(SSE Pushサーバと切断する)
        /// </summary>
        /// EventSource.Stop()をMock化できないため、ITにてテストする

        /// <summary>
        /// RegisterOnOpen(SSE Pushサーバから接続完了通知受信時の処理を登録)
        /// </summary>
        /// EventSource.StateChangedをMock化できないため、ITにてテストする
        
        /// <summary>
        /// RegisterOnClose(SSE Pushサーバから切断完了通知受信時の処理を登録)
        /// </summary>
        /// EventSource.StateChangedをMock化できないため、ITにてテストする
        
        /// <summary>
        /// RegisterOnError(SSE Pushサーバからエラー受信時の処理を登録)
        /// </summary>
        /// EventSource.RegisterOnError()をMock化できないため、ITにてテストする

        /// <summary>
        /// RegisterEvent(SSE Pushサーバからメッセージ受信時の処理を登録)(イベント名指定)
        /// </summary>
        /// EventSource.EventReceivedをMock化できないため、ITにてテストする

        /// <summary>
        /// RegisterEvent(SSE Pushサーバからメッセージ受信時の処理を登録)(イベント名未指定)
        /// </summary>
        /// EventSource.EventReceivedをMock化できないため、ITにてテストする

    }
}
