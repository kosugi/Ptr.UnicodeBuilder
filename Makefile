.PHONY: build clean test deploy

RELEASE := UnicodeBuilder\bin\x64\Release\net9.0-windows10.0.22621.0\publish
PUBLISH := UnicodeBuilder\bin\x64\Publish
DEPLOY  := $(LOCALAPPDATA)\Microsoft\PowerToys\POWERT~1\Plugins\UnicodeBuilder

clean:
	dotnet clean
	@cmd /c rmdir /S /Q UnicodeBuilder\bin
	@cmd /c rmdir /S /Q UnicodeBuilder\obj
	@cmd /c rmdir /S /Q UnicodeBuilder.UnitTests\bin
	@cmd /c rmdir /S /Q UnicodeBuilder.UnitTests\obj

test:
	dotnet test

deploy:
	#dotnet publish -c Release -r win-x64 --no-self-contained
	@dotnet publish -c Release
	@cmd /c 'xcopy /y $(RELEASE)\Images\ $(PUBLISH)\Images\'
	@cmd /c 'xcopy /y $(RELEASE)\UnicodeBuilder.* $(PUBLISH)\'
	@cmd /c 'xcopy /y $(RELEASE)\plugin.json $(PUBLISH)\'
	@cmd /c 'xcopy /y $(RELEASE)\unicode.db $(PUBLISH)\'
	@cmd /c 'xcopy /y $(RELEASE)\Microsoft.Data.Sqlite.dll $(PUBLISH)\'
	@cmd /c taskkill /f /im PowerToys.exe /t
	@cmd /c 'xcopy /s /y $(PUBLISH)\ $(DEPLOY)\'
	@cmd /c start C:\PROGRA~1\PowerToys\PowerToys.exe
