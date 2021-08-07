using System;
using System.Collections.Generic;

namespace mylib.stringworker
{
    public static class MyString
    {
        public static string takeLeft(ref string s, int len) //возвращает len символов слева
        {
            if (len <= 0) return string.Empty;
            if (s.Length < len) return string.Empty;
            return s.Substring(0, len);
        }
        public static string takeRight(ref string s, int len) //возвращает len символов справа
        {
            if (len <= 0) return string.Empty;
            if (s.Length < len) return string.Empty;
            return s.Substring(s.Length-len, len);
        }
        public static string takeMid(ref string s, int start_index, int len = 1) //возвращает len символов, начиная с start_index
        {
            if (start_index < 0 || start_index >= s.Length) return string.Empty;
            string res = removeLeft(ref s, start_index);
            res = takeLeft(ref res, len);
            return res;
        }
        public static string removeLeft(ref string s, int len) //возвращает строку, в которой удалены len символов слева
        {
            if (len <= 0) return string.Empty;
            if (s.Length < len) return string.Empty;
            return takeRight(ref s, s.Length - len);
        }
        public static string removeRight(ref string s, int len) //возвращает строку, в которой удалены len символов справа
        {
            if (len <= 0) return string.Empty;
            if (s.Length < len) return string.Empty;
            return takeLeft(ref s, s.Length - len);
        }
         //извлекает подстроку, находящуюся между s1 и s2
         //если s1 == "", то вернет все символы слева до s2
         //если s2 == "", то вернет все символы справа после s1
        //если with_range == true, то к результату добавятся границы s1 и s2
        //если обе границы пустые, то вернется пустая строка
        //если обе границы не пустые и хоть одна из них НЕ найдена, то вернется пустая строка  
        //s2 должна быть после s1, иначе вернется пустая строка
        //s1 и s2 могут совпадать
        public static string subStringByStr(ref string s, string s1, string s2, bool with_range = false)
        {
            string res = string.Empty;
            if (s1 =="" && s2 =="") return res;

            if (s1 == "")
            {
                int pos = s.IndexOf(s2);
                if (pos >= 0)
                {
                    res = takeLeft(ref s, pos);
                    if (with_range) res += s2;        
                }
            }   
            else if (s2 == "")
            {
                int pos = s.IndexOf(s1);
                if (pos >= 0)
                {
                    res = removeLeft(ref s, pos);
                    if (!with_range) res = removeLeft(ref res, s1.Length);
                }
            }        
            else
            {
                int pos1 = s.IndexOf(s1);
                int pos2 = -1;
                if (s1 == s2)
                {
                    if (pos1 >= 0)
                        pos2 = s.IndexOf(s1, pos1+s1.Length);
                }
                else pos2 = s.IndexOf(s2);

                if (pos1 >= 0 && pos2 > pos1)
                {
                    res = subStringByIndexes(ref s, pos1+s1.Length-1, pos2);
                    if (with_range) {res = (s1 + res + s2);}
                }
            }
            return res;
        }
         //извлекает подстроку, находящуюся между символами с индексами i1 и i2
         //если i1 и i2 некорректно заданы, то вернется пустая строка  
         //должно быть i2 > i1, i1 >= 0     
        //если with_range == true, то к результату добавятся граничные символы
        public static string subStringByIndexes(ref string s, int i1, int i2, bool with_range = false)
        {
            string res = String.Empty;
            if (i1 < 0 || i2 < 0) return res;
            if (i2 <= i1 || i2 >= s.Length) return res;

            if (i1 > 0) res = removeLeft(ref s, i1);
            else res = s;

            int in_len = i2 - i1 - 1;
            if (with_range) in_len += 2; 
            if (!with_range) res = removeLeft(ref res, 1); 
            res = takeLeft(ref res, in_len);
            return res;
        }

        //заменяет в строке s кусок, начинающийся с start_index длинною len на new_str
        //если какие-то значения  некорректно заданы, то вернется исходная строка 
        public static string replace(ref string s, int start_index, int len, string new_str)
        {
            string res = s;
            if (start_index < 0 || start_index >= s.Length) {Console.WriteLine("MyString::replace WARNING: invalid start_index {0}", start_index); return res;}
            if (len <= 0 || (start_index+len) > s.Length) {Console.WriteLine("MyString::replace WARNING: invalid len {0}", len); return res;}
            string s1 = takeLeft(ref s, start_index);
            string s2 = takeRight(ref s, s.Length - (start_index+len));
 //           Console.WriteLine("MyString::replace s1=[{0}] \n", s1); 
 //           Console.WriteLine("MyString::replace s2=[{0}] \n", s2); 

            res = s1 + new_str + s2;
            return res;
        }


        //находит все парные индексы местоположения для s1 и s2 и заполняет ими список, 
        //т.е. все нечетные для s1, четные для s2 (если с 1 считать)
        public static void findRangeIndexes(ref string s, string s1, string s2, List<int> list)
        {
            list.Clear();
            int pos1 = -1;
            int pos2 = -1;
            int last_pos = 0;
            for (;;)
            {
                pos1 = s.IndexOf(s1, last_pos);
                if (pos1 < 0) break;
                pos2 = s.IndexOf(s2, pos1+1);
                if (pos2 < pos1) break;

                list.Add(pos1);
                list.Add(pos2);
                last_pos = pos2 + 1;
            }
        }

        //удалить все переходы на новую строку
        public static string removeNextRowSymbols(ref string s)
        {
            string res = s;
            string rs = String.Empty;
            res = res.Replace("\n", rs);
            res = res.Replace("\r", rs);
            return res;
        } 
        //удалить все tabs
        public static string removeTabsSymbols(ref string s)
        {
            string res = s;
            string rs = String.Empty;
            res = res.Replace("\t", rs);
            return res;
        } 
        //найти все места где больше одного пробела в ряд и заменть этот кусок на один пробел
        public static string normalizeSpace(ref string s)
        {
            string res = s;
            string rs = " ";
            for (int i=100; i>=2; i--)
            {
                string fs = new String(' ', i);
                res = res.Replace(fs, rs);
            }
            return res;
        } 


        //обычная функция split, заполняет список разделенными строками,
        //whith_empty - указывает добавлять ли в контейнер пустые строки (одни пробелы в том числе)
        public static void split(ref string s, string split_s, List<string> list, bool whith_empty = false)
        {
            list.Clear();
            if (split_s == "") return;
            string[] arr = s.Split(split_s);

            int n = arr.Length;
            if (n == 0) return;

            for (int i=0; i<n; i++)
            {
                string line = arr[i].Trim();
                if (!whith_empty && line == "") continue;
                list.Add(arr[i]);
            }
        }

        //ищет позицию подстроки в заданной строке, начиная с start_index
        //особенность функции в том, что она пробегает с конца в начало
        public static int indexOfBack(ref string s, string sub_s, int start_index = -1)
        {
            if (s == "" || sub_s == "") return -1;
            if (start_index < 0 || start_index >= s.Length) start_index = s.Length - 1;
            string left_s = takeLeft(ref s, start_index);

            int last_pos = -1;
            while (start_index >= 0)
            {
                last_pos = left_s.IndexOf(sub_s, start_index);
                if (last_pos >= 0) break;
                start_index--;
            }
            return last_pos;    
        }

        //конвертирует строку в double, в случае неудачи вернет -9999
        public static double toDouble(string s)
        {
            double p = 0;
            s = s.Replace(".", ",");
            try {p = Convert.ToDouble(s);}
            catch {p = -9999;}
            return p;
        }
        public static double toInt(string s)
        {
            int p = 0;
            try {p = Convert.ToInt32(s);}
            catch {p = -9999;}
            return p;
        }
        



    }


}