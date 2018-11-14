﻿using System;
using System.Collections;

namespace Optimization
{
    /// <summary>
    /// A vissza adott listának a 6. eleme tartalmazza a legjobb algoritmus Típusát
    /// Vagy a .type változoban lehet megtalálni
    /// </summary>
    public class AlgoSelecter : BaseOptimizationMethod
    {
        private IOptimizationMethod Optimizer;
        // Clonal generation parameters
        /// <summary>
		/// Clone number coefficient. ( ]0,1] )
		/// </summary>
		private double beta;
        /// <summary>
        /// Mutation coefficient. ( ]0,1] )
        /// </summary>
        private double ro;
        /// <summary>
        /// The number of new antibodies created at the end of each generation.
        /// </summary>
        private int d;
        /// <summary>
        /// Step size relative to the entire search area
        /// </summary>
        private double cglssr;
        /// <summary>
        /// Number of local searches in each generation
        /// </summary>
        private int cgln;
        // Firework parameters
        private int m;
        private double a;
        private double b;
        private double Amax;
        private int mhat;
        // Genetic algorithm parameters
        private double pm;
        private int crossoverCount;
        // Particle swarm parameters
        private double cp;
        private double c0;
        private double cg;
        // Bee algorithm parameters
        private int Elite;
        private int MaxStep;
        // Bacterial algorithm parameters
        private int Infections;
        private int ClonesCount;

        public AlgoSelecter()
        {
            //Create optimizer object.
            //clonal generation params
            beta = 0.5;
            ro = 0.5;
            cglssr = 0.005;
            cgln = 5;
            // Firework params
            m = 50;
            a = 0.04;
            b = 0.8;
            Amax = 40;
            // Particle swarm params
            c0 = 0.8;
            cp = 0.2;
            cg = 0.2;
            // Genetic algorithm params
            pm = 0.6;
            // Bee algorithm params
            Elite = 10;
            MaxStep = 5;
            // Bacterial algorithm params
            Infections = 10;
            ClonesCount = 25;
        }
        protected override void CreateNextGeneration()
        {
            ArrayList AlgInfos = new ArrayList();
            for (int l = 0; l < 3; l++)
            {
                Result[,] results = new Result[9, 2];
                for (int j = 0; j < 9; j++)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                StoppingType = StoppingType.GenerationNumber;
                                break;
                            case 1:
                                StoppingType = StoppingType.EvaluationNumber;
                                break;
                        }
                        OptMethod(j);
                        Result Results = Optimizer.Optimize();
                        Results.InfoList.Add(Optimizer.GetType());
                        Results.InfoList.Add(l);
                        results[j, i] = Results;
                        AlgInfos.Add(results);
                    }
                }
            }
            Info.Add(BestCalc(AlgInfos));
            Info.Add(AlgInfos);
            Stop = true;
        }

        private Type BestCalc(ArrayList AlgInfos)
        {
            int index = -1;
            int index2 = -1;
            double id = double.MaxValue;
            for (int j = 0; j < AlgInfos.Count / 3; j++)
            {
                for (int i = 0; i < ((Result[,])AlgInfos[i]).Length / 2; i++)
                {
                    Result[,] results = (Result[,])AlgInfos[i];
                    double ev = double.Parse(results[i, 0].InfoList[3].ToString()) / StoppingNumberOfEvaluations;
                    double ge = double.Parse(results[i, 1].InfoList[2].ToString()) / StoppingNumberOfGenerations;
                    double deltaA = Math.Abs(ev - ge);
                    ev = double.Parse(results[i, 0].InfoList[1].ToString());
                    ge = double.Parse(results[i, 1].InfoList[1].ToString());
                    double deltaB = Math.Abs(ev - ge);
                    results[i, 0].InfoList[0] = deltaA * deltaB;
                    if ((double)results[i, 0].InfoList[0] < id)
                    {
                        id = (double)results[i, 0].InfoList[0];
                        index = i;
                        index2 = j;
                    }
                }
            }
            return (Type)((Result[,])AlgInfos[index2])[index, 0].InfoList[6];
        }

        private void OptMethod(int id)
        {
            switch (id)
            {
                case 0: //Firework
                    Optimizer = new Firework
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds  = LowerParamBounds,
                        UpperParamBounds  = UpperParamBounds,
                        Integer = Integer,
                        FitnessFunction = FitnessFunction,
                        // Number of particles in the swarm.
                        NumberOfElements  = NumberOfElements,
                        m = m,
                        ymax = double.MaxValue,
                        a = a,
                        b = b,
                        Amax = Amax,
                        mhat = NumberOfElements,
                        StoppingType = StoppingType,
                        // Number of generations.
                        StoppingNumberOfGenerations  = StoppingNumberOfGenerations,
                        // Number of allowed affinity evaluations.
                        StoppingNumberOfEvaluations  = StoppingNumberOfEvaluations,
                        // Affinity treshold.
                        StoppingFitnessTreshold  = StoppingFitnessTreshold,
                        Slow = Slow
                    };
                    break;
                case 1: //Particle Swarm
                    Optimizer = new ParticleSwarm
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds  = LowerParamBounds,
                        UpperParamBounds  = UpperParamBounds,
                        Integer = Integer,
                        FitnessFunction = FitnessFunction,
                        // Number of particles in the swarm.
                        NumberOfElements  = NumberOfElements,
                        // Number of generations.
                        StoppingNumberOfGenerations  = StoppingNumberOfGenerations,

                        c0 = c0,
                        // Multiplication factor for the distance to the personal best position.
                        cp = cp,
                        // Multiplication factor for the distance to the global best position.
                        cg = cg,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations  = StoppingNumberOfEvaluations,
                        // Fitness treshold.
                        StoppingFitnessTreshold  = StoppingFitnessTreshold,
                        StoppingType = StoppingType,
                        Slow = Slow
                    };
                    break;
                case 2: //Clonal generation
                    Optimizer = new ClonalGeneration
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds  = LowerParamBounds,
                        UpperParamBounds  = UpperParamBounds,
                        Integer = Integer,
                        FitnessFunction = FitnessFunction,
                        // Size of the antibody pool.
                        NumberOfElements  = NumberOfElements,
                        // Number of antibodies selected for cloning.
                        NumberSelectedForCloning = NumberOfElements / 3,
                        // Parameter determining the number of clones created for an antibody that was selected for cloning. (0,1]
                        Beta = beta,
                        // Number of antibodies created with random parameters in each new generation. 
                        RandomAntibodiesPerGeneration = NumberOfElements / 5,
                        // Mutation coefficient (0,1].
                        Ro = ro,
                        // Stopping criteria.
                        StoppingType = StoppingType,
                        // Number of generations.
                        StoppingNumberOfGenerations  = StoppingNumberOfGenerations,
                        // Number of allowed affinity evaluations.
                        StoppingNumberOfEvaluations  = StoppingNumberOfEvaluations,
                        // Affinity treshold.
                        StoppingFitnessTreshold  = StoppingFitnessTreshold,
                        Slow = Slow
                    };
                    break;
                case 3:
                    Optimizer = new GeneticAlgorithm
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds  = LowerParamBounds,
                        UpperParamBounds  = UpperParamBounds,
                        Integer = Integer,
                        FitnessFunction = FitnessFunction,
                        // Size of the individual pool.
                        NumberOfElements  = NumberOfElements,
                        // Number of parents in each generation.
                        ParentsInEachGeneration = NumberOfElements / 2,
                        // The probability of mutation.
                        MutationProbability = pm,
                        // The number of crossovers in each generation.
                        CrossoverPerGeneration = NumberOfElements,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations  = StoppingNumberOfEvaluations,
                        // Fitness treshold.
                        StoppingFitnessTreshold  = StoppingFitnessTreshold,
                        // Number of generations.
                        StoppingNumberOfGenerations  = StoppingNumberOfGenerations,
                        // Stopping criteria.
                        StoppingType = StoppingType,
                        Slow = Slow
                    };
                    break;
                case 4: //Clonal generation local
                    Optimizer = new ClonalGenerationLocal
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds  = LowerParamBounds,
                        UpperParamBounds  = UpperParamBounds,
                        Integer = Integer,
                        FitnessFunction = FitnessFunction,
                        // Size of the antibody pool.
                        NumberOfElements  = NumberOfElements,
                        // Number of antibodies selected for cloning.
                        NumberSelectedForCloning = NumberOfElements / 3,
                        // Parameter determining the number of clones created for an antibody that was selected for cloning. (0,1]
                        Beta = beta,
                        // Number of antibodies created with random parameters in each new generation. 
                        RandomAntibodiesPerGeneration = d,
                        // Mutation coefficient (0,1].
                        Ro = ro,
                        // RelativeStepSize
                        StepSizeRelative = cglssr,
                        // Local search/get
                        LocalSearchesPerGeneration = cgln,
                        // Stopping criteria.
                        StoppingType = StoppingType,
                        // Number of generations.
                        StoppingNumberOfGenerations  = StoppingNumberOfGenerations,
                        // Number of allowed affinity evaluations.
                        StoppingNumberOfEvaluations  = StoppingNumberOfEvaluations,
                        // Affinity treshold.
                        StoppingFitnessTreshold  = StoppingFitnessTreshold,
                        Slow = Slow
                    };
                    break;
                case 5:
                    Optimizer = new CoordinateDescent
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds  = LowerParamBounds,
                        UpperParamBounds  = UpperParamBounds,
                        // Size of the antibody pool.
                        NumberOfElements = 1,
                        Integer = Integer,
                        FitnessFunction = FitnessFunction,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations  = StoppingNumberOfEvaluations,
                        // Fitness treshold.
                        StoppingFitnessTreshold  = StoppingFitnessTreshold,
                        // Number of generations.
                        StoppingNumberOfGenerations  = StoppingNumberOfGenerations,
                        // Stopping criteria.
                        StoppingType = StoppingType,
                        Slow = Slow
                    };
                    break;
                case 6:
                    Optimizer = new ABC
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds  = LowerParamBounds,
                        UpperParamBounds  = UpperParamBounds,
                        Integer = Integer,
                        FitnessFunction = FitnessFunction,
                        // Size of the antibody pool.
                        NumberOfElements  = NumberOfElements,
                        //Felderitő méhek maximális keressi számas ciklus alatt
                        MaxStep = MaxStep,
                        //Felderítő méhek száma
                        Elite = Elite,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations  = StoppingNumberOfEvaluations,
                        // Fitness treshold.
                        StoppingFitnessTreshold  = StoppingFitnessTreshold,
                        // Number of generations.
                        StoppingNumberOfGenerations  = StoppingNumberOfGenerations,
                        // Stopping criteria.
                        StoppingType = StoppingType,
                        Slow = Slow
                    };
                    break;
                case 7:
                    Optimizer = new BFOA
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds  = LowerParamBounds,
                        UpperParamBounds  = UpperParamBounds,
                        Integer = Integer,
                        // Size of the antibody pool.
                        NumberOfElements  = NumberOfElements,
                        ClonesCount = ClonesCount,
                        Infections = Infections,
                        FitnessFunction = FitnessFunction,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations  = StoppingNumberOfEvaluations,
                        // Fitness treshold.
                        StoppingFitnessTreshold  = StoppingFitnessTreshold,
                        // Number of generations.
                        StoppingNumberOfGenerations  = StoppingNumberOfGenerations,
                        // Stopping criteria.
                        StoppingType = StoppingType,
                        Slow = Slow
                    };
                    break;
                case 8:
                    Optimizer = new AntAlg
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds = LowerParamBounds,
                        UpperParamBounds = UpperParamBounds,
                        Integer = Integer,
                        // Size of the antibody pool.
                        NumberOfElements = NumberOfElements,
                        FitnessFunction = FitnessFunction,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations = StoppingNumberOfEvaluations,
                        // Fitness treshold.
                        StoppingFitnessTreshold = StoppingFitnessTreshold,
                        // Number of generations.
                        StoppingNumberOfGenerations = StoppingNumberOfGenerations,
                        // Stopping criteria.
                        StoppingType = StoppingType,
                    };
                    break;
            }
        }
    }
}