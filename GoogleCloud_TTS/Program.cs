using Google.Cloud.TextToSpeech.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCloud_TTS
{
    class Program
    {
        /*
         * https://cloud.google.com/text-to-speech/?hl=ja
         * https://cloud.google.com/text-to-speech/docs/quickstart-client-libraries?hl=ja#client-libraries-usage-csharp
         * */

        static void Main(string[] args)
        {
            // Instantiate a client
            TextToSpeechClient client = TextToSpeechClient.Create();

            // Set the text input to be synthesized.
            SynthesisInput input = new SynthesisInput
            {
                //Text = "Hello, World!"
                Text = "3Dプリンタが停止している可能性があります。ご確認ください。"
            };

            // Build the voice request, select the language code ("en-US"),
            // and the SSML voice gender ("neutral").
            VoiceSelectionParams voice = new VoiceSelectionParams
            {
                //LanguageCode = "en-US",
                LanguageCode = "ja-JP",
                SsmlGender = SsmlVoiceGender.Female,
                Name = "ja-JP-Wavenet-B"
            };

            // Select the type of audio file you want returned.
            AudioConfig config = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            // Perform the Text-to-Speech request, passing the text input
            // with the selected voice parameters and audio file type
            var response = client.SynthesizeSpeech(new SynthesizeSpeechRequest
            {
                Input = input,
                Voice = voice,
                AudioConfig = config
            });

            // Write the binary AudioContent of the response to an MP3 file.
            using (Stream output = File.Create(DateTime.Now.ToString("MMdd_HHmmss")+".mp3"))
            {
                response.AudioContent.WriteTo(output);
                Console.WriteLine($"Audio content written to file 'sample.mp3'");
            }
        }
    }
}
