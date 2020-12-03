using System;
using System.Collections.Generic;
using System.Text;

namespace Lab1
{
    public static class Base26System
    {
        private const int letterCount = 26;
        public static string Convert(int i)
        {
            int k = 0;
            int[] arr = new int[100];
            while (i > letterCount-1)
            {
                arr[k] = i / letterCount - 1;
                k++;
                i = i % letterCount;
            }
            arr[k] = i;

            string res = "";
            for(int j =0; j<=k; j++)
            {
                res = res + ((char)('A' + arr[j])).ToString();
            }
            return res;
        }      
    }
}
