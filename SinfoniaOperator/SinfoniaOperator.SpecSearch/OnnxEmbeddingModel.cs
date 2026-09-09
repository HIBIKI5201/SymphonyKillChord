using Lokad.Tokenizers.Tokenizer;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     SentencePieceとONNX Runtimeを使用してE5埋め込みを生成する。
    /// </summary>
    public sealed class OnnxEmbeddingModel : IEmbeddingModel, IDisposable
    {
        /// <summary>
        ///     ONNX埋め込みモデルを読み込む。
        /// </summary>
        /// <param name="modelFilePath">ONNXモデルファイルのパス。</param>
        /// <param name="tokenizerFilePath">SentencePieceモデルファイルのパス。</param>
        public OnnxEmbeddingModel(string modelFilePath, string tokenizerFilePath)
        {
            if (!File.Exists(modelFilePath))
            {
                throw new FileNotFoundException("ONNXモデルファイルが見つかりません。", modelFilePath);
            }

            if (!File.Exists(tokenizerFilePath))
            {
                throw new FileNotFoundException("トークナイザファイルが見つかりません。", tokenizerFilePath);
            }

            _session = new InferenceSession(modelFilePath);
            _tokenizer = new XLMRobertaTokenizer(tokenizerFilePath, false);
        }

        /// <summary>
        ///     文字列をトークン化し、平均プーリング済みの埋め込みベクトルを生成する。
        /// </summary>
        /// <param name="text">埋め込み対象の文字列。</param>
        /// <returns>384次元を想定したL2正規化済みベクトル。</returns>
        public async Task<float[]> EmbedAsync(string text)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(text);
            await _inferenceLock.WaitAsync();
            try
            {
                return await Task.Run(() => Embed(text));
            }
            finally
            {
                _inferenceLock.Release();
            }
        }

        /// <summary>
        ///     ONNX Runtimeが保持するネイティブリソースを解放する。
        /// </summary>
        public void Dispose()
        {
            _inferenceLock.Dispose();
            _session.Dispose();
        }

        private const int MAX_TOKEN_COUNT = 512;
        private const int BATCH_SIZE = 1;
        private const int EXPECTED_OUTPUT_RANK = 3;
        private const long ATTENDED_TOKEN_VALUE = 1L;
        private const long TOKEN_TYPE_VALUE = 0L;
        private const float MINIMUM_NORM = 1.0e-12F;
        private const string INPUT_IDS_NAME = "input_ids";
        private const string ATTENTION_MASK_NAME = "attention_mask";
        private const string TOKEN_TYPE_IDS_NAME = "token_type_ids";

        private readonly InferenceSession _session;
        private readonly XLMRobertaTokenizer _tokenizer;
        private readonly SemaphoreSlim _inferenceLock = new(1, 1);

        /// <summary>
        ///     文字列の埋め込み推論を同期的に実行する。
        /// </summary>
        /// <param name="text">埋め込み対象の文字列。</param>
        /// <returns>L2正規化済みベクトル。</returns>
        private float[] Embed(string text)
        {
            long[] tokenIds = _tokenizer
                .Encode(text, null, MAX_TOKEN_COUNT, TruncationStrategy.LongestFirst, 0)
                .TokenIds
                .ToArray();
            int tokenCount = tokenIds.Length;
            long[] attentionMask = Enumerable.Repeat(ATTENDED_TOKEN_VALUE, tokenCount).ToArray();
            long[] dimensions = [BATCH_SIZE, tokenCount];

            List<string> inputNames =
            [
                INPUT_IDS_NAME,
                ATTENTION_MASK_NAME
            ];
            List<OrtValue> inputValues =
            [
                OrtValue.CreateTensorValueFromMemory(tokenIds, dimensions),
                OrtValue.CreateTensorValueFromMemory(attentionMask, dimensions)
            ];

            try
            {
                if (_session.InputMetadata.ContainsKey(TOKEN_TYPE_IDS_NAME))
                {
                    long[] tokenTypeIds = Enumerable.Repeat(TOKEN_TYPE_VALUE, tokenCount).ToArray();
                    inputNames.Add(TOKEN_TYPE_IDS_NAME);
                    inputValues.Add(OrtValue.CreateTensorValueFromMemory(tokenTypeIds, dimensions));
                }

                using RunOptions runOptions = new();
                using IDisposableReadOnlyCollection<OrtValue> results = _session.Run(
                    runOptions,
                    inputNames,
                    inputValues,
                    [_session.OutputNames[0]]);
                OrtValue lastHiddenState = results[0];
                ReadOnlySpan<float> outputData = lastHiddenState.GetTensorDataAsSpan<float>();
                long[] outputDimensions = lastHiddenState.GetTensorTypeAndShape().Shape;
                if (outputDimensions.Length != EXPECTED_OUTPUT_RANK || outputDimensions[1] != tokenCount)
                {
                    throw new InvalidOperationException("ONNXモデルの出力形状が想定する[batch, token, embedding]形式ではありません。");
                }

                int embeddingLength = checked((int)outputDimensions[^1]);
                float[] embedding = new float[embeddingLength];

                for (int tokenIndex = 0; tokenIndex < tokenCount; tokenIndex++)
                {
                    int tokenOffset = tokenIndex * embeddingLength;
                    for (int embeddingIndex = 0; embeddingIndex < embeddingLength; embeddingIndex++)
                    {
                        embedding[embeddingIndex] += outputData[tokenOffset + embeddingIndex];
                    }
                }

                for (int embeddingIndex = 0; embeddingIndex < embeddingLength; embeddingIndex++)
                {
                    embedding[embeddingIndex] /= tokenCount;
                }

                Normalize(embedding);
                return embedding;
            }
            finally
            {
                foreach (OrtValue inputValue in inputValues)
                {
                    inputValue.Dispose();
                }
            }
        }

        /// <summary>
        ///     ベクトルをL2正規化する。
        /// </summary>
        /// <param name="vector">正規化するベクトル。</param>
        private static void Normalize(float[] vector)
        {
            double squaredNorm = 0.0D;
            foreach (float value in vector)
            {
                squaredNorm += value * value;
            }

            double norm = Math.Sqrt(squaredNorm);
            if (norm < MINIMUM_NORM)
            {
                return;
            }

            for (int index = 0; index < vector.Length; index++)
            {
                vector[index] = (float)(vector[index] / norm);
            }
        }
    }
}
