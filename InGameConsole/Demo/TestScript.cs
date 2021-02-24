using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lim.InGameConsole;

namespace Lim.InGameConsole.Demo
{
    //
    public class TestScript : MonoBehaviour
    {
        [ConsoleCommand]
        public static void CountNumbers(int amount = 5)
        {
            for (int i = 0; i < amount; i++)
            {
                Debug.Log(i);
                GameConsole.Print(i);
            }
        }

        [ConsoleCommand]
        public static void CoEST()
        {
            for (int i = 0; i < 5; i++)
            {
                print(i);
            }
        }

        [ConsoleCommand]
        public static void TEST()
        {
            for (int i = 0; i < 5; i++)
            {
                print(i);
            }
        }

        [ConsoleCommand]
        public static void Sample()
        {
            for (int i = 0; i < 5; i++)
            {
                print(i);
            }
        }

        [ConsoleCommand]
        public static void Molly()
        {
            for (int i = 0; i < 5; i++)
            {
                print(i);
            }
        }
    }
    public class TestSc
    {
        [ConsoleCommand]
        public static void Molly()
        {
            for (int i = 0; i < 5; i++)
            {
                Debug.Log(i);
            }
        }
        [ConsoleCommand]
        public static void Sample(float x = 2.03f, bool t = false)
        {
            if (t)
                return;
            for (int i = 0; i < 5; i++)
            {
                Debug.Log(x + i);
            }
        }
    }
}