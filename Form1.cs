using System;
using System.Drawing;
using System.Windows.Forms;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OMSI2_Tags;
using ScintillaNET;

namespace Compiler_Project
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //TestCompile.CompileMe();
        }

        ScintillaNET.Scintilla TextArea;
        SaveFileDialog saveOCC = new SaveFileDialog();
        OpenFileDialog openOCC = new OpenFileDialog();
        SaveFileDialog saveCFG = new SaveFileDialog();

        private void Form1_Load(object sender, EventArgs e)
        {
            TextArea = new ScintillaNET.Scintilla();
            TextPanel.Controls.Add(TextArea);

            TextArea.Dock = System.Windows.Forms.DockStyle.Fill;
            TextArea.TextChanged += (this.OnTextChanged);

            TextArea.WrapMode = WrapMode.None;
            TextArea.IndentationGuides = IndentView.LookBoth;

            //InitColors();
            InitSyntaxColoring();
            InitNumberMargin();

        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openOCC.Filter = "OCC Files|*.occ";
            openOCC.ShowDialog();

            var filePath = openOCC.FileName.Split('\\').ToList();
            filePath.RemoveAt(filePath.Count - 1);

            Compiler.inputPath = filePath.Aggregate((a, b) => a + "\\" + b);

            TextArea.Text = System.IO.File.ReadAllText(openOCC.FileName);
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveOCC.Filter = "OCC Files|*.occ";
            saveOCC.ShowDialog();
            System.IO.File.WriteAllText(saveOCC.FileName, TextArea.Text);
        }

        private void BuildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp");
            string Output = "Out.exe";

            var mesh = new Mesh("");
            textBox2.Text = "";
            System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = true;

            var assemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Where(a => !parameters.ReferencedAssemblies.Contains(a.Location))
                .Select(a => a.Location);
            parameters.ReferencedAssemblies.AddRange(assemblies.ToArray());
            parameters.OutputAssembly = Output;

            var components = Compiler.GetComponents(TextArea.Text);

            saveCFG.Filter = "Model Config Files|*.cfg";
            saveCFG.ShowDialog();

            var filename = saveCFG.FileName.Replace(@"\", @"\\");
            Compiler.outputPath = filename;
            var compileString = Compiler.GetCompileString(components, Compiler.inputPath,filename);

            string inputText = "using System; using OMSI2_Tags; namespace Compiler_Project { class CompilerClass { static void Main(string[] args) {" + TextArea.Text + compileString + "}}}";
            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, inputText);

            if (results.Errors.Count > 0)
            {
                foreach (CompilerError CompErr in results.Errors)
                {
                    textBox2.Text = textBox2.Text +
                                "Line number " + CompErr.Line +
                                ", Error Number: " + CompErr.ErrorNumber +
                                ", '" + CompErr.ErrorText + ";" +
                                Environment.NewLine + Environment.NewLine;
                }
            }
            else
            {
                Process.Start(Output);
            }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {

        }

        private const int BACK_COLOR = 0x2A211C;
        private const int FORE_COLOR = 0xB7B7B7;
        private const int NUMBER_MARGIN = 1;
        private const int BOOKMARK_MARGIN = 2;
        private const int BOOKMARK_MARKER = 2;


        /*private void InitColors()
        {
            TextArea.SetSelectionBackColor(true, IntToColor(0x114D9C));
        }*/

        private void InitSyntaxColoring()
        {

            // Configure the default style
            TextArea.StyleResetDefault();
            TextArea.Styles[Style.Default].Font = "consolas";
            TextArea.Styles[Style.Default].Size = 11;
            TextArea.Styles[Style.Default].BackColor = IntToColor(0x212121);
            TextArea.Styles[Style.Default].ForeColor = IntToColor(0xFFFFFF);
            TextArea.StyleClearAll();

            // Configure the CPP (C#) lexer styles
            TextArea.Styles[Style.Cpp.Identifier].ForeColor = IntToColor(0xD0DAE2);
            TextArea.Styles[Style.Cpp.Comment].ForeColor = IntToColor(0xBD758B);
            TextArea.Styles[Style.Cpp.CommentLine].ForeColor = IntToColor(0x40BF57);
            TextArea.Styles[Style.Cpp.CommentDoc].ForeColor = IntToColor(0x2FAE35);
            TextArea.Styles[Style.Cpp.Number].ForeColor = IntToColor(0xB5CEA8);
            TextArea.Styles[Style.Cpp.String].ForeColor = IntToColor(0xD69D85);
            TextArea.Styles[Style.Cpp.Character].ForeColor = IntToColor(0xE95454);
            TextArea.Styles[Style.Cpp.Preprocessor].ForeColor = IntToColor(0x8AAFEE);
            TextArea.Styles[Style.Cpp.Operator].ForeColor = IntToColor(0xE0E0E0);
            TextArea.Styles[Style.Cpp.Regex].ForeColor = IntToColor(0xff00ff);
            TextArea.Styles[Style.Cpp.CommentLineDoc].ForeColor = IntToColor(0x77A7DB);
            TextArea.Styles[Style.Cpp.Word].ForeColor = IntToColor(0x48A8EE);
            TextArea.Styles[Style.Cpp.Word2].ForeColor = IntToColor(0xF98906);
            TextArea.Styles[Style.Cpp.CommentDocKeyword].ForeColor = IntToColor(0xB3D991);
            TextArea.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = IntToColor(0xFF0000);
            TextArea.Styles[Style.Cpp.GlobalClass].ForeColor = IntToColor(0x48A8EE);

            TextArea.Lexer = Lexer.Cpp;

            TextArea.SetKeywords(0, "class extends implements import interface new case do while else if for in switch throw get set function var try catch finally while with default break continue delete return each const namespace package include use is as instanceof typeof author copy default deprecated eventType example exampleText exception haxe inheritDoc internal link mtasc mxmlc param private return see serial serialData serialField since throws usage version langversion playerversion productversion dynamic private public partial static intrinsic internal native override protected AS3 final super this arguments null Infinity NaN undefined true false abstract as base bool break by byte case catch char checked class const continue decimal default delegate do double descending explicit event extern else enum false finally fixed float for foreach from goto group if implicit in int interface internal into is lock long new null namespace object operator out override orderby params private protected public readonly ref return switch struct sbyte sealed short sizeof stackalloc static string select this throw true try typeof uint ulong unchecked unsafe ushort using var virtual volatile void while where yield");
            TextArea.SetKeywords(1, "void Null ArgumentError arguments Array Boolean Class Date DefinitionError Error EvalError Function int Math Namespace Number Object RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError uint XML XMLList Boolean Byte Char DateTime Decimal Double Int16 Int32 Int64 IntPtr SByte Single UInt16 UInt32 UInt64 UIntPtr Void Path File System Windows Forms ScintillaNET");

        }

        public static Color IntToColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        private void InitNumberMargin()
        {

            TextArea.Styles[Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
            TextArea.Styles[Style.LineNumber].ForeColor = IntToColor(FORE_COLOR);
            TextArea.Styles[Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
            TextArea.Styles[Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);

            var nums = TextArea.Margins[NUMBER_MARGIN];
            nums.Width = 30;
            nums.Type = MarginType.Number;
            nums.Sensitive = true;
            nums.Mask = 0;

            TextArea.MarginClick += TextArea_MarginClick;
        }

        private void TextArea_MarginClick(object sender, MarginClickEventArgs e)
        {
            if (e.Margin == BOOKMARK_MARGIN)
            {
                // Do we have a marker for this line?
                const uint mask = (1 << BOOKMARK_MARKER);
                var line = TextArea.Lines[TextArea.LineFromPosition(e.Position)];
                if ((line.MarkerGet() & mask) > 0)
                {
                    // Remove existing bookmark
                    line.MarkerDelete(BOOKMARK_MARKER);
                }
                else
                {
                    // Add bookmark
                    line.MarkerAdd(BOOKMARK_MARKER);
                }
            }
        }
    }
}
