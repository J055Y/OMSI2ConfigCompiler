using OMSI2_Tags;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Compiler_Project
{
    public class Compiler
    {
        public Compiler()
        {

        }

        public static string inputPath = "";
        public static string outputPath = "";
        public static List<string> AdditionalOCC = new List<string>();
        public static Dictionary<int, string> OCCs = new Dictionary<int, string>();
        public static OrderedDictionary TextTextures = new OrderedDictionary();

        public string CompileTextTextures(OrderedDictionary textTextures)
        {
            string outputString = "";

            object[] keys = new object[textTextures.Keys.Count];
            textTextures.Keys.CopyTo(keys, 0);
            for (int i = 0; i < textTextures.Keys.Count; i++)
            {
                outputString += "\n" + i;
                foreach (var prop in textTextures[keys[i]].GetType().GetProperties())
                {
                    if (prop.Name != "Tag")
                    {
                        outputString += prop.GetValue(textTextures[keys[i]]) + "\n";
                    }
                    else
                    {
                        outputString += "\n[" + prop.GetValue(textTextures[keys[i]]) + "]\n";
                    }
                }
            }

            return outputString;
        }

        public void Compile(ChildTag child, string compInputPath, string outputPath = @"C:\Temp\ConfigOut.cfg")
        {
            inputPath = compInputPath;
            var textTextureOutput = CompileTextTextures(TextTextures);
            var outputString = UnpackProperties(child);

            System.Threading.Thread.Sleep(1000);

            if (System.IO.File.Exists(outputPath))
            {
                System.IO.File.AppendAllText(outputPath, outputString);
            }
            else
            {
                System.IO.File.WriteAllText(outputPath, textTextureOutput + outputString);
            }
        }

        public string UnpackProperties(ChildTag child)
        {
            string outputString = "";
            foreach (var prop in child.GetType().GetProperties())
            {
                if (prop.PropertyType != typeof(List<ChildTag>))
                {
                    outputString += PrintProperty(prop, child);
                }
                else
                {
                    outputString += UnpackChildList(prop, child);
                }
            }

            return outputString;
        }

        public static List<string> GetComponents(string contents)
        {
            List<string> ParentTags = new List<string>() {
                "Mesh"
                // todo: expand
            };

            List<string> components = new List<string>();

            if (contents != "")
            {
                foreach (var parentTag in ParentTags)
                {
                    MatchCollection matches = Regex.Matches(contents, "var ([^\\s]*?) = new " + parentTag + "\\(");
                    foreach (Match match in matches)
                    {
                        foreach (Capture capture in match.Groups[1].Captures)
                        {
                            components.Add(capture.ToString());
                        }
                    }
                }
            }
            return components;
        }

        public static string GetCompileString(List<string> components, string output = "")
        {
            if (output == "")
            {
                output = outputPath;
            }
            string compileString = "Compiler comp = new Compiler();";
            foreach (var component in components)
            {
                compileString += "comp.Compile(" + component + ", @\"" + inputPath + "\", @\"" + output + "\");";
            }

            return compileString;
        }

        public string UnpackChildList(PropertyInfo prop, ChildTag child)
        {
            string output = "";
            var property = prop.GetValue(child);
            var propertyList = property as List<ChildTag>;
            foreach (var item in propertyList)
            {
                foreach (var itemProp in item.GetType().GetProperties())
                {
                    output += PrintProperty(itemProp, item);
                }
            }

            return output;
        }

        public string PrintProperty(PropertyInfo prop, ChildTag child)
        {
            string output = "";
            if (prop.GetValue(child) != null)
            {
                if (prop.GetValue(child).GetType() != typeof(List<ChildTag>))
                {
                    if (prop.GetValue(child).GetType() != typeof(bool))
                    {
                        if (prop.Name != "useTextTexture")
                        {
                            if (prop.Name != "Tag")
                            {
                                if (Char.IsUpper(prop.Name[0]) == false)
                                {
                                    output += (FormatNewline(FormatTag(prop.Name) + "\n" + prop.GetValue(child) + "\n"));
                                }
                                else
                                {
                                    output += (prop.GetValue(child) + "\n");
                                }
                            }
                            else
                            {
                                output += (FormatNewline(FormatTag(prop.GetValue(child).ToString()) + "\n"));
                            }
                        }
                        else
                        {
                            output += FormatTag(prop.Name) + "\n" + GetTextTextureID(prop.GetValue(child).ToString(), TextTextures) + "\n";
                        }
                    }
                    else
                    {
                        output += (FormatNewline(FormatTag(prop.Name) + "\n\n"));
                    }
                }
                else
                {
                    output += (UnpackChildList(prop, child));
                }
            }

            return output;
        }

        public int GetTextTextureID(string name, OrderedDictionary dict)
        {
            int output = 0;
            object[] keys = new object[dict.Keys.Count];
            dict.Keys.CopyTo(keys, 0);
            for (int i = 0; i < dict.Keys.Count; i++)
            {
                if (name == keys[i].ToString())
                {
                    output = i;
                }
            }
            return output;
        }

        public string FormatTag(string tag)
        {
            return "[" + tag + "]";
        }

        public string FormatNewline(string line)
        {
            string output = "";
            if (line[0] == char.Parse("["))
            {
                output = "\n" + line;
            }

            return output;
        }
    }
}
