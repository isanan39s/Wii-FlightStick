@echo "--------- 環境変数の設定 ----------"
rem set environment for VC++ 2008 (VC9.0)
call "C:\Program Files\Microsoft Visual Studio 9.0\VC\vcvarsall.bat" x86

@echo "--------- ソースをコンパイルします ----------"
cl wii_motionplus_reader01.cpp user32.lib tiny_hid_dll.lib
del *.obj
pause
