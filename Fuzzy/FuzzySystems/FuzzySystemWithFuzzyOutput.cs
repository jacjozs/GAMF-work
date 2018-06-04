using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuzzy
{
    public class FuzzySystemWithFuzzyOutput : FuzzySystem
    {
        public override float CalculateOutputValue(float[] coordinates)
        {
            // data is stored in these parallel arrays
            var Xs = new List<float>();
            var Ys = new List<float>();
            // ruleValues are saved when a valid x-y pair is found, these will be used as divisors
            // when multiple values are found for the same X key, the values will be added

            // For each rule
            for (int ruleIndex = 0; ruleIndex < NumRules; ruleIndex++)
            {
                float RuleValue = CalculateRuleValue(coordinates, ruleIndex);


                // For each X
                for (float X = Outputs[0].Range[0]; X <= Outputs[0].Range[1]; X += (Outputs[0].Range[1] - Outputs[0].Range[0]) / 100)
                {
                    //get an output value Y (based on the output shape's value and the MF value calculated through the rule)
                    float Y = GetRuleAndMembershipFunctionValueConnectionResult(RuleValue, Outputs[0].MFs[Rules[ruleIndex].Outputs[0] - 1].GetMembershipValue(X));
                    //if !=0 add to the XY-lists
                    if (Y != 0.0f)
                    {
                        if (DefuzzMethod == DefuzzMethodType.COA && Xs.Contains(X))
                        {
                            // id is the id where X == Xs[id]
                            int id = Xs.FindIndex(x => Equals(x, X));
                            if (Ys[id] < Y)
                                Ys[id] = Y;
                        }
                        else
                        {
                            Xs.Add(X);
                            Ys.Add(Y);
                        }
                    }
                }
            }

            if (Xs.Count == 0)
                return Outputs[0].Range.Average();

            if (DefuzzMethod == DefuzzMethodType.COA || DefuzzMethod == DefuzzMethodType.COG)
            {
                // Calculate sumX*Y/sumY and return it
                float sumXY = 0;
                for (int i = 0; i < Xs.Count; i++)
                {
                    sumXY += Xs[i] * Ys[i];
                }
                return sumXY / Ys.Sum();
            }

            if (DefuzzMethod == DefuzzMethodType.SOM || DefuzzMethod == DefuzzMethodType.MOM || DefuzzMethod == DefuzzMethodType.LOM)
            {
                List<int> maxXs = new List<int>();
                float maxY = Ys.Max();
                for (int i = 0; i < Ys.Count; i++)
                {
                    if (Ys[i] == maxY)
                        maxXs.Add(i);
                }
                if (DefuzzMethod == DefuzzMethodType.SOM)
                    return Xs[maxXs[0]];
                if (DefuzzMethod == DefuzzMethodType.MOM)
                    return Xs[maxXs[maxXs.Count / 2]];
                if (DefuzzMethod == DefuzzMethodType.LOM)
                    return Xs[maxXs[maxXs.Count - 1]];
            }

            throw new Exception("Incompatible fuzzy system type and defuzzification method.");
        }

        protected virtual float GetRuleAndMembershipFunctionValueConnectionResult(float ruleValue, float mfValue)
        {
            throw new NotImplementedException();
        }
    }
}
