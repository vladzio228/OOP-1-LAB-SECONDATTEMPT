using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1
{
    class Visitor : Lab1BaseVisitor<double>
    {
        public override double VisitCompileUnit(Lab1Parser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitNumberExpr(Lab1Parser.NumberExprContext context)
        {
            var result = double.Parse(context.GetText());
            Debug.WriteLine(result);

            return result;
        }
        public override double VisitUnminExpr(Lab1Parser.UnminExprContext context)
        {
            var number = WalkLeft(context);
            Debug.WriteLine("-{0}", number);
             return -number;
        }

        public override double VisitIdentifierExpr(Lab1Parser.IdentifierExprContext context)
        {

            var result = context.GetText();
            if (MainWindow.cellNameToCellObject.ContainsKey(result))
                 return Double.Parse(MainWindow.cellNameToCellObject[result].Value.ToString());
            else return Double.Parse("NOT A DOUBLE");                                                                                                                                                                                                                                                                                
        }

        public override double VisitParenthesizedExpr(Lab1Parser.ParenthesizedExprContext context)
        {


            return Visit(context.expression());
        }

        public override double VisitExponentialExpr(Lab1Parser.ExponentialExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            Debug.WriteLine("{0} ^ {1}", left, right);
            return System.Math.Pow(left, right);
        }

        public override double VisitAdditiveExpr(Lab1Parser.AdditiveExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == Lab1Lexer.ADD)
            {
                Debug.WriteLine("{0} + {1}", left, right);
                return left + right;
            }
            else 
            {
                Debug.WriteLine("{0} - {1}", left, right);
                return left - right;
            }
        }

        public override double VisitMultiplicativeExpr(Lab1Parser.MultiplicativeExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == Lab1Lexer.MULTIPLY)
            {
                Debug.WriteLine("{0} * {1}", left, right);
                return left * right;
            }
            else 
            {
                Debug.WriteLine("{0} / {1}", left, right);
                if (right == 0)
                    return (int)left / (int)right; 
                return left / right;
            }
        }       
       
        public override double VisitMaxMinExpr(Lab1Parser.MaxMinExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            if (context.operatorToken.Type == Lab1Lexer.MAX)
            {
                Debug.WriteLine("max({0};{1})", left, right);
                return Math.Max(left, right) ;
            }
            Debug.WriteLine("min({0};{1})", left, right);
            return Math.Min(left, right);

        }

        public override double VisitMmaxMminExpr(Lab1Parser.MmaxMminExprContext context)
        {
            List<double> list = new List<double>();
            int idx = 0;
            while (WalkN(context, idx) != 1.1010)
            {
                list.Add(WalkN(context, idx));
                idx++;
            }
            var sortedList = from l in list orderby l select l;
            if (context.operatorToken.Type == Lab1Lexer.MMAX)
            {
                Debug.WriteLine("mmax(");
                for (idx = 0; idx < list.Count(); ++idx)
                {
                    Debug.WriteLine("{0}", list.ElementAt(idx));
                    if (idx != list.Count() - 1)
                        Debug.WriteLine(" , ");
                }
                Debug.WriteLine(")");
                return sortedList.ElementAt(sortedList.Count() - 1);
            }
                Debug.WriteLine("mmin(");
                for (idx = 0; idx < list.Count(); ++idx)
                {
                    Debug.WriteLine("{0}", list.ElementAt(idx));
                    if (idx != list.Count() - 1)
                        Debug.WriteLine(" , ");
                }
                Debug.WriteLine(")");
                return sortedList.ElementAt(0);
            
        }

        private double WalkLeft(Lab1Parser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<Lab1Parser.ExpressionContext>(0));
        }

        private double WalkRight(Lab1Parser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<Lab1Parser.ExpressionContext>(1));
        }

        private double WalkN(Lab1Parser.ExpressionContext context, int idx)
        {
            try
            {
                return Visit(context.GetRuleContext<Lab1Parser.ExpressionContext>(idx));
            }
            catch (NullReferenceException)
            { 
                return 1.1010;
            }
        }
    }
}

