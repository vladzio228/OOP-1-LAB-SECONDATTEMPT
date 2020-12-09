using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lab1;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lab1.Tests
{
    [TestClass()]
    public class CalculatorTests
    {
        public void Compare(string expression, double expected)
        {
            double actual = Calculator.Evaluate(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void max1()
        {
            string expression = "max(3; 4)";
            Compare(expression, 4);
        }
        [TestMethod()]
        public void max2()
        {
            string expression = "2+max(4; 4)";
            Compare(expression, 6);
        }
        [TestMethod()]
        public void max3()
        {
            string expression = "max(13; 3)^2";
            Compare(expression, 169);
        }

        [TestMethod()]
        public void Mmax1()
        {
            string expression = "mmax(3; 4; 6)";
            Compare(expression, 6);
        }

        [TestMethod()]
        public void Mmax2()
        {
            string expression = "mmax(12; 0; 42)";
            Compare(expression, 42);
        }

        [TestMethod()]
        public void Mmax3()
        {
            string expression = "mmax(-5; -7)";
            Compare(expression, -5);
        }

        [TestMethod()]
        public void unary1()
        {
            string expression = "-----8";
            Compare(expression, -8);
        }

        [TestMethod()]
        public void unary2()
        {
            string expression = "--+--8";
            Compare(expression, 8);
        }

        [TestMethod()]
        public void unary3()
        {
            string expression = "-8-8+8";
            Compare(expression, -8);
        }

        [TestMethod()]
        public void difficultexpression1()
        {
            string expression = "(3^max(4;5))-(mmin(2;5;7)^mmax(5;6;10))";
            Compare(expression, -781);
        }

    }
}