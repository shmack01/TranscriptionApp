# TranscriptionApp
WinUI 3  Windows Desktop Application for Speech-to-Text and Translation

## Get Started
Solution uses Translator and Speech services. The current solution pulls values from Environment Variables. This will need to be updated for more secure method

### Requirements 
- [Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/system-requirements#windows-app-sdk)
- [WinUI 3 for Visual Studio](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/system-requirements#visual-studio-support-for-winui-3-tools)
- [Currently in development for mp3 and mp4 files]: Current solution will ingest .wav audio files - 16 kHz or 8 kHz, 16-bit, and mono PCM .
    - For MP4 files, dependencies need to be installed. [For more information](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/how-to-use-codec-compressed-audio-input-streams?tabs=windows%2Cdebian%2Cjava-android%2Cterminal&pivots=programming-language-csharp)
### Translator
- **TRANSLATOR_SERVICE_REGION**
- **TRANSLATOR_TEXT_RESOURCE_KEY**
- **TRANSLATOR_ENDPOINT** - with trailing slash, exactly like what is in the portal. 

### Speech
- **SPEECH_SERVICE_REGION**
- **SPEECH_TEXT_RESOURCE_KEY**

## Deployment
Add the following lines to the **PropertyGroup** in the csproj file

```
	<WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
	<WindowsPackageType>None</WindowsPackageType>
```

  ![Picture of Application](/TranscriptionApp/Assets/app.jpg)
