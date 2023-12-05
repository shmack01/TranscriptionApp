/*
 * Sample Code is provided for the purpose of illustration only and is not intended to be used in a 
 * production environment. THIS SAMPLE CODE AND ANY RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT 
 * WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
 * We grant You a nonexclusive, royalty-free right to use and modify the Sample Code and to 
 * reproduce and distribute the object code form of the Sample Code, provided that. 
 * You agree: (i) to not use Our name, logo, or trademarks to market 
 * Your software product in which the Sample Code is embedded; (ii) to include a 
 * valid copyright notice on Your software product in which the Sample Code is 
 * embedded; and (iii) to indemnify, hold harmless, and defend Us and 
 * Our suppliers from and against any claims or lawsuits, including attorneys’ fees, 
 * that arise or result from the use or distribution of the Sample Code
 * 
 *  Copyright (c) Microsoft. All rights reserved.
 *  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
 *  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
 *  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
 *  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
 */
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Threading.Tasks;
using System.Net;
using TranscriptionApp.Services;
using Newtonsoft.Json;
using Windows.Globalization;
using System.Net.Http;
using System.Collections.ObjectModel;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.UI.Xaml.Documents;
using Windows.Media.Streaming.Adaptive;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TranscriptionApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        //TODO
        private static string LANGUAGES_ENDPOINT = @"languages?api-version=3.0";
        
        private static string endpoint_var = "TRANSLATOR_ENDPOINT";
        private const string region_var = "SPEECH_SERVICE_REGION";
        private const string speech_endpoint_var = "SPEECH_ENDPOINT";
        private const string key_var = "SPEECH_TEXT_RESOURCE_KEY";

        private static readonly string endpoint = Environment.GetEnvironmentVariable(endpoint_var);
        private static readonly string region = Environment.GetEnvironmentVariable(region_var);
        private static readonly string resourceKey = Environment.GetEnvironmentVariable(key_var);
        private static readonly string speechEndpoint = Environment.GetEnvironmentVariable(speech_endpoint_var);



        public MainWindow()
        {
            this.InitializeComponent();
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            Languages = GetLanguages().Result;
            langTocb.ItemsSource = Languages;
            langFromcb.ItemsSource = Languages;
            //default is English
            Services.Language defaultLang = (Services.Language)(Languages.Where(l => l.ShortName == "en").FirstOrDefault());
            langTocb.SelectedItem = defaultLang;
            langFromcb.SelectedItem = defaultLang;
            ConvertFromLanguage = defaultLang;
            ConvertToLanguage = defaultLang;

            StopSpeechRecognitionButton.IsEnabled = false;
            SpeechRecognitionButton.IsEnabled = true;
            infoBar.Message = "Enable Microphone, then start or Choose a file for Speech Recognition";
            infoBar.Severity = InfoBarSeverity.Informational;

        }

        public ObservableCollection<Services.Language> Languages { get; set; }
        public static Services.Language ConvertFromLanguage { get; set; }
        public static Services.Language ConvertToLanguage { get; set; }
        private TaskCompletionSource<int> stopRecognition;
        public bool SpeechEnabled { get; set; }
        private async void EnableMicrophone_ButtonClicked(object sender, RoutedEventArgs e)
        {
            bool isMicAvailable = EnableMicrophoneButton.IsChecked.Value;
            try
            {
                if (isMicAvailable)
                {
                    var mediaCapture = new Windows.Media.Capture.MediaCapture();
                    var settings = new Windows.Media.Capture.MediaCaptureInitializationSettings();
                    settings.StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.Audio;
                    await mediaCapture.InitializeAsync(settings);
                }
            }
            catch (Exception)
            {
                isMicAvailable = false;
                EnableMicrophoneButton.IsChecked = false;
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-microphone"));
            
            }
            if (!isMicAvailable)
            {
                StopSpeechRecognition();
                SpeechRecognitionButton.IsEnabled = false;
            }
            else
            {
                //Todo: update async
                infoBar.Severity = InfoBarSeverity.Informational;
                infoBar.Message = "Microphone was enabled";

            }
            
        }
        private async void StopSpeechRecognitionFromMicrophone_ButtonClicked(object sender, RoutedEventArgs e)
        {
            StopSpeechRecognition();
        }
        void StopSpeechRecognition()
        {
            StopSpeechRecognitionButton.IsEnabled = false;
            SpeechRecognitionButton.IsEnabled = true;
            if (stopRecognition != null)
            {
                stopRecognition.TrySetResult(0);

            }
            NotifyUser("Choose File", NotifyType.CommandBarMessage);
        }

        private async void SpeechRecognitionFromMicrophone_ButtonClicked(object sender, RoutedEventArgs e)
        {
            // </skeleton_1>
            // <create_speech_configuration>
            var config = SpeechConfig.FromEndpoint(new Uri(speechEndpoint), resourceKey);

            // Set the mode of input language detection to either "AtStart" (the default) or "Continuous".
            // Please refer to the documentation of Language ID for more information.
            // https://aka.ms/speech/lid?pivots=programming-language-csharp
            config.SetProperty(PropertyId.SpeechServiceConnection_LanguageIdMode, "Continuous");

            // Creates an instance of a speech config with specified subscription key and service region.
            // Replace with your own subscription key and service region (e.g., "westus").
            //Use this only for commerical endpoing: var config = SpeechConfig.FromSubscription(resourceKey, region);

            //TODO: Work out issues with binding to a property
            SpeechRecognitionButton.IsEnabled = false;
            StopSpeechRecognitionButton.IsEnabled = true;
            // Define the set of input spoken languages to detect
            var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "en-US" });

            // </create_speech_configuration>

            // <skeleton_2>
            try
            {
                NotifyUser("Starting Speech Recognition from microphone", NotifyType.StatusMessage);

                // </skeleton_2>
                // <create_speech_recognizer_1>
                // Creates a speech recognizer using microphone as audio input.
                using (var recognizer = new SpeechRecognizer(config, autoDetectSourceLanguageConfig))
                {
                    await StartSpeechProcessing(recognizer).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                infoBar.Severity = InfoBarSeverity.Error;
                infoBar.Message = $"Enable Microphone First.\n {ex.ToString()}";

            }
        }

        private async Task StartSpeechProcessing(SpeechRecognizer recognizer)
        {
            StringBuilder sb = new StringBuilder();
            // The TaskCompletionSource to stop recognition.
            stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            // Subscribes to events.
            recognizer.SpeechStartDetected += (s, e) =>
            {
                NotifyUser("Speech Detected ....", NotifyType.CommandBarMessage);
            };
            recognizer.Recognizing += (s, e) =>
            {
                NotifyUser("Recognizing ....", NotifyType.CommandBarMessage);
                sb.Clear();
            };

            recognizer.Recognized += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedIntent)
                {
                    Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");

                }
                else if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    sb.AppendLine($"RECOGNIZED: Text={e.Result.Text}");
                    NotifyUser(sb.ToString(), NotifyType.StatusMessage);
                }
                else if (e.Result.Reason == ResultReason.NoMatch)
                {
                    NotifyUser("NOMATCH: Speech could not be recognized.", NotifyType.StatusMessage);

                }
            };

            recognizer.Canceled += (s, e) =>
            {
                Console.WriteLine($"CANCELED: Reason={e.Reason}");

                if (e.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                    Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    var cancellation = CancellationDetails.FromResult(e.Result);
                    sb.AppendLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        sb.AppendLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        sb.AppendLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        sb.AppendLine($"CANCELED: Did you update the subscription info?");
                    }
                }

                stopRecognition.TrySetResult(0);
            };

            recognizer.SessionStarted += (s, e) =>
            {
                Console.WriteLine("\n    Session started event.");
            };

            recognizer.SessionStopped += (s, e) =>
            {
                Console.WriteLine("\n    Session stopped event.");
                Console.WriteLine("\nStop recognition.");
                stopRecognition.TrySetResult(0);

            };

            

            // Update the UI
            NotifyUser(sb.ToString(), NotifyType.StatusMessage);

            // </print_results>
            // <create_speech_recognizer_2>
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            Task.WaitAny(new[] { stopRecognition.Task });

            // Stops recognition.
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

            // </create_speech_recognizer_2>
            // <skeleton_3>

        }

        private enum NotifyType
        {
            StatusMessage,
            ErrorMessage,
            CommandBarMessage,
            InfoBar
        }
        private void NotifyUser(string strMessage, NotifyType type)
        {
            // If called from the UI thread, then update immediately.
            // Otherwise, schedule a task on the UI thread to perform the update
            //UWP methods
            // Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            //var Dispatcher = Windows.UI.Threading.Dispatcher.CurrentDispatcher;

            if (DispatcherQueue.HasThreadAccess)//Dispatcher.HasThreadAccess)
            {
                UpdateStatus(strMessage, type);
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    UpdateStatus(strMessage, type);
                });
                //var task = Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => UpdateStatus(strMessage, type));
            }
        }

        private async void UpdateStatus(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBlock.Text += string.IsNullOrEmpty(StatusBlock.Text) ? strMessage : "\n" + strMessage;
                    infoBar.Message = "Updating Speech Recognition";
                    infoBar.Severity = InfoBarSeverity.Success;
                    break;
                case NotifyType.ErrorMessage:
                    infoBar.Message = strMessage;
                    infoBar.Severity = InfoBarSeverity.Error;
                    break;
                case NotifyType.CommandBarMessage:
                    commandBarText.Text = strMessage;
                    break;
                case NotifyType.InfoBar:
                    infoBar.Message = strMessage;
                    infoBar.Severity = InfoBarSeverity.Informational;
                    break;
            }
            
            StatusScroller.ScrollToVerticalOffset(StatusScroller.ExtentHeight);
            // Collapse the StatusBlock if it has no text to conserve real estate.
            //StatusBorder.Visibility = !string.IsNullOrEmpty(StatusBlock.Text) ? Visibility.Visible : Visibility.Collapsed;
            if (!string.IsNullOrEmpty(StatusBlock.Text))
            {
                //StatusBorder.Visibility = Visibility.Visible;
                ///StatusPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ///StatusBorder.Visibility = Visibility.Collapsed;
                //StatusPanel.Visibility = Visibility.Collapsed;
            }
            //Translate
            if ((!String.IsNullOrEmpty(strMessage)) && NotifyType.StatusMessage  == type)
            {
                string translatedTxt = await TranslateAsync(strMessage);
                TranslateBlock.Text += string.IsNullOrEmpty(TranslateBlock.Text) ? translatedTxt : translatedTxt;
                TranslateScroller.ScrollToVerticalOffset(TranslateScroller.ExtentHeight);
                
            }

            // Raise an event if necessary to enable a screen reader to announce the status update.
            var peer = Microsoft.UI.Xaml.Automation.Peers.FrameworkElementAutomationPeer.FromElement(StatusBlock);
            if (peer != null)
            {
                peer.RaiseAutomationEvent(Microsoft.UI.Xaml.Automation.Peers.AutomationEvents.LiveRegionChanged);
            }
        }

        private async Task<string> TranslateAsync(string textToTranslate)
        {
            // This is our main function.
            // Output languages are defined in the route.
            // For a complete list of options, see API reference.
            // https://docs.microsoft.com/azure/cognitive-services/translator/reference/v3-0-translate

            List<string> text = await Services.Translate.TranslateTextRequest(textToTranslate, ConvertFromLanguage.ShortName, ConvertToLanguage.ShortName, false);


            StringBuilder sb = new StringBuilder();
            foreach (string str in text)
            {
                sb.AppendLine(str);
            }
            return sb.ToString();

        }

        static public async Task<ObservableCollection<Services.Language>> GetLanguages()
        {
            ObservableCollection<Services.Language> list = new ObservableCollection<Services.Language>();
            string result = String.Empty;
            Services.LanguageResult deserializedOutput = null;

            string url = endpoint + LANGUAGES_ENDPOINT;
            //Uncomment for obtaining Languages dynamically
            //otherwise the languages will be populated from the json file
            //check if the environment variable is set
            /*if (!String.IsNullOrEmpty(endpoint))
            {
                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage())
                {
                    // Build the request.
                    request.Method = HttpMethod.Get;
                    try
                    {
                        request.RequestUri = new Uri(url);

                        // Send the request and get response.
                        HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                        // Read response as a string.
                        result = await response.Content.ReadAsStringAsync();
                        deserializedOutput = JsonConvert.DeserializeObject<LanguageResult>(result);
                    }
                    catch (Exception ex)
                    {
                        //Error with reaching out to endpoint. Will use the backup languages.json file
                        result = String.Empty;
                    }

                }
            }*/


            if(String.IsNullOrEmpty(result))
            {
                string sDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string fileName = System.IO.Path.Combine(sDirectory, "Assets/languages.json");

                //Closes file when completed
                result = File.ReadAllText(fileName);
                deserializedOutput = JsonConvert.DeserializeObject<LanguageResult>(result);
            }
            

            foreach (var pair in deserializedOutput.Translation)
            {
                list.Add(new Services.Language { Name = pair.Value.Name, ShortName = pair.Key });
            }
            return list;
        }

        private void langTocb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConvertToLanguage = e.AddedItems[0] as Services.Language;
        }

        private void langFromcb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConvertFromLanguage = e.AddedItems[0] as Services.Language;
        }
        private async void PickAFileButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFile file = await GetFileFromPicker(".wav", ".mp4", ".mp3" );
            if (file != null)
            {
                // The StorageFile has read/write access to the picked file.
                // See the FileAccess sample for code that uses a StorageFile to read and write.
                infoBar.Severity = InfoBarSeverity.Informational;
                infoBar.Message = "Speech Recognition for file:" + file.Path;
                await SpeechRecognitionFromFile(file.Path);
            }
            else
            {
                infoBar.Severity = InfoBarSeverity.Warning;
                infoBar.Message = "Operation cancelled.";

            }
        }
        private async void UseStorageButton_Click(object sender, RoutedEventArgs e)
        {
            //Follow Steps:
            //https://learn.microsoft.com/en-us/azure/ai-services/speech-service/batch-transcription-audio-data?tabs=portal#enable-system-assigned-managed-identity-for-the-speech-resource

        }

        private async void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFile file = await GetFileFromPicker(".txt");
            if (file != null)
            {
                // The StorageFile has read/write access to the picked file.
                // See the FileAccess sample for code that uses a StorageFile to read and write.
                infoBar.Severity = InfoBarSeverity.Informational;
                infoBar.Message = "Speech Recognition for file:" + file.Path;
                if (DispatcherQueue.HasThreadAccess)//Dispatcher.HasThreadAccess)
                {
                    await System.IO.File.AppendAllTextAsync(file.Path, $"Status: {StatusBlock.Text} \n Translation: \n {TranslateBlock.Text} \n", Encoding.UTF8);
                }
                else
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        System.IO.File.AppendAllTextAsync(file.Path, $"Status: {StatusBlock.Text} \n Translation: \n {TranslateBlock.Text} \n", Encoding.UTF8);
                    });
                    //var task = Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => UpdateStatus(strMessage, type));
                }

            }
            else
            {
                infoBar.Severity = InfoBarSeverity.Warning;
                infoBar.Message = "Operation cancelled.";

            }
        }

        private async Task<StorageFile> GetFileFromPicker(params string[] fileNames)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            foreach(string f in fileNames)
            {
                openPicker.FileTypeFilter.Add(f);
            }

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);
            StorageFile file = await openPicker.PickSingleFileAsync();
            return file;
        }

        async Task SpeechRecognitionFromFile(string filePath)
        {

            //For compressed audio files such as MP4,
            //install GStreamer and use PullAudioInputStream or PushAudioInputStream.
            //For more information, see How to use compressed input audio.
            //https://learn.microsoft.com/en-us/azure/ai-services/speech-service/how-to-use-codec-compressed-audio-input-streams?tabs=windows%2Cdebian%2Cjava-android%2Cterminal&pivots=programming-language-csharp

            var config = SpeechConfig.FromEndpoint(new Uri(speechEndpoint), resourceKey);

            // Set the mode of input language detection to either "AtStart" (the default) or "Continuous".
            // Please refer to the documentation of Language ID for more information.
            // https://aka.ms/speech/lid?pivots=programming-language-csharp
            config.SetProperty(PropertyId.SpeechServiceConnection_LanguageIdMode, "Continuous");

            //TODO
            // Define the set of input spoken languages to detect
            var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "en-US", ConvertFromLanguage.ShortName });
            PullAudioInputStream pullStream = null;

            //MP3/4 Files
            if ((new[] { ".mp3", ".mp4" }).Contains(System.IO.Path.GetExtension(filePath)))
            {
                pullStream = AudioInputStream.CreatePullStream(new BinaryAudioStreamReader(new BinaryReader(File.OpenRead(filePath))), AudioStreamFormat.GetCompressedFormat(AudioStreamContainerFormat.ANY));

            }


            using (var audioConfig = ((new[] { ".mp3", ".mp4" }).Contains(System.IO.Path.GetExtension(filePath))) ? AudioConfig.FromStreamInput(pullStream) : AudioConfig.FromWavFileInput(filePath))
            {
                using (var recognizer = new SpeechRecognizer(config, audioConfig))
                {
                    try
                    {
                        await StartSpeechProcessing(recognizer).ConfigureAwait(false);
                        if (stopRecognition != null)
                        {
                            stopRecognition.TrySetResult(0);
                        }

                    }
                    catch (Exception ex)
                    {
                        NotifyUser($"Error with processing file.\n {filePath}", NotifyType.ErrorMessage);

                    }
                }
            }
            NotifyUser("Choose File", NotifyType.CommandBarMessage);
            NotifyUser($"Speech Recognition completed for {filePath}", NotifyType.InfoBar);

        }
    }
}
