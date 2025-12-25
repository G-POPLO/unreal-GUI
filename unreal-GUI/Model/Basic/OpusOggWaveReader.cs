using Concentus.Oggfile;
using Concentus.Structs;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;

namespace unreal_GUI.Model.Basic
{

    /// <summary>
    /// 提供将 OGG/OPUS 文件转换为 PCM 数据流的功能，以便 NAudio 使用（需要改进）
    /// </summary>
    public class OpusOggWaveReader : WaveStream
    {
        readonly WaveFormat waveFormat;
        readonly MemoryStream oggStream;
        readonly OpusDecoder decoder;
        readonly OpusOggReadStream decodeStream;
        byte[] wavData;

        [Obsolete]
        private OpusOggWaveReader(string oggFile)
        {
            try
            {
                using (FileStream fileStream = new(oggFile, FileMode.Open, FileAccess.Read))
                {
                    oggStream = new MemoryStream();
                    fileStream.CopyTo(oggStream);
                }
                oggStream.Seek(0, SeekOrigin.Begin);
                waveFormat = new WaveFormat(48000, 16, 2);
                decoder = new OpusDecoder(48000, 2);
                decodeStream = new OpusOggReadStream(decoder, oggStream);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"加载声音文件时发生未知错误: {oggFile}", ex);
            }
        }

        [Obsolete]
        byte[] Decode()
        {
            try
            {
                using var wavStream = new MemoryStream();
                var decoder = new OpusDecoder(48000, 2);
                var oggIn = new OpusOggReadStream(decoder, oggStream);

                // 检查是否有数据包可以解码
                if (!oggIn.HasNextPacket)
                    throw new InvalidOperationException("声音文件中没有可解码的数据包");

                while (oggIn.HasNextPacket)
                {
                    try
                    {
                        short[] packet = oggIn.DecodeNextPacket();
                        if (packet != null)
                        {
                            byte[] binary = ShortsToBytes(packet);
                            wavStream.Write(binary, 0, binary.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"解码声音数据包时发生错误: {ex.Message}", ex);
                    }
                }
                return wavStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"解码声音文件时发生错误: {ex.Message}", ex);
            }
        }

        public override WaveFormat WaveFormat => waveFormat;

        public override long Length => wavData.LongLength;

        public override TimeSpan TotalTime => decodeStream.TotalTime;

        public override long Position { get; set; }

#pragma warning disable CA1041 // 提供 ObsoleteAttribute 消息
        [Obsolete]
#pragma warning restore CA1041 // 提供 ObsoleteAttribute 消息
        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                if (wavData == null)
                {
                    wavData = Decode();
                }
                int n = (int)Math.Min(wavData.Length - Position, count);
                Array.Copy(wavData, Position, buffer, offset, n);
                Position += n;
                return n;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"读取声音数据时发生错误: {ex.Message}", ex);
            }
        }

        static byte[] ShortsToBytes(short[] input)
        {
            byte[] output = new byte[input.Length * sizeof(short) / sizeof(byte)];
            for (int i = 0; i < input.Length; ++i)
            {
                output[i * 2] = (byte)(input[i] & 0xFF);
                output[i * 2 + 1] = (byte)((input[i] >> 8) & 0xFF);
            }
            return output;
        }

        protected override void Dispose(bool disposing)
        {
            oggStream.Dispose();
            base.Dispose(disposing);
        }


        /// <summary>
        /// 播放OPUS音频文件
        /// </summary>
        /// <param name="opusFilePath">OPUS文件路径</param>
#pragma warning disable CA1041 // 提供 ObsoleteAttribute 消息
        [Obsolete]
#pragma warning restore CA1041 // 提供 ObsoleteAttribute 消息
        public static void PlayOpusFile(string opusFilePath)
        {
            if (!File.Exists(opusFilePath))
                throw new FileNotFoundException($"找不到指定的声音文件: {opusFilePath}", opusFilePath);

            // 创建WasapiOut播放器和OpusOggWaveReader
            using var reader = new OpusOggWaveReader(opusFilePath);
            using var player = new WasapiOut();

            // 初始化播放器
            player.Init(reader);

            // 播放音频
            

            // 等待播放完成
            while (player.PlaybackState == PlaybackState.Playing)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 异步播放OPUS音频文件
        /// </summary>
        /// <param name="opusFilePath">OPUS文件路径</param>
        /// <returns>异步任务</returns>
#pragma warning disable CA1041 // 提供 ObsoleteAttribute 消息
        [Obsolete]
#pragma warning restore CA1041 // 提供 ObsoleteAttribute 消息
        public static async Task PlayOpusFileAsync(string opusFilePath)
        {

            if (!File.Exists(opusFilePath))
                throw new FileNotFoundException($"找不到指定的声音文件: {opusFilePath}", opusFilePath);

            // 使用Task.Run在后台线程播放音频
            await Task.Run(() =>
            {
                // 创建WaveOutEvent播放器和OpusOggWaveReader
                using var reader = new OpusOggWaveReader(opusFilePath);
                using var player = new WasapiOut();


                // 初始化播放器
                player.Init(reader);

                // 播放音频
                

                // 等待播放完成
                while (player.PlaybackState == PlaybackState.Playing)
                {
                    System.Threading.Thread.Sleep(100);
                }
            });
        }

    }
}