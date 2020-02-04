using OMSI2_Tags;
using System;
using System.Windows.Forms;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Compiler_Project
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.button1.DragDrop += new
                System.Windows.Forms.DragEventHandler(this.button1_DragDrop);
            this.button1.DragEnter += new
                System.Windows.Forms.DragEventHandler(button1_DragEnter);
        }

        SaveFileDialog saveOCC = new SaveFileDialog();
        OpenFileDialog openOCC = new OpenFileDialog();
        SaveFileDialog saveCFG = new SaveFileDialog();

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            openOCC.Filter = "OCC Files|*.occ";
            openOCC.ShowDialog();

            if (openOCC.FileName != "")
            {
                GenerateInputPath();
            }
            BuildConfig();
        }

        private void button1_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void button1_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            openOCC.FileName = s[0];
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
            var fileContents = Minify(openOCC.FileName);
            int i = 0;
            foreach (var thing in fileContents.Split(";".ToCharArray()))
            {
                if (thing.IndexOf("Compiler.AdditionalOCC.Add") > -1 && !Compiler.OCCs.ContainsKey(i))
                {
                    Compiler.OCCs.Add(i, OMSI2_Tags.Methods.getBetween(thing, "(\"", "\")"));
                }
                else
                {
                    i++;
                }
            }
            foreach (KeyValuePair<int, string> kvp in Compiler.OCCs)
            {
                i = 0;
                var occContent = Minify(Compiler.inputPath + "\\" + kvp.Value);
                foreach (var thing in fileContents.Split(";".ToCharArray()))
                {
                    if (kvp.Key == i)
                    {
                        fileContents = fileContents.Replace(thing+";", occContent);
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
            if (Compiler.inputPath != "")
            {
                Console.WriteLine(Compiler.inputPath);
            }
            else
            {
                Console.WriteLine("input path is null!");
            }
            var mesh = new Mesh("");
            textBox2.Text = "";

            CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp");
            string Output = "Out.exe";
            var parameters = InitParameters();
            parameters.OutputAssembly = Output;

            var contents = GetContents();
            var configComponents = Compiler.GetComponents(contents);

            saveCFG.Filter = "Model Config Files|*.cfg";
            saveCFG.ShowDialog();

            if (saveCFG.FileName != "")
            {
                var filename = saveCFG.FileName.Replace(@"\", @"\\");
                Compiler.outputPath = filename;
                var compileString = Compiler.GetCompileString(configComponents, filename);
                string inputText = "using System; using OMSI2_Tags; namespace Compiler_Project { class CompilerClass { static void Main(string[] args) {" + contents + compileString + "}}}";
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
        }

        private static CompilerParameters InitParameters()
        {
            var parameters = new CompilerParameters {GenerateExecutable = true};
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
            var filePath = openOCC.FileName.Split('\\').ToList();
            filePath.RemoveAt(filePath.Count - 1);
            Compiler.inputPath = filePath.Aggregate((a, b) => a + "\\" + b);
        }
    }
}
