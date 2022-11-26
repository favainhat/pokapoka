using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace param_tool
{

    class Program
    {
        public static uint swapEndianness(uint x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);
            uint result = BitConverter.ToUInt32(bytes, 0);
            return result;
        }

        public static byte[] ReadString(FileStream fs)
        {
            var ms = new MemoryStream();
            byte[] buffer = new byte[1024];

            for (int i = 0; fs.Position < fs.Length; i++)
            {
                byte[] buff = new byte[1];
                fs.Read(buff, 0, 1);
                if (buff[0] == '\0')
                {
                    break;
                }
                ms.Write(buff, 0, 1);
            }
            return ms.ToArray();
        }

        public static byte[] ReadString_ver2(FileStream fs)
        {
            var ms = new MemoryStream();
            byte[] buffer = new byte[1024];

            for (int i = 0; fs.Position < fs.Length; i++)
            {
                byte[] buff = new byte[1];
                byte[] buff2 = new byte[1];
                fs.Read(buff, 0, 1);
                if (buff[0] == '\n')
                {
                    break;
                }
                if (buff[0] == '\r')
                {
                    fs.Read(buff2, 0, 1);
                    if (buff2[0] == '\n')
                    {
                        break;
                    }
                    else
                    {
                        fs.Seek(-1, SeekOrigin.Current);
                    }
                }
                ms.Write(buff, 0, 1);
            }
            return ms.ToArray();
        }

        public static byte[] ReadString_ver3(FileStream fs)
        {
            var ms = new MemoryStream();
            byte[] buffer = new byte[1024];

            for (int i = 0; fs.Position < fs.Length; i++)
            {
                byte[] buff = new byte[2];
                fs.Read(buff, 0, 2);
                
                //var temp_str = Encoding.Unicode.GetString(buff);
                //var temp = temp_str[0];
                
                //var temp = BitConverter.ToChar(buff, 0);
                //if (temp == '\0')
                //{
                //    break;
                //}

                if (buff[0] == '\0'  && buff[1] == '\0')
                {
                    break;
                }
                ms.Write(buff, 0, 2);
            }
            return ms.ToArray();
        }
/*
        public static byte[] ReadString_ver3_1(FileStream fs)
        {
            var ms = new MemoryStream();
            byte[] buffer = new byte[1024];

            for (int i = 0; fs.Position < fs.Length; i++)
            {
                byte[] buff = new byte[1];
                fs.Read(buff, 0, 1);
                if (buff[0] == '\0')
                {
                    fs.Read(buff, 0, 1);
                    if (buff[0] == '\0')
                        break;
                    else
                        fs.Seek(-1, SeekOrigin.Current);

                }
                ms.Write(buff, 0, 1);
            }
            return ms.ToArray();
        }
*/
        private static void checkUnicodeBOM(FileStream fs)
        {
            byte[] buff = new byte[3];
            fs.Read(buff, 0, 3);
            if ((buff[0] != 0xef) || (buff[1] != 0xbb) || (buff[2] != 0xbf))
            {
                fs.Seek(0, SeekOrigin.Begin);
            }
        }


        private static int calculateTotalSize_UTF8(List<String> st, int align)
        {
            int total = 0;
            foreach (String str in st)
            {
                int len = Encoding.GetEncoding("UTF-8").GetBytes(str).Length;
                if (str.Length > 0)
                    total += len + 1;
            }
            return total;
        }

        private static int calculateTotalSize_SJIS(List<String> st, int align)
        {
            int total = 0;
            foreach (String str in st)
            {
                int len = Encoding.GetEncoding("Shift-jis").GetBytes(str).Length;
                if (str.Length > 0)
                    total += len + 1;
            }
            return total;
        }

        private static int calculateTotalSizeUTF16LE(List<String> st, int align)
        {
            int total = 0;
            foreach (String str in st)
            {
                int len = Encoding.Unicode.GetBytes(str).Length;
                if (str.Length > 0)
                    total += len + 1;
            }
            return total;
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("fmg_tool.exe (-e/-r) filename.fmg");
                return;
            }
            string option = args[0];
            string fname = args[1];
            if (args[0] == "-e")
            {
                extract(fname);
            }
            else if (args[0] == "-r")
            {
                rebuild(fname);

            }
            else
            {
                Console.WriteLine("fmg_tool.exe (-e/-r) filename.fmg");
                return;
            }

        }
        static void extract(string fname)
        {
            String Filename = fname;

            FileStream fs = new FileStream(Filename, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);

            fs.Seek(0x08, SeekOrigin.Begin);
            byte encode = br.ReadByte();
            if(encode == 0)
            {
                Console.WriteLine("sjis");
            }
            else if (encode == 1)
            {
                Console.WriteLine("utf16le");
            }
            else {
                Console.WriteLine("Error unknown encode type: " + encode);
                return;
            }
            fs.Seek(0x10, SeekOrigin.Begin);
            UInt32 str_count = br.ReadUInt32();
            UInt32 table_start = br.ReadUInt32();


            fs.Seek(table_start, SeekOrigin.Begin);

            String paramStr = "";

            for (UInt32 i = 0; i < str_count; i++)
            {
                fs.Seek(table_start+ 4*i, SeekOrigin.Begin);
                UInt32 str_start = br.ReadUInt32();
                string str = "";
                if (str_start != 0)
                {
                    fs.Seek(str_start, SeekOrigin.Begin);
                    if (encode == 0) {
                        byte[] temp = ReadString(fs);
                        str = Encoding.GetEncoding("Shift-jis").GetString(temp);
                    } else {
                        byte[] temp = ReadString_ver3(fs);
                        str = Encoding.Unicode.GetString(temp);
                    }
                    //Console.WriteLine(str);
                }

                if (str.Length < 1)
                {
                    //paramStr += "<EMPTY STRING>";
                }
                else
                {
                    //str = str.Replace("\r\r\n", "<NEWLINE>");  //0d 0d 0a...
                    //paramStr += str.Replace("\r\n", "<NEWLINE>");
                    paramStr += str.Replace("\n", "<NEWLINE>");
                    paramStr += "\r\n";
                }
                //paramStr += "\r\n";
            }


            fs.Close();

            String no_ext_Filename = Filename.Remove(Filename.LastIndexOf("."));
            FileStream fw = File.Create(no_ext_Filename + ".txt");
            byte[] encodedText = Encoding.UTF8.GetBytes(paramStr);
            fw.Write(encodedText, 0, encodedText.Length);
            fw.Close();
        }

        public static void CopyStream(Stream input, Stream output, int bytes)
        {
            byte[] buffer = new byte[32768];
            int read;
            while (bytes > 0 &&
                   (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }

        static void rebuild(string fname)
        {

            String Filename = fname;
            String no_ext_Filename = Filename.Remove(Filename.LastIndexOf("."));
            String Filename1 = no_ext_Filename + ".txt";

            FileStream f1 = new FileStream(Filename, FileMode.Open, FileAccess.Read);

            FileStream fs1 = new FileStream(Filename1, FileMode.Open, FileAccess.Read);
            FileStream fs2 = new FileStream(no_ext_Filename + ".out", FileMode.Create, FileAccess.ReadWrite);


            List<String> param_strs = new List<String> { };
            checkUnicodeBOM(fs1);
            for (;;)
            {
                byte[] temp = ReadString_ver2(fs1);
                string str = Encoding.UTF8.GetString(temp);
                if (str == "")
                {
                    break;
                }
                str = str.Replace("<NEWLINE>", "\n");
                //str = str.Replace("<EMPTY STRING>", "");
                param_strs.Add(str);
            }
            BinaryReader br1 = new BinaryReader(f1);
            BinaryReader br2 = new BinaryReader(fs2);
            BinaryWriter bw2 = new BinaryWriter(fs2);


            f1.Seek(0x08, SeekOrigin.Begin);
            byte encode = br1.ReadByte();
            if (encode == 0)
            {
                Console.WriteLine("sjis");
            }
            else if (encode == 1)
            {
                Console.WriteLine("utf16le");
            }
            else
            {
                Console.WriteLine("Error unknown encode type: " + encode);
                return;
            }
            f1.Seek(0x10, SeekOrigin.Begin);
            UInt32 str_count = br1.ReadUInt32();
            UInt32 table_start = br1.ReadUInt32();
            

            f1.Seek(table_start, SeekOrigin.Begin);
            //UInt32 tempz = br1.ReadUInt32();
            UInt32 tempz = 0;
            for (int i = 0; tempz == 0 && i < str_count; i++)
            {
                tempz = br1.ReadUInt32();
            }

            f1.Seek(0, SeekOrigin.Begin);
            fs2.Seek(0, SeekOrigin.Begin);
            CopyStream(f1, fs2, (int)tempz);
            f1.Close();


            fs2.Seek(table_start, SeekOrigin.Begin);

            int cnt = 0;
            int cnt2 = 0;
            UInt32 tempk = tempz; // new string start
            for (UInt32 i = 0; i < str_count; i++)
            {
                fs2.Seek(table_start + 4 * i, SeekOrigin.Begin);
                UInt32 str_start = br2.ReadUInt32();
                string str = "";
                if (str_start != 0)
                {
                    fs2.Seek(str_start, SeekOrigin.Begin);
                    String temp = param_strs[cnt2];
                    byte[] bytes;
                    /*
                    if (encode == 0)
                    {
                        bytes = Encoding.GetEncoding("Shift-jis").GetBytes(temp);
                        fs2.Write(bytes, 0, bytes.Length);
                        fs2.WriteByte(0);
                    }
                    else
                    {
                        bytes = Encoding.Unicode.GetBytes(temp);
                        fs2.Write(bytes, 0, bytes.Length);
                        fs2.WriteByte(0);
                        fs2.WriteByte(0);
                    }
                    */
                    //force UTF16
                    ///*
                    bytes = Encoding.Unicode.GetBytes(temp);
                    fs2.Write(bytes, 0, bytes.Length);
                    fs2.WriteByte(0);
                    fs2.WriteByte(0);
                    //*/
                    UInt32 current_offset = (UInt32)fs2.Position;
                    fs2.Seek(table_start + 4 * i, SeekOrigin.Begin);
                    bw2.Write(tempk); //pointer update
                    tempk = current_offset;
                    cnt2++;
                }
                else
                {
                    //do nothing
                }
                cnt++;
            }
            fs2.Seek(0, SeekOrigin.End);
            UInt32 newsize = (UInt32)fs2.Position;
            fs2.Seek(0x04, SeekOrigin.End);
            bw2.Write(newsize); //size update
            bw2.Write((byte)1);//force UTF16
            fs1.Close();
            fs2.Close();




        }
    }
}