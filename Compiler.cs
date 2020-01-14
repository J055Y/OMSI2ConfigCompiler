using OMSI2_Tags;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Compiler_Project
{
    public class Compiler
    {
        public Compiler()
        {

        }

        public static string inputPath = "";
        public static string outputPath = "";
        //public static string OCCCompileString = "";
        public static List<string> AdditionalOCC = new List<string>();
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

        public void Compile(ChildTag child, string inputPath, string outputPath = @"C:\Temp\ConfigOut.cfg")
        {
            var textTextureOutput = CompileTextTextures(TextTextures);
            var outputString = UnpackProperties(child);

            //Console.WriteLine(inputPath + "\\" + AdditionalOCC[0]);
            if (AdditionalOCC.Any())
            {
                //string compileString = "";
                foreach (var occ in AdditionalOCC)
                {
                    Console.WriteLine(inputPath + "\\" + occ);
                    var file = System.IO.File.ReadAllText(inputPath + "\\" + occ);
                    //Console.WriteLine(file);
                    var components = GetComponents(file);
                    /*OCCCompileString += GetCompileString(components, inputPath, outputPath);
                    Console.WriteLine(OCCCompileString);*/
                    //Console.WriteLine(components);
                }
            }

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
                "mesh"
                // todo: expand
            };

            List<string> components = new List<string>();
            using (StringReader reader = new StringReader(contents))
            {
                var s = "";

                while ((s = reader.ReadLine()) != null)
                {
                    var component = OMSI2_Tags.Methods.getBetween(s, "var ", " = new ");

                    if (!components.Contains(component) && component != "" && ParentTags.Contains(component))
                        components.Add(component);
                }
            }

            return components;
        }

        public static string GetCompileString(List<string> components, string input, string output = "")
        {
            if (output == "")
            {
                output = outputPath;
            }
            string compileString = "Compiler comp = new Compiler();";
            foreach (var component in components)
            {
                compileString += "comp.Compile(" + component + ", @\"" + input + "\", @\"" + output + "\");";
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
