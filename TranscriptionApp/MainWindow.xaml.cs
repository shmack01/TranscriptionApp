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
 * Our suppliers from and against any claims or lawsuits, including attorneys� fees, 
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
using System.Threading.Tasks;
using System.Net;
using TranscriptionApp.Services;
using Newtonsoft.Json;
using Windows.Globalization;
using System.Net.Http;
using System.Collections.ObjectModel;
using Windows.Security.Cryptography.Core;


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
        private static string endpoint_var = "TRANSLATOR_ENDPOINT";
        private const string region_var = "SPEECH_SERVICE_REGION";
        private const string key_var = "SPEECH_TEXT_RESOURCE_KEY";
        private static readonly string endpoint = Environment.GetEnvironmentVariable(endpoint_var);
        private static readonly string region = Environment.GetEnvironmentVariable(region_var);
        private static readonly string resourceKey = Environment.GetEnvironmentVariable(key_var);


        public MainWindow()
        {
            this.InitializeComponent();
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
        }

        public ObservableCollection<Services.Language> Languages { get; set; }
        public static Services.Language ConvertFromLanguage { get; set; }
        public static Services.Language ConvertToLanguage { get; set; }
        private TaskCompletionSource<int> stopRecognition;
        public bool SpeechEnabled { get; set; }
        private async void EnableMicrophone_ButtonClicked(object sender, RoutedEventArgs e)
        {
            bool isMicAvailable = true;
            try
            {
                var mediaCapture = new Windows.Media.Capture.MediaCapture();
                var settings = new Windows.Media.Capture.MediaCaptureInitializationSettings();
                settings.StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.Audio;
                await mediaCapture.InitializeAsync(settings);
            }
            catch (Exception)
            {
                isMicAvailable = false;
            }
            if (!isMicAvailable)
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-microphone"));
            }
            else
            {
                NotifyUser("Microphone was enabled", NotifyType.StatusMessage);
            }
        }
        private async void StopSpeechRecognitionFromMicrophone_ButtonClicked(object sender, RoutedEventArgs e)
        {
            StopSpeechRecognitionButton.IsEnabled = false;
            SpeechRecognitionButton.IsEnabled = true;
            if (stopRecognition != null)
            {
                stopRecognition.TrySetResult(0);

            }

        }

        private async void SpeechRecognitionFromMicrophone_ButtonClicked(object sender, RoutedEventArgs e)
        {
            // </skeleton_1>
            // <create_speech_configuration>
            // Creates an instance of a speech config with specified subscription key and service region.
            // Replace with your own subscription key and service region (e.g., "westus").
            var config = SpeechConfig.FromSubscription(resourceKey, region);
            //TODO: Work out issues with binding to a property
            SpeechRecognitionButton.IsEnabled = false;
            StopSpeechRecognitionButton.IsEnabled = true;

            // </create_speech_configuration>

            // <skeleton_2>
            try
            {
                // </skeleton_2>
                // <create_speech_recognizer_1>
                // Creates a speech recognizer using microphone as audio input.
                using (var recognizer = new SpeechRecognizer(config))
                {
                    StringBuilder sb = new StringBuilder();
                    // The TaskCompletionSource to stop recognition.
                    stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
                    // Subscribes to events.
                    recognizer.Recognizing += (s, e) =>
                    {
                        // Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
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
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
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
                }
                // </create_speech_recognizer_2>
                // <skeleton_3>
            }
            catch (Exception ex)
            {
                NotifyUser($"Enable Microphone First.\n {ex.ToString()}", NotifyType.ErrorMessage);

            }
        }

        private enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };

        private void NotifyUser(string strMessage, NotifyType type)
        {
            // If called from the UI thread, then update immediately.
            // Otherwise, schedule a task on the UI thread to perform the update
            //var Dispatcher = Microsoft.UI.Xaml.Window.DispatcherQueue;// Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

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
                    //StatusBorder.Background = new SolidColorBrush(Microsoft.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    StatusBorder.Background = new SolidColorBrush(Microsoft.UI.Colors.Red);
                    break;
            }
            StatusBlock.Text += string.IsNullOrEmpty(StatusBlock.Text) ? strMessage : "\n" + strMessage;

            StatusScroller.ScrollToVerticalOffset(StatusScroller.ExtentHeight);
            // Collapse the StatusBlock if it has no text to conserve real estate.
            StatusBorder.Visibility = !string.IsNullOrEmpty(StatusBlock.Text) ? Visibility.Visible : Visibility.Collapsed;
            if (!string.IsNullOrEmpty(StatusBlock.Text))
            {
                StatusBorder.Visibility = Visibility.Visible;
                StatusPanel.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBorder.Visibility = Visibility.Collapsed;
                StatusPanel.Visibility = Visibility.Collapsed;
            }
            //Translate
            if (!String.IsNullOrEmpty(strMessage))
            {
                string translatedTxt = await TranslateAsync(strMessage);
                TranslateBlock.Text += string.IsNullOrEmpty(TranslateBlock.Text) ? translatedTxt :  translatedTxt;
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
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(endpoint);

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                // Read response as a string.
                string result = await response.Content.ReadAsStringAsync();
                Services.LanguageResult deserializedOutput = JsonConvert.DeserializeObject<LanguageResult>(result);

                // Iterate over the deserialized results.
                foreach (var pair in deserializedOutput.Translation)
                {
                    list.Add(new Services.Language { Name = pair.Value.Name, ShortName = pair.Key });
                }
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
    }
}