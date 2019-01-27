setlocal
cd /d %~dp0
call setkey.cmd
cd atgit
dotnet pack
dotnet nuget push bin\Debug\atgit.0.8.3.nupkg -k %NUGET_ORG_KEY% -s https://api.nuget.org/v3/index.json