namespace Optimization
{
    public enum InfoTypes
    {
        /// <summary>
        /// Indulási fitnesz
        /// </summary>
        InitialFitness,
        /// <summary>
        /// Végső paraméter lista
        /// </summary>
        FinalParameters,
        /// <summary>
        /// Végső fitnesz
        /// </summary>
        FinalFitness,
        /// <summary>
        /// Generációk száma
        /// </summary>
        Generations,
        /// <summary>
        /// Számítások mennyisége
        /// </summary>
        Evaluations,
        Affinities,
        /// <summary>
        /// Idő tartam
        /// </summary>
        ExecutionTime,
        /*Választónál használt elemek*/
        /// <summary>
        /// Algoritmus típusa
        /// </summary>
        AlgType,
        /// <summary>
        /// Kiválasztott algoritmus eredménye
        /// </summary>
        SelectAlgResult,
        /// <summary>
        /// Kiválasztott algoritmus típusa
        /// </summary>
        SelectAlgType,
        /// <summary>
        /// Tesztek utáni eredmények az algoritmusoktól
        /// </summary>
        SelectAlgInfos,
        /// <summary>
        /// Hanyadik körben futto az algoritmus
        /// </summary>
        SelectAlgNum,
        /// <summary>
        /// Az algoritmus teljesítmény fitnesze
        /// </summary>
        SelectAlgFitness
    }
}
