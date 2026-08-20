using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    /// <summary>
    ///     トークンバケット方式でリクエスト送信間隔を平準化するレート制限器。
    ///     Notion APIの実効レート制限を超えないよう送信自体のペースを落とし、
    ///     429の発生と再試行そのものを減らすために使用する。
    /// </summary>
    internal sealed class RequestRateLimiter
    {
        private readonly double _tokensPerSecond;
        private readonly double _burstCapacity;
        private readonly object _lock = new();
        private double _availableTokens;
        private long _lastRefillTimestamp;

        /// <summary>
        ///     レート制限器を生成する。
        /// </summary>
        /// <param name="tokensPerSecond">秒間の平均許可リクエスト数。</param>
        /// <param name="burstCapacity">瞬間的に許可するバースト分のトークン数。</param>
        internal RequestRateLimiter(double tokensPerSecond, double burstCapacity)
        {
            _tokensPerSecond = tokensPerSecond;
            _burstCapacity = burstCapacity;
            _availableTokens = burstCapacity;
            _lastRefillTimestamp = Stopwatch.GetTimestamp();
        }

        /// <summary>
        ///     トークンを1つ消費できるようになるまで待機する。
        /// </summary>
        internal async Task WaitAsync()
        {
            while (true)
            {
                TimeSpan waitTime;
                lock (_lock)
                {
                    Refill();
                    if (_availableTokens >= 1)
                    {
                        _availableTokens -= 1;
                        return;
                    }

                    double missingTokens = 1 - _availableTokens;
                    waitTime = TimeSpan.FromSeconds(missingTokens / _tokensPerSecond);
                }

                await Task.Delay(waitTime);
            }
        }

        /// <summary>
        ///     経過時間に応じてトークンを補充する。呼び出し元で_lockを取得していること。
        /// </summary>
        private void Refill()
        {
            long now = Stopwatch.GetTimestamp();
            double elapsedSeconds = (now - _lastRefillTimestamp) / (double)Stopwatch.Frequency;
            if (elapsedSeconds <= 0) { return; }

            _lastRefillTimestamp = now;
            _availableTokens = Math.Min(_burstCapacity, _availableTokens + elapsedSeconds * _tokensPerSecond);
        }
    }
}
