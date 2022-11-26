#ifdef _MSC_VER
 #define _CRT_SECURE_NO_WARNINGS
#endif

#include <stdio.h>

/*
https://forum.xentax.com/viewtopic.php?t=3610
Re: Monster Hunter 3 (wii) demo - bin file
Post by tpu Wed Aug 05, 2009 2:41 am

About itemdata.bin:
just reverse each byte in file, you will find the SJIS text
CODE: SELECT ALL

	for(i=0; i<len; i++){
		fread (&ch, 1, 1, ifp);
		if(ch!=0x00 && ch!=0xff)
			ch = ~ch;
		fwrite(&ch, 1, 1, ofp);
	}
*/

//extreamly slow on large file
//looks like It should be changed to use a buffer.
int main(int argc, char**argv) {
	if (argc < 2) {
		printf("error\n");
		return;
	}
	char* filename = argv[1];
	FILE *ifp = fopen(filename, "rb");
	int slen = strlen(filename);
	char *s2 = malloc(sizeof(char) * slen + 4);
	strcpy(s2, filename);
	strcat(s2, ".out");
	FILE *ofp = fopen(s2, "wb");
	fseek(ifp, 0, SEEK_END);
	int len = ftell(ifp);
	fseek(ifp, 0L, SEEK_SET);
	unsigned char ch;
	for (int i = 0; i < len; i++) {
		fread(&ch, 1, 1, ifp);
		if (ch != 0x00 && ch != 0xff)
			ch = ~ch;
		fwrite(&ch, 1, 1, ofp);
	}
}