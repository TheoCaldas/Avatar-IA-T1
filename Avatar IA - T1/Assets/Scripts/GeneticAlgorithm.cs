using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

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
    
    public Solution(byte[] gen, int size)
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

    public float geneEmptyError()
    {
        int emptyCount = 0;
        for (int i = 0; i < genSize; i++)
        {
            if (genotype[i] == 0x0)
                emptyCount++;
        }
        return (float) emptyCount;
    }

    public float validationError()
    {
        float[] r = residual();

        float error = 0.0f;
        for (int d = 0; d < 7; d++)
            error += r[d] * r[d];
        return error;//Mathf.Sqrt(error);
    }

    public float[] residual()
    {
        byte a = 0x1;
        float[] residual = new float[7];

        for (int d = 0; d < 7; d++)
        {
            int energyPoints = 8;
            for (int i = 0; i < genSize; i++)
            {
                if ((a & genotype[i]) != 0x0)
                    energyPoints--;
            }
            a = unchecked((byte)(a << 1));
            residual[d] = energyPoints;
        }
        return residual;
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
            if (UnityEngine.Random.Range(0, 10) == 0 && energyPoints[d] > 0)
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

    public void printGenotype()
    {
        string str = "";
        for (int i = 0; i < genSize; i++)
            str += genotype[i].ToString("X") + ", ";
        Debug.Log(str);
    }

    public void printResidual()
    {
        float[] r = residual();
        string str = "";
        for (int i = 0; i < 7; i++)
            str += r[i].ToString() + ", ";
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
    private int nParents;
    private float validationFactor;
    private float emptyFactor;

    //Other
    private int genotypeSize;
    private int generationIndex;
    private Solution[] currentGeneration;
    private Solution bestSolution;
    private string bestSolutionFile = "Assets/Resources/bestSolution.txt";
    
    public void startGenetic()
    {
        populationSize = 500; //500
        mutationRate = 0.15f; //0.15
        maxGenerations = 50; //50
        nParents = 30; //30
        validationFactor = 50.0f; //50
        emptyFactor = 40.0f; //40

        genotypeSize = MapManager.Instance.eventTiles.Count - 1;
        generationIndex = 0;
        currentGeneration = new Solution[populationSize];
        
        fistGeneration();
        bestSolution.printGenotype();
        Debug.Log("Valid Error: " + bestSolution.validationError());
        Debug.Log("Empty Error: " + bestSolution.geneEmptyError());
        Debug.Log("Is Valid: " + bestSolution.isValid());
        bestSolution.printResidual();

        generationsLoop();
        bestSolution.printGenotype();
        Debug.Log("Valid Error: " + bestSolution.validationError());
        Debug.Log("Empty Error: " + bestSolution.geneEmptyError());
        Debug.Log("Is Valid: " + bestSolution.isValid());
        bestSolution.printResidual();

        saveBestSolution();
    }

    //GENERATIONS
    private void fistGeneration()
    {
        generationIndex++;
        for (int i = 0; i < populationSize; i++)
        {
            currentGeneration[i] = Solution.buildPossibleRandomSolution(genotypeSize);
            currentGeneration[i].score = fitness(currentGeneration[i]);
        }

        sortCurrentGenerationByScore();
        bestSolution = currentGeneration[0];
        Debug.Log("Generation: " + generationIndex + ", Best Solution Score: " + bestSolution.score.ToString("f6"));
    }

    private void generationsLoop()
    {
        Solution[] selectedParents;
        int genEqualsLimit = 5;
        int genEqualsCount = 0;
        while (generationIndex < maxGenerations)
        {
            generationIndex++;

            selectedParents = solutionSelection(currentGeneration, nParents);
            currentGeneration = crossOver(selectedParents, nParents, populationSize);
            for(int i = 0; i < populationSize; i++)
            {
                mutate(currentGeneration[i], mutationRate);
                currentGeneration[i].score = fitness(currentGeneration[i]);
            }
            
            sortCurrentGenerationByScore();
            if (currentGeneration[0].score < bestSolution.score)
            {
                bestSolution = currentGeneration[0];
                genEqualsCount = 0;
            }
            else if (currentGeneration[0].score == bestSolution.score)
                genEqualsCount++;

            if (genEqualsCount >= genEqualsLimit)
            {
                mixCurrentGenWithRandomSolutions();
                sortCurrentGenerationByScore();
            }
        
            Debug.Log("Generation: " + generationIndex + ", Best Solution Score: " + bestSolution.score.ToString("f6"));
        }
    }

    //SAVE RESULTS
    private void saveBestSolution()
    {
        byte [] previousBestGenotype = File.ReadAllBytes(bestSolutionFile);
        Solution previousBest = new Solution(previousBestGenotype, genotypeSize);
        previousBest.score = fitness(previousBest);

        Debug.Log("----Previous best-----");
        Debug.Log("Score = " + previousBest.score + ", isValid = " + previousBest.isValid() + ", Empty Error = " + previousBest.geneEmptyError());

        if (bestSolution.score < previousBest.score && bestSolution.isValid() && bestSolution.geneEmptyError() == 0)
        {
            Debug.Log("NEW BEST!");
            File.WriteAllBytes(bestSolutionFile, bestSolution.genotype);
        }
    }

    //CREATIONISM
    private void mixCurrentGenWithRandomSolutions()
    {   
        for(int i = 0; i < populationSize; i++)
        {
            if (i % 2 == 0)
            {
                Solution random = Solution.buildPossibleRandomSolution(genotypeSize);
                currentGeneration[i] = random;
                currentGeneration[i].score = fitness(currentGeneration[i]);
            }
        }
    }

    //FITNESS
    private float fitness(Solution solution)
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

        return (score) + (solution.validationError() * validationFactor) + (solution.geneEmptyError() * emptyFactor); //* ((float) generationIndex));
    }

    //PARENTS SELECTION
    private Solution[] solutionSelection(Solution[] currentGen, int nParents) //using rollette method
    {
        float[] probabilities = new float[populationSize];
        bool[] isSelected = new bool[populationSize];

        float scoreSum = 0.0f;
        for (int i = 0; i < populationSize; i++)
            scoreSum += currentGen[i].score;
        for (int i = 0; i < populationSize; i++)
        {
            isSelected[i] = false;
            probabilities[i] = 1.0f - (currentGen[i].score / scoreSum);
            if (i > 0)
                probabilities[i] += probabilities[i - 1];
        }
        
        Solution[] selectedParents = new Solution[nParents];
        int j = 0;
        while (j < nParents)
        {
            float r = UnityEngine.Random.Range(0.0f, 1.0f);
            for (int i = 0; i < populationSize; i++)
            {
                if (probabilities[i] >= r && !isSelected[i])
                {
                    selectedParents[j] = currentGen[i];
                    isSelected[i] = true;
                    j++;
                    break;
                }
            }
        }
        return selectedParents;
    }

    private void sortCurrentGenerationByScore()
    {
        Array.Sort(currentGeneration, (a, b) => a.score.CompareTo(b.score));
    }
    
    //CREATE CHILDREN
    private (Solution, Solution) createChildrenRandomGenes (Solution parent1, Solution parent2) //genes are swapped randomly
    {
        byte[] genotypeChild1 = new byte[genotypeSize];
        byte[] genotypeChild2 = new byte[genotypeSize];

        for (int i = 0; i < genotypeSize; i++)
        {
            genotypeChild1[i] = parent1.genotype[i];
            genotypeChild2[i] = parent2.genotype[i];
        }

        int nChanges = UnityEngine.Random.Range(0, genotypeSize);
        int[] indexes = new int[nChanges];
        for (int i = 0; i < nChanges; i++)
            indexes[i] = UnityEngine.Random.Range(0, genotypeSize);

        foreach(int i in indexes)
        {
            genotypeChild1[i] = parent2.genotype[i];
            genotypeChild2[i] = parent1.genotype[i];
        }

        Solution child1 = new Solution(genotypeChild1, genotypeSize);
        Solution child2 = new Solution(genotypeChild2, genotypeSize);
        return (child1, child2);
    }

    private (Solution, Solution) createChildrenAllGenes (Solution parent1, Solution parent2) //genes are swapped consecutively
    {
        byte[] genotypeChild1 = new byte[genotypeSize];
        byte[] genotypeChild2 = new byte[genotypeSize];

        for(int i = 0; i < genotypeSize; i++)
        {
            genotypeChild1[i] = (i % 2 == 0) ? parent1.genotype[i]: parent2.genotype[i];
            genotypeChild2[i] = (i % 2 == 0) ? parent2.genotype[i]: parent1.genotype[i];
        }

        Solution child1 = new Solution(genotypeChild1, genotypeSize);
        Solution child2 = new Solution(genotypeChild2, genotypeSize);
        return (child1, child2);
    }

    //CROSSOVER
    private bool isPairChosen(Dictionary<int, List<int>> chosenPairs, int index1, int index2)
    {
        foreach(int index in chosenPairs[index1])
            if (index == index2) return true;
        foreach(int index in chosenPairs[index2])
            if (index == index1) return true;
        return false;
    }

    private Solution[] crossOver(Solution[] parents, int nParents, int nChildren) 
    {
        Solution[] children = new Solution[nChildren];
        for (int i = 0; i < nParents; i++)
            children[i] = parents[i];
        
        int childrenCount = nParents;
        Dictionary<int, List<int>> chosenPairs = new Dictionary<int, List<int>>();
        for (int i = 0; i < nParents; i++)
            chosenPairs[i] = new List<int>();

        while(childrenCount < nChildren)
        {
            int randomIndex1 = UnityEngine.Random.Range(0, nParents);
            int randomIndex2 = UnityEngine.Random.Range(0, nParents);
            if (randomIndex1 == randomIndex2) continue;
            if (isPairChosen(chosenPairs, randomIndex1, randomIndex2)) continue;
            chosenPairs[randomIndex1].Add(randomIndex2);
            (Solution child1, Solution child2) = createChildrenRandomGenes(parents[randomIndex1], parents[randomIndex2]);
            children[childrenCount] = child1;
            childrenCount++;
            if (childrenCount < nChildren)
            {
                children[childrenCount] = child2;
                childrenCount++;
            }
        }
        return children;
    }

    //MUTATION
    private (bool, byte) invertSignal(byte gen, byte mask)
    {
        if((mask & gen) != 0x0) //if is on
        {
            gen &= (byte) ~mask;
            return (true, gen);
        }
        gen |= mask;
        return (false, gen);
    }

    private void mutate(Solution solution, float mutationRate)
    {
        float randomFactor = UnityEngine.Random.Range(0.0f, 1.0f); 
        if (randomFactor > mutationRate) return;

        int randomGene = UnityEngine.Random.Range(0, genotypeSize);
        int randomShift = UnityEngine.Random.Range(0, 7);
        byte d = (byte) (0x1 << randomShift);

        (bool isOn, byte changedGene) = invertSignal(solution.genotype[randomGene], d);
        solution.genotype[randomGene] = changedGene;

        // for (int i = 0; i < genotypeSize; i++) //does not change solution validation
        // {
        //     if (i != randomGene)
        //     {
        //         if (isOn)
        //         {
        //             if ((d & solution.genotype[i]) == 0x0)
        //                 (isOn, solution.genotype[i]) = invertSignal(solution.genotype[randomGene], d);
        //         }
        //         else
        //         {
        //             if ((d & solution.genotype[i]) != 0x0)
        //                 (isOn, solution.genotype[i]) = invertSignal(solution.genotype[randomGene], d);
        //         }
        //     }
        // }
    }
}
