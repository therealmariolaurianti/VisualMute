using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Timers;
using System.Windows;
using Accord.Math;
using NAudio.Wave;
using ScottPlot;

namespace VisualMute.Workers
{
    public class GraphPlotter
    {
        private const int _rate = 44100;
        private readonly int _bufferSize = (int) Math.Pow(2, 11);

        private BufferedWaveProvider _bufferedWaveProvider;
        private Timer _timer;
        private WpfPlot _wpfPlot;

        public void Initialize(WpfPlot wpfPlot)
        {
            _wpfPlot = wpfPlot;

            _timer = new Timer(100) {Enabled = true};
            _timer.Elapsed += OnTimerTick;

            StartListeningToMicrophone();
        }

        private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            _timer.Enabled = false;

            _wpfPlot.Dispatcher.Invoke(() =>
            {
                PlotLatestData();
                _wpfPlot.Plot.SetAxisLimits(0, 15, 0, 2);
                _wpfPlot.Render();
            });

            _timer.Enabled = true;
        }

        private void StartListeningToMicrophone(int audioDeviceNumber = 0)
        {
            var wi = new WaveIn();
            wi.DeviceNumber = audioDeviceNumber;
            wi.WaveFormat = new WaveFormat(_rate, 1);
            wi.BufferMilliseconds = (int) (_bufferSize / (double) _rate * 1000.0);
            wi.DataAvailable += AudioDataAvailable;
            _bufferedWaveProvider = new BufferedWaveProvider(wi.WaveFormat);
            _bufferedWaveProvider.BufferLength = _bufferSize * 2;
            _bufferedWaveProvider.DiscardOnBufferOverflow = true;
            try
            {
                wi.StartRecording();
            }
            catch
            {
                var msg = "Could not record from audio device!\n\n";
                msg += "Is your microphone plugged in?\n";
                msg += "Is it set as your default recording device?";
                MessageBox.Show(msg, "ERROR");
            }
        }

        private void AudioDataAvailable(object sender, WaveInEventArgs e)
        {
            _bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }

        private void PlotLatestData()
        {
            var frameSize = _bufferSize;
            var audioBytes = new byte[frameSize];
            _bufferedWaveProvider.Read(audioBytes, 0, frameSize);

            if (audioBytes.Length == 0)
                return;
            if (audioBytes[frameSize - 2] == 0)
                return;

            var BYTES_PER_POINT = 2;

            var graphPointCount = audioBytes.Length / BYTES_PER_POINT;

            var pcm = new double[graphPointCount];
            var fft = new double[graphPointCount];
            var fftReal = new double[graphPointCount / 2];

            for (var i = 0; i < graphPointCount; i++)
            {
                var val = BitConverter.ToInt16(audioBytes, i * 2);

                pcm[i] = val / Math.Pow(2, 16) * 200.0;
            }

            fft = FFT(pcm);

            double fftMaxFreq = _rate / 2;
            var fftPointSpacingHz = fftMaxFreq / graphPointCount;

            Array.Copy(fft, fftReal, fftReal.Length);

            _wpfPlot.Plot.Clear();
            _wpfPlot.Plot.AddSignal(fftReal, fftPointSpacingHz, Color.Red);
        }

        private double[] FFT(IReadOnlyList<double> data)
        {
            var fft = new double[data.Count];
            var fftComplex = new Complex[data.Count];

            for (var i = 0; i < data.Count; i++)
                fftComplex[i] = new Complex(data[i], 0.0);

            FourierTransform.FFT(fftComplex, FourierTransform.Direction.Forward);

            for (var i = 0; i < data.Count; i++)
                fft[i] = fftComplex[i].Magnitude;

            return fft;
        }
    }
}