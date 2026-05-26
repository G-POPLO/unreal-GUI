using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace unreal_GUI.Model.Basic;

static class SoundFX
{
    public static void PlaySound(byte type)
    {
        string soundFile = type switch
        {
            0 => "ui-sound-on.opus",
            1 => "ui-sound-off.opus",
            2 => "ui-sound-error.opus",
            3 => "ui-sound-notification.opus",
            4 => "ui-sound-success.opus",
            _ => "ui-sound-on.opus"
        };

        string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", soundFile);
        _ = PlayAsync(soundPath);
    }

    static async Task PlayAsync(string soundPath)
    {
        if (!File.Exists(soundPath))
        {
            Debug.WriteLine($"找不到指定的声音文件: {soundPath}");
            return;
        }

        try
        {
            using var reader = new Opus2PCM(soundPath);
            using var player = new WaveOutEvent();
            var tcs = new TaskCompletionSource<bool>();

            player.PlaybackStopped += (_, _) => tcs.TrySetResult(true);
            player.Init(reader);
            player.Play();

            await tcs.Task;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"播放声音时发生错误: {ex.Message}");
        }
    }
}
