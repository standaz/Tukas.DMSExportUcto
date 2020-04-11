using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DMSExportUcto.Security
{
    internal static class Cipher
    {
		private const string CipherKey = "56df645g46sHg4YUD4lj";
			
        internal static string CipherPassword(string password)
        {
            int passmod = 0, j = 1;
            do
            {
                passmod = ('g' + password.Length / j++) % 122;
            } while (passmod < 97);

            char uZnak = (passmod < 97) ? 'a' : (char)passmod;
            StringBuilder cipherPassword = new StringBuilder();

            int iPasswordChar;
            for (int i = 0; i < password.Length; i++)
            {
                iPasswordChar = password[i] ^ CipherKey[(i + 1) % CipherKey.Length + 1];
                cipherPassword.Append(iPasswordChar.ToString("X"));
            }

            iPasswordChar = 0;
            string passwordCoded = cipherPassword.ToString();
            cipherPassword.Remove(0, cipherPassword.Length);
            for (int i = 0; i < passwordCoded.Length - 1; i += 2)
            {
                try
                {
                    iPasswordChar = Int32.Parse(passwordCoded.Substring(i, 2));

                    if ((iPasswordChar >= 65 && iPasswordChar <= 90) || (iPasswordChar >= 97 && iPasswordChar <= 122))
                        cipherPassword.Append(Convert.ToChar(iPasswordChar));
                    else
                        AppendChars(cipherPassword, passwordCoded, i);
                }
                catch
                {
                    AppendChars(cipherPassword, passwordCoded, i);
                }
            }
            return cipherPassword.ToString();
        }

        private static void AppendChars(StringBuilder result, string cipherPasswordStr, int i)
        {
            result.Append((cipherPasswordStr.Length > i + 2) ? cipherPasswordStr.Substring(i, 2) :
                cipherPasswordStr.Substring(i, cipherPasswordStr.Length - i));
        }

        internal static string GetLicenceKey()
        {
            string str3 = string.Format("eHGfd{0}XFgbd", Dns.GetHostName().ToUpper());
            return string.Format("{0:X}", (object)str3.GetHashCode());
        }
    }
}
