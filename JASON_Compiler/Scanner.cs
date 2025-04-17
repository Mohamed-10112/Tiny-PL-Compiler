using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;

public enum Token_Class
{
    Int, Float, String, Read, Write, Repeat, Until, If, ElseIf, Else, Then, Return, Endl, End,
    Main, Dot, Semicolon, Comma, LParanthesis, RParanthesis, LCurlyBracket, RCurlyBracket,
    IsEqualOp, LessThanOp, GreaterThanOp, NotEqualOp, PlusOp, MinusOp, MultiplyOp, DivideOp, AssignmentOp,
    ANDop, ORop, Idenifier, Integer, FloatNum, Comment, T_String
}
namespace JASON_Compiler
{


    public class Token
    {
        public string lex;
        public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        public Scanner()
        {
            ReservedWords.Add("int", Token_Class.Int);
            ReservedWords.Add("float", Token_Class.Float);
            ReservedWords.Add("string", Token_Class.String);
            ReservedWords.Add("read", Token_Class.Read);
            ReservedWords.Add("write", Token_Class.Write);
            ReservedWords.Add("repeat", Token_Class.Repeat);
            ReservedWords.Add("until", Token_Class.Until);
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("elseif", Token_Class.ElseIf);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("then", Token_Class.Then);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("endl", Token_Class.Endl);
            ReservedWords.Add("end", Token_Class.End);
            ReservedWords.Add("main", Token_Class.Main);

            Operators.Add(".", Token_Class.Dot);
            Operators.Add(";", Token_Class.Semicolon);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add("(", Token_Class.LParanthesis);
            Operators.Add(")", Token_Class.RParanthesis);
            Operators.Add("{", Token_Class.LCurlyBracket);
            Operators.Add("}", Token_Class.RCurlyBracket);
            Operators.Add("=", Token_Class.IsEqualOp);
            Operators.Add(":=", Token_Class.AssignmentOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("<>", Token_Class.NotEqualOp);
            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);
            Operators.Add("&&", Token_Class.ANDop);
            Operators.Add("||", Token_Class.ORop);


        }

        public void StartScanning(string SourceCode)
        {
            for (int i = 0; i < SourceCode.Length; i++)
            {
                int j = i;

                char CurrentChar = SourceCode[i];
                string CurrentLexeme = "";

                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n')
                    continue;

                if ((CurrentChar >= 'A' && CurrentChar <= 'z')) //if you read a character
                {

                    while (j < SourceCode.Length && ((CurrentChar >= 'A' && CurrentChar <= 'z') || (CurrentChar >= '0' && CurrentChar <= '9')))
                    {
                        CurrentLexeme += CurrentChar.ToString();
                        j++;
                        if (j < SourceCode.Length)
                        {
                            CurrentChar = SourceCode[j];
                        }

                    }
                    i = j - 1;
                    FindTokenClass(CurrentLexeme);
                }
                else if (CurrentChar >= '0' && CurrentChar <= '9')
                {
                    while (j < SourceCode.Length && ((CurrentChar >= '0' && CurrentChar <= '9') || CurrentChar == '.'))
                    {
                        CurrentLexeme += CurrentChar.ToString();
                        j++;
                        if (j < SourceCode.Length)
                        {
                            CurrentChar = SourceCode[j];
                        }
                    }
                    i = j - 1;
                    FindTokenClass(CurrentLexeme);
                }
                else if (CurrentChar == '&' || CurrentChar == '|' || CurrentChar == ':' || CurrentChar == '=')
                {
                    while (j < SourceCode.Length && (CurrentChar == '&' || CurrentChar == '|' || CurrentChar == ':' || CurrentChar == '='))
                    {
                        CurrentLexeme += CurrentChar.ToString();
                        j++;
                        if (j < SourceCode.Length)
                        {
                            CurrentChar = SourceCode[j];
                        }
                    }
                    i = j - 1;
                    FindTokenClass(CurrentLexeme);
                }
                else if (CurrentChar == '"')
                {
                    CurrentLexeme = "\"";
                    i++;
                    while (i < SourceCode.Length && SourceCode[i] != '"')
                    {
                        CurrentLexeme += SourceCode[i];
                        i++;
                    }

                    if (i < SourceCode.Length && SourceCode[i] == '"')
                    {
                        CurrentLexeme += "\"";
                        FindTokenClass(CurrentLexeme);
                        CurrentLexeme = "";
                    }
                    else
                    {
                        Errors.Error_List.Add("invalid string");
                    }
                    continue;
                }
                else if (CurrentChar == '/' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '*')
                {
                    CurrentLexeme = "/*";
                    i += 2;
                    while (i < SourceCode.Length - 1 && !(SourceCode[i] == '*' && SourceCode[i + 1] == '/'))
                    {
                        CurrentLexeme += SourceCode[i];
                        i++;
                    }

                    if (i < SourceCode.Length - 1 && SourceCode[i] == '*' && SourceCode[i + 1] == '/')
                    {
                        CurrentLexeme += "*/";
                        //FindTokenClass(CurrentLexeme);
                        i++;
                        CurrentLexeme = "";
                    }
                    else
                    {
                        Errors.Error_List.Add("invalid comment");
                    }
                    continue;
                }
                else
                {
                    CurrentLexeme += CurrentChar.ToString();
                    FindTokenClass(CurrentLexeme);

                }
            }

            JASON_Compiler.TokenStream = Tokens;
        }
        void FindTokenClass(string Lex)
        {
            Token Tok = new Token();
            Tok.lex = Lex;
            //Is it a reserved word?
            if (ReservedWords.ContainsKey(Lex))
            {
                Tok.token_type = ReservedWords[Lex];
                Tokens.Add(Tok);

            }

            //Is it an identifier?
            else if (isIdentifier(Lex))
            {
                Tok.token_type = Token_Class.Idenifier;
                Tokens.Add(Tok);
            }

            //Is it a Constant?
            else if (isConstant(Lex))
            {
                Tok.token_type = Token_Class.Integer;
                Tokens.Add(Tok);

            }
            else if (isFloat(Lex))
            {
                Tok.token_type = Token_Class.FloatNum;
                Tokens.Add(Tok);

            }
            //Is it an string
            else if (isString(Lex))
            {
                Tok.token_type = Token_Class.T_String;
                Tokens.Add(Tok);
            }
            //Is it an comment
            else if (isComment(Lex))
            {
                Tok.token_type = Token_Class.Comment;
                Tokens.Add(Tok);
            }
            //Is it an operator?
            else if (Operators.ContainsKey(Lex))
            {
                Tok.token_type = Operators[Lex];
                Tokens.Add(Tok);
            }
            //Is it an undefined?
            else
            {
                Errors.Error_List.Add("\"" + Lex + "\" " + "is undefined");
            }
        }
        bool isIdentifier(string lex)
        {
            bool isValid = true;
            var id = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*$");
            if (id.IsMatch(lex))
            {
                return isValid;
            }
            return false;
        }
        bool isConstant(string lex)
        {
            bool isValid = true;
            var con = new Regex(@"^-?[0-9]+$");
            if (con.IsMatch(lex))
            {
                return isValid;
            }
            return false;
        }
        bool isFloat(string lex)
        {
            bool isValid = true;
            var con = new Regex(@"^-?[0-9]+(\.[0-9]+)?$");
            if (con.IsMatch(lex))
            {
                return isValid;
            }
            return false;
        }
        bool isComment(string lex)
        {
            bool isValid = true;
            if (lex.StartsWith("/*") && lex.EndsWith("*/"))
            {
                return isValid;
            }
            return false;

        }
        bool isString(string lex)
        {
            bool isValid = true;
            if (lex.StartsWith("\"") && lex.EndsWith("\""))
            {
                return isValid;
            }
            return false;
        }
    }
}
