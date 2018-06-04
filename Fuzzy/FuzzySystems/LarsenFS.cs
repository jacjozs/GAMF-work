using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuzzy
{
    public class LarsenFS : FuzzySystemWithFuzzyOutput
    {
        public LarsenFS()
        {
            Type = FuzzyType.Larsen;
        }

        protected override float GetRuleAndMembershipFunctionValueConnectionResult(float ruleValue, float mfValue)
        {
            return ruleValue * mfValue;
        }
    }
}
