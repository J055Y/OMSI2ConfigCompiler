using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OMSI2_Tags;

namespace Compiler_Project
{
    class TestCompile
    {
        public TestCompile()
        {

        }

        public static void CompileMe()
        {
            Compiler.TextTextures.Add("testTextTexture", new TextTexture("textTextureVariable", "theFontName", 20, 30, 1, 123, 87, 21));
            Compiler.TextTextures.Add("testTextTexture2", new TextTexture("textTextureVariable2", "theFontName2", 25, 40, 1, 233, 24, 164));

            var mesh = new Mesh("path/to/mesh.o3d");
            mesh.mouseevent = "aMouseEvent";
            var illumination = new Mesh.IlluminationInterior(1, 1, 1, 1);
            var visible = new Mesh.Visible("aVisibleVariable", 0);
            mesh.addChild(illumination);
            mesh.addChild(visible);

            var material = new Material("path/to/texture", 1);
            material.useTextTexture = "testTextTexture";
            material.matl_alpha = 2;
            var lightmap = new Material.LightMap("path/to/lightmapTexture", "LightMap_Variable");
            var envomap = new Material.EnvironmentMap("path/to/envomapTexture", 1.036);
            var change = new Material.Change("path/to/texture", 3, "Change_Variable");
            material.addChild(lightmap);
            material.addChild(envomap);
            material.addChild(change);

            var material2 = new Material("path/to/another/texture", 2);
            material2.useTextTexture = "testTextTexture2";
            material2.matl_alpha = 1;
            var lightmap2 = new Material.LightMap("path/to/lmap2", "Lightmap2_Variable");
            var envomap2 = new Material.EnvironmentMap("path/to/envo2", 1.356);
            var change2 = new Material.Change("path/to/other/texture", 7, "Change Variable");
            material2.addChild(lightmap2);
            material2.addChild(envomap2);
            material2.addChild(change2);

            mesh.addChild(material);
            mesh.addChild(material2);

            Compiler.AdditionalOCC.Add("Controller.occ");

            Compiler comp = new Compiler();
            comp.Compile(mesh, @"C:\Users\Administrator\Desktop\test.txt");
        }
    }
}
