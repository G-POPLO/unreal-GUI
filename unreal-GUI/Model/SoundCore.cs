namespace unreal_GUI.Model
{

    /// <summary>
    /// 提供将 OGG/OPUS 文件转换为 PCM 数据流的功能，以便 NAudio 使用。
    /// </summary>
    //public class OpusWaveProvider : IWaveProvider, IDisposable // 实现 IDisposable
    //{
    //    private readonly OpusOggReadStream _opusStream; // 移除 readonly，因为我们将在构造函数中赋值
    //    private readonly WaveFormat _waveFormat;
    //    // 使用足够大的缓冲区以适应典型的一帧解码数据。
    //    // 48kHz, Stereo, 16-bit, 120ms (opus 最大帧大小) 大约是 48000 * 2 * 2 * 0.12 = ~23KB.
    //    // 16KB 缓冲区通常足够用于单帧，但如果遇到非常大的帧，可能需要更大或动态调整。
    //    private readonly byte[] _pcmBuffer = new byte[16384];
    //    private int _bufferOffset;
    //    private int _bytesInBuffer;
    //    private readonly MemoryStream _oggStream; // 保留对内存流的引用以进行 dispose

    //    /// <summary>
    //    /// 获取此 WaveProvider 的格式。
    //    /// </summary>
    //    public WaveFormat WaveFormat => _waveFormat;

    //    /// <summary>
    //    /// 初始化一个新的 OpusWaveProvider 实例。
    //    /// </summary>
    //    /// <param name="opusFilePath">OPUS 文件的路径。</param>
    //    public OpusWaveProvider(string opusFilePath)
    //    {
    //        // 将文件内容加载到内存流中
    //        using (FileStream fileStream = new(opusFilePath, FileMode.Open, FileAccess.Read))
    //        {
    //            _oggStream = new MemoryStream();
    //            fileStream.CopyTo(_oggStream);
    //        }

    //        // 必须在使用流之前将其位置重置为开头
    //        _oggStream.Position = 0;

    //        // 使用内存流初始化 OpusOggReadStream
    //        // OpusOggReadStream 会自动检测采样率和通道数
    //        _opusStream = new OpusOggReadStream(_oggStream);

    //        // 根据解码器的实际输出设置 WaveFormat
    //        // 注意：Concentus 解码为 float[], 但我们将其转换为 16-bit PCM
    //        // NAudio 的 WaveFormat 通常用于表示目标 PCM 格式
    //        _waveFormat = new WaveFormat(_opusStream.SampleRate, 16, _opusStream.NumChannels); // 动态获取参数
    //    }

    //    /// <summary>
    //    /// 从音频流中读取 PCM 数据。
    //    /// </summary>
    //    /// <param name="buffer">存储读取数据的目标缓冲区。</param>
    //    /// <param name="offset">目标缓冲区中开始写入的位置。</param>
    //    /// <param name="count">请求读取的最大字节数。</param>
    //    /// <returns>实际读取并写入缓冲区的字节数。</returns>
    //    public int Read(byte[] buffer, int offset, int count)
    //    {
    //        int totalRead = 0;

    //        while (totalRead < count)
    //        {
    //            // 如果内部缓冲区还有数据，则从中复制
    //            if (_bufferOffset < _bytesInBuffer)
    //            {
    //                int toCopy = Math.Min(_bytesInBuffer - _bufferOffset, count - totalRead);
    //                Array.Copy(_pcmBuffer, _bufferOffset, buffer, offset + totalRead, toCopy);
    //                _bufferOffset += toCopy;
    //                totalRead += toCopy;
    //                continue; // 继续循环，看是否还需要更多数据
    //            }

    //            // 内部缓冲区已空，尝试从 Opus 流读取并解码下一帧
    //            float[] samples;
    //            try
    //            {
    //                // ReadNextRawPacket 返回 float[]，范围通常在 [-1.0f, 1.0f] 之间
    //                samples = _opusStream.ReadNextRawPacket();
    //            }
    //            catch (EndOfStreamException)
    //            {
    //                // 已到达流末尾
    //                break;
    //            }
    //            catch (Exception ex) // 捕获其他可能的解码错误
    //            {
    //                // 可以选择记录日志或重新抛出异常
    //                System.Diagnostics.Debug.WriteLine($"Error reading OPUS packet: {ex.Message}");
    //                break; // 或 throw;
    //            }


    //            // 如果没有更多数据可读，则结束
    //            if (samples == null || samples.Length == 0)
    //            {
    //                break;
    //            }

    //            // 将 float[] 转换为 16-bit PCM 字节并填充内部缓冲区
    //            _bytesInBuffer = 0;
    //            _bufferOffset = 0;

    //            // 检查缓冲区是否有足够空间 (每个 float -> 2 bytes)
    //            if (samples.Length * 2 > _pcmBuffer.Length)
    //            {
    //                // 这里可以选择扩展缓冲区或只处理能容纳的部分
    //                // 对于标准 opus 帧，通常不会发生这种情况
    //                System.Diagnostics.Debug.WriteLine("Warning: Decoded frame is larger than internal buffer.");
    //                // 简单处理：只处理能容纳的部分
    //                Array.Resize(ref _pcmBuffer, samples.Length * 2);
    //            }

    //            for (int i = 0; i < samples.Length; i++)
    //            {
    //                // Clamping might be redundant if decoder guarantees [-1,1]
    //                // but it's safer to include.
    //                float clamped = Math.Clamp(samples[i], -1.0f, 1.0f);
    //                // Convert float [-1,1] to short [-32768, 32767]
    //                // Adding 0.5f before truncation is sometimes used for better rounding,
    //                // but simple cast is common and usually sufficient.
    //                short sample16 = (short)(clamped * short.MaxValue); // short.MaxValue is 32767

    //                // Little-endian storage: low byte first
    //                _pcmBuffer[_bytesInBuffer++] = (byte)(sample16 & 0xFF);
    //                _pcmBuffer[_bytesInBuffer++] = (byte)((sample16 >> 8) & 0xFF);
    //            }
    //            _bufferOffset = 0; // Reset offset for the newly filled buffer
    //            // 循环会继续，下一次迭代将从新填充的内部缓冲区复制数据
    //        }

    //        return totalRead;
    //    }

    //    #region IDisposable Support
    //    private bool disposedValue = false; // 要检测冗余调用

    //    protected virtual void Dispose(bool disposing)
    //    {
    //        if (!disposedValue)
    //        {
    //            if (disposing)
    //            {
    //                // 释放托管状态 (托管对象)
    //                _opusStream?.Dispose();
    //                _oggStream?.Dispose();
    //            }

    //            // 释放非托管资源 (非托管对象) 并重写终结器
    //            // TODO: 设置大型字段为 null

    //            disposedValue = true;
    //        }
    //    }

    //    // 仅当以上 Dispose(bool disposing) 有处置非托管资源的代码时才重写终结器
    //    // ~OpusWaveProvider()
    //    // {
    //    //   // 不要更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
    //    //   Dispose(false);
    //    // }

    //    // 添加此代码以正确实现可处置模式。
    //    public void Dispose()
    //    {
    //        // 不要更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
    //        Dispose(true);
    //        // TODO: 如果以上终结器被重写，则取消注释以下行。
    //        // GC.SuppressFinalize(this);
    //    }
    //    #endregion
    //}

}