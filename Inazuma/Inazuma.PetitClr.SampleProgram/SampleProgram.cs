using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Inazuma.PetitClr.SampleHost
{
    class FizzBuzz
    {
        public void Run()
        {
            Debugger.Break();
            for (var i = 1; i < 100; i++)
            {
                if (i % 15 == 0)
                {
                    this.Fizz();
                    this.Buzz();
                }
                else if (i % 3 == 0)
                {
                    this.Fizz();
                }
                else if (i % 5 == 0)
                {
                    this.Buzz();
                }
                else
                {
                    Console.Write(i);
                }
                Console.WriteLine();
            }
        }

        public void Fizz()
        {
            Console.Write("Fizz");
        }
        public void Buzz()
        {
            Console.Write("Buzz");
        }
    }

    class SampleProgram
    {
        public void Main()
        {
            new FizzBuzz().Run();
        }

        public void Main_()
        {
            //try
            //{
            //    try
            //    {
            //        Throw();
            //    }
            //    catch (InvalidOperationException ex)
            //    {
            //        System.Console.WriteLine(ex.ToString());
            //    }
            //}
            //catch (Exception ex1)
            //{
            //    System.Console.WriteLine(ex1.ToString());
            //}

            A.StaticValue = "StaticValue";
            System.Console.WriteLine(A.StaticValue);

            var hoge = new B("Hauhau Mofumofu", 1, new object());
            System.Console.WriteLine(hoge.Fuga());
            System.Console.WriteLine(hoge.Add(1, 2));
            System.Console.WriteLine(hoge.Hauhau());

            IHauhau hogeA = hoge;
            System.Console.WriteLine(hogeA.Hauhau());

            var text = "Hello World! {0}";
            var num = 128;
            num = 128 + num;

            var num2 = num;


            for (var i = 0; i < 10; i++)
            {
                System.Console.WriteLine(System.String.Format("{0}", num));
                if (num > 260)
                {
                    System.Console.WriteLine("over 260!");
                }
                num++;
            }

            checked
            {
                num += 100;
                num = num % 10;
            }

            text = System.String.Format(text, num);
            System.Console.WriteLine("[{0}]", text);

            var obj = new object();
            System.Console.WriteLine(obj.ToString());

            hoge.Throw();
        }

        public static void Throw()
        {
            throw new Exception("T H R O W !");
        }
    }

    class A : IHauhau
    {
        public static string StaticValue { get; set; }
        public string Value { get; set; }
        public int IntValue { get; set; }
        public object ObjValue { get; set; }

        public virtual string Hauhau()
        {
            return "Hauhau";
        }
    }

    interface IHauhau
    {
        string Hauhau();
    }

    class B : A, IHauhau
    {
        public B(string value, int intValue, object obj)
        {
            Value = value;
            IntValue = intValue;
            ObjValue = obj;
        }

        public string Fuga()
        {
            return Value;
        }
        public override string Hauhau()
        {
            return "H A U H A U !";
        }

        string IHauhau.Hauhau()
        {
            return "I am hauhau.";
        }

        public int Add(int a, int b)
        {
            return a + b;
        }

        public void Throw()
        {
            throw new Exception("hoge");
        }
    }
}
