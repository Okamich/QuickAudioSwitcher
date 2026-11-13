using System;
using System.Threading;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace QuickAudioSwitcher.Core
{
    public class ChimeSampleProvider : ISampleProvider
    {
        private readonly WaveFormat _format;
        private readonly double _freq1 = 587.33; // D5
        private readonly double _freq2 = 880.00; // A5
        private readonly int _totalSamples;
        private readonly int _switchSample;
        private int _sampleIndex;

        public ChimeSampleProvider(WaveFormat deviceFormat, double durationSeconds = 0.55)
        {
            // WASAPI shared mode natively expects IEEE float 32-bit
            int sampleRate = deviceFormat?.SampleRate ?? 48000;
            int channels = Math.Max(1, deviceFormat?.Channels ?? 2);
            _format = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);

            _totalSamples = (int)(_format.SampleRate * durationSeconds);
            _switchSample = (int)(_format.SampleRate * 0.16);
        }

        public WaveFormat WaveFormat => _format;

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesWritten = 0;
            int channels = _format.Channels;

            while (samplesWritten < count && _sampleIndex < _totalSamples)
            {
                double t = (double)_sampleIndex / _format.SampleRate;
                double sampleValue;

                if (_sampleIndex < _switchSample)
                {
                    double env = Math.Min(1.0, (double)_sampleIndex / (_format.SampleRate * 0.015)) * Math.Exp(-5.5 * t);
                    sampleValue = Math.Sin(2.0 * Math.PI * _freq1 * t) * env;
                }
                else
                {
                    double t2 = (double)(_sampleIndex - _switchSample) / _format.SampleRate;
                    double env = Math.Min(1.0, t2 / 0.015) * Math.Exp(-4.0 * t2);
                    sampleValue = Math.Sin(2.0 * Math.PI * _freq2 * t2) * env;
                }

                float finalFloat = (float)(Math.Clamp(sampleValue * 0.6, -1.0, 1.0));

                for (int ch = 0; ch < channels && samplesWritten < count; ch++)
                {
                    buffer[offset + samplesWritten++] = finalFloat;
                }

                _sampleIndex++;
            }

            return samplesWritten;
        }
    }

    public static class AudioTester
    {
        private static readonly object _lock = new();
        private static CancellationTokenSource? _currentCts;

        /// <summary>
        /// Plays a pleasant test chime on the specified audio endpoint without changing Windows default device.
        /// </summary>
        public static async Task PlayTestChimeAsync(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) return;

            lock (_lock)
            {
                _currentCts?.Cancel();
                _currentCts = new CancellationTokenSource();
            }

            var token = _currentCts.Token;

            await Task.Run(() =>
            {
                try
                {
                    // Create MMDeviceEnumerator directly on this thread to avoid COM apartment marshalling issues
                    using var enumerator = new MMDeviceEnumerator();
                    var device = enumerator.GetDevice(deviceId);
                    if (device == null) return;

                    var mixFormat = device.AudioClient.MixFormat;
                    var chime = new ChimeSampleProvider(mixFormat, 0.55);

                    using var wasapi = new WasapiOut(device, AudioClientShareMode.Shared, false, 50);
                    wasapi.Init(chime);
                    wasapi.Play();

                    while (wasapi.PlaybackState == PlaybackState.Playing && !token.IsCancellationRequested)
                    {
                        Thread.Sleep(30);
                    }

                    if (wasapi.PlaybackState == PlaybackState.Playing)
                    {
                        wasapi.Stop();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Interrupted by next test sound click
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error playing test sound: {ex.Message}");
                }
            }, token);
        }
    }
}
