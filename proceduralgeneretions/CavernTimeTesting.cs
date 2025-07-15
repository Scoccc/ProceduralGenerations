using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;

public partial class CavernTimeTesting : Node2D
{
    [Export] public Array<int> recursionSteps;
    [Export] public Array<Vector2I> CaveSizes;
    [Export] public Array<float> density;
    [Export] public Array<int> max;
    [Export] public Array<int> min;

    [Export] public CaveGenerator generator;
    [Export] public int generations = 1;

    private Stopwatch timer = new Stopwatch();
    private string outputPath = "user://CaveTimings.csv";
    private string filePath;

    private int totalCombos;
    private int combosDone = 0;

    private int stepIndex = 0;
    private int sizeIndex = 0;
    private int densityIndex = 0;
    private int maxIndex = 0;
    private int minIndex = 0;

    private int genIndex = 0;
    private int rowNumber = 2;

    private List<double> currentTimings = new List<double>();

    public override void _Ready()
    {
        filePath = ProjectSettings.GlobalizePath(outputPath);

        if (generator == null
            || recursionSteps == null
            || CaveSizes == null
            || density == null
            || max == null
            || min == null
            || generations <= 0)
        {
            GD.PrintErr("Parametri non validi.");
            SetProcess(false);
            return;
        }

        totalCombos = recursionSteps.Count
                    * CaveSizes.Count
                    * density.Count
                    * max.Count
                    * min.Count;

        if (!File.Exists(filePath))
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            // Intestazione
            string header = "RecursionSteps;GroupSizeX;GroupSizeY;Density;MaxEmptyNeighbors;MinEmptyNeighbors";
            for (int i = 0; i < generations; i++)
                header += $";Gen{i + 1}";
            header += ";Average";
            writer.WriteLine(header);
        }

        GD.Print($"Inizio benchmark: {totalCombos} combinazioni, {generations} iterazioni ciascuna.");
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        if (stepIndex >= recursionSteps.Count)
        {
            GD.Print("Benchmark completato al 100%.");
            SetProcess(false);
            return;
        }

        // Imposta parametri correnti
        int iterations = recursionSteps[stepIndex];
        Vector2I size = CaveSizes[sizeIndex];
        float dens = density[densityIndex];
        int mx = max[maxIndex];
        int mn = min[minIndex];

        generator.iterations = iterations;
        generator.gridSize = size;
        generator.noiseDensity = dens;
        generator.maxEmptyNeighbors = mx;
        generator.minEmptyNeighbors = mn;

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
            string line = $"{iterations};{size.X};{size.Y};{dens.ToString("F3").Replace('.', ',')};{mx};{mn}";
            foreach (var ms in currentTimings)
                line += $";{ms.ToString("F3").Replace('.', ',')}";
            char colStart = 'G'; // G è la colonna di Gen1 (A=1,B=2,C=3,D=4,E=5,F=6)
            char colEnd = (char)(colStart + generations - 1);
            string avgFormula = $"=MEDIA({colStart}{rowNumber}:{colEnd}{rowNumber})";
            line += $";{avgFormula}";

            using var writer = new StreamWriter(filePath, true, Encoding.UTF8);
            writer.WriteLine(line);

            // Log progresso
            combosDone++;
            float percent = combosDone * 100f / totalCombos;
            GD.Print($"Combinazione {combosDone}/{totalCombos} → {percent:F1}%");

            // Reset stato per la prossima combo
            rowNumber++;
            currentTimings.Clear();
            genIndex = 0;

            // Avanzo indici nidificati: min → max → density → size → step
            minIndex++;
            if (minIndex >= min.Count)
            {
                minIndex = 0;
                maxIndex++;
                if (maxIndex >= max.Count)
                {
                    maxIndex = 0;
                    densityIndex++;
                    if (densityIndex >= density.Count)
                    {
                        densityIndex = 0;
                        sizeIndex++;
                        if (sizeIndex >= CaveSizes.Count)
                        {
                            sizeIndex = 0;
                            stepIndex++;
                        }
                    }
                }
            }
        }
    }
}
