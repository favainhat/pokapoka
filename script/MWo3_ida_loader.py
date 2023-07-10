'''
https://gtamods.com/wiki/PS2_Code_Overlay
4b - fourcc (MWo3)
4b - unknown (number of segments?)
4b - load address/entry point?
4b - size of text segment
4b - size of data segment
4b - size of bss segment
4b - start address of overlay load callbacks
4b - end address of overlay load callbacks
32b - file name


Overlay load callbacks
The start and end address of overlay load callbacks forms an array of 32bit pointers. The addresses have to be aligned to 4 byte boundaries and have to either point inside the game executable or the code overlay. The addresses are absolute memory offsets, not relative to the code overlay base address. The end address delimits the array and thus it does not point to a valid routine address.
 numfuncs = ( end-addr - start-addr ) / 4

'''

import struct
import idaapi
import ida_idp
import ida_bytes
import ida_segregs
from idc import *

MWo3_HEADER_MAGIC 		= b"MWo3"
MWo3_HEADER_SIZE 		= 64

def accept_file(f, filename):
	f.seek(0)
	magic = f.read(4)
	if magic == MWo3_HEADER_MAGIC:
		return "MWo3"
	return 0

def load_file(f, neflags, format):
	f.seek(0)
	
	magic 		 	= f.read(4);
	unk 		 	= struct.unpack("<I", f.read(4))[0]
	entrypoint 		 	= struct.unpack("<I", f.read(4))[0]
	textLen 	 	= struct.unpack("<I", f.read(4))[0]
	dataLen 	 	= struct.unpack("<I", f.read(4))[0]
	bssLen 	 	 	= struct.unpack("<I", f.read(4))[0]
	start_callbacks  	 	= struct.unpack("<I", f.read(4))[0]
	end_callbacks  	 	= struct.unpack("<I", f.read(4))[0]
	file_name  	 	= f.read(32);
	
	idaapi.set_processor_type("mipsl", ida_idp.SETPROC_LOADER)

	# Set VA for .text and add the segment
	textVA = entrypoint
	dataVA = entrypoint + textLen + MWo3_HEADER_SIZE
	bssVA = entrypoint + textLen + dataLen + MWo3_HEADER_SIZE
	f.file2base(0, textVA, textVA + textLen, True)
	idaapi.add_segm(0, textVA, textVA + textLen, ".text", "CODE")

	# Set VA for .data and add the segment
	f.file2base(textLen, dataVA, dataVA + dataLen, True)
	idaapi.add_segm(0, dataVA, dataVA + dataLen, ".data", "DATA")
	
	# Add BSS segment after .text and .data
	idaapi.add_segm(0, bssVA, bssVA + bssLen, ".bss", "BSS")

	return 1
