using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormBot.Models
{
    public class Vector
    {
        List<double> scores;
        public Vector(List<double> scores)
        {
            this.scores = scores;
        }

        public static double GetCosineSimilarity(Vector vec1, Vector vec2)
        {
            return GetCosineSimilarity(vec1.scores, vec2.scores);
        }

        private static double GetCosineSimilarity(List<double> vector1, List<double> vector2)
        {
            if (vector1 == null ||
                vector2 == null ||
                vector1.Count != vector2.Count)
            {
                return 0.0;
            }

            var p = 0d;
            var n1 = 0d;
            var n2 = 0d;

            for (var i = 0; i < vector1.Count; i++)
            {
                p += vector1[i] * vector2[i];
                n1 += vector1[i] * vector1[i];
                n2 += vector2[i] * vector2[i];
            }

            if (n1 == 0 || n2 == 0)
            {
                return 0.0;
            }

            n1 = Math.Sqrt(n1);
            n2 = Math.Sqrt(n2);

            return p / (n1 * n2);
        }

    }
}