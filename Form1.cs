using OMSI2_Tags;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Compiler_Project
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button1.DragDrop += button1_DragDrop;
            button1.DragEnter += button1_DragEnter;
        }

        private readonly OpenFileDialog _openOcc = new OpenFileDialog();
        private readonly SaveFileDialog _saveCfg = new SaveFileDialog();

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            _openOcc.Filter = @"OCC Files|*.occ";
            _openOcc.ShowDialog();

            if (_openOcc.FileName != "")
            {
                GenerateInputPath();
                BuildConfig();
            }
        }

        private static void button1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
        }

        private void button1_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            _openOcc.FileName = s[0];
            GenerateInputPath();
            BuildConfig();
        }

        private string Minify(string filePath)
        {
            var contents = "";
            if (filePath != "")
            {
                var fileLines = File.ReadAllLines(filePath);
                foreach (var line in fileLines)
                {
                    if (line != "\n")
                    {
                        contents += line;
                    }
                }
            }
            return contents;
        }

        private string GetContents()
        {
            var fileContents = Minify(_openOcc.FileName);
            int i = 0;
            foreach (var thing in fileContents.Split(";".ToCharArray()))
            {
                if (thing.IndexOf("Compiler.AdditionalOCC.Add", StringComparison.Ordinal) > -1 && !Compiler.OCCs.ContainsKey(i))
                {
                    Compiler.OCCs.Add(i, Methods.getBetween(thing, "(\"", "\")"));
                }
                else
                {
                    i++;
                }
            }
            foreach (KeyValuePair<int, string> kvp in Compiler.OCCs)
            {
                i = 0;
                var occContent = Minify(Compiler.InputPath + "\\" + kvp.Value);
                foreach (var thing in fileContents.Split(";".ToCharArray()))
                {
                    if (kvp.Key == i)
                    {
                        fileContents = fileContents.Replace(thing + ";", occContent);
                        i++;
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            return fileContents;
        }

        private void BuildConfig()
        {
            var dummy = new Mesh("");
            textBox2.Text = "";

            var codeProvider = CodeDomProvider.CreateProvider("CSharp");
            var Output = "Out.exe";
            var parameters = InitParameters();
            parameters.OutputAssembly = Output;

            var contents = GetContents();
            contents = contents.Replace(@"\", @"\\");
            var configComponents = Compiler.GetComponents(contents);

            _saveCfg.Filter = @"Model Config Files|*.cfg";
            _saveCfg.ShowDialog();

            if (_saveCfg.FileName != "")
            {
                var filename = _saveCfg.FileName.Replace(@"\", @"\\");
                Compiler.OutputPath = filename;
                var compileString = Compiler.GetCompileString(configComponents, filename);
                var inputText = "using System; using OMSI2_Tags; namespace Compiler_Project { class CompilerClass { static void Main(string[] args) {" + contents + compileString + "}}}";
                var results = codeProvider.CompileAssemblyFromSource(parameters, inputText);

                if (results.Errors.Count > 0)
                {
                    foreach (CompilerError compErr in results.Errors)
                    {
                        textBox2.Text = textBox2.Text +
                                        @"Line number " + compErr.Line +
                                        @", Error Number: " + compErr.ErrorNumber +
                                        @", '" + compErr.ErrorText + @";" +
                                        Environment.NewLine + Environment.NewLine;
                    }
                }
                else
                {
                    var process = Process.Start(Output);
                    if (process == null) return;
                    var procId = process.Id;

                    try
                    {
                        while (true)
                        {
                            Process.GetProcessById(procId);
                        }
                    }
                    catch (ArgumentException)
                    {
                        File.Delete(Output);
                    }
                }
            }
        }

        private static CompilerParameters InitParameters()
        {
            var parameters = new CompilerParameters { GenerateExecutable = true };
            var assemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Where(a => !parameters.ReferencedAssemblies.Contains(a.Location))
                .Select(a => a.Location);
            parameters.ReferencedAssemblies.AddRange(assemblies.ToArray());

            return parameters;
        }

        private void GenerateInputPath()
        {
            var filePath = _openOcc.FileName.Split('\\').ToList();
            filePath.RemoveAt(filePath.Count - 1);
            Compiler.InputPath = filePath.Aggregate((a, b) => a + "\\" + b);
        }
    }
}
