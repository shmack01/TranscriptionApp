# TranscriptionApp
WinUI 3  Windows Desktop Application for Speech-to-Text and Translation

**TODO LIST**
- Add feature for saving to a file
- Implement parsing MP3/4 files - **COMPLETED AND TESTED v1.22.6**
- Current parsing audio files status
- Add code for large wav files
- Modify Code to use Key Vault
- [Optional] Develop with Transcription Batch if needed. 

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
- **SPEECH_SERVICE_REGION** - (e.g. usgovvirginia)
- **SPEECH_TEXT_RESOURCE_KEY**
- **SPEECH_ENDPOINT** - (e.g. wss://usgovvirginia.stt.speech.azure.us/speech/universal/v2)

```
[System.Environment]::SetEnvironmentVariable('TRANSLATOR_SERVICE_REGION','usgovvirginia',[System.EnvironmentVariableTarget]::Machine)
[System.Environment]::SetEnvironmentVariable('TRANSLATOR_TEXT_RESOURCE_KEY','****',[System.EnvironmentVariableTarget]::Machine)
[System.Environment]::SetEnvironmentVariable('TRANSLATOR_ENDPOINT','https://<name of service>.cognitiveservices.azure.us/',[System.EnvironmentVariableTarget]::Machine)
[System.Environment]::SetEnvironmentVariable('SPEECH_SERVICE_REGION','usgovvirginia',[System.EnvironmentVariableTarget]::Machine)
[System.Environment]::SetEnvironmentVariable('SPEECH_TEXT_RESOURCE_KEY','*****',[System.EnvironmentVariableTarget]::Machine)
[System.Environment]::SetEnvironmentVariable('SPEECH_ENDPOINT','wss://usgovvirginia.stt.speech.azure.us/speech/universal/v2',[System.EnvironmentVariableTarget]::Machine)

[System.Environment]::SetEnvironmentVariable('GST_PLUGIN_PATH','C:\gstreamer\1.0\msvc_x86_64\lib\gstreamer-1.0',[System.EnvironmentVariableTarget]::Machine)
[System.Environment]::SetEnvironmentVariable('GSTREAMER_ROOT_X86_64','C:\gstreamer\1.0\msvc_x86_64',[System.EnvironmentVariableTarget]::Machine)

# Then Add Path variable

```

## Deployment
The following software components need to be installed before running the application. The components are listed in order it was tested. 
- [Prerequisites](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/deploy-unpackaged-apps#prerequisites)
    - .Net 6 Runtime
    - Microsoft Visual C++ Redistributable
    - Windows App SDK runtime

## Additional Information
Additional information to help with development and understanding the code. 
### Development
For self containment during development, ensure the following lines are added to the **PropertyGroup** in the csproj file and then publish to folder. This will deploy all the Windows App SDK and .NET dlls with the the application. 
```
<SelfContained>true</SelfContained>
<WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
<WindowsPackageType>None</WindowsPackageType>
```

  ![Picture of Application](/TranscriptionApp/Assets/app.jpg)
