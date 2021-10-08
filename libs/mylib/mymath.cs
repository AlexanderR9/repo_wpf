using System;
using System.Collections.Generic;

namespace mylib.math
{
    public static class MyMath
    {
        //возвращает целое число от a1 до a2 включительно
        public static int randInt(int a1, int a2)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            return random.Next(a1, a2);
        }
        //возвращает вещественное число от 0 до 1 включительно
        public static double rand()
        {
            int dig = 10000;
            int a = randInt(0, dig);
            return ((double)a)/((double)dig);
        }


    }

}