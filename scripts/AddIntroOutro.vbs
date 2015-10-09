Const ServiceExecutable="C:\Users\jkelly\Documents\Church\SermonProcessor\SermonProcessor\bin\Debug\SermonProcessor.exe AddIntroOutro "
Const SermonFolder="C:\Users\jkelly\Documents\Church\Radio Ministry\Original\Exported WAV\Step 2 - Trimmed"
Const IntroFile="C:\Users\jkelly\Documents\Church\Radio Ministry\Original\Exported WAV\Media\Intro 2015.wav"
Const OutroFile="C:\Users\jkelly\Documents\Church\Radio Ministry\Original\Exported WAV\Media\Outro 2015.wav"
Const SermonStartTime = 31
Const OutroStartTime = 22
Const ResultsFolder="C:\Users\jkelly\Documents\Church\Radio Ministry\Original\Exported WAV\Step 3 - IntroOutroAdded"

Set WshShell = WScript.CreateObject("WScript.Shell")

Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objFolder = objFSO.GetFolder(SermonFolder)
Set objFiles = objFolder.Files

Wscript.Echo "Processing Files in Folder: " & objFolder

For Each objFile In objFiles
    'Wscript.Echo objFile.Type

	If objFile.Type = "Wave Sound" Then

'        Wscript.Echo ServiceExecutable & " """ & _
'           objFile.Path & """ """ & _
'           IntroFile & """ """ & _
'           OutroFile & """ " & _
'           SermonStartTime & " " & _
'           OutroStartTime & " """ & _
'           ResultsFolder & """"

		Result = WshShell.Run (ServiceExecutable & " """ & _
           objFile.Path & """ """ & _
           IntroFile & """ """ & _
           OutroFile & """ " & _
           SermonStartTime & " " & _
           OutroStartTime & " """ & _
           ResultsFolder & """", 1, true)
	End If
Next
