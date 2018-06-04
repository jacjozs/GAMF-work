using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuzzy
{
    public class MamdaniFS : FuzzySystemWithFuzzyOutput
    {
        public MamdaniFS()
        {
            Type = FuzzyType.Mamdani;
        }

        protected override float GetRuleAndMembershipFunctionValueConnectionResult(float ruleValue, float mfValue)
        {
            return Math.Min(ruleValue, mfValue);
        }
    }
}
