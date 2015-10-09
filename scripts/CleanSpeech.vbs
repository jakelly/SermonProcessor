Set WshShell = WScript.CreateObject("WScript.Shell")

Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objFolder = objFSO.GetFolder("c:\Users\jkelly\Documents\Church\Radio Ministry\Original\Exported WAV\")
Set objFiles = objFolder.Files

Wscript.Echo "Processing Files in Folder: " & objFolder

For Each objFile In objFiles
	If objFile.Type = "WAV File" Then
		Wscript.Echo "soxeffects.bat ""..\" & objFile.Name & """ ""..\Step 1 - Speech Cleaned\" & objFile.Name & """"
		Result = WshShell.Run ("soxeffects.bat ""..\" & objFile.Name & """ ""..\Step 1 - Speech Cleaned\" & objFile.Name & """", 1, true)
		'Exit For
	End If
Next

