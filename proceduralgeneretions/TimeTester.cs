using Godot;
using Godot.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;

public partial class TimeTester : Node2D
{
    [Export] public Array<int> recursionSteps;
    [Export] public Array<Vector2I> groupSizes;
    [Export] public PrisonGenerator generator;
    [Export] public int generations = 1;

    private Stopwatch timer = new Stopwatch();
    private string outputPath = "user://timings.csv"; // percorso relativo nella sandbox di Godot

    public override void _Ready()
    {
        var filePath = ProjectSettings.GlobalizePath(outputPath);
        using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
        {
            // Intestazione CSV
            string intestazione = "RecursionSteps;GroupSizeX;GroupSizeY" ;
            for (int i = 0; i < generations; i++)
            {
                intestazione += $";Milliseconds[{i+1}]";
            }
            intestazione += ";Milliseconds Average";

            writer.WriteLine(intestazione);

            foreach (var recursion in recursionSteps)
            {
                foreach (Vector2I size in groupSizes)
                {
                    generator.RecursionSteps = recursion;
                    generator.groupGenerator.Size = size;

                    string str = $"{recursion};{size.X};{size.Y}";
                    double avg = 0;

                    for (int i = 0; i < generations; i++)
                    {
                        timer.Reset();
                        timer.Restart();
                        generator.generate();
                        timer.Stop();
                        avg += timer.Elapsed.TotalMilliseconds;
                        str += $";{timer.Elapsed.TotalMilliseconds}";
                    }

                    str += $";{avg / generations}";

                    writer.WriteLine(str);
                }
            }
        }

        GD.Print("Risultati salvati in: ", filePath);
    }
}
