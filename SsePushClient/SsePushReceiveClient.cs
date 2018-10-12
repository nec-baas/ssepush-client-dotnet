using ServerSentEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace SsePushClient
{
    /// <summary>
    /// Server Sent Events(SSE) 通信ライブラリ
    /// </summary>
    public class SsePushReceiveClient
    {
        /// <summary>
        /// メッセージ受信用デリゲート関数
        /// </summary>
        /// <param name="message">受信メッセージ</param>
        public delegate void OnMessage(SseMessage message);

        /// <summary>
        /// エラー受信用デリゲート関数
        /// </summary>
        /// <param name="statusCode">HTTPステータスコード</param>
        /// <param name="response">エラーメッセージ</param>
        public delegate void OnError(HttpStatusCode statusCode, HttpWebResponse response);

        /// <summary>
        /// 接続完了通知用デリゲート関数
        /// </summary>
        public delegate void OnOpen();

        /// <summary>
        /// 切断完了通知用デリゲート関数
        /// </summary>
        public delegate void OnClose();

        private EventSource _eventSource;

        
        // デリゲート関数保存用フィールド
        private Dictionary<string, OnMessage> _onMessageDelegateDictionary;
        
        // 排他制御用オブジェクト
        private static readonly object _lock = new object();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="uri">SSEサーバのURI</param>
        public SsePushReceiveClient(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new InvalidOperationException("Null URI");
            }
            // EventSource作成
            _eventSource = new EventSource(new Uri(uri));
            _onMessageDelegateDictionary = new Dictionary<string, OnMessage>();

            // Eventのコールバック初回登録のみここで行う。
            // 2回目以降はRegisterEvent()APIで、Dictionaryにイベント名とコールバックを追加する。
            RegisterEventCallback();
        }

        /// <summary>
        /// SSE Pushサーバと接続する
        /// </summary>
        /// <remarks>Basic認証を行わない場合は"username", "password"をnullにすること。
        /// </remarks>
        /// <param name="username">ユーザ名</param>
        /// <param name="password">パスワード</param>
        public void Open(string username, string password)
        {
            Debug.WriteLine("SsePushReceiveClient: Open() <start> username=" + username);

            // 受信開始
            _eventSource.Start(username, password);

            Debug.WriteLine("SsePushReceiveClient: Open() <end>");
        }

        // エラー受信用コールバック
        private class OnErrorCallback : OnErrorReceived
        {
            private SsePushReceiveClient ssePushReceiveClient;
            private OnError Callback;

            public OnErrorCallback(SsePushReceiveClient ssePushReceiveClient, OnError Callback)
            {
                this.ssePushReceiveClient = ssePushReceiveClient;
                this.Callback = Callback;
            }

            public void OnError(HttpStatusCode statusCode, HttpWebResponse response)
            {
                // エラーログ出力
                Debug.WriteLine("[Error Response]");
                Debug.WriteLine("StatusCode:  " + statusCode);
                Debug.WriteLine("WebResponse: " + response);

                // エラー受信用デリゲート関数実行
                var cb = Callback;
                if (cb != null)
                {
                    cb(statusCode, response);
                }
            }
        }

        /// <summary>
        /// SSE Pushサーバと切断する
        /// </summary>
        public void Close()
        {
            Debug.WriteLine("SsePushReceiveClient: Close() <start>");
            _eventSource.Stop();
            Debug.WriteLine("SsePushReceiveClient: Close() <end>");
        }

        /// <summary>
        /// SSE Pushサーバから接続完了通知受信時の処理を登録
        /// </summary>
        /// <param name="Callback">接続完了通知受信時の処理</param>
        public void RegisterOnOpen(OnOpen Callback)
        {
            Debug.WriteLine("SsePushReceiveClient: RegisterOnOpen() <start>");

            // コールバック保存
            _eventSource.StateChanged += (sender, e) =>
            {
                if (e.State == EventSourceState.OPEN)
                {
                    var cb = Callback;
                    if (cb != null)
                    {
                        cb();
                    }
                }
            };
            Debug.WriteLine("SsePushReceiveClient: RegisterOnOpen() <end>");
        }

        /// <summary>
        /// SSE Pushサーバから切断完了通知受信時の処理を登録
        /// </summary>
        /// <param name="Callback">切断完了通知受信時の処理</param>
        public void RegisterOnClose(OnClose Callback)
        {
            Debug.WriteLine("SsePushReceiveClient: RegisterOnClose() <start>");
            // コールバック保存
            _eventSource.StateChanged += (sender, e) =>
            {
                if (e.State == EventSourceState.CLOSED)
                {
                    var cb = Callback;
                    if (cb != null)
                    {
                        cb();
                    }
                }
            };
            Debug.WriteLine("SsePushReceiveClient: RegisterOnClose() <end>");
        }

        /// <summary>
        /// SSE Pushサーバからエラー受信時の処理を登録
        /// </summary>
        /// <param name="Callback">エラー受信時の処理</param>
        public void RegisterOnError(OnError Callback)
        {
            Debug.WriteLine("SsePushReceiveClient: RegisterOnError() <start>");

            // コールバック保存
            _eventSource.RegisterOnError(new OnErrorCallback(this, Callback));

            Debug.WriteLine("SsePushReceiveClient: RegisterOnError() <end>");
        }

        /// <summary>
        /// SSE Pushサーバからメッセージ受信時の処理を登録
        /// </summary>
        /// <param name="Event">取得するイベント名</param>
        /// <param name="Callback">メッセージ受信時の処理</param>
        public void RegisterEvent(string Event, OnMessage Callback)
        {
            Debug.WriteLine("SsePushReceiveClient: RegisterEvent() <start> Event=" + Event);

            // コールバック保存
            _onMessageDelegateDictionary[Event] = Callback;

            Debug.WriteLine("SsePushReceiveClient: RegisterEvent() <end>");
        }

        /// <summary>
        /// SSE Pushサーバからメッセージ受信時の処理を登録。
        /// 
        /// 取得するイベント名は、"message"が設定される。
        /// </summary>
        /// <param name="Callback">メッセージ受信時の処理</param>
        public void RegisterEvent(OnMessage Callback)
        {
            // コールバック保存
            RegisterEvent("message", Callback);
        }

        // SSE Pushサーバにメッセージ受信時のコールバックを登録
        private void RegisterEventCallback()
        {
            // イベント受信時の処理
            _eventSource.EventReceived += (sender, e) =>
            {
                var eventType = e.Message.EventType;

                // 受信したイベント名が null or 空の場合は、"message"イベントに変換する
                if (string.IsNullOrEmpty(eventType))
                {
                    eventType = "message";
                }

                // 登録したイベント名と受信したイベント名が一致する場合はデリゲート関数を実行する
                if (_onMessageDelegateDictionary.ContainsKey(eventType))
                {
                    // メッセージ格納
                    var sseMessage = new SseMessage();
                    sseMessage.EventType = eventType;
                    sseMessage.Data = e.Message.Data;
                    sseMessage.LastEventId = e.Message.LastEventId;
                    if (e.Message.Retry.HasValue)
                    {
                        sseMessage.Retry = e.Message.Retry.ToString();
                    }

                    // デリゲート関数実行
                    _onMessageDelegateDictionary[eventType](sseMessage);
                }
            };
        }
    }
}
