using Concentus;
using Concentus.Oggfile;
using NAudio.Wave;
using System;
using System.IO;

namespace unreal_GUI.Model.Basic;

/// <summary>
/// 提供将 OGG/OPUS 文件转换为 PCM 数据流的功能，以便 NAudio 使用
/// </summary>
public class Opus2PCM : WaveStream
{
    readonly WaveFormat waveFormat;
    readonly MemoryStream oggStream;
    readonly OpusOggReadStream decodeStream;
    byte[]? wavData;

    public Opus2PCM(string oggFile)
    {
        try
        {
            using var fileStream = new FileStream(oggFile, FileMode.Open, FileAccess.Read);
            oggStream = new MemoryStream();
            fileStream.CopyTo(oggStream);
            oggStream.Position = 0;

            waveFormat = new WaveFormat(48000, 16, 2);
            var decoder = OpusCodecFactory.CreateDecoder(48000, 2);
            decodeStream = new OpusOggReadStream(decoder, oggStream);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"加载声音文件时发生未知错误: {oggFile}", ex);
        }
    }

    byte[] Decode()
    {
        if (!decodeStream.HasNextPacket)
            throw new InvalidOperationException("声音文件中没有可解码的数据包");

        using var wavStream = new MemoryStream();

        while (decodeStream.HasNextPacket)
        {
            short[]? packet = decodeStream.DecodeNextPacket();
            if (packet != null)
            {
                byte[] binary = new byte[packet.Length * sizeof(short)];
                Buffer.BlockCopy(packet, 0, binary, 0, binary.Length);
                wavStream.Write(binary, 0, binary.Length);
            }
        }

        return wavStream.ToArray();
    }

    public override WaveFormat WaveFormat => waveFormat;

    public override long Length => wavData?.LongLength ?? 0;

    public override long Position { get; set; }

    public override int Read(byte[] buffer, int offset, int count)
    {
        wavData ??= Decode();

        int n = (int)Math.Min(wavData.Length - Position, count);
        Array.Copy(wavData, Position, buffer, offset, n);
        Position += n;
        return n;
    }

    protected override void Dispose(bool disposing)
    {
        oggStream.Dispose();
        base.Dispose(disposing);
    }
}
