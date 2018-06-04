using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuzzy
{
    public enum MFtype
    {
        trapmf,
        trimf,
        //singlmf, // Singloid
        constant
    }

    public class MembershipFunction
    {
        /// <summary>
        /// The type of this mf
        /// </summary>
		private MFtype _Type;
        /// <summary>
        /// The type of this mf, setting it will initialize the arrays
        /// </summary>
		public MFtype Type
        {
            get { return _Type; }
            set
            {
                _Type = value;
                switch (value)
                {
                    case MFtype.trapmf:
                        Params = new float[4];
                        Values = new float[4];
                        break;
                    case MFtype.trimf:
                        Params = new float[3];
                        Values = new float[3];
                        break;
                    case MFtype.constant:
                        Params = new float[1];
                        Values = new float[1];
                        Values[0] = 1.0f;
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// Parameters of the MF, usually coordinates
        /// </summary>
        public float[] Params { get; set; }
        /// <summary>
        /// Values at coordinates
        /// </summary>
        public float[] Values { get; set; }

        /// <summary>
        /// Returns membership value at a given coordinate
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
		public float GetMembershipValue(float coordinate)
        {
            switch (_Type)
            {
                // THIS CODE WORKS FOR ANY LINEAR MF
                case MFtype.trapmf:
                case MFtype.trimf:
                    //case MFtype.otherLinearMF:..
                    // Check if the coordiate is the same as one or muliple parameters (in case it's a vertical segment)
                    {
                        float result = -1.0f;
                        for (int j = 0; j < Params.Length; j++)
                        {
                            if (coordinate == Params[j] && Values[j] > result)
                                result = Values[j];
                        }
                        if (result != -1.0f)
                            return result;
                    }

                    // Check if the coordinate is outside the trapeze shepe
                    if (coordinate <= Params[0])
                        return Values[0];
                    if (coordinate >= Params[Params.Length - 1])
                        return Values[Params.Length - 1];
                    // Find the greatest stored coordinate which is less than the coordinate we're checking
                    int i = Params.Length - 2;
                    while (coordinate <= Params[i])
                        i--;
                    // Return the value on the line between the 2 points
                    return (coordinate - Params[i]) / (Params[i + 1] - Params[i]) * (Values[i + 1] - Values[i]) + Values[i];
                //break;

                case MFtype.constant:
                    return Params[0];

                default:
                    break;
            }

            // This should never be reached when used properly
            // Return a value that will make it obvious that something is broken; replace with an exception in the future?
            return float.MinValue;
        }

        public MembershipFunction()
        {

        }
        // Example string 'A_{1;1}':'trapmf',[-2.40 -2.10 -1.90 -1.60]![0 1 1 0]
        public MembershipFunction(string DataString)
        {
            // todo: Do something with whatever the first part is

            // Set the shape
            string shapeString = DataString.Split(':')[1].Split(',')[0].Replace("'", "");
            if (shapeString == "trapmf") Type = MFtype.trapmf;
            else if (shapeString == "trimf") Type = MFtype.trimf;
            else if (shapeString == "singlmf") Type = MFtype.constant;
            else if (shapeString == "constant") Type = MFtype.constant;
            //else if..

            // Store the coordinates
            string[] coordinateStrings = DataString.Split(',')[1].Split('!')[0].Replace("[", "").Replace("]", "").Split(' ');
            for (int i = 0; i < Params.Length; i++)
            {
                Params[i] = float.Parse(coordinateStrings[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            if (DataString.Contains("!"))
            {
                // Store the values
                string[] valueStrings = DataString.Split('!')[1].Replace("[", "").Replace("]", "").Split(' ');
                for (int i = 0; i < Values.Length; i++)
                {
                    Values[i] = float.Parse(valueStrings[i], System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            else
            {
                if (Type == MFtype.constant)
                    Values[0] = 1;
                else
                {
                    Values[0] = 0;
                    for (int i = 1; i < Values.Length - 1; i++)
                        Values[i] = 1;
                    Values[Values.Length - 1] = 0;
                }
            }
        }
        
        /// <summary>
        /// Returns a copy of this membership function situated at a different reference point
        /// </summary>
        /// <param name="referencePoint"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public MembershipFunction GetAtRefPoint(float referencePoint)
        {
            switch (Type)
            {
                case MFtype.trapmf:
                    return CreateTrapezoid(referencePoint, GetTrapBottomBase(), GetTrapTopBase());
                case MFtype.trimf:
                    return CreateTriangle(referencePoint, GetTriBottomSize());
                case MFtype.constant:
                    return CreateConstant(referencePoint);
                default:
                    break;
            }

            return null;
        }
        /// <summary>
        /// Returns a trapezoid MF generated using the given parameters
        /// </summary>
        /// <param name="referencePoint">The center coordinate of the trapezoid</param>
        /// <param name="bottomBaseSize">The size of the bottom base</param>
        /// <param name="topBaseSize">The size of the top base</param>
        /// <returns></returns>
        public static MembershipFunction CreateTrapezoid(float referencePoint, float bottomBaseSize, float topBaseSize)
        {
            // Initialize
            MembershipFunction result = new MembershipFunction();

            // Set data
            result.Type = MFtype.trapmf;

            // Set coordinates
            result.Params[0] = referencePoint - bottomBaseSize * 0.5f;
            result.Params[1] = referencePoint - topBaseSize * 0.5f;
            result.Params[2] = referencePoint + topBaseSize * 0.5f;
            result.Params[3] = referencePoint + bottomBaseSize * 0.5f;

            // Set values
            result.Values[0] = 0.0f;
            result.Values[1] = 1.0f;
            result.Values[2] = 1.0f;
            result.Values[3] = 0.0f;

            // Return result
            return result;
        }
        /// <summary>
        /// Returns a triangle MF generated using the given parameters
        /// </summary>
        /// <param name="referencePoint">The tip of the triangle</param>
        /// <returns></returns>
        public static MembershipFunction CreateTriangle(float referencePoint, float bottomSize)
        {
            // Initialize
            MembershipFunction result = new MembershipFunction();

            // Set data
            result.Type = MFtype.trimf;

            // Set coordinates
            result.Params[0] = referencePoint + bottomSize * 0.5f;
            result.Params[1] = referencePoint;
            result.Params[2] = referencePoint + bottomSize * 0.5f;

            // Set values
            result.Values[0] = 0.0f;
            result.Values[1] = 1.0f;
            result.Values[2] = 0.0f;

            // Return result
            return result;
        }

        public static MembershipFunction CreateConstant(float refPoint)
        {
            MembershipFunction result = new MembershipFunction();

            result.Type = MFtype.constant;

            result.Params[0] = refPoint;

            return result;
        }

        public override string ToString()
        {
            return ToString();
        }
        public string ToString(bool includeValues = true)
        {
            string outString = "";

            outString += "'" + Type + "',[";

            for (int i = 0; i < Params.Length - 1; i++)
                outString += Params[i].ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB")) + " ";
            outString += Params[Params.Length - 1].ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB"));

            if (!includeValues)
                return outString + "]\r\n";

            outString += "]![";
            for (int i = 0; i < Values.Length - 1; i++)
                outString += Values[i].ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB")) + " ";
            outString += Values[Values.Length - 1].ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB")) + "]\r\n";

            return outString;
        }

        public float GetTrapReferencePoint()
        {
            if (Type != MFtype.trapmf)
                return float.NaN;
            return (Params[1] + Params[2]) / 2.0f;
        }
        public float GetTrapBaseRatio()
        {
            if (Type != MFtype.trapmf)
                return float.NaN;
            return (Params[2] - Params[1]) / (Params[3] - Params[0]);
        }
        public float GetTrapBottomBase()
        {
            if (Type != MFtype.trapmf)
                return float.NaN;
            return Params[3] - Params[0];
        }
        public float GetTrapTopBase()
        {
            if (Type != MFtype.trapmf)
                return float.NaN;
            return Params[2] - Params[1];
        }

        public float GetTriReferencePoint()
        {
            if (Type != MFtype.trimf)
                return float.NaN;
            return Params[1];
        }
        public float GetTriBottomSize()
        {
            if (Type != MFtype.trimf)
                return float.NaN;
            return Params[2]-Params[0];
        }

        public float GetReferencePoint()
        {
            switch (Type)
            {
                case MFtype.trapmf:
                    return GetTrapReferencePoint();
                case MFtype.trimf:
                    return GetTriReferencePoint();
                case MFtype.constant:
                    return Params[0];
                default:
                    break;
            }
            return float.NaN;
        }
    }
}
