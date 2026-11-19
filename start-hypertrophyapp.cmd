@echo off
rem Double-click to start HypertrophyApp locally: starts Docker Desktop + PostgreSQL, runs the app and opens the browser.
cd /d "%~dp0"

docker info >nul 2>&1
if errorlevel 1 (
    echo Starting Docker Desktop...
    start "" "C:\Program Files\Docker\Docker\Docker Desktop.exe"
)
:waitdocker
docker info >nul 2>&1
if errorlevel 1 (
    timeout /t 3 /nobreak >nul
    goto waitdocker
)

echo Starting database...
docker compose up -d --wait
if errorlevel 1 (
    echo Could not start the database. Is Docker Desktop running?
    pause
    exit /b 1
)

echo.
echo HypertrophyApp is starting. Your browser will open at http://localhost:5243
echo Keep this window open while you use the app. Close it to stop HypertrophyApp.
echo.
dotnet run --project src\HypertrophyApp.Api --launch-profile http
pause
