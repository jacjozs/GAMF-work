namespace Optimization
{
    public enum InfoTypes
    {
        InitialFitness,//Indulási fitnesz
        FinalParameters,//Végső paraméter lista
        FinalFitness,//Végső fitnesz
        Generations,//Generációk száma
        Evaluations,//Számítások mennyisége
        Affinities,
        ExecutionTime,//Idő tartam
        /*Választónál használt elemek*/
        AlgType,//Algoritmus típusa
        SelectAlgResult,//Kiválasztott algoritmus eredménye
        SelectAlgType,//Kiválasztott algoritmus típusa
        SelectAlgInfos,//Tesztek utáni eredmények az algoritmusoktól
        SelectAlgNum,//Hanyadik körben futto az algoritmus
        SelectAlgFitness//Az algoritmus teljesítmény fitnesze
    }
}
