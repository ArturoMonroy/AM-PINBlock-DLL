using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;
using System.Windows.Forms;


namespace PinBlockDLL
{
    public class Class1
    {
    //var s = Encode("04216BBFFEFCC9DF", "AAAABBBBCCCCDDDDEEEEFFFFAAAABBBB");


        //Solo para probar
        [DllExport("add", CallingConvention = CallingConvention.Cdecl)]
        public static int add(int left, int right)
        {
            return left + right;
        }

        [DllExport("adds", CallingConvention = CallingConvention.Cdecl)]
        public static int adds(IntPtr a, IntPtr b, out IntPtr result)
        {
            string concatenado = Marshal.PtrToStringAuto(a) + Marshal.PtrToStringAuto(b);

            result = Marshal.AllocHGlobal(concatenado.Length);

            Marshal.Copy(concatenado.ToCharArray(), 0, result, concatenado.Length);
                        
            return concatenado.Length;           
            
        }

        [DllExport("PINBlock", CallingConvention = CallingConvention.Cdecl)]
        public static int PINBlock(IntPtr PIN_P, IntPtr PAN_P, IntPtr llave_P, out IntPtr PINBlock_P)
        {

            int result = -1;
            string resultPINBlock;
            PINBlock_P = Marshal.AllocHGlobal(1024);
            try
            {
                string XOR_PIN_PAN = "";
                string PIN = Marshal.PtrToStringAuto(PIN_P);
                string PAN = Marshal.PtrToStringAuto(PAN_P);
                string llave = Marshal.PtrToStringAuto(llave_P);

                XOR_FORMAT_01(PIN, PAN, out XOR_PIN_PAN);
                    
                XOR_PIN_PAN = XOR_PIN_PAN.Replace(":", "");

                //1234 4766840000704997 AAAABBBBCCCCDDDDEEEEFFFFAAAABBBB -> 501E9A300E5046B4
                resultPINBlock = getPINBlock(XOR_PIN_PAN, llave);

                result = resultPINBlock.Length;
                
            }
            catch (Exception e)
            {
                resultPINBlock = e.Message;            
            }
            
            Marshal.Copy(resultPINBlock.ToCharArray(), 0, PINBlock_P, resultPINBlock.Length);                

            return result;
            
        }

        public static string getPINBlock(string XOR_PIN_PAN, string key)
        {

            var toEncryptArray = StringToByteArray(XOR_PIN_PAN); //UTF8Encoding.UTF8.GetBytes(input);


            var tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = StringToByteArray(key); //keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.None;
            ICryptoTransform transformation = tdes.CreateDecryptor();


            var cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return BitConverter.ToString(resultArray).Replace("-", "");
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private static string FromBase64ToHEX(string base64)
        {
            char[] c = new char[base64.Length * 2];
            byte b;
            for (int i = 0; i < base64.Length; ++i)
            {
                b = ((byte)(base64[i] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                b = ((byte)(base64[i] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }
            return new string(c);
        }

        private static void XOR_FORMAT_01(string PIN, string PAN, out string XOR)
        {
            /*
            Ejemplo de la documentacion thales, la tarjeta  puede ser inferior a 16 caracteres

            PIN   PAN
            92389 4000001234562

            0592 389F FFFF FFFF
            0000 4000 0012 3456
            -------------------
            05 92 78 9F FF ED CB A9 
            */
            int[] aArray = new int[8];
            string aPIN = "";
            string aPAN = "";

            XOR = "";

            aPIN = PIN.Length.ToString();

            if (aPIN.Length == 1)
                aPIN = "0" + aPIN;

            aPIN = aPIN + PIN;

            while (aPIN.Length < 16)
                aPIN = aPIN + "F";

            aPAN = getPAN12(PAN);

            while (aPAN.Length < 16)
                aPAN = "0" + aPAN;


            aArray[0] = Convert.ToByte(aPIN.Substring(0, 2), 16) ^ Convert.ToByte(aPAN.Substring(0, 2), 16);
            aArray[1] = Convert.ToByte(aPIN.Substring(2, 2), 16) ^ Convert.ToByte(aPAN.Substring(2, 2), 16);
            aArray[2] = Convert.ToByte(aPIN.Substring(4, 2), 16) ^ Convert.ToByte(aPAN.Substring(4, 2), 16);
            aArray[3] = Convert.ToByte(aPIN.Substring(6, 2), 16) ^ Convert.ToByte(aPAN.Substring(6, 2), 16);
            aArray[4] = Convert.ToByte(aPIN.Substring(8, 2), 16) ^ Convert.ToByte(aPAN.Substring(8, 2), 16);
            aArray[5] = Convert.ToByte(aPIN.Substring(10, 2), 16) ^ Convert.ToByte(aPAN.Substring(10, 2), 16);
            aArray[6] = Convert.ToByte(aPIN.Substring(12, 2), 16) ^ Convert.ToByte(aPAN.Substring(12, 2), 16);
            aArray[7] = Convert.ToByte(aPIN.Substring(14, 2), 16) ^ Convert.ToByte(aPAN.Substring(14, 2), 16);


            for (int i = 0; i < 8; i++)
                XOR += ":" + (aArray[i] < 10 ? "0" : "") + aArray[i].ToString("X");

            if (XOR.StartsWith(":"))
                XOR = XOR.Substring(1);
                            
        }

        private static string getPAN12(string PAN)
        {

            string ZERO_PAD = "0000000000000000";
            string aPAN;
            int x;
            int i;
            string result = "";
            /*
            //Ejemplo
            //xxx			       x
            //4766840010336205 -> 684001033620

            Ejemplo de la documentacion thales, la tarjeta  puede ser inferior a 16 caracteres

            PIN   PAN
            92389 4000001234562

            0592 389F FFFF FFFF
            0000 4000 0012 3456
            -------------------
            05 92 78 9F FF ED CB A9
            */

            aPAN = ZERO_PAD + PAN;
            x = aPAN.Length;
            i = x - 13;
            result = aPAN.Substring(i);
            result = result.Substring(0, result.Length - 1);

            return result;
        }


        
    }
}
