using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public enum Character
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

    public float geneEmptyError()
    {
        int emptyCount = 0;
        for (int i = 0; i < genSize; i++)
        {
            if (genotype[i] == 0x0 || genotype[i] == 0x80)
                emptyCount++;
        }
        return (float) emptyCount;
    }

    public float validationError()
    {
        int[] r = residual();

        float error = 0.0f;
        for (int d = 0; d < 7; d++)
            error += (float) (r[d] * r[d]);
        return error;
    }

    public int[] residual()
    {
        byte a = 0x1;
        int[] residual = new int[7];

        for (int d = 0; d < 7; d++)
        {
            int energyPoints = 8;
            if (d == 6)
                energyPoints = 7;
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

    static public Solution buildValidRandomSolution(int size) //each character can be chosen 8 times max and every byte > 0x0
    {
        List<(int, int)> energyPoints = new List<(int, int)>();
        for (int j = 0; j < 7; j++)
        {
            if (j == 6)
                energyPoints.Add((j, 7));
            else
                energyPoints.Add((j, 8));
        }

        byte[] genotype = new byte[size];
        for (int j = 0; j < size; j++)
            genotype[j] = 0x0;
        byte a = 0x1;
        byte shift;

        int i = size - 1;
        int pointSum = 55;
        while (pointSum > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, energyPoints.Count);
            (int digit, int points) = energyPoints[randomIndex];
            shift = unchecked((byte)(a << digit));

            genotype[i] += shift;
            pointSum--;
            if (points - 1 > 0)
                energyPoints[randomIndex] = (digit, points - 1);
            else
                energyPoints.RemoveAt(randomIndex);
            i--;
            if (i < 0)
                i += size;

        }
        return new Solution(genotype, size);
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
        int[] r = residual();
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
    private string bestSolutionFile = "Assets/Resources/bestSolution1.txt";
    private float initialTime;
    private float maxTime;

    public GeneticAlgorithm(int eventCount)
    {
        populationSize = 5000; //5000
        mutationRate = 0.2f; //0.2
        maxGenerations = 100; //100
        nParents = 101; //101
        validationFactor = 70.0f; //70
        emptyFactor = 40.0f; //40
        maxTime = 60.0f; //60

        genotypeSize = eventCount - 1;
        generationIndex = 0;
        currentGeneration = new Solution[populationSize];
    }
    
    public void startGenetic()
    {
        fistGeneration();
        bestSolution.printGenotype();
        Debug.Log("Valid Error: " + bestSolution.validationError() + ", Empty Error: " + bestSolution.geneEmptyError());
        bestSolution.printResidual();

        initialTime = Time.realtimeSinceStartup;
        generationsLoop();
        bestSolution.printGenotype();
        Debug.Log("Valid Error: " + bestSolution.validationError() + ", Empty Error: " + bestSolution.geneEmptyError());
        bestSolution.printResidual();

        saveBestSolution();
    }

    //GENERATIONS
    private void fistGeneration()
    {
        generationIndex++;
        for (int i = 0; i < populationSize; i++)
        {
            currentGeneration[i] = Solution.buildValidRandomSolution(genotypeSize);
            currentGeneration[i].score = fitness(currentGeneration[i]);
        }

        sortCurrentGenerationByScore();
        bestSolution = currentGeneration[0];
        Debug.Log("Generation: " + generationIndex + ", Best Solution Score: " + bestSolution.score.ToString("f6"));
    }

    private void generationsLoop()
    {
        Solution[] selectedParents;
        int genEqualsLimit = 10;
        int genEqualsCount = 0;
        // while (generationIndex < maxGenerations)
        while (Time.realtimeSinceStartup - initialTime <= maxTime)
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

    //SHARE SAVED RESULT
    public List<(float, List<Character>)> getResults()
    {
        Solution savedSolution = getSavedSolution();
        // Debug.Log("Score = " + savedSolution.score + ", isValid = " + savedSolution.isValid() + ", Empty Error = " + savedSolution.geneEmptyError());
        // savedSolution.printGenotype();
        List<(float, List<Character>)> results = new List<(float, List<Character>)>();
        for (int i = 0; i < genotypeSize; i++)
        {
            int cost = MapManager.Instance.eventTiles[i + 1].timeCost;
            List<Character> characters = new List<Character>();
            float sum = 0.0f;
            foreach (Character character in Enum.GetValues(typeof(Character)))  
            {  
                if ((savedSolution.genotype[i] & digit[character]) != 0x0)
                {
                    sum += agility[character]; 
                    characters.Add(character);
                }
            }  
            float geneScore = (float) cost / ((sum > 0.0f) ? sum : 1.0f);
            results.Add((geneScore, characters));
        }
        return results;
    }

    //SAVE RESULTS
    private Solution getSavedSolution()
    {
        byte [] savedGenotype = File.ReadAllBytes(bestSolutionFile);
        Solution savedSolution = new Solution(savedGenotype, genotypeSize);
        savedSolution.score = fitness(savedSolution);
        return savedSolution;
    }

    private void saveBestSolution()
    {
        Solution previousBest = getSavedSolution();

        Debug.Log("----Previous best-----");
        Debug.Log("Score = " + previousBest.score + "Valid Error: " + previousBest.validationError() + ", Empty Error = " + previousBest.geneEmptyError());

        if (bestSolution.score < previousBest.score && bestSolution.validationError() == 0.0f && bestSolution.geneEmptyError() == 0)
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
            Solution random = Solution.buildValidRandomSolution(genotypeSize);
            currentGeneration[i] = random;
            currentGeneration[i].score = fitness(currentGeneration[i]);
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

    private void swap(byte[] genotype, int i, int j)
    {
        byte temp = genotype[i];
        genotype[i] = genotype[j];
        genotype[j] = temp;
    }

    private (Solution, Solution) createChildrenBySwap (Solution parent1, Solution parent2) //genes are swapped randomly
    {
        byte[] genotypeChild1 = new byte[genotypeSize];
        byte[] genotypeChild2 = new byte[genotypeSize];

        for (int i = 0; i < genotypeSize; i++)
        {
            genotypeChild1[i] = parent1.genotype[i];
            genotypeChild2[i] = parent2.genotype[i];
        }

        int random1 = 0;
        int random2 = 0;
        while (random1 == random2)
        {
            random1 = UnityEngine.Random.Range(0, genotypeSize);
            random2 = UnityEngine.Random.Range(0, genotypeSize);
        }        

        swap(genotypeChild1, random1, random2);
        swap(genotypeChild2, random1, random2);

        Solution child1 = new Solution(genotypeChild1, genotypeSize);
        Solution child2 = new Solution(genotypeChild2, genotypeSize);
        return (child1, child2);
    }

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
            if (Time.realtimeSinceStartup - initialTime > maxTime / 2.0f)
                (child1, child2) = createChildrenBySwap(parents[randomIndex1], parents[randomIndex2]);
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
    }
}
