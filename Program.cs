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

    public List<Individual> InitializePopulation(int numberOfItems, List<Item> items)
    {
        var population = new List<Individual>();

        for (int i = 0; i < PopulationSize; i++)
        {
            var individual = new Individual(numberOfItems);
            int totalWeight = 0;

            var randomItems = items.OrderBy(x => random.Next()).ToList();

            foreach (var item in randomItems)
            {
                if (totalWeight + item.Weight <= Common.Capacity)
                {
                    individual.Genes[items.IndexOf(item)] = true;
                    totalWeight += item.Weight;
                }
            }

            population.Add(individual);
        }
        return population;
    }

    public void EvaluateQuality(Individual individual, List<Item> items)
    {
        int totalValue = 0;
        int totalWeight = 0;

        for (int i = 0; i < items.Count; i++)
        {
            if (individual.Genes[i])
            {
                totalValue += items[i].Value;
                totalWeight += items[i].Weight;
            }
        }

        if (totalWeight > Common.Capacity)
        {
            individual.Quality = 0;
        }
        else
        {
            individual.Quality = totalValue;
        }
    }

    public void Crossover(Individual parent1, Individual parent2, out Individual offspring1, out Individual offspring2)
    {
        offspring1 = new Individual(Common.NumberOfItems);
        offspring2 = new Individual(Common.NumberOfItems);

        int point1 = random.Next(Common.NumberOfItems);
        int point2 = random.Next(point1, Common.NumberOfItems);
        int point3 = random.Next(point2, Common.NumberOfItems);

        for (int i = 0; i < Common.NumberOfItems; i++)
        {
            if (i < point1 || (i >= point2 && i < point3))
            {
                offspring1.Genes[i] = parent1.Genes[i];
                offspring2.Genes[i] = parent2.Genes[i];
            }
            else
            {
                offspring1.Genes[i] = parent2.Genes[i];
                offspring2.Genes[i] = parent1.Genes[i];
            }
        }
    }

    public void Mutate(Individual individual)
    {
        if (random.NextDouble() < MutationRate)
        {
            int geneToMutate = random.Next(Common.NumberOfItems);
            individual.Genes[geneToMutate] = !individual.Genes[geneToMutate];
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
                double bestValuePerWeight = (double)items[i].Value / items[i].Weight;
                int bestItemIndex = i;
                double bestItemValuePerWeight = bestValuePerWeight;

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

    //public Individual RouletteSelection(List<Individual> population)
    //{
    //    int totalQuality = population.Sum(ind => ind.Quality);
    //    double pick = random.NextDouble() * totalQuality;

    //    var probabilities = population.Select(ind => (double)ind.Quality / totalQuality).ToList();

    //    double randomValue = random.NextDouble();

    //    double cumulativeProbability = 0.0;
    //    for (int i = 0; i < population.Count; i++)
    //    {
    //        cumulativeProbability += probabilities[i];
    //        if (randomValue < cumulativeProbability)
    //        {
    //            return population[i];
    //        }
    //    }
    //    return population.Last();
    //}

    //public Individual TournamentSelection(List<Individual> population, int tournamentSize)
    //{
    //    var tournament = new List<Individual>();
    //    for (int i = 0; i < tournamentSize; i++)
    //    {
    //        var randomIndividual = population[random.Next(population.Count)];
    //        tournament.Add(randomIndividual);
    //    }
    //    return tournament.OrderByDescending(ind => ind.Quality).First();
    //}


    public void SolveProblem(ref List<Individual> population, List<Item> items)
    {
        for (int generation = 1; generation <= Generations; generation++)
        {
            foreach (var individual in population)
            {
                EvaluateQuality(individual, items);
            }

            population = population.OrderByDescending(ind => ind.Quality).ToList();
            List<Individual> newPopulation = new List<Individual>();

            for (int i = 0; i < PopulationSize / 2; i++)
            {
                //Individual parent1 = RouletteSelection(population);
                //Individual parent2 = RouletteSelection(population);

                //Individual parent1 = population[random.Next(PopulationSize)];
                //Individual parent2 = population[random.Next(PopulationSize)];

                //Individual parent1 = TournamentSelection(population, 3);
                //Individual parent2 = TournamentSelection(population, 3);

                int numberRandItems = 10;
                Individual[] tempBestInd = new Individual[numberRandItems];
                for (int j = 0; j < numberRandItems; j++)
                {
                    tempBestInd[j] = population.OrderByDescending(ind => ind.Quality).ToArray()[j];
                }

                Individual parent1 = population[random.Next(PopulationSize)];
                Individual parent2 = tempBestInd[random.Next(numberRandItems)];

                if (random.NextDouble() < CrossoverRate)
                {
                    Individual offspring1, offspring2;
                    Crossover(parent1, parent2, out offspring1, out offspring2);

                    Mutate(offspring1);
                    Mutate(offspring2);
                    LocalImprovement(offspring1, items);
                    LocalImprovement(offspring2, items);

                    newPopulation.Add(offspring1);
                    newPopulation.Add(offspring2);
                }
                else
                {
                    // Емпірично підвищило вдвічі ефективність, відходить від біологічного принципу
                    Mutate(parent1);
                    Mutate(parent2);
                    LocalImprovement(parent1, items);
                    LocalImprovement(parent2, items);

                    newPopulation.Add(parent1);
                    newPopulation.Add(parent2);
                }
            }
            population = newPopulation;

            if (generation % 10 == 0)
            {
                Individual bestIndividual = population.OrderByDescending(ind => ind.Quality).First();
                Console.WriteLine($"Generation {generation} - Best Fitness: {bestIndividual.Quality}");
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
        List<Individual> population = gm.InitializePopulation(Common.NumberOfItems, items);

        gm.SolveProblem(ref population, items);

        Individual finalBestIndividual = population.OrderByDescending(ind => ind.Quality).First();
        Console.WriteLine($"Best value after 1000 gens: {finalBestIndividual.Quality}");
    }
}