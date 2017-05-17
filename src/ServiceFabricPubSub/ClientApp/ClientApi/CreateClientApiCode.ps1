$autorestExe= "C:\Users\gianlucb\AppData\Roaming\npm\autorest.cmd";
$swaggerFile = "swagger_api.json";
$outputCSFile =  "ClientApi.cs";

If (Test-Path $outputCSFile){
	Remove-Item $outputCSFile
}

If (Test-Path $swaggerFile){
	Remove-Item $swaggerFile
}

Invoke-WebRequest -Uri http://localhost:8979/swagger/docs/v3 -OutFile $swaggerFile;
Start-Process -FilePath $autorestExe -NoNewWindow -ArgumentList "--input-file=$swaggerFile --csharp --output-folder=. --namespace=ClientApi" -RedirectStandardOutput log.txt -RedirectStandardError logError.txt
