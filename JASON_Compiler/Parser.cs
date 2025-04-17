using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace JASON_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();
        
        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }
    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;
        public Node root;

        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            root.Children.Add(Program());
            return root;
        }
        Node Program() {
            Node program = new Node("Program");
            program.Children.Add(Function_Statements());
            program.Children.Add(Main_Function());
            MessageBox.Show("Success");
            return program;
        }
        Node Function_Statements()
        {
            Node function_statements = new Node("Function_Statements");
            if ((TokenStream[InputPointer].token_type == Token_Class.Int
                || TokenStream[InputPointer].token_type == Token_Class.Float
                || TokenStream[InputPointer].token_type == Token_Class.String) 
                && PeekToken(1).token_type != Token_Class.Main)
            {
                //Errors.Error_List.Add("stat");
                function_statements.Children.Add(Function_Statement());
                function_statements.Children.Add(Function_Statements_Tail());
                return function_statements;
            }

            return null;


        }
        Node Function_Statement()
        {
            Node function_statement = new Node("Function_Statement");
            function_statement.Children.Add(Function_Declaration());
            function_statement.Children.Add(Function_Body());

            return function_statement;
        }
        Node Function_Declaration()
        {
            Node function_declaration = new Node("Function_Declaration");
            function_declaration.Children.Add(Datatype());
            function_declaration.Children.Add(match(Token_Class.Idenifier));
            function_declaration.Children.Add(match(Token_Class.LParanthesis));
            function_declaration.Children.Add(Parameters());
            function_declaration.Children.Add(match(Token_Class.RParanthesis));



            return function_declaration;
        }
        Node Parameters()
        {
            Node parameters = new Node("Parameters");
            if (TokenStream[InputPointer].token_type == Token_Class.Int
                || TokenStream[InputPointer].token_type == Token_Class.Float
                || TokenStream[InputPointer].token_type == Token_Class.String)
            {
                parameters.Children.Add(Parameter());
                parameters.Children.Add(Parameter_List());
                return parameters;
            }
            return null;
            
        }
        Node Parameter() {
            Node parameter = new Node("Parameter");
            parameter.Children.Add(Datatype());
            parameter.Children.Add(match(Token_Class.Idenifier));
            return parameter;


        }
        Node Parameter_List()
        {
            Node parameter_list = new Node("Parameter_List");
            if (TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                parameter_list.Children.Add(match(Token_Class.Comma));
                parameter_list.Children.Add(Parameter());
                parameter_list.Children.Add(Parameter_List());
                return parameter_list;
            }
            return null;
            

        }
        Node Function_Body()
        {
            Node function_body = new Node("Function_Body");
            function_body.Children.Add(match(Token_Class.LCurlyBracket));
            function_body.Children.Add(Statements());
            function_body.Children.Add(Return_Statement());
            function_body.Children.Add(match(Token_Class.RCurlyBracket));

            return function_body;
        }
        Node Statements()
        {
            Node statements = new Node("Statements");
            if ((TokenStream[InputPointer].token_type == Token_Class.Int
                || TokenStream[InputPointer].token_type == Token_Class.Float
                || TokenStream[InputPointer].token_type == Token_Class.String
                || TokenStream[InputPointer].token_type == Token_Class.Idenifier
                || TokenStream[InputPointer].token_type == Token_Class.Write
                || TokenStream[InputPointer].token_type == Token_Class.Read
                || TokenStream[InputPointer].token_type == Token_Class.If
                || TokenStream[InputPointer].token_type == Token_Class.Repeat)
                && PeekToken(1).token_type != Token_Class.Main)              
            {
                statements.Children.Add(Statement());
                statements.Children.Add(Statements_Tail());
                return statements;
            }
            return null;
            

        }
        Node Statements_Tail()
        {
            Node statements_tail = new Node("Statements_Tail");
            if ((TokenStream[InputPointer].token_type == Token_Class.Int
                || TokenStream[InputPointer].token_type == Token_Class.Float
                || TokenStream[InputPointer].token_type == Token_Class.String
                || TokenStream[InputPointer].token_type == Token_Class.Idenifier
                || TokenStream[InputPointer].token_type == Token_Class.Write
                || TokenStream[InputPointer].token_type == Token_Class.Read
                || TokenStream[InputPointer].token_type == Token_Class.If
                || TokenStream[InputPointer].token_type == Token_Class.Repeat)
                && PeekToken(1).token_type != Token_Class.Main)
            {
                statements_tail.Children.Add(Statement());
                statements_tail.Children.Add(Statements_Tail());
                return statements_tail;
            }
            return null;
            
        }
        Node Statement()
        {
            Node statement = new Node("Statement");
            if ((TokenStream[InputPointer].token_type == Token_Class.Int
                || TokenStream[InputPointer].token_type == Token_Class.Float
                || TokenStream[InputPointer].token_type == Token_Class.String)
                && PeekToken(1).token_type != Token_Class.Main)
            {
                statement.Children.Add(Declaration_Statement());
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Idenifier)
            {
                statement.Children.Add(Assignment_Statement());
            }
            else if(TokenStream[InputPointer].token_type == Token_Class.Write)
            {
                statement.Children.Add(Write_Statement());
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Read)
            {
                statement.Children.Add(Read_Statement());
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.If)
            {
                statement.Children.Add(If_Statement());
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Repeat)
            {
                statement.Children.Add(Repeat_Statement());
            }
            return statement;

        }
        Node Declaration_Statement()
        {
            Node declaration_statement = new Node("Declaration_Statement");
            declaration_statement.Children.Add(Datatype());
            declaration_statement.Children.Add(Declaration_Tail());

            return declaration_statement;

        }
        Node Declaration_Tail()
        {
            Node declaration_tail = new Node("Declaration_Tail");
            if (TokenStream[InputPointer].token_type == Token_Class.Idenifier && TokenStream[InputPointer + 1].token_type != Token_Class.AssignmentOp)
            {
                declaration_tail.Children.Add(Identifiers());
                declaration_tail.Children.Add(match(Token_Class.Semicolon));

            }
            else if(TokenStream[InputPointer].token_type == Token_Class.Idenifier && TokenStream[InputPointer + 1].token_type == Token_Class.AssignmentOp)
            {

                declaration_tail.Children.Add(Identifiers_With_Assignment());
                declaration_tail.Children.Add(match(Token_Class.Semicolon));

            }
            return declaration_tail;
        }
        Node Identifiers()
        {
            Node identifiers = new Node("Identifiers");
            identifiers.Children.Add(match(Token_Class.Idenifier));
            identifiers.Children.Add(Identifiers_Tail());

            return identifiers;
        }
        Node Identifiers_Tail()
        {
            Node identifiers_tail = new Node("Identifiers_Tail");
            if (TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                identifiers_tail.Children.Add(match(Token_Class.Comma));
                identifiers_tail.Children.Add(Identifiers());
                return identifiers_tail;
            }
            return null;

        }
        Node Identifiers_With_Assignment() {
            Node identifiers_with_assignment = new Node("Identifiers_With_Assignment");
            identifiers_with_assignment.Children.Add(match(Token_Class.Idenifier));
            identifiers_with_assignment.Children.Add(match(Token_Class.AssignmentOp));
            identifiers_with_assignment.Children.Add(Expressions());
            identifiers_with_assignment.Children.Add(Identifiers_With_Assignment_Tail());
            return identifiers_with_assignment;
        }
        Node Identifiers_With_Assignment_Tail()
        {
            Node identifiers_with_assignment_tail = new Node("Identifiers_With_Assignment_Tail");
            if (TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                Node identifiers_with_assignment = new Node("Identifiers_With_Assignment");
                identifiers_with_assignment_tail.Children.Add(match(Token_Class.Idenifier));
                identifiers_with_assignment_tail.Children.Add(match(Token_Class.AssignmentOp));
                identifiers_with_assignment_tail.Children.Add(Expressions());
                identifiers_with_assignment_tail.Children.Add(Identifiers_With_Assignment_Tail());
                return identifiers_with_assignment_tail;
            }
            return null;
            

        }
        Node Assignment_Statement()
        {
            Node assignment_statement = new Node("Assignment_Statement");
            assignment_statement.Children.Add(match(Token_Class.Idenifier));
            assignment_statement.Children.Add(match(Token_Class.AssignmentOp));
            assignment_statement.Children.Add(Expressions());
            assignment_statement.Children.Add(match(Token_Class.Semicolon));


            return assignment_statement;

        }
        Node Expressions()
        {
            Node expressions = new Node("Expressions");
            if (TokenStream[InputPointer].token_type == Token_Class.T_String)
            {
                expressions.Children.Add(match(Token_Class.T_String));

            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Integer
                      || TokenStream[InputPointer].token_type == Token_Class.FloatNum
                      || TokenStream[InputPointer].token_type == Token_Class.Idenifier
                      || TokenStream[InputPointer].token_type == Token_Class.LParanthesis)
            {
                expressions.Children.Add(Equation());

            }
            return expressions;


        }
        Node Equation() {
            Node equation = new Node("Equation");
            equation.Children.Add(Term());
            equation.Children.Add(Equation_Tail());

            return equation;


        }
        Node Equation_Tail() {
            Node equation_tail = new Node("Equation_Tail");
            if (TokenStream[InputPointer].token_type == Token_Class.PlusOp || TokenStream[InputPointer].token_type == Token_Class.MinusOp)
            {
                equation_tail.Children.Add(Add_Sub_Operator());
                equation_tail.Children.Add(Term());
                equation_tail.Children.Add(Equation_Tail());
                return equation_tail;
            }
            return null;

        }
        Node Add_Sub_Operator() { 
            Node add_sub_operator = new Node("Add_Sub_Operator");
            if (TokenStream[InputPointer].token_type == Token_Class.PlusOp)
            {
                add_sub_operator.Children.Add(match(Token_Class.PlusOp));

            }
            else if (TokenStream[InputPointer].token_type == Token_Class.MinusOp)
            {
                add_sub_operator.Children.Add(match(Token_Class.MinusOp));

            }
            return add_sub_operator;

        }

        Node Term()
        {
            Node term = new Node("Term");
            term.Children.Add(Factor());
            term.Children.Add(Term_Tail());

            return term;

        }
        Node Factor()
        {
            Node factor = new Node("Factor");
            if (TokenStream[InputPointer].token_type == Token_Class.Integer)
            {
                factor.Children.Add(match(Token_Class.Integer));

            }
            else if (TokenStream[InputPointer].token_type == Token_Class.FloatNum)
            {
                factor.Children.Add(match(Token_Class.FloatNum));

            }else if (TokenStream[InputPointer].token_type == Token_Class.Idenifier && TokenStream[InputPointer + 1].token_type == Token_Class.LParanthesis)
            {
                factor.Children.Add(Function_Call());

            }else if (TokenStream[InputPointer].token_type == Token_Class.Idenifier && TokenStream[InputPointer + 1].token_type != Token_Class.LParanthesis)
            {
                factor.Children.Add(match(Token_Class.Idenifier));

            }
            else if (TokenStream[InputPointer].token_type == Token_Class.LParanthesis)
            {
                factor.Children.Add(match(Token_Class.LParanthesis));
                factor.Children.Add(Equation());
                factor.Children.Add(match(Token_Class.RParanthesis));

            }
            return factor;
        }
        Node Function_Call()
        {
            Node function_call = new Node("Function_Call");
            function_call.Children.Add(match(Token_Class.Idenifier));
            function_call.Children.Add(match(Token_Class.LParanthesis));
            function_call.Children.Add(Arguments());
            function_call.Children.Add(match(Token_Class.RParanthesis));


            return function_call;
        }
        Node Arguments()
        {
            Node arguments = new Node("Arguments");
            if (TokenStream[InputPointer].token_type == Token_Class.Idenifier
                || TokenStream[InputPointer].token_type == Token_Class.Integer
                || TokenStream[InputPointer].token_type == Token_Class.FloatNum
                || TokenStream[InputPointer].token_type == Token_Class.T_String)
            {
                arguments.Children.Add(Argument());
                arguments.Children.Add(Arguments_List());
                return arguments;

            }
            return null;
            
        }
        Node Argument()
        {
            Node argument = new Node("Argument");
            if (TokenStream[InputPointer].token_type == Token_Class.Idenifier)
            {
                argument.Children.Add(match(Token_Class.Idenifier));

            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Integer)
            {
                argument.Children.Add(match(Token_Class.Integer));

            }
            else if (TokenStream[InputPointer].token_type == Token_Class.FloatNum)
            {
                argument.Children.Add(match(Token_Class.FloatNum));

            }
            else if (TokenStream[InputPointer].token_type == Token_Class.T_String)
            {
                argument.Children.Add(match(Token_Class.T_String));

            }
            return argument;
        }
        Node Arguments_List() {

            Node arguments_list = new Node("Arguments_List");
            if (TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                arguments_list.Children.Add(match(Token_Class.Comma));
                arguments_list.Children.Add(Argument());
                arguments_list.Children.Add(Arguments_List());

                return arguments_list;
            }
            return null;
            
        }
        Node Term_Tail() {
            Node term_tail = new Node("Term_Tail");
            if (TokenStream[InputPointer].token_type == Token_Class.MultiplyOp || TokenStream[InputPointer].token_type == Token_Class.DivideOp)
            {
                term_tail.Children.Add(Mul_Div_Operator());
                term_tail.Children.Add(Factor());
                term_tail.Children.Add(Term_Tail());
                return term_tail;
            }
            return null;
            
        }
        Node Mul_Div_Operator() {

            Node mul_div_operator = new Node("Mul_Div_Operator");
            if (TokenStream[InputPointer].token_type == Token_Class.MultiplyOp)
            {
                mul_div_operator.Children.Add(match(Token_Class.MultiplyOp));

            }
            else if (TokenStream[InputPointer].token_type == Token_Class.DivideOp)
            {
                mul_div_operator.Children.Add(match(Token_Class.DivideOp));

            }
            return mul_div_operator;
        }
        Node Write_Statement()
        {
            Node write_statement = new Node("Write_Statement");
           
            if (TokenStream[InputPointer].token_type == Token_Class.Write && TokenStream[InputPointer + 1].token_type == Token_Class.Endl)
            {
                write_statement.Children.Add(match(Token_Class.Write));
                write_statement.Children.Add(match(Token_Class.Endl));

            }
            else
            {
                write_statement.Children.Add(match(Token_Class.Write));
                write_statement.Children.Add(Expressions());

            }
            write_statement.Children.Add(match(Token_Class.Semicolon));

            return write_statement;
        }
        Node Read_Statement()
        {
            Node read_statement = new Node("Read_Statement");
            read_statement.Children.Add(match(Token_Class.Read));
            read_statement.Children.Add(match(Token_Class.Idenifier));
            read_statement.Children.Add(match(Token_Class.Semicolon));


            return read_statement;
        }
        Node Return_Statement()
        {
            Node return_statement = new Node("Return_Statement");
            return_statement.Children.Add(match(Token_Class.Return));
            return_statement.Children.Add(Expressions());
            return_statement.Children.Add(match(Token_Class.Semicolon));


            return return_statement;
        }
        Node Main_Function()
        {
            Node main_function = new Node("Main_Function");
            main_function.Children.Add(Datatype());
            main_function.Children.Add(match(Token_Class.Main));
            main_function.Children.Add(match(Token_Class.LParanthesis));
            main_function.Children.Add(match(Token_Class.RParanthesis));
            main_function.Children.Add(Function_Body());


            return main_function;
        }
        Node Datatype()
        {
            Node datatype = new Node("Datatype");
            if (TokenStream[InputPointer].token_type == Token_Class.Int)
            {
                datatype.Children.Add(match(Token_Class.Int));

            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Float)
            {
                datatype.Children.Add(match(Token_Class.Float));

            }
            else if (TokenStream[InputPointer].token_type == Token_Class.String)
            {
                datatype.Children.Add(match(Token_Class.String));

            }
            return datatype;
        }
        Node Function_Statements_Tail()
        {
            Node function_statements_tail = new Node("Function_Statements_Tail");
            if ((TokenStream[InputPointer].token_type == Token_Class.Int
                || TokenStream[InputPointer].token_type == Token_Class.Float
                || TokenStream[InputPointer].token_type == Token_Class.String)
                && PeekToken(1).token_type != Token_Class.Main)
            {
                function_statements_tail.Children.Add(Function_Statement());
                function_statements_tail.Children.Add(Function_Statements_Tail());
                return function_statements_tail;
            }
            return null;
            
        }
        Node If_Statement()
        {
            Node if_statement = new Node("If_Statement");
            if_statement.Children.Add(match(Token_Class.If));
            if_statement.Children.Add(Condition_Statement());
            if_statement.Children.Add(match(Token_Class.Then));
            if_statement.Children.Add(Statements());
            if_statement.Children.Add(Else_If_Statements());

            if_statement.Children.Add(Else_Statement());
            if_statement.Children.Add(match(Token_Class.End));


            return if_statement;
        }
        Node Condition_Statement()
        {
            Node condition_statement = new Node("Condition_Statement");
            condition_statement.Children.Add(Condition());
            condition_statement.Children.Add(Condition_Tail());

            return condition_statement;

        }
        Node Condition()
        {
            Node condition = new Node("Condition");
            condition.Children.Add(match(Token_Class.Idenifier));
            condition.Children.Add(Condition_Operator());
            condition.Children.Add(Term());


            return condition;

        }
        Node Condition_Tail() { 
            Node condition_tail = new Node("Condition_Tail");
            if (TokenStream[InputPointer].token_type == Token_Class.LessThanOp
                || TokenStream[InputPointer].token_type == Token_Class.GreaterThanOp
                || TokenStream[InputPointer].token_type == Token_Class.IsEqualOp
                || TokenStream[InputPointer].token_type == Token_Class.NotEqualOp)
            {
                condition_tail.Children.Add(Condition_Operator());
                condition_tail.Children.Add(Term());


            }
            else if (TokenStream[InputPointer].token_type == Token_Class.ANDop
                     || TokenStream[InputPointer].token_type == Token_Class.ORop)
            {
                condition_tail.Children.Add(Boolean_Operator());
                condition_tail.Children.Add(Condition_Statement());
            }
            return condition_tail;

        }
        Node Condition_Operator() {
            Node condition_operator = new Node("Condition_Operator");
            if (TokenStream[InputPointer].token_type == Token_Class.LessThanOp)
            {
                condition_operator.Children.Add(match(Token_Class.LessThanOp));

            }
            else if (TokenStream[InputPointer].token_type == Token_Class.GreaterThanOp)
            {
                condition_operator.Children.Add(match(Token_Class.GreaterThanOp));

            }
            else if (TokenStream[InputPointer].token_type == Token_Class.IsEqualOp)
            {
                condition_operator.Children.Add(match(Token_Class.IsEqualOp));

            }
            else if (TokenStream[InputPointer].token_type == Token_Class.NotEqualOp)
            {
                condition_operator.Children.Add(match(Token_Class.NotEqualOp));

            }
            return condition_operator;
        }
        Node Boolean_Operator()
        {
            Node boolean_operator = new Node("Boolean_Operator");
            if (TokenStream[InputPointer].token_type == Token_Class.ANDop)
            {
                boolean_operator.Children.Add(match(Token_Class.ANDop));

            }
            else if (TokenStream[InputPointer].token_type == Token_Class.ORop)
            {
                boolean_operator.Children.Add(match(Token_Class.ORop));

            }
            return boolean_operator;
        }
        Node Else_If_Statements()
        {
            Node else_if_statements = new Node("Else_If_Statements");
            if(TokenStream[InputPointer].token_type == Token_Class.ElseIf)
            {
                else_if_statements.Children.Add(Else_If_Statement());
                else_if_statements.Children.Add(Else_If_Statements_Tail());
                return else_if_statements;

            }
            return null;
            
        }
        Node Else_If_Statement()
        {
            Node else_if_statement = new Node("Else_If_Statement");
            else_if_statement.Children.Add(match(Token_Class.ElseIf));
            else_if_statement.Children.Add(Condition_Statement());
            else_if_statement.Children.Add(match(Token_Class.Then));
            else_if_statement.Children.Add(Statements());


            return else_if_statement;

        }
        Node Else_If_Statements_Tail()
        {
            Node else_if_statement_tail = new Node("Else_If_Statements_Tail");
            if (TokenStream[InputPointer].token_type == Token_Class.ElseIf)
            {
                else_if_statement_tail.Children.Add(Else_If_Statement());
                else_if_statement_tail.Children.Add(Else_If_Statements_Tail());
                return else_if_statement_tail;
            }
            return null;
            
        }
        Node Else_Statement()
        {
            Node else_statement = new Node("Else_Statement");
            if (TokenStream[InputPointer].token_type == Token_Class.Else)
            {
                else_statement.Children.Add(match(Token_Class.Else));
                else_statement.Children.Add(Statements());
                return else_statement;

            }
            return null;
            
        }
        Node Repeat_Statement()
        {
            Node repeat_statement = new Node("Repeat_Statement");
            repeat_statement.Children.Add(match(Token_Class.Repeat));
            repeat_statement.Children.Add(Statements());
            repeat_statement.Children.Add(match(Token_Class.Until));
            repeat_statement.Children.Add(Condition_Statement());

            return repeat_statement;

        }


        //Node Program()
        //{
        //    Node program = new Node("Program");
        //    program.Children.Add(Header());
        //    program.Children.Add(DeclSec());
        //    program.Children.Add(Block());
        //    program.Children.Add(match(Token_Class.Dot));
        //    MessageBox.Show("Success");
        //    return program;
        //}

        //Node Header()
        //{
        //    Node header = new Node("Header");
        //    // write your code here to check the header sructure
        //    return header;
        //}
        //Node DeclSec()
        //{
        //    Node declsec = new Node("DeclSec");
        //    // write your code here to check atleast the declare sturcure 
        //    // without adding procedures
        //    return declsec;
        //}
        //Node Block()
        //{
        //    Node block = new Node("block");
        //    // write your code here to match statements
        //    return block;
        //}

        // Implement your logic here
        private Token PeekToken(int offset)
        {
            int peekIndex = InputPointer + offset;
            if (peekIndex >= 0 && peekIndex < TokenStream.Count)
            {
                return TokenStream[peekIndex];
            }
            return null;
        }

        public Node match(Token_Class ExpectedToken)
        {

            if (InputPointer < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    if (TokenStream.Count - 1 != InputPointer)
                        InputPointer++;
                    Node newNode = new Node(ExpectedToken.ToString());

                    return newNode;

                }

                else
                {
                    if (TokenStream[InputPointer].token_type.ToString() != "Comment")
                    {

                        Errors.Error_List.Add("Parsing Error: Expected "
                            + ExpectedToken.ToString() + " and " +
                            TokenStream[InputPointer].token_type.ToString() +
                            "  found\r\n");
                        if (TokenStream.Count - 1 != InputPointer)
                            InputPointer++;
                        return null;
                    }
                    return null;
                }
            }
            else
            {
                if (TokenStream[InputPointer].token_type.ToString() != "Comment")
                {
                    Errors.Error_List.Add("Parsing Error: Expected "
                            + ExpectedToken.ToString() + "\r\n");
                    //if (TokenStream.Count - 1 != InputPointer)
                    //    InputPointer++;
                    return null;
                }
                return null;
                
            }
        }


        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}
