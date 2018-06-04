using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuzzy
{
    public class SugenoFS : FuzzySystem
    {
        public SugenoFS()
        {
            Type = FuzzyType.Sugeno;
        }

        public override float CalculateOutputValue(float[] coordinates)
        {
            // Numerator = sum of (ruleValue*outputValue)-s
            // Denominator = sum of ruleValues
            float SugenoNumerator = 0.0f;
            float SugenoDenominator = 0.0f;
            
            // Get values
            for (int ruleIndex = 0; ruleIndex < NumRules; ruleIndex++)
            {
                float RuleValue = CalculateRuleValue(coordinates, ruleIndex);
                SugenoNumerator += RuleValue * Outputs[0].MFs[Rules[ruleIndex].Outputs[0] - 1].GetMembershipValue(0.0f);
                SugenoDenominator += RuleValue;
            }

            // If no valid output return NaN
            if (SugenoDenominator == 0.0f)
                //return float.NaN;
                return Outputs[0].Range.Average();

            // Weighed average
            if (DefuzzMethod == DefuzzMethodType.wtaver)
                return SugenoNumerator / SugenoDenominator;
            // Weighed sum
            if (DefuzzMethod == DefuzzMethodType.wtsum)
                return SugenoNumerator;

            throw new Exception("Incompatible fuzzy system type and defuzzification method.");
        }
    }
}
