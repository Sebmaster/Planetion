#!/bin/sh

mkbundle --deps -c -o temp.c -oo temp.o Game.exe Planetion OpenTK
sed -e '/#include <windows.h>/ {
	n
	a #undef _WIN32
}' -e 'N;$!P;$!D;$d' temp.c > temp.c
gcc -g -o Deploy/GameBundle.exe -Wall temp.c `pkg-config --cflags --libs mono-2|dos2unix`  temp.o
rm temp.*