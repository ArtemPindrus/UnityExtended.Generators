@echo off
for %%f in (*.txt) do (echo. > "%%f")
echo All text files have been cleared.