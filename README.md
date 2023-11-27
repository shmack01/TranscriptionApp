# TranscriptionApp
WinUI 3  Windows Desktop Application for Speech-to-Text and Translation

**TODO LIST**
- Add feature for saving to a file
- Implement parsing MP3/4 files - **COMPLETED** Tested with v1.22.6
- Current parsing audio files status
- Test with large wav files, incorporate batch.

## Get Started
Solution uses Translator and Speech services. The current solution pulls values from Environment Variables. This will need to be updated for more secure method

### Requirements 
- [Windows App SDK 1.4.231008000](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/system-requirements#windows-app-sdk)
- [WinUI 3 for Visual Studio](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/system-requirements#visual-studio-support-for-winui-3-tools)
- Current solution will ingest .wav audio files - 16 kHz or 8 kHz, 16-bit, and mono PCM .
    - For MP4 files, dependencies need to be installed. [For more information](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/how-to-use-codec-compressed-audio-input-streams?tabs=windows%2Cdebian%2Cjava-android%2Cterminal&pivots=programming-language-csharp)
 
#### Environment Variables
Environment variables need to be configured for connection to Translator and Speech services. 
**Translator**
- **TRANSLATOR_SERVICE_REGION**
- **TRANSLATOR_TEXT_RESOURCE_KEY**
- **TRANSLATOR_ENDPOINT** - with trailing slash, exactly like what is in the portal. e.g. https://{name}.cognitiveservices.azure.us/

**Speech**
- **SPEECH_SERVICE_REGION**
- **SPEECH_TEXT_RESOURCE_KEY**
- **SPEECH_ENDPOINT**



## Deployment
Ensure the following lines are added to the **PropertyGroup** in the csproj file and then publish to folder. This will deploy all the Windows App SDK and .NET dlls with the the application. 

- [Prerequisites](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/deploy-unpackaged-apps#prerequisites)
    - .net 6 runtime
    - Windows App SDK runtime
    - Microsoft Visual C++ Redistributable
```
<SelfContained>true</SelfContained>
<WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
<WindowsPackageType>None</WindowsPackageType>
```

  ![Picture of Application](/TranscriptionApp/Assets/app.jpg)
