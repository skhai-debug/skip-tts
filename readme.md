# StreamElements TTS Skipper
A standalone program that skips StreamElements TTS donations. This program can be run using something like [AutoHotKey](https://www.autohotkey.com/) to create a custom skip tts hotkey. You can look in the Sample AHK files supplied in the [samples](./samples). One uses a [token file](./samples/SampleTokenFile.ahk) while the other uses a [command line argument](./samples/SampleArg.ahk).

## Setup
Download the .exe from the releases page or build yourself.

You may have to install [.NET 5 runtime](https://dotnet.microsoft.com/download/dotnet/5.0/runtime).

Go to your streamelements [user page](https://streamelements.com/dashboard/account/channels) and get your overlay token.

![Overlay Token](https://i.imgur.com/1CDI9Tf.png)

Copy and paste your overlay token into a file named `token.txt` in the same folder as the program. Or supply the token via command line argument.

`./SkipTTS.exe [your overlay key here]`

![Token file](https://i.imgur.com/mUkQZfJ.png)

Run the program and viola, annoying TTS skipped.
