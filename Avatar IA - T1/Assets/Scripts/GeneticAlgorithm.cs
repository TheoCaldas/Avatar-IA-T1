using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    public byte[] genotype;
    private int genSize;
    public float score;
    
    private Solution(byte[] gen, int size)
    {
        genotype = gen;
        score = 0.0f;
        genSize = size;
    }

    public bool isValid()
    {
        byte a = 0x1;
        for (int d = 0; d < 7; d++)
        {
            int energyPoints = 8;
            for (int i = 0; i < genSize; i++)
            {
                if ((a & genotype[i]) != 0x0)
                    energyPoints--;
                
                if (energyPoints < 0)
                    return false;
            }
            a = unchecked((byte)(a << 1));
        }
        return true;
    }

    static public Solution buildPossibleRandomSolution(int size) //each character can be chosen 8 times max
    {
        int[] energyPoints = new int[8];

        for (int i = 0; i < 8; i++)
            energyPoints[i] = 8;

        byte[] solution = new byte[size];
        for (int i = 0; i < size; i++)
            solution[i] = buildPossibleRandomByte(energyPoints);
        return new Solution(solution, size);
    }

    static private byte buildPossibleRandomByte(int[] energyPoints)
    {
        byte a = 0x0;
        byte one = 0x1;
        for (int d = 0; d < 8; d++)
        {
            a = unchecked((byte)(a << 1));
            if (UnityEngine.Random.Range(0, 3) == 0 && energyPoints[d] > 0)
            {
                a += one;
                energyPoints[d]--;
            }
        }
        return a;
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
            a += (UnityEngine.Random.Range(0, 2) == 0) ? zero : one;
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
    Dictionary<Character, float> agility = new Dictionary<Character, float>{
        {Character.Aang, 1.8f},
        {Character.Zukko, 1.6f},
        {Character.Toph, 1.6f},
        {Character.Katara, 1.6f},
        {Character.Sokka, 1.4f},
        {Character.Appa, 0.9f},
        {Character.Momo, 0.7f},
    };

    Dictionary<Character, byte> digit = new Dictionary<Character, byte>{
        {Character.Aang, 0x1},
        {Character.Zukko, 0x1 << 1},
        {Character.Toph, 0x1 << 2},
        {Character.Katara, 0x1 << 3},
        {Character.Sokka, 0x1 << 4},
        {Character.Appa, 0x1 << 5},
        {Character.Momo, 0x1 << 6},
    };

    //Changeable parameters
    private int populationSize;
    private float mutationRate;
    private int maxGenerations;
    private int numberOfSelectedParents;

    //Other
    private int genotypeSize;
    private int generationIndex;
    private Solution[] currentGeneration;
    
    public void startGenetic()
    {
        //TO DO: Crossover
        //TO DO: Mutation
        //TO DO: Solutions Selection
        //TO DO: Save best solution
        populationSize = 100;
        mutationRate = 0.1f;
        maxGenerations = 100;
        numberOfSelectedParents = 30;

        genotypeSize = MapManager.Instance.eventTiles.Count - 1;
        generationIndex = 0;
        currentGeneration = new Solution[populationSize];

        for (int i = 0; i < populationSize; i++)
        {
            currentGeneration[i] = Solution.buildPossibleRandomSolution(genotypeSize);
            // currentGeneration[i].print();
            currentGeneration[i].score = fitness(currentGeneration[i]);
            // Debug.Log(currentGeneration[i].score);
        }
        Debug.Log("MELHORES:");
        Solution[] selectedParents = solutionSelection(numberOfSelectedParents);
        for (int i = 0; i < numberOfSelectedParents; i++)
           Debug.Log(selectedParents[i].score);
    }

    private Solution[] solutionSelection(int numberOfParents) //using rollette method
    {
        Array.Sort(currentGeneration, (a, b) => a.score.CompareTo(b.score));

        float[] probabilities = new float[populationSize];
        bool[] isSelected = new bool[populationSize];

        float scoreSum = 0.0f;
        for (int i = 0; i < populationSize; i++)
            scoreSum += currentGeneration[i].score;
        for (int i = 0; i < populationSize; i++)
        {
            isSelected[i] = false;
            probabilities[i] = 1.0f - (currentGeneration[i].score / scoreSum);
            if (i > 0)
                probabilities[i] += probabilities[i - 1];
        }
        
        Solution[] selectedParents = new Solution[numberOfParents];
        int j = 0;
        while (j < numberOfParents)
        {
            float r = UnityEngine.Random.Range(0.0f, 1.0f);
            for (int i = 0; i < populationSize; i++)
            {
                if (probabilities[i] >= r && !isSelected[i])
                {
                    selectedParents[j] = currentGeneration[i];
                    isSelected[i] = true;
                    j++;
                    break;
                }
            }
        }
        return selectedParents;
    }

    public float fitness(Solution solution)
    {
        byte[] genotype = solution.genotype;
        float score = 0.0f;
        for (int i = 0; i < genotypeSize; i++)
        {
            int cost = MapManager.Instance.eventTiles[i + 1].timeCost;
            float sum = 0.0f;
            foreach (Character character in Enum.GetValues(typeof(Character)))  
            {  
                if ((genotype[i] & digit[character]) != 0x0)
                    sum += agility[character]; 
            }  
            float geneScore = (float) cost / ((sum > 0.0f) ? sum : 1.0f);
            // Debug.Log("timecost = " + cost + " sum = " + sum + " gene score = " + geneScore);
            score += geneScore;
        }
        return score;
    }
}
