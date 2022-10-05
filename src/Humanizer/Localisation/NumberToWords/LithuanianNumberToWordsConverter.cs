using System;
using System.Collections.Generic;

using Humanizer.Localisation.GrammaticalNumber;

namespace Humanizer.Localisation.NumberToWords
{
    internal class LithuanianNumberToWordsConverter : GenderedNumberToWordsConverter
    {
        private static readonly string[] UnitsMap = { "nulis", "vien", "d", "tr", "ketur", "penk", "šeš", "septyn", "aštuon", "devyn", "dešimt", "vienuolika", "dvylika", "trylika", "keturiolika", "penkiolika", "šešiolika", "septyniolika", "aštuoniolika", "devyniolika" };
        private static readonly string[] TensMap = { "nulis", "dešmit", "dvidešimt", "trisdešimt", "keturiasdešimt", "penkiasdešimt", "šešiasdešimt", "septyniasdešimt", "aštuoniasdešimt", "devyniasdešimt" };
        private static readonly string[] HundredsMap = { "nulis", "šimtas", "du šimtai", "trys šimtai", "keturi šimtai", "penki šimtai", "šeši šimtai", "septyni šimtai", "aštuoni šimtai", "devyni šimtai" };
        private static readonly string[] UnitsOrdinal = { "nulin", "pirm", "antr", "treč", "ketvirt", "penkt", "šešt", "septint", "aštunt", "devint", "dešimt", "vienuolikt", "dvylikt", "trylikt", "keturiolikt", "penkiolikt", "šešiolikt", "septyniolikt", "aštuoniolikt", "devyniolikt", "dvidešimt" };

        public override string Convert(long input, GrammaticalGender gender, bool addAnd = true)
        {
            if (input == 0)
            {
                return UnitsMap[0];
            }
            
            var parts = new List<string>();
            
            var number = input;
            
            if (number < 0)
            {
                parts.Add("minus");
                number = -number;
            }

            var ops = new []
            {
                new { Power = 18, Singular = "kvintilijonas", Plural = "kvintilijonai", SpecialCase = "kvintilijonų" },
                new { Power = 15, Singular = "kvadrilijonas", Plural = "kvadrilijonai", SpecialCase = "kvadrilijonų" },
                new { Power = 12, Singular = "trilijonas", Plural = "trilijonai", SpecialCase = "trilijonų" },
                new { Power = 9, Singular = "milijardas", Plural = "milijardai", SpecialCase = "milijardų" },
                new { Power = 6, Singular = "milijonas", Plural = "milijonai", SpecialCase = "milijonų" },
                new { Power = 3, Singular = "tūkstantis", Plural = "tūkstančiai", SpecialCase = "tūkstančių" },
                new { Power = 2, Singular = "šimtas", Plural = "šimtai", SpecialCase = "šimtų" },
            };

            foreach (var op in ops)
            {
                var pow = (long) Math.Pow(10, op.Power);
                var units = number / pow;

                if (units > 0)
                {
                    parts.Add(GetNumber(units, op.Singular, op.Plural, op.SpecialCase));
                
                    number %= pow;                    
                }
            }

            if (number > 19)
            {
                parts.Add(TensMap[number / 10]);
                number %= 10;
            }

            if (number > 0)
            {
                parts.Add(UnitsMap[number] + GetCardinalEndingForGender(gender, number));
            }

            return string.Join(" ", parts);
        }

        public override string ConvertToOrdinal(int input, GrammaticalGender gender)
        {
            if (input == 0)
            {
                return UnitsOrdinal[0] + GetOrdinalEndingForGender(gender);
            }
            
            var parts = new List<string>();

            if (input < 0)
            {
                parts.Add("minus");
                input = -input;
            }

            var number = (long)input;

            if ((number / 1000000) > 0)
            {
                var millionPart = "";
                if (number == 1000000)
                {
                    millionPart = "miljonas";
                }
                else
                {
                    millionPart = Convert(number / 1000000, GrammaticalGender.Masculine) + " miljonai";
                }
                number %= 1000000;
                parts.Add(millionPart);
            }

            if ((number / 1000) > 0)
            {
                var thousandsPart = "";
                if (number == 1000)
                {
                    thousandsPart = "tūkstančiai";
                }
                else
                {
                    thousandsPart = Convert(number / 1000, GrammaticalGender.Masculine) + " tūkstančiai";
                }
                parts.Add(thousandsPart);
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                var hundredsPart = "";
                if ((number % 100) == 0)
                {
                    hundredsPart = HundredsMap[(number / 100)] + GetOrdinalEndingForGender(gender);
                }
                else
                {
                    if (number > 100 && number < 200)
                    {
                        hundredsPart = "šimtas";
                    }
                    else
                    {
                        hundredsPart = Convert(number / 100, GrammaticalGender.Masculine) + " šimtai";
                    }
                }
                parts.Add(hundredsPart);
                number %= 100;
            }

            if (number > 19)
            {
                var tensPart = TensMap[(number / 10)];
                if ((number % 10) == 0)
                {
                    tensPart += GetOrdinalEndingForGender(gender);
                }
                parts.Add(tensPart);
                number %= 10;
            }

            if (number > 0)
            {
                parts.Add(UnitsOrdinal[number] + GetOrdinalEndingForGender(gender));
            }

            return string.Join(" ", parts);
        }
        
        private string GetNumber(long number, string singular, string plural, string specialCase)
        {
            if (number == 1)
            {
                return singular;
            }

            var text = Convert(number, GrammaticalGender.Masculine);
            
            return LithuanianGrammaticalNumberDetector.Detect(number) switch
            {
                LithuanianGrammaticalNumber.Singular => $"{text} {singular}",
                LithuanianGrammaticalNumber.Plural => $"{text} {plural}",
                LithuanianGrammaticalNumber.SpecialCase or _ => $"{text} {specialCase}",
            };
        }

        private static string GetOrdinalEndingForGender(GrammaticalGender gender)
        {
            return gender switch
            {
                GrammaticalGender.Masculine => "as",
                GrammaticalGender.Feminine => "a",
                _ => throw new ArgumentOutOfRangeException(nameof(gender))
            };
        }

        private static string GetCardinalEndingForGender(GrammaticalGender gender, long number)
        {
            return gender switch
            {
                GrammaticalGender.Masculine => number switch
                {
                    1 => "as",
                    2 => "u",
                    3 => "ys",
                    >0 and < 10 => "i",
                    _ => ""
                },
                GrammaticalGender.Feminine => number switch
                {
                    1 => "a",
                    2 => "vi",
                    3 => "ejos",
                    >0 and < 10 => "ios",
                    _ => ""
                },
                _ => throw new ArgumentOutOfRangeException(nameof(gender))
            };
        }
    }
}
