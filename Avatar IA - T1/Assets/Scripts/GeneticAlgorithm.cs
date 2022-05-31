using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Character
{
    Aang,
    Zukko,
    Toph,
    Katara,
    Sokka,
    Appa,
    Momo
}

public class Solution
{
    private byte[] genotype;
    private int genSize;
    private float score;
    

    private Solution(byte[] gen, int size)
    {
        genotype = gen;
        score = 0.0f;
        genSize = size;
    }

    static public Solution buildRandomSolution(int size)
    {
        byte[] solution = new byte[size];
        for (int i = 0; i < size; i++)
            solution[i] = buildRandomByte();
        return new Solution(solution, size);
    }

    static private byte buildRandomByte()
    {
        byte a = 0x0;
        byte zero = 0x0;
        byte one = 0x1;
        for (int d = 0; d < 8; d++)
        {
            a = unchecked((byte)(a << 1));
            a += (Random.Range(0, 2) == 0) ? zero : one;
        }
        return a;
    }

    public void print()
    {
        string str = "";
        for (int i = 0; i < genSize; i++)
            str += genotype[i].ToString("X") + ", ";
        Debug.Log(str);
    }
}

public class GeneticAlgorithm
{
    // private byte[] genotype;
    Dictionary<Character, float> agility = new Dictionary<Character, float>{
        {Character.Aang, 1.8f},
        {Character.Zukko, 1.6f},
        {Character.Toph, 1.6f},
        {Character.Katara, 1.6f},
        {Character.Sokka, 1.4f},
        {Character.Appa, 0.9f},
        {Character.Momo, 0.7f},
    };
    private int populationSize;
    private float mutationRate;
    
    private int maxGenerations;

    private Solution[] currentGeneration;
    private int generationIndex;

    private int genotypeSize;
    public void startGenetic()
    {
        populationSize = 10;
        mutationRate = 0.1f;
        genotypeSize = MapManager.Instance.eventTiles.Count - 1;
        generationIndex = 0;
        maxGenerations = 100;
        currentGeneration = new Solution[populationSize];

        for (int i = 0; i < populationSize; i++)
        {
            currentGeneration[i] = Solution.buildRandomSolution(genotypeSize);
            currentGeneration[i].print();
        }
    }
}
