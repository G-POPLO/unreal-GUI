using System;
using System.IO;
using System.Threading.Tasks;

namespace unreal_GUI.Model
{
    class SoundFX
    {

        [Obsolete]
        public void PlaySound(byte type)
        {
            string soundFile = type switch
            {
                0 => "ui-sound-on.opus",
                1 => "ui-sound-off.opus",
                _ => "ui-sound-on.opus"
            };

            string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", soundFile);

            // 检查文件是否存在
            if (!File.Exists(soundPath))
                throw new FileNotFoundException($"声音文件不存在: {soundPath}");

            // 在后台线程播放音频以避免阻塞UI
            Task.Run(() =>
            {
                try
                {
                    OpusOggWaveReader.PlayOpusFile(soundPath);
                }
                catch (Exception ex)
                {
                    // 记录异常但不抛出，避免影响主程序流程
                    System.Diagnostics.Debug.WriteLine($"播放声音时发生错误: {ex.Message}");
                }
            });
        }
    }
}
