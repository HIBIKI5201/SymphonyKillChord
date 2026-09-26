using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     仕様書チャンクとページ属性を保持し、索引の永続化を提供する。
    /// </summary>
    public sealed class SpecIndex
    {
        /// <summary>
        ///     仕様書チャンクから検索インデックスを生成する。
        /// </summary>
        /// <param name="records">保持する仕様書チャンク。</param>
        public SpecIndex(SpecChunkRecord[] records)
        {
            ArgumentNullException.ThrowIfNull(records);
            _records = records.ToArray();
            ValidateVectors(_records);
        }

        /// <summary> 保持している仕様書チャンク。 </summary>
        public IReadOnlyList<SpecChunkRecord> Records => _records;

        /// <summary>
        ///     インデックスをバイナリファイルへ保存する。
        /// </summary>
        /// <param name="path">保存先ファイルのパス。</param>
        public void Save(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            string fullPath = Path.GetFullPath(path);
            string? directoryPath = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using FileStream stream = File.Create(fullPath);
            using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: false);
            writer.Write(FILE_MAGIC);
            writer.Write(FILE_VERSION);
            writer.Write(_records.Length);
            foreach (SpecChunkRecord record in _records)
            {
                writer.Write(record.SourceFile);
                writer.Write(record.HeadingBreadcrumb);
                writer.Write(record.NotionUrl);
                writer.Write(record.Text);
                writer.Write(JsonConvert.SerializeObject(record.Metadata));
                writer.Write(record.Vector.Length);
                foreach (float value in record.Vector)
                {
                    writer.Write(value);
                }
            }
        }

        /// <summary>
        ///     バイナリファイルからインデックスを読み込む。
        /// </summary>
        /// <param name="path">読み込むファイルのパス。</param>
        /// <returns>復元した検索インデックス。</returns>
        public static SpecIndex Load(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            using FileStream stream = File.OpenRead(path);
            using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
            if (reader.ReadUInt32() != FILE_MAGIC)
            {
                throw new InvalidDataException("仕様検索インデックスの識別子が不正です。");
            }

            if (reader.ReadInt32() != FILE_VERSION)
            {
                throw new InvalidDataException("仕様検索インデックスの形式が古いか未対応です。indexコマンドで再生成してください。");
            }

            int recordCount = ReadNonNegativeCount(reader, "チャンク件数");
            SpecChunkRecord[] records = new SpecChunkRecord[recordCount];
            for (int recordIndex = 0; recordIndex < recordCount; recordIndex++)
            {
                string sourceFile = reader.ReadString();
                string breadcrumb = reader.ReadString();
                string notionUrl = reader.ReadString();
                string text = reader.ReadString();
                SpecPageMetadata metadata = JsonConvert.DeserializeObject<SpecPageMetadata>(reader.ReadString())
                    ?? throw new InvalidDataException("ページ属性が不正です。");
                int vectorLength = ReadNonNegativeCount(reader, "ベクトル次元");
                float[] vector = new float[vectorLength];
                for (int vectorIndex = 0; vectorIndex < vectorLength; vectorIndex++)
                {
                    vector[vectorIndex] = reader.ReadSingle();
                }

                records[recordIndex] = new SpecChunkRecord(sourceFile, breadcrumb, notionUrl, text, vector, metadata);
            }

            if (stream.Position != stream.Length)
            {
                throw new InvalidDataException("仕様検索インデックスに未知のデータが含まれています。");
            }

            return new SpecIndex(records);
        }

        private const uint FILE_MAGIC = 0x58444E53U;
        private const int FILE_VERSION = 2;
        private const double MINIMUM_NORM = 1.0e-12D;

        private readonly SpecChunkRecord[] _records;

        /// <summary>
        ///     すべての埋め込みベクトルが有効で同一次元か検証する。
        /// </summary>
        /// <param name="records">検証する仕様書チャンク。</param>
        private static void ValidateVectors(SpecChunkRecord[] records)
        {
            if (records.Length == 0)
            {
                return;
            }

            int vectorLength = records[0].Vector.Length;
            if (vectorLength == 0 || records.Any(record => record.Vector.Length != vectorLength
                || record.Vector.Any(value => !float.IsFinite(value))))
            {
                throw new ArgumentException("埋め込みベクトルは空でない同一次元である必要があります。", nameof(records));
            }
        }

        /// <summary>
        ///     2つのベクトルのコサイン類似度を計算する。
        /// </summary>
        /// <param name="left">左辺のベクトル。</param>
        /// <param name="right">右辺のベクトル。</param>
        /// <returns>コサイン類似度。</returns>
        internal static double CalculateCosineSimilarity(float[] left, float[] right)
        {
            double dotProduct = 0.0D;
            double leftSquaredNorm = 0.0D;
            double rightSquaredNorm = 0.0D;
            for (int index = 0; index < left.Length; index++)
            {
                dotProduct += (double)left[index] * right[index];
                leftSquaredNorm += (double)left[index] * left[index];
                rightSquaredNorm += (double)right[index] * right[index];
            }

            double denominator = Math.Sqrt(leftSquaredNorm) * Math.Sqrt(rightSquaredNorm);
            return denominator < MINIMUM_NORM ? double.NegativeInfinity : dotProduct / denominator;
        }

        /// <summary>
        ///     バイナリ入力から非負の件数を読み取る。
        /// </summary>
        /// <param name="reader">読み取り元。</param>
        /// <param name="valueName">エラー表示用の値名。</param>
        /// <returns>読み取った非負の件数。</returns>
        private static int ReadNonNegativeCount(BinaryReader reader, string valueName)
        {
            int value = reader.ReadInt32();
            if (value < 0)
            {
                throw new InvalidDataException($"{valueName}が不正です。");
            }

            return value;
        }
    }
}
