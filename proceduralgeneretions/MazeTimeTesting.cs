using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;

public partial class MazeTimeTesting : Node2D
{
    [Export] public int firstSize = 1;
    [Export] public int numberOfSizes = 1;
    [Export] public int sizesStep = 1;
    [Export] public int generations = 1;
    [Export] public MazeGenerator generator;

    private Stopwatch timer = new Stopwatch();
    private string outputPath = "user://MazeTimings.csv";
    private string filePath;

    private int index = 0;

    private int genIndex = 0;
    private int rowNumber = 2;
    private Vector2I currentSize;

    private List<double> currentTimings = new List<double>();

    public override void _Ready()
    {
        filePath = ProjectSettings.GlobalizePath(outputPath);

        if (generator == null
            || generations <= 0)
        {
            GD.PrintErr("Parametri non validi.");
            SetProcess(false);
            return;
        }

        if (!File.Exists(filePath))
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            // Intestazione
            string header = "MazeSize";
            for (int i = 0; i < generations; i++)
                header += $";Gen{i + 1}";
            header += ";Average";
            writer.WriteLine(header);
        }
        currentSize = new Vector2I(firstSize, firstSize);
        GD.Print($"Inizio benchmark: {numberOfSizes} dimensioni, {generations} iterazioni ciascuna.");
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        if (index >= numberOfSizes)
        {
            GD.Print("Benchmark completato al 100%.");
            SetProcess(false);
            return;
        }

        generator.MazeSize = currentSize;

        // Misura una singola generazione
        timer.Restart();
        generator.generate();
        timer.Stop();

        currentTimings.Add(timer.Elapsed.TotalMilliseconds);
        genIndex++;

        // Quando raggiungo 'generations', scrivo la riga e avanzo gli indici
        if (genIndex >= generations)
        {
            // Costruisco la riga CSV
            string line = $"{currentSize.X}";
            foreach (var ms in currentTimings)
                line += $";{ms.ToString("F3").Replace('.', ',')}";
            char colStart = 'B';
            char colEnd = (char)(colStart + generations - 1);
            string avgFormula = $"=MEDIA({colStart}{rowNumber}:{colEnd}{rowNumber})";
            line += $";{avgFormula}";

            using var writer = new StreamWriter(filePath, true, Encoding.UTF8);
            writer.WriteLine(line);

            GD.Print($"Combinazione {index}/{numberOfSizes}");

            // Reset stato per la prossima combo
            rowNumber++;
            currentTimings.Clear();
            genIndex = 0;

            currentSize = new Vector2I(currentSize.X + sizesStep, currentSize.Y + sizesStep) ;
            index++;
        }
    }
}
