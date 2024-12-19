class Common
{
    public static int Capacity = 250;
    public static int NumberOfItems = 100;
}

class Item
{
    public int Value { get; set; }
    public int Weight { get; set; }
}

class Individual
{
    public bool[] Genes { get; set; }
    public int Quality { get; set; }

    public Individual(int numberOfItems)
    {
        Genes = new bool[numberOfItems];
    }

    public Individual(bool[] items)
    {
        Genes = new bool[items.Length];

        for (int k = 0; k < items.Length; k++)
        {
            Genes[k] = items[k];
        }
    }
}

class GenMethod
{
    private int PopulationSize { get; set; }
    private int Generations { get; set; }
    private double CrossoverRate { get; set; }
    private double MutationRate { get; set; }

    private Random random = new Random();

    public GenMethod(int popSize, int gens, double crossRate, double mutRate)
    {
        PopulationSize = popSize;
        Generations = gens;
        CrossoverRate = crossRate;
        MutationRate = mutRate;
    }

    public List<Individual> InitializePopulation(List<Item> items)
    {
        var population = new List<Individual>();

        for (int i = 0; i < PopulationSize; i++)
        {
            var individual = new Individual(Common.NumberOfItems);
            individual.Genes[random.Next(Common.NumberOfItems)] = true;
            individual.Quality = CalculateQuality(individual, items);
            population.Add(individual);
        }

        return population;
    }

    private int CalculateQuality(Individual individual, List<Item> items)
    {
        int totalValue = 0;

        for (int i = 0; i < Common.NumberOfItems; i++)
        {
            if (individual.Genes[i])
            {
                totalValue += items[i].Value;
            }
        }

        return totalValue;
    }


    public bool IsDead(Individual individual, List<Item> items)
    {
        int totalWeight = 0;

        for (int i = 0; i < individual.Genes.Length; i++)
        {
            if (individual.Genes[i])
            {
                totalWeight += items[i].Weight;
            }
        }

        return totalWeight > Common.Capacity;
    }

    public void Crossover(Individual parent1, Individual parent2, out Individual offspring, List<Item> items)
    {
        offspring = new Individual(Common.NumberOfItems);

        int point1 = random.Next(Common.NumberOfItems);
        int point2 = random.Next(point1, Common.NumberOfItems);
        int point3 = random.Next(point2, Common.NumberOfItems);

        bool swapSegment1 = random.Next(2) == 0;
        bool swapSegment2 = random.Next(2) == 0;
        bool swapSegment3 = random.Next(2) == 0;
        bool swapSegment4 = random.Next(2) == 0;

        int count = 0;

        do
        {
            for (int i = 0; i < Common.NumberOfItems; i++)
            {
                if (i < point1)
                {
                    offspring.Genes[i] = swapSegment1 ? parent2.Genes[i] : parent1.Genes[i];
                }
                else if (i >= point1 && i < point2)
                {
                    offspring.Genes[i] = swapSegment2 ? parent2.Genes[i] : parent1.Genes[i];
                }
                else if (i >= point2 && i < point3)
                {
                    offspring.Genes[i] = swapSegment3 ? parent2.Genes[i] : parent1.Genes[i];
                }
                else
                {
                    offspring.Genes[i] = swapSegment4 ? parent2.Genes[i] : parent1.Genes[i];
                }
            }
            count++;
        } while (IsDead(offspring, items) && count < 4);


        //Console.WriteLine("CROSSOVER");
        //Console.WriteLine(string.Join(", ", offspring.Genes.Select(x => x ? 1 : 0)));
        offspring.Quality = CalculateQuality(offspring, items);

        if (IsDead(offspring, items) || offspring.Quality == 0)
        {
            offspring.Quality = 0;
        }
    }

    public void Mutate(Individual individual, List<Item> items)
    {
        if (random.NextDouble() < MutationRate)
        {
            Individual tempInd;

            tempInd = new Individual(individual.Genes);
            int geneToMutate = random.Next(Common.NumberOfItems);
            tempInd.Genes[geneToMutate] = !tempInd.Genes[geneToMutate];

            individual.Quality = CalculateQuality(individual, items);

            if (IsDead(tempInd, items))
            {
                Array.Copy(tempInd.Genes, individual.Genes, individual.Genes.Length);
            }
        }
    }

    public void LocalImprovement(Individual individual, List<Item> items)
    {
        int totalWeight = individual.Genes.Select((gene, index) => gene ? items[index].Weight : 0).Sum();
        int totalValue = individual.Genes.Select((gene, index) => gene ? items[index].Value : 0).Sum();

        for (int i = 0; i < items.Count; i++)
        {
            if (!individual.Genes[i])
            {
                if (totalWeight + items[i].Weight <= Common.Capacity)
                {
                    individual.Genes[i] = true;
                    totalWeight += items[i].Weight;
                    totalValue += items[i].Value;
                }
            }
            else
            {
                int bestItemIndex = i;
                double bestItemValuePerWeight = (double)items[i].Value / items[i].Weight;

                int weightWithoutCurrentItem = totalWeight - items[i].Weight;

                for (int j = 0; j < items.Count; j++)
                {
                    if (!individual.Genes[j] && weightWithoutCurrentItem + items[j].Weight <= Common.Capacity)
                    {
                        double valuePerWeight = (double)items[j].Value / items[j].Weight;

                        if (valuePerWeight > bestItemValuePerWeight)
                        {
                            bestItemValuePerWeight = valuePerWeight;
                            bestItemIndex = j;
                        }
                    }
                }

                if (bestItemIndex != i)
                {
                    individual.Genes[i] = false;
                    totalWeight -= items[i].Weight;
                    totalValue -= items[i].Value;

                    individual.Genes[bestItemIndex] = true;
                    totalWeight += items[bestItemIndex].Weight;
                    totalValue += items[bestItemIndex].Value;
                }
            }

            individual.Quality = totalValue;
        }
    }

    public Individual RouletteSelection(List<Individual> population)
    {
        int totalQuality = population.Sum(ind => ind.Quality);

        var probabilities = population.Select(ind => (double)ind.Quality / totalQuality).ToList();

        double randomValue = random.NextDouble();

        double cumulativeProbability = 0.0;
        for (int i = 0; i < population.Count; i++)
        {
            cumulativeProbability += probabilities[i];
            if (randomValue < cumulativeProbability)
            {
                return population[i];
            }
        }
        return population.Last();
    }

    public void SolveProblem(ref List<Individual> population, List<Item> items)
    {
        foreach (var individual in population)
        {
            individual.Quality = CalculateQuality(individual, items);
        }

        for (int generation = 1; generation <= Generations; generation++)
        {
            Individual parent1 = RouletteSelection(population);
            Individual parent2 = RouletteSelection(population);

            //Individual parent1 = population[random.Next(PopulationSize)];
            //Individual parent2 = population[random.Next(PopulationSize)];

            //Individual parent1 = TournamentSelection(population, 5);
            //Individual parent2 = TournamentSelection(population, 5);

            //int numberRandItems = 10;
            //Individual[] tempBestInd = new Individual[numberRandItems];
            //for (int j = 0; j < numberRandItems; j++)
            //{
            //    tempBestInd[j] = population.OrderByDescending(ind => ind.Quality).ToArray()[j];
            //}

            //Individual parent1 = population[random.Next(PopulationSize)];
            //Individual parent2 = tempBestInd[random.Next(numberRandItems)];

            Individual offspring = new Individual(Common.NumberOfItems);
            if (random.NextDouble() < CrossoverRate)
            {
                Crossover(parent1, parent2, out offspring, items);

                Mutate(offspring, items);

                if (generation > 200)
                {
                    LocalImprovement(offspring, items);
                }

                var worst = population.OrderBy(ind => ind.Quality).First();
                if (offspring.Quality > worst.Quality)
                {
                    population.Remove(worst);
                    population.Add(offspring);
                }
            }

            if (generation % 10 == 0)
            {
                Individual bestIndividual = population.OrderByDescending(ind => ind.Quality).First();
                Console.WriteLine($"Generation {generation} - Best Quality: {bestIndividual.Quality}");
            }
        }
    }
}

class Program
{
    const int populationSize = 100;
    static public int generations = 1000;
    static public double crossoverRate = 0.25;
    static public double mutationRate = 0.05;

    static List<Item> GenerateItems()
    {
        Random random = new Random();

        var items = new List<Item>();
        for (int i = 0; i < Common.NumberOfItems; i++)
        {
            items.Add(new Item
            {
                Value = random.Next(2, 31),
                Weight = random.Next(1, 26)
            });
        }
        return items;
    }

    static void Main()
    {
        GenMethod gm = new GenMethod(populationSize, generations, crossoverRate, mutationRate);

        List<Item> items = GenerateItems();
        List<Individual> population = gm.InitializePopulation(items);

        gm.SolveProblem(ref population, items);

        Individual finalBestIndividual = population.OrderByDescending(ind => ind.Quality).First();
        Console.WriteLine($"Best value after 1000 gens: {finalBestIndividual.Quality}");
    }
}