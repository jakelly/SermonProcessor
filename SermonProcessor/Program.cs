using System;
using System.IO;
using NAudio.Wave;
using SoundTouchNet;
using System.Diagnostics;
using SoundTouch.Properties;

namespace SoundTouchExample
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            //Location of Wav Files that are approximately 29 minutes in length
            /*
            Directory.SetCurrentDirectory(Settings.Default.BaseDirectory);
                       
            if (!Directory.Exists(Settings.Default.TrimmedDirectory))
                Directory.CreateDirectory(Settings.Default.TrimmedDirectory);

            if (!Directory.Exists(Settings.Default.SpeechCleanedDirectory)) 
                Directory.CreateDirectory(Settings.Default.SpeechCleanedDirectory);

            if (!Directory.Exists(Settings.Default.IntroOutroDirectory))
                Directory.CreateDirectory(Settings.Default.IntroOutroDirectory);
            */

            Directory.SetCurrentDirectory(Settings.Default.BaseDirectory);
            
            foreach (string File in Directory.GetFiles(".", "*.wav"))
            {
                string OutFile = SoxEffects(File);
                TrimAudioLength(OutFile);
                //AddIntroOutro(TrimAudioLength(OutFile));
            }
            Console.WriteLine("Finished Processing Files");
            Console.ReadLine();
        }

        private static string TrimAudioLength(string File)
        {
            TimeSpan TargetDuration = new TimeSpan(0, 29, 0); // 29 minutes

            WaveFileReader reader = new WaveFileReader(File);
            //Calculate Tempo
            float PercentChange = CalculateTempo(reader.TotalTime, TargetDuration);
            Console.ResetColor();
            if (Math.Abs(PercentChange) > 2)
                Console.ForegroundColor = ConsoleColor.Green; //Greater than 2% delta
            if (Math.Abs(PercentChange) > 3)
                Console.ForegroundColor = ConsoleColor.Yellow; //Greater than 3% delta
            if (Math.Abs(PercentChange) > 4)
                Console.ForegroundColor = ConsoleColor.Red; //Greater than 4% delta

            string FileOut = File.Replace(Settings.Default.SpeechCleanedDirectory, Settings.Default.TrimmedDirectory);
            processWave(File, FileOut, 1 + PercentChange * 0.01f, 1.0f, 1.0f);
            
            //AddIntroOutro(FileOut);

            Console.WriteLine("{2}%\t{1}\t{0}", File.Replace(@".\Joel Chapman", ""), reader.TotalTime, PercentChange.ToString("N3"));
            reader.Close();
            
            return FileOut;
        }

        private static string SoxEffects(string File)
        {
            string FileOut = File.Replace(@".\", Settings.Default.SpeechCleanedDirectory);
            
            
            Process p = Process.Start(Settings.Default.SoxEffectBatchFile, string.Format("\"{0}\" \"{1}\"", File, FileOut));

            if (p.WaitForExit(10 * 60 * 1000)) //10 minutes;
            {
                Console.WriteLine("File {0} updated and copied to {1}", File, FileOut);
            }
            else
            {
                Console.WriteLine("File {0} failed to apply sox effects in a timely manner.", File, FileOut);
            }

            return FileOut;
        }


        private static void AddIntroOutro(string OriginalAudioFileName)
        {
            WaveFileReader intro = new WaveFileReader(Settings.Default.IntroFile);
            WaveFileReader outro = new WaveFileReader(Settings.Default.OutroFile);
            //WaveFileReader intro = new WaveFileReader(@"C:\Users\jkelly\Documents\Church\Radio Ministry\Original\Exported WAV\Test\Media\Intro.wav");
            //WaveFileReader outro = new WaveFileReader(@"C:\Users\jkelly\Documents\Church\Radio Ministry\Original\Exported WAV\Test\Media\Outro.wav");

            WaveFileReader audio = new WaveFileReader(OriginalAudioFileName);

            WaveMixerStream32 mixer = new WaveMixerStream32();
            //mixer.AutoStop;

            WaveOffsetStream audioOffsetted = new WaveOffsetStream(
                audio,
                TimeSpan.FromSeconds(Settings.Default.SermonStartTime), //22.5 seconds after start of intro.
                TimeSpan.Zero, 
                audio.TotalTime);

            TimeSpan outroOffset = TimeSpan.FromSeconds(Settings.Default.SermonStartTime) + audio.TotalTime +
               TimeSpan.FromSeconds(Settings.Default.OutroStartTime) - outro.TotalTime;

            WaveOffsetStream outroOffsetted = new WaveOffsetStream(
               outro, 
               outroOffset, 
               TimeSpan.Zero, 
               outro.TotalTime);

            WaveChannel32 intro32 = new WaveChannel32(intro);
            intro32.PadWithZeroes = false;
            mixer.AddInputStream(intro32);

            WaveChannel32 outro32 = new WaveChannel32(outroOffsetted);
            outro32.PadWithZeroes = false;
            mixer.AddInputStream(outro32);

            WaveChannel32 audio32 = new WaveChannel32(audioOffsetted);
            audio32.PadWithZeroes = false;
            mixer.AddInputStream(audio32);

            //string FileOut = OriginalAudioFileName.Replace(@".\Trimmed\", @".\IntroOutroAdded\");
            string FileOut = OriginalAudioFileName.Replace(Settings.Default.TrimmedDirectory, Settings.Default.IntroOutroDirectory);


            WaveFileWriter.CreateWaveFile(FileOut, new Wave32To16Stream(mixer));

        }

        private static float CalculateTempo(TimeSpan CurrentDuration, TimeSpan TargetDuration)
        {
            return ((CurrentDuration.Ticks * 100.0f) / TargetDuration.Ticks) - 100.0f;
        }

        /// <summary>    
        /// Load wave file and change its tempo, pitch and rate and save it to another file    
        /// </summary>    
        static void processWave(string fileIn, string fileOut, float newTempo, float newPitch, float newRate)
        {
            WaveFileReader reader = new WaveFileReader(fileIn);
            int numChannels = reader.WaveFormat.Channels;
            if (numChannels > 2)
                throw new Exception("SoundTouch supports only mono or stereo.");
            int sampleRate = reader.WaveFormat.SampleRate;
            int bitPerSample = reader.WaveFormat.BitsPerSample;
            const int BUFFER_SIZE = 1024 * 16;
            SoundStretcher stretcher = new SoundStretcher(sampleRate, numChannels);
            WaveFileWriter writer = new WaveFileWriter(fileOut, new WaveFormat(sampleRate, 16, numChannels));
            stretcher.Tempo = newTempo;
            stretcher.Pitch = newPitch;
            stretcher.Rate = newRate;
            byte[] buffer = new byte[BUFFER_SIZE];
            short[] buffer2 = null;

            if (bitPerSample != 16 && bitPerSample != 8)
            {
                throw new Exception("Not implemented yet.");
            }

            if (bitPerSample == 8)
            {
                buffer2 = new short[BUFFER_SIZE];
            }

            bool finished = false;
            while (true)
            {
                int bytesRead = 0;
                if (!finished)
                {
                    bytesRead = reader.Read(buffer, 0, BUFFER_SIZE);
                    if (bytesRead == 0)
                    {
                        finished = true;
                        stretcher.Flush();
                    }
                    else
                    {
                        if (bitPerSample == 16)
                        {
                            stretcher.PutSamplesFromBuffer(buffer, 0, bytesRead);
                        }
                        else if (bitPerSample == 8)
                        {
                            for (int i = 0; i < BUFFER_SIZE; i++)
                                buffer2[i] = (short)((buffer[i] - 128) * 256);
                            stretcher.PutSamples(buffer2);
                        }
                    }
                }

                bytesRead = stretcher.ReceiveSamplesToBuffer(buffer, 0, BUFFER_SIZE);
                writer.WriteData(buffer, 0, bytesRead);
                if (finished && bytesRead == 0)
                    break;
            }

            reader.Close();
            writer.Close();
        }
    }

}
